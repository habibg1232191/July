using July.Core.Plugin;

namespace WallhavenPlugin;

public class WallpaperInfo : IWallpaperInfo
{
    public WallpaperInfo(string previewImageUrl, string sizeImage)
    {
        PreviewImageUrl = previewImageUrl;
        SizeImage = sizeImage;
    }

    public string PreviewImageUrl { get; }

    public string SizeImage { get; }
    public async Task<string> GetFullImage(CancellationToken cancellationToken)
    {
        var result = PreviewImageUrl.Replace("small", "full");
        char[] charArray = result.ToCharArray();
        Array.Reverse(charArray);
        result = result.Remove(8, 2).Insert(8, "w").Insert(result.Length - new string(charArray).IndexOf("/", StringComparison.Ordinal) - 1, "wallhaven-");

        if (!(await new HttpClient().GetAsync(result, cancellationToken)).IsSuccessStatusCode)
            result = result.Replace("jpg", "png");
        return await Task.FromResult(result);
        // if (_fullImageUrl is null)
        // {
        //     var config = Configuration.Default;
        //     using var client = new HttpClient();
        //     using var response = await client.GetAsync(_urlForParseFullImage, cancellationToken);
        //     var pageSource = await response.Content.ReadAsStringAsync(cancellationToken);
        //
        //     using var context = BrowsingContext.New(config);
        //     using var document = await context.OpenAsync(req => req.Content(pageSource), cancellationToken);
        //     var cellSelector = "img#wallpaper";
        //     var cell = document.QuerySelector(cellSelector);
        //     _fullImageUrl = cell?.GetAttribute("data-cfsrc");
        //     if (_fullImageUrl is null) throw new NullReferenceException("FullImageUrl is null");
        // }
        // return _fullImageUrl;
    }

}