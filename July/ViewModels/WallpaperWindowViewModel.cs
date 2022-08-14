using System;
using System.Threading;
using July.Core.Services;
using ReactiveUI;

namespace July.ViewModels;

public class WallpaperWindowViewModel : ViewModelBase
{
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private string _testSource = "";

    public string TestSource
    {
        get => _testSource;
        set => this.RaiseAndSetIfChanged(ref _testSource, value);
    }

    public WallpaperWindowViewModel()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
    }
    
    public async void OnWallpaperChange(WallpaperChangedEventArgs eventArgs)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        try
        {
            TestSource = await eventArgs.WallpaperInfo.GetFullImage(_cancellationToken);
            Console.WriteLine(TestSource);
        }
        catch (Exception e)
        {
            // ignored
            Console.WriteLine(e);
        }
    }
}