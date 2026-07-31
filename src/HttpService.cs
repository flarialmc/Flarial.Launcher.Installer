using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flarial.Launcher.Installer;

static partial class HttpService
{
    static readonly HttpClient s_client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
    }, true);

    internal static Task<byte[]> GetBytesAsync(string uri)
    {
        return s_client.GetByteArrayAsync(uri);
    }
}