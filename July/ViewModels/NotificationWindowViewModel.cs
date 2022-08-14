using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;

namespace July.ViewModels;

public class NotificationWindowViewModel : ViewModelBase
{
    public static NotificationWindowViewModel? MainNotificationViewModel;
    private ObservableCollection<Notification> _notifications = new ();

    public ObservableCollection<Notification> Notifications
    {
        get => _notifications;
        set => this.RaiseAndSetIfChanged(ref _notifications, value);
    }

    public NotificationWindowViewModel()
    {
        MainNotificationViewModel = this;
    }

    public void Notify(string message, long durationMillisecondsClose = 5000)
    {
        Notify(new Notification(message), TimeSpan.FromMilliseconds(durationMillisecondsClose));
    }
    
    public void Notify(Notification message, TimeSpan durationClose)
    {
        Notifications.Add(message);

        async void DestroyNotification()
        {
            await Task.Delay(durationClose);
            Notifications.Remove(message);
        }

        Dispatcher.UIThread.Post(DestroyNotification);
    }
}

public record Notification(string Message);