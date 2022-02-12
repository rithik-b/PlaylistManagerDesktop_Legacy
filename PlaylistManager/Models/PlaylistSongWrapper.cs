using BeatSaberPlaylistsLib.Types;

namespace PlaylistManager.Models
{
    /// <summary>
    /// A wrapper class that combines PlaylistSong with ICustomLevelData
    /// </summary>
    public class PlaylistSongWrapper
    {
        public readonly IPlaylistSong playlistSong;
        public readonly ICustomLevelData customLevelData;
        
        public PlaylistSongWrapper(IPlaylistSong playlistSong, ICustomLevelData customLevelData)
        {
            this.playlistSong = playlistSong;
            this.customLevelData = customLevelData;
        }
    }
}