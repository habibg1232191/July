using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using July.Core.Plugin;
using July.Core.Services;
using ReactiveUI;

namespace July.ViewModels;

public delegate void WallpaperChangedEvent(WallpaperChangedEventArgs eventArgs);

public record WallpaperChangedEventArgs(IWallpaperInfo WallpaperInfo);

public enum MainEnum
{
    Close, Hide
}

public class MainWindowViewModel : ViewModelBase
{
    public WallpaperChangedEvent? WallpaperChangedEvent;
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource = new();

    private int _pageIndex = 1;
        
    private ObservableCollection<IWallpaperInfo> _wallpapers = new();
    private string _searchQuery = "";
    private bool _isVisible = true;
    
    private MainEnum _enumCollection = MainEnum.Close;

    public ObservableCollection<IWallpaperInfo> Wallpapers
    {
        get => _wallpapers;
        private set => this.RaiseAndSetIfChanged(ref _wallpapers, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public MainEnum EnumCollection
    {
        get => _enumCollection;
        private set => this.RaiseAndSetIfChanged(ref _enumCollection, value);
    }

    private ReactiveCommand<Unit, String> Refresh { get; set; }
    
    public MainWindowViewModel()
    {
        CloseLastTask();
        Refresh = ReactiveCommand.Create<Unit, String>(_ => SearchQuery);

        this.ObservableForProperty(input => input.SearchQuery).Value().DistinctUntilChanged().Merge(Refresh)
            .Throttle(TimeSpan.FromSeconds(1))
            .Where(_ => true)
            .Subscribe(OnSearchStringChanged);
        OnSearchStringChanged(SearchQuery);
        NetworkChange.NetworkAvailabilityChanged += (sender, args) =>
        {
            Console.WriteLine(args.IsAvailable ? "Network connected!" : "Network dis connected!");
        };
    }

    public void VisibleChange()
    {
        IsVisible = !IsVisible;
        EnumCollection = EnumCollection == MainEnum.Close ? MainEnum.Hide : MainEnum.Close;
    }

    private async void OnSearchStringChanged(string newValue)
    {
        // Search.Execute(Unit.Default);
        await TryLoadWallpapers();
    }

    private async Task TryLoadWallpapers()
    {
        CloseLastTask();
        if (!await InternetSession.IsInternetConnection() || _cancellationToken.IsCancellationRequested)
        {
            NotificationWindowViewModel.MainNotificationViewModel?.Notify(new Notification("Нет подключения к интернету"), TimeSpan.FromSeconds(10));
            return;
        }
        try
        {
            var addedWallpapers =
                await WallpaperPluginService.SelectedPlugin.Search(SearchQuery, _pageIndex, _cancellationToken).WaitAsync(_cancellationToken);
            _cancellationToken.ThrowIfCancellationRequested();
            Wallpapers = new ObservableCollection<IWallpaperInfo>(addedWallpapers.ToList());
        }
        catch (Exception)
        {
            Console.WriteLine("Close Task");
            // CloseLastTask();
            // TryLoadWallpapers();
        }
    }

    public void LoadNextPage()
    {
        async void LoadingPage()
        {
            _pageIndex++;

            if (!await InternetSession.IsInternetConnection() || _cancellationToken.IsCancellationRequested)
            {
                NotificationWindowViewModel.MainNotificationViewModel?.Notify("Нет подключения к интернету");
                return;
            }
            
            var loadedWallpapers = await WallpaperPluginService.SelectedPlugin.Search(SearchQuery, _pageIndex, _cancellationToken);
            Wallpapers.Add(new ObservableCollection<IWallpaperInfo>(loadedWallpapers.ToList()));
        }

        Dispatcher.UIThread.Post(LoadingPage);
    }

    private void CloseLastTask()
    {
        _pageIndex = 1;
        _cancellationTokenSource.Cancel();
        // _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
    }

    ~MainWindowViewModel()
    {
        _cancellationTokenSource.Dispose();
    }
}