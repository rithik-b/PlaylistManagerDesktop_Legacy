using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace PlaylistManager.Models
{
    public interface ICustomLevelData : ILevelData
    {
        /// <summary>
        /// Mapper of the level
        /// </summary>
        public string LevelAuthorName { get; }
        
        /// <summary>
        /// The SHA1 Hash of the level
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// Asynchronously get the BeatSaver ID of the level
        /// </summary>
        public Task<string?> GetKeyAsync();
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