using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;

namespace PlaylistManager.Clipboard
{
    /// <summary>
    /// A clipboard handler for avalonia
    /// </summary>
    public class AvaloniaClipboardHandler : IClipboardHandler
    {
        public async Task Cut(IEnumerable<PlaylistCoverViewModel> playlistOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var playlists = new List<IPlaylist>();
            var tempPaths = new List<string>();
            
            foreach (var playlistOrManager in playlistOrManagers)
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
            clipboardData.Set(IClipboardHandler.kPlaylistData, playlists);
            clipboardData.Set(DataFormats.FileNames, tempPaths);
            var clipboard = Application.Current?.Clipboard;
            
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(clipboardData);
            }        
        }

        public async Task Copy(IEnumerable<PlaylistCoverViewModel> playlistOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var playlists = new List<IPlaylist>();
            var playlistPaths = new List<string>();
            
            foreach (var playlistOrManager in playlistOrManagers)
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
            var clipboard = Application.Current?.Clipboard;
            
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(clipboardData);
            }
        }

        public void Cut(PlaylistSongWrapper level)
        {
            throw new System.NotImplementedException();
        }

        public void Copy(PlaylistSongWrapper level)
        {
            throw new System.NotImplementedException();
        }
    }
}