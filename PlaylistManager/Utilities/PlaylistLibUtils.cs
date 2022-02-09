using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Types;

namespace PlaylistManager.Utilities
{
    public class PlaylistLibUtils
    {
        private readonly Assembly assembly;
        private readonly ConfigModel configModel;
        private BeatSaberPlaylistsLib.PlaylistManager playlistManager;
        public event Action<BeatSaberPlaylistsLib.PlaylistManager>? PlaylistManagerChanged;
        
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
            
            configModel.DirectoryChanged += dir =>
            {
                playlistManager = new BeatSaberPlaylistsLib.PlaylistManager(
                    Path.Combine(dir, "Playlists"),
                    legacyPlaylistHandler, blistPlaylistHandler);
                PlaylistManagerChanged?.Invoke(playlistManager);
            };
        }
        
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

        public static void OnPlaylistMove(IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            playlist.Filename = "";
            playlistManager?.StorePlaylist(playlist); 
        }

        public static async Task OnPlaylistFileCopy(IEnumerable<string> files, BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            foreach (var file in files)
            {
                var handler = playlistManager.GetHandlerForExtension(Path.GetExtension(file));
                if (handler != null)
                {
                    if (File.Exists(file))
                    {
                        var playlist = await Task.Run(async () =>
                        {
                            await using Stream fileStream = new FileStream(file, FileMode.Open);
                            return handler.Deserialize(fileStream);
                        });
                        playlistManager.StorePlaylist(playlist);
                    }
                }
            }
        }
    }

    public static class PlaylistLibExtensions
    {
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
        
        public static string GetPlaylistPath(this IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        => Path.Combine(parentManager.PlaylistPath, playlist.Filename + "." + 
                                                    (playlist.SuggestedExtension ??
                                                     parentManager.DefaultHandler?.DefaultExtension ?? "bplist"));
    }
}