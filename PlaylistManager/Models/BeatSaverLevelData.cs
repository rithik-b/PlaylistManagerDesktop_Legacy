using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using BeatSaverSharp.Models;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.Models;

public class BeatSaverLevelData : IRemoteLevelData
{
    private readonly Beatmap beatmap;
    private Bitmap? coverImage;
    public string SongName => beatmap.Metadata.SongName;
    public string SongSubName => beatmap.Metadata.SongSubName;
    public string SongAuthorName => beatmap.Metadata.SongAuthorName;
    public string LevelAuthorName => beatmap.Metadata.LevelAuthorName;
    public string Hash { get; }
    public string Key => beatmap.ID;
    public Dictionary<string, List<Difficulty>> Difficulties { get; } = new Dictionary<string, List<Difficulty>>();
    
    public BeatSaverLevelData(Beatmap beatmap, string hash)
    {
        this.beatmap = beatmap;
        Hash = hash;

        foreach (var difficulty in beatmap.LatestVersion.Difficulties)
        {
            var characteristic = difficulty.Characteristic.ToString();
            if (!Difficulties.ContainsKey(characteristic))
            {
                Difficulties[characteristic] = new List<Difficulty>();
            }
            Difficulties[characteristic].Add(Enum.Parse<Difficulty>(difficulty.Difficulty.ToString()));
        }
    }

    public Task<string?> GetKeyAsync() => Task.FromResult(Key)!;

    public Task<byte[]?> DownloadLevel(CancellationToken? cancellationToken = null, IProgress<double>? progress = null)
        => IRemoteLevelData.DownloadLevelCommon(this, beatmap, cancellationToken, progress);

    public async Task<Bitmap?> GetCoverImageAsync(CancellationToken? cancellationToken = null)
    {
        if (coverImage == null)
        {
            var httpClientService = Locator.Current.GetService<HttpClientService>()!;
            var result = await httpClientService.GetAsync(beatmap.LatestVersion.CoverURL,
                cancellationToken ?? CancellationToken.None);
            if (result.Successful)
            {
                await using var imageStream = await result.ReadAsStreamAsync();
                coverImage = Bitmap.DecodeToHeight(imageStream, 512);
            }
        }
        return coverImage;
    }
}