using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace July.Core.Services;

public static class InternetSession
{
    public static async Task<bool> IsInternetConnection()
    {
        using var client = new HttpClient();
        try
        {
            await client.GetAsync("https://google.com");
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}