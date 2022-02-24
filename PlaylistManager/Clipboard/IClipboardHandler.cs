using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;

namespace PlaylistManager.Clipboard
{
    /// <summary>
    /// A clipboard interface
    /// </summary>
    public interface IClipboardHandler
    {
        /// <summary>
        /// Cut a playlist
        /// </summary>
        /// <param name="playlist">Playlist to cut</param>
        /// <param name="parentManager"></param>
        public Task Cut(IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager);

        /// <summary>
        /// Copy a playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="parentManager"></param>
        public Task Copy(IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager);
        
        /// <summary>
        /// Cut a level
        /// </summary>
        /// <param name="level"></param>
        public void Cut(PlaylistSongWrapper level);
        
        /// <summary>
        /// Copy a level
        /// </summary>
        /// <param name="level"></param>
        public void Copy(PlaylistSongWrapper level);
    }
}