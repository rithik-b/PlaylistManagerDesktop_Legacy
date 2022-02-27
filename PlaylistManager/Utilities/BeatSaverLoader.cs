using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp;
using PlaylistManager.Models;

namespace PlaylistManager.Utilities;

public class BeatSaverLoader
{
    private readonly BeatSaver beatSaverInstance;

    public BeatSaverLoader(Assembly assembly)
    {
        var assemblyName = assembly.GetName();
        BeatSaverOptions options = new BeatSaverOptions(assemblyName.Name!, assemblyName.Version!);
        beatSaverInstance = new BeatSaver(options);
    }

    public async Task<BeatSaverLevelData?> GetLevelByHashAsync(string hash, CancellationToken? cancellationToken = null)
    {
        var map = await beatSaverInstance.BeatmapByHash(hash, cancellationToken ?? CancellationToken.None);
        var levelData = map != null ? new BeatSaverLevelData(map, hash) : null;
        return levelData;
    }
    
    public async Task<BeatSaverLevelData?> GetLevelByKeyAsync(string key, CancellationToken? cancellationToken = null)
    {
        var map = await beatSaverInstance.Beatmap(key, cancellationToken ?? CancellationToken.None);
        var levelData = map != null ? new BeatSaverLevelData(map, map.LatestVersion.Hash.ToUpper()) : null;
        return levelData;
    }
}