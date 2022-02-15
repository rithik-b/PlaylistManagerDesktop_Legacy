using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlaylistManager.Models;

namespace PlaylistManager.Utilities
{
    public class LevelMatcher
    {
        // TODO: Potentially add BeatSaver song lookup as a final fallback?
        private readonly LevelLoader levelLoader;
        private readonly SongDetailsLoader songDetailsLoader;
        
        public LevelMatcher(LevelLoader levelLoader, SongDetailsLoader songDetailsLoader)
        {
            this.levelLoader = levelLoader;
            this.songDetailsLoader = songDetailsLoader;
        }
        
        /// <summary>
        /// Looks up a level by hash locally then on SongDetails
        /// </summary>
        /// <param name="hash">The SHA1 Hash of the level</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The level if found, null otherwise</returns>
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
        
        /// <summary>
        /// Looks up a level by key on SongDetails.
        /// </summary>
        /// <param name="key">The BeatSaver ID of the level</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The level if found, null otherwise</returns>
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

        /// <summary>
        /// Searches for a level using SongDetails.
        /// </summary>
        /// <param name="input">The search term</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An enumerable of all levels that were found</returns>
        public async Task<IEnumerable<ICustomLevelData>> SearchLevelsAsync(string input, CancellationToken? cancellationToken = null)
        {
            await songDetailsLoader.Init();

            var searchTerms = input.Split(' ');
            var results = new List<ICustomLevelData>();

            try
            {
                results.AddRange(await Task.Run(() => songDetailsLoader.SeachLevels(searchTerms)
                    .Cast<ICustomLevelData>(), cancellationToken ?? CancellationToken.None).ConfigureAwait(false));
            }
            catch (TaskCanceledException) { }

            if (results.Count > 0)
            {
                var localLevels = await levelLoader.GetCustomLevelsAsync(cancellationToken: cancellationToken);
                for (var i = 0; i < results.Count; i++)
                {
                    if (localLevels.TryGetValue(results[i].Hash, out var customLevel))
                    {
                        var localLevel = await customLevel.GetLevelDataAsync();
                        if (localLevel != null)
                        {
                            results[i] = localLevel;
                        }
                    }
                }   
            }

            return results;
        }
    }
}