using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly PlaylistLibUtils playlistLibUtils;
        private readonly LevelMatcher levelMatcher;

        public AvaloniaClipboardHandler(PlaylistLibUtils playlistLibUtils, LevelMatcher levelMatcher)
        {
            this.playlistLibUtils = playlistLibUtils;
            this.levelMatcher = levelMatcher;
        }
        
        public async Task Cut(IEnumerable<PlaylistCoverViewModel> playlistsOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var clipboardData = await IClipboardHandler.PartialCut(playlistsOrManagers, parentManager);
            var clipboard = Application.Current?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(clipboardData);
            }        
        }

        public async Task Copy(IEnumerable<PlaylistCoverViewModel> playlistsOrManagers, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var clipboardData = IClipboardHandler.PartialCopy(playlistsOrManagers, parentManager);
            var clipboard = Application.Current?.Clipboard;
            
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(clipboardData);
            }
        }

        public async Task<IEnumerable<IPlaylist>?> PastePlaylists()
        {
            var clipboard = Application.Current?.Clipboard;
            if (clipboard != null)
            {
                var data = await clipboard.GetFormatsAsync();

                if (await clipboard.GetDataAsync(DataFormats.FileNames) is List<string> playlistPaths)
                {
                    var playlists = new List<IPlaylist>();
                    foreach (var path in playlistPaths)
                    {
                        var handler = playlistLibUtils.PlaylistManager.GetHandlerForExtension(Path.GetExtension(path));
                        if (handler != null)
                        {
                            if (File.Exists(path))
                            {
                                var playlist = await Task.Run(async () =>
                                {
                                    await using Stream fileStream = new FileStream(path, FileMode.Open);
                                    return handler.Deserialize(fileStream);
                                });
                                playlists.Add(playlist);
                            }
                        }
                    }
                    return playlists;
                }
            }
            return null;
        }

        public async Task Copy(IEnumerable<PlaylistSongWrapper> playlistSongWrappers)
        {
            var clipboardData = new DataObject();
            var playlistSongs = new List<IPlaylistSong>();
            foreach (var playlistSongWrapper in playlistSongWrappers)
            {
                playlistSongs.Add(playlistSongWrapper.playlistSong);
            }
            
            clipboardData.Set(IClipboardHandler.kPlaylistSongData, playlistSongs);
            var clipboard = Application.Current?.Clipboard;
            
            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(clipboardData);
            }
        }

        public async Task<IEnumerable<PlaylistSongWrapper>?> PastePlaylistSongWrappers()
        {
            var clipboard = Application.Current?.Clipboard;
            if (clipboard != null)
            {
                var data = await clipboard.GetFormatsAsync();
                
                if (data.Contains(IClipboardHandler.kPlaylistSongData) && await clipboard.GetDataAsync(IClipboardHandler.kPlaylistSongData) is IEnumerable<IPlaylistSong> playlistSongs)
                {
                    var playlistSongWrappers = new List<PlaylistSongWrapper>();

                    foreach (var playlistSong in playlistSongs)
                    {
                        if (playlistSong.TryGetIdentifierForPlaylistSong(out var identifier, out var identifierType))
                        {
                            var levelData = identifierType == Identifier.Hash ? await levelMatcher.GetLevelByHash(identifier) :
                                identifierType == Identifier.Key ? await levelMatcher.GetLevelByKey(identifier) : null;
                            if (levelData != null)
                            {
                                playlistSongWrappers.Add(new PlaylistSongWrapper(playlistSong, levelData));
                            }
                        }
                    }
                    
                    return playlistSongWrappers;
                }
            }
            return null;
        }
    }
}