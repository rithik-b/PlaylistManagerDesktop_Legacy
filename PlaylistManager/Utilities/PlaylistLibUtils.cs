using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Types;
using PlaylistManager.Views;

namespace PlaylistManager.Utilities
{
    public class PlaylistLibUtils
    {
        private readonly Assembly assembly;
        private readonly ConfigModel configModel;
        private BeatSaberPlaylistsLib.PlaylistManager playlistManager;
        public event Action<BeatSaberPlaylistsLib.PlaylistManager>? PlaylistManagerChanged;
        public event Action? PlaylistManagerRefreshed;
        
        private const string ICON_PATH = "PlaylistManager.Icons.DefaultIcon.png";

        public PlaylistLibUtils(Assembly assembly, ConfigModel configModel)
        {
            this.assembly = assembly;
            this.configModel = configModel;
            var legacyPlaylistHandler = new LegacyPlaylistHandler();
            var blistPlaylistHandler = new BlistPlaylistHandler();
            
            playlistManager = new BeatSaberPlaylistsLib.PlaylistManager(
                Path.Combine(configModel.BeatSaberDir, "Playlists"), 
                legacyPlaylistHandler, blistPlaylistHandler);
            
            playlistManager.PlaylistsRefreshRequested += RaiseRefresh;

            configModel.DirectoryChanged += dir =>
            {
                playlistManager.PlaylistsRefreshRequested -= RaiseRefresh;
                playlistManager = new BeatSaberPlaylistsLib.PlaylistManager(
                    Path.Combine(dir, "Playlists"),
                    legacyPlaylistHandler, blistPlaylistHandler);
                PlaylistManagerChanged?.Invoke(playlistManager);
                playlistManager.PlaylistsRefreshRequested += RaiseRefresh;
            };
        }

        private void RaiseRefresh(object? sender, string source) => PlaylistManagerRefreshed?.Invoke();

        public BeatSaberPlaylistsLib.PlaylistManager PlaylistManager => playlistManager;

        public BeatSaberPlaylistsLib.Types.IPlaylist CreatePlaylist(string playlistName, string playlistAuthorName, BeatSaberPlaylistsLib.PlaylistManager playlistManager, 
            bool defaultCover = true, bool allowDups = true)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist playlist = playlistManager.CreatePlaylist("", playlistName, playlistAuthorName, "");

            if (defaultCover)
            {
                using Stream? imageStream = assembly.GetManifestResourceStream(ICON_PATH);
                if (imageStream != null)
                {
                    playlist.SetCover(imageStream);
                }
            }

            if (!allowDups)
            {
                playlist.AllowDuplicates = false;
            }

            playlistManager.StorePlaylist(playlist);
            playlistManager.RequestRefresh("PlaylistManager (Desktop)");
            return playlist;
        }

        public async Task<IPlaylist[]> GetPlaylistsAsync(BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool includeChildren = false)
        {
            return await Task.Run(() => playlistManager.GetAllPlaylists(includeChildren));
        }
    }

    public static class PlaylistLibExtensions
    {
        public static void MovePlaylist(this IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager oldManager, BeatSaberPlaylistsLib.PlaylistManager newManager)
        {
            oldManager.DeletePlaylist(playlist);
            playlist.Filename = "";
            newManager.StorePlaylist(playlist);
        }
        
        public static string? GetIdentifierForPlaylistSong(this IPlaylistSong playlistSong)
        {
            if (playlistSong.Identifiers.HasFlag(Identifier.Hash))
            {
                return playlistSong.Hash;
            }
            if (playlistSong.Identifiers.HasFlag(Identifier.Key))
            {
                return playlistSong.Key;
            }
            if (playlistSong.Identifiers.HasFlag(Identifier.LevelId))
            {
                return playlistSong.LevelId;
            }
            return null;
        }
    }
}