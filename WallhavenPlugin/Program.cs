namespace WallhavenPlugin;

public static class Program
{
    static async Task Main()
    {
        Console.WriteLine(await new WallpaperInfo("https://th.wallhaven.cc/small/k9/k9x751.jpg", "").GetFullImage(CancellationToken.None));
    }
}