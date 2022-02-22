using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberPlaylistsLib.Blist;
using BeatSaberPlaylistsLib.Legacy;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;
using Difficulty = PlaylistManager.Models.Difficulty;

namespace PlaylistManager.Utilities
{
    public class PlaylistLibUtils
    {
        private readonly Assembly assembly;
        private readonly ConfigModel configModel;
        private BeatSaberPlaylistsLib.PlaylistManager playlistManager;
        public event Action<BeatSaberPlaylistsLib.PlaylistManager>? PlaylistManagerChanged;
        
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

        public IPlaylist CreatePlaylistWithConfig(string playlistName, BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            var playlist = CreatePlaylist(playlistName, configModel.AuthorName, playlistManager);
            using var coverStream = new MemoryStream();
            configModel.coverImage.Save(coverStream);
            playlist.SetCover(coverStream);
            return playlist;
        }

        public static IPlaylist CreatePlaylist(string playlistName, string playlistAuthorName, BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool allowDups = true)
        {
            IPlaylist playlist = playlistManager.CreatePlaylist("", playlistName, playlistAuthorName, "");

            if (!allowDups)
            {
                playlist.AllowDuplicates = false;
            }

            playlistManager.StorePlaylist(playlist);
            return playlist;
        }

        public async Task<IPlaylist[]> GetPlaylistsAsync(BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool includeChildren = false)
            => await Task.Run(() => playlistManager.GetAllPlaylists(includeChildren)).ConfigureAwait(false);
        
        public async Task RefreshPlaylistsAsync(BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool includeChildren = false)
            => await Task.Run(() => playlistManager.RefreshPlaylists(includeChildren)).ConfigureAwait(false);

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
        public static bool TryGetIdentifierForPlaylistSong(this IPlaylistSong playlistSong, out string identifier, out Identifier identifierType)
        {
            if (playlistSong.Identifiers.HasFlag(Identifier.Hash))
            {
                identifier = playlistSong.Hash!;
                identifierType = Identifier.Hash;
            }
            else if (playlistSong.Identifiers.HasFlag(Identifier.Key))
            {
                identifier = playlistSong.Key!;
                identifierType = Identifier.Key;
            }
            else if (playlistSong.Identifiers.HasFlag(Identifier.LevelId))
            {
                identifier = playlistSong.LevelId!;
                identifierType = Identifier.LevelId;
            }
            else
            {
                identifier = null!;
                identifierType = Identifier.None;
                return false;
            }
            return true;
        }
        
        public static string GetPlaylistPath(this IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        => Path.Combine(parentManager.PlaylistPath, playlist.Filename + "." + 
                                                    (playlist.SuggestedExtension ??
                                                     parentManager.DefaultHandler?.DefaultExtension ?? "bplist"));

        public static IPlaylistHandler? GetHandlerForPlaylist(this IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            var file = playlist.GetPlaylistPath(parentManager);
            var handler = parentManager.GetHandlerForExtension(Path.GetExtension(file));
            return handler;
        }

        public static Difficulty? GetDifficulty(this BeatSaberPlaylistsLib.Types.Difficulty plDifficulty)
        {
            switch (plDifficulty.DifficultyValue)
            {
                case 0:
                    return Difficulty.Easy;
                case 1:
                    return Difficulty.Normal;
                case 2:
                    return Difficulty.Hard;
                case 3:
                    return Difficulty.Expert;
                case 4:
                    return Difficulty.ExpertPlus;
            }
            return null;
        }
    }
}