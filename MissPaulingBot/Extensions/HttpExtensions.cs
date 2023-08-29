using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MissPaulingBot.Extensions;

public static class HttpExtensions
{
    public static async Task<MemoryStream> GetMemoryStreamAsync(this HttpClient client, string url)
    {
        var data = new MemoryStream();
        await using var stream = await client.GetStreamAsync(url);
        await stream.CopyToAsync(data);
        data.Seek(0, SeekOrigin.Begin);

        return data;
    }
}