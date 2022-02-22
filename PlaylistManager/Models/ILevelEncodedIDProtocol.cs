using System.Threading;
using System.Threading.Tasks;

namespace PlaylistManager.Models
{
    /// <summary>
    /// Interface for parsing text with encoded level IDs (i.e. BeatSaver Links, ScoreSaber Links, etc.)
    /// </summary>
    public interface ILevelEncodedIDProtocol
    {
        /// <summary>
        /// Parses a level ID from an input
        /// </summary>
        /// <param name="input">Search ter that hopefully contains a level ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Resulting ID or null if it doesn't exist</returns>
        public Task<SearchResult?> FindResultAsync(string input, CancellationToken? cancellationToken = null);
    }
    
    public enum IDType
    {
        Hash,
        Key
    }
    
    public struct SearchResult
    {
        public string ID;
        public IDType Type;
        
        public SearchResult(string id, IDType type)
        {
            ID = id;
            Type = type;
        }
    }
}