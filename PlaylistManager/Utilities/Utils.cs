using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlaylistManager.Models;
using Splat;

namespace PlaylistManager.Utilities
{
    public static class Utils
    {
        public const int kHoldDelay = 300;

        public static void Serialize(object value, Stream s)
        {
            using (StreamWriter writer = new StreamWriter(s))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, value);
                jsonWriter.Flush();
            }
        }
        
        public static void OpenURL(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }

        public static async Task<byte[]?> DownloadLevelByCustomURL(string url, CancellationToken? cancellationToken = null, IProgress<double>? progress = null)
        {
            var httpClientService = Locator.Current.GetService<HttpClientService>()!;
            try
            {
                HttpClientResponse httpResponse = await httpClientService.GetAsync(url, cancellationToken ?? CancellationToken.None, progress);
                if (httpResponse.Successful)
                {
                    byte[] zip = await httpResponse.ReadAsByteArrayAsync();
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }

    }
}