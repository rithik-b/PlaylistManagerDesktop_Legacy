using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace PlaylistManager.Models
{
    public interface ICustomLevelData
    {
        /// <summary>
        /// Name of the song
        /// </summary>
        public string SongName { get; }
        
        /// <summary>
        /// Sub name of the song
        /// </summary>
        public string SongSubName { get; }
        
        /// <summary>
        /// Artist of the song
        /// </summary>
        public string SongAuthorName { get; }
        
        /// <summary>
        /// Mapper of the level
        /// </summary>
        public string LevelAuthorName { get; }
        
        /// <summary>
        /// The SHA1 Hash of the level
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Is the map downloaded (locally available)?
        /// </summary>
        public bool Downloaded { get; }

        /// <summary>
        /// A dictionary that maps from Characteristic to a List of Difficulties in that Characteristic
        /// </summary>
        public Dictionary<string, List<Difficulty>> Difficulties { get; }
        
        /// <summary>
        /// Asynchronously get the BeatSaver ID of the level
        /// </summary>
        public Task<string?> GetKeyAsync();
        
        /// <summary>
        /// Asynchronously loads and parses the cover image of a level
        /// </summary>
        public Task<Bitmap?> GetCoverImageAsync(CancellationToken? cancellationToken = null);
    }
    
    public enum Difficulty
    {
        Easy = 1,
        Normal = 3,
        Hard = 5,
        Expert = 7,
        ExpertPlus = 9
    }
}