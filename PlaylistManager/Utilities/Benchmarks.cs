using System;
using System.Diagnostics;
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
                Console.WriteLine($"Song load time: {time}ms");
            }
        }
    }
}