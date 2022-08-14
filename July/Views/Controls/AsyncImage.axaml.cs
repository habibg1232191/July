using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace July.Views.Controls;

public class AsyncImage : TemplatedControl
{
    private static List<AsyncImageCache> _imageCaches = new();

    private readonly HttpClient _client = new();
    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;

    public static readonly StyledProperty<string> SourceProperty =
        AvaloniaProperty.Register<AsyncImage, string>(nameof(Source));

    private Image? _imageControl;
    public string Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
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
            var bitmap = await GetAsyncBitmapFromUrl(newValue);
            if (_imageControl != null)
                _imageControl.Source = bitmap;
            _cancellationToken.ThrowIfCancellationRequested();
            
            if (_imageCaches.Count >= 20)
                _imageCaches.RemoveAt(0);
            
            _imageCaches.Add(new AsyncImageCache(newValue, bitmap));
        }
        catch (Exception e)
        {
            // ignored
            Console.WriteLine(e);
            Console.WriteLine(newValue);
        }
    }

    private async Task<Bitmap> GetAsyncBitmapFromUrl(string url)
    {
        var imageFind = _imageCaches.FirstOrDefault(x => x.Url == url);
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