using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlaylistManager.Models;

namespace PlaylistManager.Utilities
{
    /// <summary>
    /// A wrapper for Http client with useful utilities
    /// Pretty much yoinked from BeatSaverSharper
    /// </summary>
    public class HttpClientService
    {
        private readonly HttpClient httpClient;

        public HttpClientService(Assembly assembly)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate)
            };
            httpClient = new HttpClient(handler);
            var assemblyName = assembly.GetName();
            var userAgent = $"{assemblyName.Name}/{assemblyName.Version}";
            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        public async Task<HttpClientResponse> GetAsync(string url, CancellationToken token = default, IProgress<double>? progress = null)
        {
            // We read starting with the response headers so we can update the IProgress<double> if it exists, as well as stopping the body if necessary.
            HttpResponseMessage message = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);

            if (token.IsCancellationRequested)
                throw new TaskCanceledException();

            using MemoryStream ms = new MemoryStream();
            using Stream streamedBody = await message.Content.ReadAsStreamAsync().ConfigureAwait(false);

            long total = 0;
            byte[] buffer = new byte[8192];
            long? length = message.Content.Headers.ContentLength;
            progress?.Report(0);

            while (true)
            {
                int read = await streamedBody.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (read <= 0)
                    break;

                if (token.IsCancellationRequested)
                    throw new TaskCanceledException();

                if (length.HasValue)
                    progress?.Report((double)total / length.Value);

                await ms.WriteAsync(buffer, 0, read).ConfigureAwait(false);
                total += read;
            }
            progress?.Report(1);
            byte[] body = ms.ToArray();
            return new HttpClientResponse(body, message);
        }

        public async Task<HttpClientResponse> PostAsync(string url, object? body = null, CancellationToken token = default)
{
            var serializedBody = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            HttpResponseMessage message = await httpClient.PostAsync(url, serializedBody, token).ConfigureAwait(false);
            return new HttpClientResponse(message);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            httpClient.Dispose();
        }
    }
}