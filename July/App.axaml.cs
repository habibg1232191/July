using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using July.Core.Services;
using July.ViewModels;
using July.Views;
using ReactiveUI;

namespace July
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public ReactiveCommand<Unit, Unit> ExitCommand { get; set; }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                WallpaperPluginService.LoadPlugins();
                var notificationWindow = new NotificationWindow
                {
                    DataContext = new NotificationWindowViewModel()
                };
                notificationWindow.Show();
                var mainWm = new MainWindowViewModel();
                var mainWindow = new MainWindow { DataContext = mainWm };
                desktop.MainWindow = mainWindow;
                mainWindow.ShowWallpaperWindow(mainWm);
                var tray = new TrayIcon();
                tray.ToolTipText = "July";
                tray.Icon = new WindowIcon("avalonia-logo.ico");
                tray.Clicked += (_, _) => mainWindow.Show();
                
                ExitCommand = ReactiveCommand.Create(() =>
                {
                    WallpaperPluginService.Dispose();
                    tray.Dispose();
                    desktop.TryShutdown();
                });
                
                tray.Menu = new NativeMenu
                {
                    Items = 
                    { 
                        new NativeMenuItem("Exit")
                        {
                            Command = ExitCommand
                        }
                    }
                };
                // mainWindow.Closing += (_, _) =>
                // {
                //     WallpaperPluginService.Dispose();
                //     tray.Dispose();
                //     desktop.TryShutdown();
                // };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}