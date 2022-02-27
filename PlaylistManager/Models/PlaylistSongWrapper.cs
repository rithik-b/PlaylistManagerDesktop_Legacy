using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Utilities;

namespace PlaylistManager.Models
{
    /// <summary>
    /// A wrapper class that combines PlaylistSong with ICustomLevelData
    /// </summary>
    public class PlaylistSongWrapper : ILevelData
    {
        public readonly IPlaylistSong playlistSong;
        private readonly ICustomLevelData? customLevelData;
        
        public string SongName
        {
            get
            {
                if (customLevelData != null)
                {
                    return customLevelData.SongName;
                }

                if (playlistSong.Name != null)
                {
                    return playlistSong.Name;
                }

                return playlistSong.TryGetIdentifierForPlaylistSong(out var identifier, out var identifierType)
                    ? identifier : "Unknown";
            }
        }
        public string SongSubName => customLevelData?.SongSubName ?? "";
        public string SongAuthorName => customLevelData != null ? customLevelData.SongAuthorName : "Unknown";
        public string LevelAuthorName => customLevelData != null 
            ? customLevelData.LevelAuthorName 
            : playlistSong.LevelAuthorName ?? "Unknown";
        public bool Downloaded => customLevelData != null && customLevelData is not IRemoteLevelData;
        public Dictionary<string, List<Difficulty>> Difficulties => customLevelData?.Difficulties ?? new Dictionary<string, List<Difficulty>>();
        
        public PlaylistSongWrapper(IPlaylistSong playlistSong) => this.playlistSong = playlistSong;
        public PlaylistSongWrapper(IPlaylistSong playlistSong, ICustomLevelData customLevelData) : this(playlistSong) =>
            this.customLevelData = customLevelData;
        
        public async Task<string?> GetKeyAsync()
        {
            if (customLevelData != null)
            {
                return await customLevelData.GetKeyAsync();
            }
            return null;
        }

        public async Task<Bitmap?> GetCoverImageAsync(CancellationToken? cancellationToken = null)
        {
            if (customLevelData != null)
            {
                return await customLevelData.GetCoverImageAsync(cancellationToken);
            }
            return null;
        }
    }
}