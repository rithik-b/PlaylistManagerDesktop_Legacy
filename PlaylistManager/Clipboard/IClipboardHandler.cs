using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Input;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;

namespace PlaylistManager.Clipboard
{
    /// <summary>
    /// A clipboard interface
    /// </summary>
    public interface IClipboardHandler
    {
        public const string kPlaylistData = "application/com.rithik-b.PlaylistManager.Playlist";
        public const string kPlaylistSongData = "application/com.rithik-b.PlaylistManager.PlaylistSong";

        /// <summary>
        /// Cut playlists or managers
        /// </summary>
        /// <param name="playlistsOrManagers"></param>
        /// <param name="parentManager"></param>
        public Task Cut(IEnumerable<PlaylistCoverViewModel> playlistsOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager);

        /// <summary>
        /// Copy playlists or managers
        /// </summary>
        /// <param name="playlistsOrManagers"></param>
        /// <param name="parentManager"></param>
        public Task Copy(IEnumerable<PlaylistCoverViewModel> playlistsOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager);

        /// <summary>
        /// Paste <see cref="IPlaylist"/> objects
        /// </summary>
        /// <returns>An enumerable of playlists if in clipboard, null otherwise</returns>
        public Task<IEnumerable<IPlaylist>?> PastePlaylists();
        
        
        /// <summary>
        /// Copy <see cref="PlaylistSongWrapper"/> objects
        /// </summary>
        /// <param name="playlistSongWrappers"></param>
        public Task Copy(IEnumerable<PlaylistSongWrapper> playlistSongWrappers);

        /// <summary>
        /// Paste <see cref="PlaylistSongWrapper"/> objects
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<PlaylistSongWrapper>?> PastePlaylistSongWrappers();

        /// <summary>
        /// Common method used for moving playlists to a temporary folder, and getting the appropriate DataObject
        /// </summary>
        /// <param name="playlistsOrManagers"></param>
        /// <param name="parentManager"></param>
        /// <returns>DataObject with file paths of playlists and the playlists itself</returns>
        protected static async Task<DataObject> PartialCut(IEnumerable<PlaylistCoverViewModel> playlistsOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var playlists = new List<IPlaylist>();
            var tempPaths = new List<string>();

            var itemsToDelete = new List<PlaylistCoverViewModel>(playlistsOrManagers);
            
            foreach (var playlistOrManager in itemsToDelete)
            {
                var playlist = playlistOrManager.playlist;
                if (playlistOrManager.isPlaylist && playlist != null)
                {
                    playlists.Add(playlist);
                    var playlistPath = playlist.GetPlaylistPath(parentManager);
                    var tempPath = Path.GetTempPath() + Path.GetFileName(playlistPath);
                    tempPaths.Add(tempPath);
                    if (File.Exists(playlistPath))
                    {
                        await Task.Run(async () =>
                        {
                            await using FileStream fileStream = new(tempPath, FileMode.Create);
                            playlist.GetHandlerForPlaylist(parentManager)?.Serialize(playlist, fileStream);
                            parentManager.DeletePlaylist(playlist);
                            parentManager.RequestRefresh("PlaylistManager (desktop)");
                        });
                    }
                }
            }

            var clipboardData = new DataObject();
            clipboardData.Set(kPlaylistData, playlists);
            clipboardData.Set(DataFormats.FileNames, tempPaths);

            return clipboardData;
        }

        protected static DataObject PartialCopy(IEnumerable<PlaylistCoverViewModel> playlistsOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var playlists = new List<IPlaylist>();
            var playlistPaths = new List<string>();
            
            foreach (var playlistOrManager in playlistsOrManagers)
            {
                var playlist = playlistOrManager.playlist;
                if (playlistOrManager.isPlaylist && playlist != null)
                {
                    playlists.Add(playlist);
                    playlistPaths.Add(playlist.GetPlaylistPath(parentManager));
                }
            }
            
            var clipboardData = new DataObject();
            clipboardData.Set(IClipboardHandler.kPlaylistData, playlists);
            clipboardData.Set(DataFormats.FileNames, playlistPaths);

            return clipboardData;
        }
    }
}