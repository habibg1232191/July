using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Rendering;
using FFmpeg.AutoGen;
using July.FFmpeg;

namespace July;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .UseSkia()
            .LogToDebug();

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            bool dwmEnabled;
            if (DwmIsCompositionEnabled(out dwmEnabled) == 0 && dwmEnabled)
            {
                var wp = builder.WindowingSubsystemInitializer;
                return builder.UseWindowingSubsystem(() =>
                {
                    wp();
                    AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToConstant(new WindowsDWMRenderTimer());
                });
            }
        }

        return builder;
    }

    [DllImport("Dwmapi.dll")]
    private static extern int DwmIsCompositionEnabled(out bool enabled);
}

class WindowsDWMRenderTimer : IRenderTimer
{
    public bool RunsInBackground => true;
    public event Action<TimeSpan> Tick;
    private Thread _renderTick;

    public WindowsDWMRenderTimer()
    {
        _renderTick = new Thread(() =>
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (true)
            {
                DwmFlush();
                Tick?.Invoke(sw.Elapsed);
            }
        });
        _renderTick.IsBackground = true;
        _renderTick.Start();
    }

    [DllImport("Dwmapi.dll")]
    private static extern int DwmFlush();
}

static class FFmpegBuilder
{
    public static T WithFFmpeg<T>(this T builder) where T : AppBuilderBase<T>, new()
    {
        ConfigureFFmpeg();
        FFmpegUtils.SetupLogging();
        return builder;
    }

    private static void ConfigureFFmpeg()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var current = Environment.CurrentDirectory;
            var probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");

            while (current != null)
            {
                var ffmpegBinaryPath = Path.Combine(current, probe);

                if (Directory.Exists(ffmpegBinaryPath))
                {
                    ffmpeg.RootPath = ffmpegBinaryPath;
                    return;
                }

                current = Directory.GetParent(current)?.FullName;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            ffmpeg.RootPath = "/lib/x86_64-linux-gnu/";
        else
            throw new NotSupportedException(); // fell free add support for platform of your choose
    }
}