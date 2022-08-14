using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Win32;
using July.Core.Plugin;
using July.ViewModels;

namespace July.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        MainScrollViewer.ScrollChanged += ScrollChanged;
    }

    private void ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var mainViewModel = (MainWindowViewModel)DataContext!;
        var scrollViewer = (ScrollViewer)sender!;
        var isScrollToEnd = Math.Abs(scrollViewer.Extent.Height - (scrollViewer.Offset.Y + scrollViewer.Bounds.Height)) == 0;
        if (isScrollToEnd && mainViewModel.Wallpapers.Count > 0)
            mainViewModel.LoadNextPage();
    }
    
    public void ShowWallpaperWindow(MainWindowViewModel mainWm)
    {
        var wallpaperWm = new WallpaperWindowViewModel();
        WallpaperWindow wallpaperWindow = new WallpaperWindow
        {
            DataContext = wallpaperWm
        };
        wallpaperWindow.Show();
#if DEBUG
        Closing += (_, _) => wallpaperWindow.Close();
#endif
        mainWm.WallpaperChangedEvent += wallpaperWm.OnWallpaperChange;
        
        // if (wallpaperWindow.PlatformImpl != null)
        //     Win32Utils.SetWindowProc(wallpaperWindow.PlatformImpl.Handle.Handle, (wnd, msg, param, lParam) =>
        //     {
        //         Console.WriteLine(msg);
        //         return Win32Utils.DefWindowProc(wnd, (int)msg, param, lParam);
        //     });
        wallpaperWindow.SetWindowToWallpaper();
    }

    protected override bool HandleClosing()
    {
        Hide();
        return true;
    }

    private void LoadFullWallpaper(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Control { DataContext: IWallpaperInfo wallpaperInfo } && DataContext is MainWindowViewModel mainWindowViewModel)
            mainWindowViewModel.WallpaperChangedEvent.Invoke(new WallpaperChangedEventArgs(wallpaperInfo));
    }
}

class WindowIm : WindowImpl
{
    
}