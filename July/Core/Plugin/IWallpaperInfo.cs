using System.Threading;
using System.Threading.Tasks;

namespace July.Core.Plugin;

public interface IWallpaperInfo
{
    public string PreviewImageUrl { get; }
    public string SizeImage { get; }
    public Task<string> GetFullImage(CancellationToken cancellationToken);
}