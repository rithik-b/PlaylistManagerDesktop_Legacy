using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Splat;

namespace PlaylistManager.Utilities
{
    public class Benchmarks
    {
        public Benchmarks()
        {
            // _ = SongBenchmark();
            // _ = PlaylistBenchmark();
        }
        
        public async Task SongBenchmark()
        {
            var songLoader = Locator.Current.GetService<SongLoader>();
            if (songLoader != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var songs = await songLoader.GetCustomLevelsAsync();
                stopwatch.Stop();
                var time = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"All songs load time: {time}ms");

                if (songs.Count > 0)
                {
                    stopwatch.Reset();
                    var song = songs.First().Value;
                    stopwatch.Start();
                    var levelData = await song.GetLevelDataAsync();
                    var cover = await levelData!.GetCoverImageAsync();
                    stopwatch.Stop();
                    time = stopwatch.ElapsedMilliseconds;
                    Console.WriteLine($"Song parse time: {time}ms");
                }
            }
        }
        
        public async Task PlaylistBenchmark()
        {
            var playlistLibUtils = Locator.Current.GetService<PlaylistLibUtils>();
            if (playlistLibUtils != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var playlists = await playlistLibUtils.GetPlaylistsAsync(playlistLibUtils.PlaylistManager, true);
                stopwatch.Stop();
                var time = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Playlist load time: {time}ms");
            }
        }
    }
}