using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using July.Core.Plugin;

namespace July.Core.Services;

public enum WallpaperPluginServiceState
{
    Loaded, Loading, NotFound
}

public delegate void PluginServiceStateChanged(WallpaperPluginServiceState serviceState);

public static class WallpaperPluginService
{
    public static event PluginServiceStateChanged? PluginsLoaded;
    private static readonly List<IWallpaperPlugin> Plugins = new();
    private static FileSystemWatcher? _dllFileWatcher;

    private static int _selectedPluginIndex;

    private static int SelectedPluginIndex
    {
        get => _selectedPluginIndex;
        set => _selectedPluginIndex = Math.Clamp(value, 0, Plugins.Count);
    }

    public static IWallpaperPlugin SelectedPlugin => Plugins[SelectedPluginIndex];

    private static WallpaperPluginServiceState _pluginServiceState;

    public static void LoadPlugins()
    {
        var pluginsPath = Directory.GetCurrentDirectory() + @"\Plugins";
        _dllFileWatcher = new FileSystemWatcher(pluginsPath, "*.dll");
        _dllFileWatcher.NotifyFilter = NotifyFilters.Attributes
                                       | NotifyFilters.CreationTime
                                       | NotifyFilters.DirectoryName
                                       | NotifyFilters.FileName
                                       | NotifyFilters.LastAccess
                                       | NotifyFilters.LastWrite
                                       | NotifyFilters.Security
                                       | NotifyFilters.Size;
        
        _dllFileWatcher.Changed += (_, args) =>
        {
        };
        _dllFileWatcher.Created += (_, args) =>
        {
        };
        _dllFileWatcher.IncludeSubdirectories = true;
        _dllFileWatcher.EnableRaisingEvents = true;

        foreach (var pluginPath in Directory.GetFiles(pluginsPath, "*Plugin.dll"))
        {
            var plugin = TryLoadPlugin(pluginPath);
            if(plugin != null)
                Plugins.Add(plugin);
        }
        _pluginServiceState = Plugins.Count == 0 ? WallpaperPluginServiceState.NotFound : WallpaperPluginServiceState.Loaded;
        if (_pluginServiceState != WallpaperPluginServiceState.NotFound)
            SelectedPluginIndex = 0;
        PluginsLoaded?.Invoke(_pluginServiceState);
    }

    private static IWallpaperPlugin? TryLoadPlugin(string pluginPath)
    {
        try
        {
            var assemblyLoadContext = new AssemblyLoadContext(pluginPath);
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(pluginPath);
        
            foreach (var type in assembly.GetTypes())
                if (type.Name.EndsWith("Plugin") && Activator.CreateInstance(type) is IWallpaperPlugin wallpaperPlugin)
                    return wallpaperPlugin;
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void Dispose()
    {
        _dllFileWatcher?.Dispose();
        Plugins.Clear();
        _pluginServiceState = WallpaperPluginServiceState.NotFound;
    }
}