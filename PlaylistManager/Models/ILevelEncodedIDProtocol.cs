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
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<SearchResult?> Result(string input, CancellationToken? cancellationToken = null);
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