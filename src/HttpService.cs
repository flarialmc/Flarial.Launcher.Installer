using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Net.Http.HttpCompletionOption;

namespace Flarial.Launcher.Installer;

static partial class HttpService
{
    static readonly HttpClient s_client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
    }, true);

    static readonly int s_length = Environment.SystemPageSize;

    internal static Task<byte[]> GetBytesAsync(string uri)
    {
        return s_client.GetByteArrayAsync(uri);
    }

    static Task<HttpResponseMessage> GetAsync(string uri)
    {
        return s_client.GetAsync(uri, ResponseHeadersRead);
    }

    internal static async Task DownloadAsync(string uri, string path, Action<int> callback)
    {
        using var response = await GetAsync(uri);
        response.EnsureSuccessStatusCode();

        using var destination = File.Create(path);
        using var source = await response.Content.ReadAsStreamAsync();

        int count = 0; double value = 0;
        var buffer = new byte[s_length];
        var length = response.Content.Headers.ContentLength ?? 0;

        while ((count = await source.ReadAsync(buffer, 0, s_length)) != 0)
        {
            await destination.WriteAsync(buffer, 0, count);
            if (length > 0) callback((int)((value += count) / length * 100));
        }
    }
}