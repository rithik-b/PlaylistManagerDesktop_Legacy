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
    private readonly ConcurrentDictionary<string, BeatSaverLevelData?> cachedHashLevels;
    private readonly ConcurrentDictionary<string, BeatSaverLevelData?> cachedKeyLevels;

    public BeatSaverLoader(Assembly assembly)
    {
        var assemblyName = assembly.GetName();
        BeatSaverOptions options = new BeatSaverOptions(assemblyName.Name!, assemblyName.Version!);
        beatSaverInstance = new BeatSaver(options);
        cachedHashLevels = new ConcurrentDictionary<string, BeatSaverLevelData?>();
        cachedKeyLevels = new ConcurrentDictionary<string, BeatSaverLevelData?>();
    }

    public async Task<BeatSaverLevelData?> GetLevelByHashAsync(string hash, CancellationToken? cancellationToken = null)
    {
        if (cachedHashLevels.TryGetValue(hash, out var cachedLevel))
        {
            return cachedLevel;
        }
        
        var map = await beatSaverInstance.BeatmapByHash(hash, cancellationToken ?? CancellationToken.None);
        var levelData = map != null ? new BeatSaverLevelData(map, hash) : null;
        cachedHashLevels.TryAdd(hash, levelData);
        if (levelData != null)
        {
            cachedKeyLevels.TryAdd(levelData.Key, levelData);
        }
        return levelData;
    }
    
    public async Task<BeatSaverLevelData?> GetLevelByKeyAsync(string key, CancellationToken? cancellationToken = null)
    {
        if (cachedKeyLevels.TryGetValue(key, out var cachedLevel))
        {
            return cachedLevel;
        }
        
        var map = await beatSaverInstance.Beatmap(key, cancellationToken ?? CancellationToken.None);
        var levelData = map != null ? new BeatSaverLevelData(map, map.LatestVersion.Hash) : null;
        cachedKeyLevels.TryAdd(key, levelData);
        if (levelData != null)
        {
            cachedHashLevels.TryAdd(levelData.Hash, levelData);
        }
        return levelData;
    }
}