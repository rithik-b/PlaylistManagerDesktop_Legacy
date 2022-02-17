using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlaylistManager.Models;
using SongDetailsCache;

namespace PlaylistManager.Utilities
{
    public class SongDetailsLoader
    {
        private SongDetails? songDetails;

        public async Task Init() => songDetails = await SongDetails.Init();

        public bool TryGetLevelByHash(string hash, out SongDetailsLevelData level)
        {
            level = null!;
            
            // No returning anything if we weren't initialized
            if (songDetails == null)
            {
                return false;
            }

            var returnVal = songDetails.songs.FindByHash(hash, out var song);
            if (returnVal)
            {
                level = new SongDetailsLevelData(song);
            }
            return returnVal;
        }
        
        public bool TryGetLevelByKey(string key, out SongDetailsLevelData level)
        {
            level = null!;
            
            // No returning anything if we weren't initialized
            if (songDetails == null)
            {
                return false;
            }

            var returnVal = songDetails.songs.FindByMapId(key, out var song);
            if (returnVal)
            {
                level = new SongDetailsLevelData(song);
            }
            return returnVal;
        }
        
        public IEnumerable<SongDetailsLevelData> SeachLevels(string[] searchTexts, HashSet<string>? excludedHashes = null)
        {
            var results = new List<SongDetailsLevelData>();
            
            if (songDetails != null && searchTexts.Length > 0 && !string.IsNullOrWhiteSpace(searchTexts[0]))
            {
                foreach (var song in songDetails.songs)
                {
                    if (excludedHashes != null && excludedHashes.Contains(song.hash))
                    {
                        continue;
                    }
                
                    var words = 0;
                    var matches = 0;

                    var songName = $" {song.songName} ";
                    var songAuthorName = $" {song.songAuthorName} ";
                    var levelAuthorName = $" {song.levelAuthorName} ";

                    for (int i = 0; i < searchTexts.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(searchTexts[i]))
                        {
                            words++;

                            string searchTerm = $" {searchTexts[i]} ";
                            if (i == searchTexts.Length - 1)
                            {
                                searchTerm = searchTerm.Substring(0, searchTerm.Length - 1);
                            }

                            if (songName.IndexOf(searchTerm, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                songAuthorName.IndexOf(searchTerm, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                                levelAuthorName.IndexOf(searchTerm, 0, StringComparison.InvariantCultureIgnoreCase) != -1)
                            {
                                matches++;
                            }
                        }
                    }

                    if (matches == words)
                    {
                        results.Add(new SongDetailsLevelData(song));
                    }
                }
            }

            return results;
        }
    }
}