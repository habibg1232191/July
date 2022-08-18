using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace July.Views.Controls;

[PseudoClasses("loading")]
public class AsyncImage : TemplatedControl
{
    private static readonly List<AsyncImageCache> ImageCaches = new();

    private readonly HttpClient _client = new();
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;

    public static readonly StyledProperty<string> SourceProperty =
        AvaloniaProperty.Register<AsyncImage, string>(nameof(Source));

    private static readonly StyledProperty<DownloadProgressChangedEventHandler?> DownloadProgressChangedEventProperty =
        AvaloniaProperty.Register<AsyncImage, DownloadProgressChangedEventHandler?>(nameof(DownloadProgressChangedEvent));
    
    private Image? _imageControl;
    
    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public DownloadProgressChangedEventHandler? DownloadProgressChangedEvent
    {
        get => GetValue(DownloadProgressChangedEventProperty);
        set => SetValue(DownloadProgressChangedEventProperty, value);
    }

    public AsyncImage()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        this.WhenAnyValue(x => x.Source).Subscribe(OnSourceChanged);
    }

    private async void OnSourceChanged(string? newValue)
    {
        if(newValue is null) return;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        try
        {
            PseudoClasses.Set(":loading", true);
            var bitmap = await GetAsyncBitmapFromUrl(newValue);
            if (_imageControl != null)
                _imageControl.Source = bitmap;
            _cancellationToken.ThrowIfCancellationRequested();

            // if (ImageCaches.Count >= 20)
            //     ImageCaches.RemoveAt(0);

            ImageCaches.Add(new AsyncImageCache(newValue, bitmap));
            PseudoClasses.Set(":loading", false);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (InvalidOperationException)
        {
            // ignored
        }
    }

    private async Task<Bitmap> GetAsyncBitmapFromUrl(string url)
    {
        var imageFind = ImageCaches.FirstOrDefault(x => x.Url == url);
        if (imageFind != null)
            return imageFind.Bitmap;
        using var response = await _client.GetAsync(url, _cancellationToken);
        await using var inputStream = await response.Content.ReadAsStreamAsync(_cancellationToken);
        return new Bitmap(inputStream);
    }

    private void CloseHttpSession()
    {
        _cancellationTokenSource.Cancel();
        _client.Dispose();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _imageControl = e.NameScope.Find<Image>("Image");
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        CloseHttpSession();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CloseHttpSession();
    }
}

record AsyncImageCache(string Url, Bitmap Bitmap);