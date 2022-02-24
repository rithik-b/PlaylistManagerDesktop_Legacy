using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;
using PlaylistManager.UserControls;

namespace PlaylistManager.Clipboard
{
    /// <summary>
    /// A clipboard interface
    /// </summary>
    public interface IClipboardHandler
    {
        public const string kPlaylistData = "application/com.rithik-b.PlaylistManager.Playlist";

        /// <summary>
        /// Cut a playlist
        /// </summary>
        /// <param name="playlistOrManagers">Playlists to cut</param>
        /// <param name="parentManager"></param>
        public Task Cut(IEnumerable<PlaylistCoverViewModel> playlistOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager);

        /// <summary>
        /// Copy a playlist
        /// </summary>
        /// <param name="playlistOrManagers"></param>
        /// <param name="parentManager"></param>
        public Task Copy(IEnumerable<PlaylistCoverViewModel> playlistOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager);
        
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