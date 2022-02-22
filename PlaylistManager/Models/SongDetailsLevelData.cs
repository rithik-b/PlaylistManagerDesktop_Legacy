using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using PlaylistManager.Utilities;
using SongDetailsCache.Structs;
using Splat;

namespace PlaylistManager.Models
{
    /// <summary>
    /// A wrapper for SongDetails structs that implements the ICustomLevelData interface
    /// </summary>
    public class SongDetailsLevelData : ICustomLevelData
    {
        private readonly Song song;
        private Bitmap? coverImage;
        public string SongName => song.songName;
        public string SongSubName => "";
        public string SongAuthorName => song.songAuthorName;
        public string LevelAuthorName => song.levelAuthorName;
        public string Hash => song.hash;
        public string Key => song.key;
        public bool Downloaded => false;
        public Dictionary<string, List<Difficulty>> Difficulties { get; } = new Dictionary<string, List<Difficulty>>();
        
        public SongDetailsLevelData(Song song)
        {
            this.song = song;

            foreach (var difficulty in song.difficulties)
            {
                var characteristic = difficulty.characteristic.ToString();
                if (!Difficulties.ContainsKey(characteristic))
                {
                    Difficulties[characteristic] = new List<Difficulty>();
                }
                Difficulties[characteristic].Add(Enum.Parse<Difficulty>(difficulty.difficulty.ToString()));
            }
        }

        public Task<string?> GetKeyAsync() => Task.FromResult(Key)!;

        public async Task<Bitmap?> GetCoverImageAsync(CancellationToken? cancellationToken = null)
        {
            if (coverImage == null)
            {
                var httpClientService = Locator.Current.GetService<HttpClientService>()!;
                var result =
                    await httpClientService.GetAsync(song.coverURL, cancellationToken ?? CancellationToken.None);
                if (result.Successful)
                {
                    await using var imageStream = await result.ReadAsStreamAsync();
                    coverImage = Bitmap.DecodeToHeight(imageStream, 512);
                }
            }
            return coverImage;
        }
    }
}