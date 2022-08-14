using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using July.Utils;

namespace July.Views;

public partial class WallpaperWindow : Window
{
    public WallpaperWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void SetWindowToWallpaper()
    {
        if (PlatformImpl is null || Screens.Primary is null) return;
        Win32Utils.SetParent(PlatformImpl.Handle.Handle, Win32Utils.FindDtwWindow());
        Position = new PixelPoint(0, 0);
        var screenSize = Screens.Primary.Bounds;
        Width = screenSize.Width;
        Height = screenSize.Height;
    }
}