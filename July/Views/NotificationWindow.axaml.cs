using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using July.Utils;
using July.ViewModels;

namespace July.Views;

public partial class NotificationWindow : Window
{
    public NotificationWindow()
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

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Screens.Primary != null)
        {
            Win32Utils.RECT rc = default;
            Win32Utils.SystemParametersInfo(0x0030, 0, ref rc, 0);
            Position = new PixelPoint(rc.right, rc.bottom);
            Position -= PixelPoint.FromPoint(new Point(availableSize.Width, availableSize.Height), 1.25);
        }

        if (NotificationWindowViewModel.MainNotificationViewModel?.Notifications.Count <= 0)
        {
            Hide();
            return Size.Empty;
        }

        Show();
        return base.MeasureOverride(availableSize);
    }
}