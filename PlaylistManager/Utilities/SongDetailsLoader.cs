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
    }
}