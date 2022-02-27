using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.Models
{
    /// <summary>
    /// A Custom Level stored in a local file
    /// </summary>
    public class CustomLevel
    {
        private const string kInfoFile = "Info.dat";
        
        private readonly string hash;
        
        /// <summary>
        /// Location of the song
        /// </summary>
        public readonly string path;

        private ICustomLevelData? levelData;

        public CustomLevel(string hash, string path)
        {
            this.hash = hash;
            this.path = path;
        }

        public async Task<ICustomLevelData?> GetLevelDataAsync()
        {
            if (levelData == null)
            {
                var infoPath = Path.Combine(path, kInfoFile);
                if (!File.Exists(infoPath))
                {
                    return null;
                }

                levelData = await Task.Run(() =>
                {
                    using var streamReader = File.OpenText(infoPath);
                    {
                        using var jsonTextReader = new JsonTextReader(streamReader);
                        var jsonSerializer = new JsonSerializer();
                        var customLevelData = jsonSerializer.Deserialize<CustomLevelData>(jsonTextReader);
                        customLevelData?.SetHashAndPath(hash, path);
                        return customLevelData;
                    }
                }).ConfigureAwait(false);
            }
            return levelData;
        }
    }
    
    /// <summary>
    /// A wrapper for Custom Levels that implements the ICustomLevelData interface
    /// </summary>
    public class CustomLevelData : ICustomLevelData
    {
        [JsonProperty]
        private string? _songName { get; set; }
        
        [JsonProperty]
        private string? _songSubName { get; set; }
        
        [JsonProperty]
        private string? _songAuthorName { get; set; }
        
        [JsonProperty]
        private string? _levelAuthorName { get; set; }
        
        [JsonProperty]
        private string? _coverImageFilename { get; set; }
        
        [JsonProperty]
        private List<DifficultyBeatmapSet>? _difficultyBeatmapSets { get; set; }

        private string? hash;
        private string? path;
        private string? key;
        private bool attemptedSongDetailsLookup;
        private Bitmap? coverImage;

        public string SongName => _songName ?? "";
        public string SongSubName => _songSubName ?? "";
        public string SongAuthorName => _songAuthorName ?? "";
        public string LevelAuthorName => _levelAuthorName ?? "";
        public string Hash => hash ?? "";
        public Dictionary<string, List<Difficulty>> Difficulties { get; } = new();
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_difficultyBeatmapSets != null)
            {
                foreach (var difficultyBeatmapSet in _difficultyBeatmapSets)
                {
                    if (!Difficulties.ContainsKey(difficultyBeatmapSet.beatmapCharacteristicName))
                    {
                        Difficulties[difficultyBeatmapSet.beatmapCharacteristicName] = new List<Difficulty>();
                    }

                    foreach (var difficultyBeatmap in difficultyBeatmapSet.difficultyBeatmaps)
                    {
                        Difficulties[difficultyBeatmapSet.beatmapCharacteristicName].Add(difficultyBeatmap.difficulty);
                    }
                    
                    // Throwing away the pointer to the original list should hopefully save memory
                    _difficultyBeatmapSets = null;
                }   
            }
        }

        public async Task<string?> GetKeyAsync()
        {
            if (!attemptedSongDetailsLookup)
            {
                var songDetailsLoader = Locator.Current.GetService<SongDetailsLoader>()!;
                if (hash != null && songDetailsLoader.TryGetLevelByHash(hash, out var songDetailsLevel))
                {
                    await songDetailsLoader.Init();
                    key = songDetailsLevel.Key;
                }
                attemptedSongDetailsLookup = true;
            }
            return key;
        }

        public async Task<Bitmap?> GetCoverImageAsync(CancellationToken? cancellationToken = null)
        {
            if (coverImage == null)
            {
                var imagePath = Path.Combine(path ?? "", _coverImageFilename ?? "");
                
                if (!File.Exists(imagePath))
                {
                    return null;
                }
                
                await using var fileStream = new FileStream(imagePath, FileMode.Open);
                coverImage = Bitmap.DecodeToHeight(fileStream, 50);
            }
            return coverImage;
        }

        /// <summary>
        /// Sets the hash and path, does nothing once both are set
        /// Only used internally during initialization
        /// </summary>
        /// <param name="hash">Hash of the map</param>
        /// <param name="path">Path to the map</param>
        internal void SetHashAndPath(string hash, string path)
        {
            this.hash = hash;
            this.path = path;
        }
    }

    public class DifficultyBeatmapSet
    {
        [JsonProperty("_beatmapCharacteristicName")]
        public string beatmapCharacteristicName;
        
        [JsonProperty("_difficultyBeatmaps")]
        public List<DifficultyBeatmap> difficultyBeatmaps;
        
        public class DifficultyBeatmap
        {
            [JsonProperty("_difficultyRank")]
            public Difficulty difficulty;
        }
    }
}