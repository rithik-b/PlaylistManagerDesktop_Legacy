using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PlaylistManager.Models
{
    /// <summary>
    /// A wrapper for Http client responses with useful utilities
    /// Pretty much yoinked from BeatSaverSharper
    /// </summary>
    public class HttpClientResponse
    {
        public int Code => (int)httpResponseMessage.StatusCode;
        public bool Successful => httpResponseMessage.IsSuccessStatusCode;

        private byte[]? bytes;
        private string? bodyAsString;
        private readonly HttpResponseMessage httpResponseMessage;
        
        public HttpClientResponse(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public HttpClientResponse(byte[] bytes, HttpResponseMessage httpResponseMessage)
        {
            this.bytes = bytes;
            this.httpResponseMessage = httpResponseMessage;
        }

        public async Task<byte[]> ReadAsByteArrayAsync()
        {
            return bytes ??= await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        public async Task<string> ReadAsStringAsync()
        {
            if (bodyAsString is null)
            {
                bytes ??= await ReadAsByteArrayAsync().ConfigureAwait(false);
                bodyAsString = Encoding.UTF8.GetString(bytes);
            }
            return bodyAsString;
        }

        public async Task<Stream> ReadAsStreamAsync()
        {
            bytes ??= await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return new MemoryStream(bytes);
        }
    }
}