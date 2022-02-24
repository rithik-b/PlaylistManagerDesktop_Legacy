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
        public async Task Cut(IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var playlistPath = playlist.GetPlaylistPath(parentManager);
            var tempPath = Path.GetTempPath() + Path.GetFileName(playlistPath);
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
                
            var clipboardData = new DataObject();
            clipboardData.Set(PlaylistCoverView.kPlaylistData, playlist);
            clipboardData.Set(DataFormats.FileNames, new string[1]
            {
                tempPath
            });
            var clipboard = Application.Current?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(clipboardData);
            }        
        }

        public async Task Copy(IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var playlistPath = playlist.GetPlaylistPath(parentManager);
            var clipboardData = new DataObject();
            clipboardData.Set(PlaylistCoverView.kPlaylistData, playlist);
            clipboardData.Set(DataFormats.FileNames, new string[1]
            {
                playlistPath
            });
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