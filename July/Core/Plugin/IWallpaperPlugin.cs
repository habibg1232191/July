using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace July.Core.Plugin;

public interface IWallpaperPlugin : IPlugin
{
    public Task<IEnumerable<IWallpaperInfo>> Search(string searchString, int page = 1, CancellationToken cancellationToken = new());
}