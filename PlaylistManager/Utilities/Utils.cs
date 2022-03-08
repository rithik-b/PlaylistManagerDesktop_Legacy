using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp.Models;
using Newtonsoft.Json;
using PlaylistManager.Models;
using Splat;

namespace PlaylistManager.Utilities
{
    public static class Utils
    {
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

        private static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength) + truncationSuffix
                : value;
        }
        
        public static string FolderNameForBeatSaverMap(Beatmap song)
        {
            // A workaround for the max path issue and long folder names
            string longFolderName = song.ID + " (" + song.Metadata.LevelAuthorName + " - " + song.Metadata.SongName;
            return longFolderName.Truncate(49) + ")";
        }
        
        /// <summary>
        /// Extracts a zip for a custom level
        /// </summary>
        /// <param name="zip">Zip in bytes</param>
        /// <param name="customSongsPath">Base path to extract level to</param>
        /// <param name="songName">Name of folder</param>
        /// <param name="overwrite">If overwriting is desired</param>
        /// <returns>Path to the extracted level</returns>
        public static async Task<string?> ExtractZipAsync(byte[] zip, string customSongsPath, string songName, bool overwrite = false)
        {
            Stream zipStream = new MemoryStream(zip);
            try
            {
                var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                string basePath = "";
                basePath = string.Join("", songName.Split(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray()));
                string path = Path.Combine(customSongsPath, basePath);

                if (!overwrite && Directory.Exists(path))
                {
                    int pathNum = 1;
                    while (Directory.Exists(path + $" ({pathNum})")) ++pathNum;
                    path += $" ({pathNum})";
                }

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                await Task.Run(() =>
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Name) && entry.Name == entry.FullName)
                        {
                            var entryPath = Path.Combine(path, entry.Name); // Name instead of FullName for better security and because song zips don't have nested directories anyway
                            if (overwrite || !File.Exists(entryPath)) // Either we're overwriting or there's no existing file
                                entry.ExtractToFile(entryPath, overwrite);
                        }
                    }
                }).ConfigureAwait(false);
                archive.Dispose();
                return path;
            }
            catch (Exception)
            {
                // ignore
            }
            zipStream.Close();
            return null;
        }
    }
}