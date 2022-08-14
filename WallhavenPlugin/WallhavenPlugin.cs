using AngleSharp;
using AngleSharp.Dom;
using Avalonia.Media.Imaging;
using July.Core.Plugin;

namespace WallhavenPlugin;

public class WallhavenPlugin : IWallpaperPlugin
{
    public string Name => "Wallhaven";
    public string UrlWebSite => @"https://wallhaven.cc";
    public Bitmap? Icon => null;
    public string Version => "0.1 beta";

    private readonly HttpClient _httpClient = new();

    public async Task<IEnumerable<IWallpaperInfo>> Search(string searchString, int page = 1, CancellationToken cancellationToken = new())
    {
        if (page <= 0) throw new ArgumentException("Page index must not be less than 1");
        cancellationToken.ThrowIfCancellationRequested();
        var config = Configuration.Default;
        var address = $"{UrlWebSite}/search?q={searchString}&page={page}";
        using var response = await _httpClient.GetAsync(address, cancellationToken);
        string pageSource = await response.Content.ReadAsStringAsync(cancellationToken);
        using var context = BrowsingContext.New(config);
        using var document = await context.OpenAsync(req => req.Content(pageSource), cancel: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cellSelector = "li > figure.thumb";
        var cells = document.QuerySelectorAll(cellSelector);
        return cells.Select(m =>
        {
            var previewImageUrl = m.Children.Filter("img").First().GetAttribute("data-src")!;
            var sizeTextInfo = m.Children.Filter("div.thumb-info").First().Children.Filter("span.wall-res").First().InnerHtml;
            return new WallpaperInfo(previewImageUrl, sizeTextInfo.Replace(" ", ""));
        });
    }
}