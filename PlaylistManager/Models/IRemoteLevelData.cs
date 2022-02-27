using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp.Models;
using PlaylistManager.Utilities;

namespace PlaylistManager.Models;

public interface IRemoteLevelData : ICustomLevelData
{
    public Task<byte[]?> DownloadLevel(CancellationToken? cancellationToken = null, IProgress<double>? progress = null);

    protected static async Task<byte[]?> DownloadLevelCommon(IRemoteLevelData levelData, Beatmap beatmap, CancellationToken? cancellationToken = null,
        IProgress<double>? progress = null)
    {
        BeatmapVersion? matchingVersion = null;
        foreach (BeatmapVersion version in beatmap.Versions)
        {
            if (string.Equals(levelData.Hash, version.Hash, StringComparison.OrdinalIgnoreCase))
            {
                matchingVersion = version;
            }
        }

        try
        {
            if (matchingVersion != null)
            {
                return await matchingVersion.DownloadZIP(cancellationToken ?? CancellationToken.None, progress);
            }
            
            BeatmapVersion latest = beatmap.LatestVersion;
            return await Utils.DownloadLevelByCustomURL(
                latest.DownloadURL.Replace(latest.Hash, levelData.Hash.ToLowerInvariant()), cancellationToken, progress);
        }
        catch
        {
            return null;
        }
    }
}