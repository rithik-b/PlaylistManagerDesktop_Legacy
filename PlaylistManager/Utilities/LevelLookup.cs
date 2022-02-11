using System.Threading;
using System.Threading.Tasks;
using PlaylistManager.Models;

namespace PlaylistManager.Utilities
{
    public class LevelLookup
    {
        // TODO: Potentially add BeatSaver song lookup as a final fallback?
        private readonly LevelLoader levelLoader;
        private readonly SongDetailsLoader songDetailsLoader;
        
        public LevelLookup(LevelLoader levelLoader, SongDetailsLoader songDetailsLoader)
        {
            this.levelLoader = levelLoader;
            this.songDetailsLoader = songDetailsLoader;
        }
        
        public async Task<ICustomLevelData?> GetLevelByHash(string hash, CancellationToken? cancellationToken = null)
        {
            // Find level locally, if exists and is parsable, return
            var localLevels = await levelLoader.GetCustomLevelsAsync(cancellationToken: cancellationToken);
            if (localLevels.TryGetValue(hash, out var customLevel))
            {
                var customLevelData = await customLevel.GetLevelDataAsync();
                if (customLevelData != null)
                {
                    return customLevelData;
                }
            }
            
            // Find level on SongDetails, if exists, return
            await songDetailsLoader.Init();
            if (songDetailsLoader.TryGetLevelByHash(hash, out var songDetailsLevel))
            {
                return songDetailsLevel;
            }
            
            // Level not found
            return null;
        }
        
        public async Task<ICustomLevelData?> GetLevelByKey(string key, CancellationToken? cancellationToken = null)
        {
            // First we need to find the level on SongDetails to get the hash
            await songDetailsLoader.Init();
            if (songDetailsLoader.TryGetLevelByKey(key, out var songDetailsLevel))
            {
                // Try finding the equivalent local level
                var localLevels = await levelLoader.GetCustomLevelsAsync(cancellationToken: cancellationToken);
                if (localLevels.TryGetValue(songDetailsLevel.Hash, out var customLevel))
                {
                    var customLevelData = await customLevel.GetLevelDataAsync();
                    if (customLevelData != null)
                    {
                        return customLevelData;
                    }
                }
                // Local level not found
                return songDetailsLevel;
            }
            
            // Level not found
            return null;
        }
    }
}