using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using PlaylistManager.Models;

namespace PlaylistManager.Utilities;

public class BeatSaverLoader
{
    public readonly BeatSaver beatSaverInstance;

    public BeatSaverLoader(Assembly assembly)
    {
        var assemblyName = assembly.GetName();
        BeatSaverOptions options = new BeatSaverOptions(assemblyName.Name!, assemblyName.Version!);
        beatSaverInstance = new BeatSaver(options);
    }

    public async Task<BeatSaverLevelData?> GetLevelByHashAsync(string hash, CancellationToken? cancellationToken = null)
    {
        Beatmap? map = null;
        try
        {
            map = await beatSaverInstance.BeatmapByHash(hash, cancellationToken ?? CancellationToken.None);
        }
        catch
        {
            // ignored
        }

        var levelData = map != null ? new BeatSaverLevelData(map, hash) : null;
        return levelData;
    }
    
    public async Task<BeatSaverLevelData?> GetLevelByKeyAsync(string key, CancellationToken? cancellationToken = null)
    {
        Beatmap? map = null;
        try
        {
            map = await beatSaverInstance.Beatmap(key, cancellationToken ?? CancellationToken.None);
        }
        catch
        {
            // ignored
        }
        
        var levelData = map != null ? new BeatSaverLevelData(map, map.LatestVersion.Hash.ToUpper()) : null;
        return levelData;
    }
}