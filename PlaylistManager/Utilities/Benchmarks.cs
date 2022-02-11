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
            // _ = LevelBenchmark();
            // _ = PlaylistBenchmark();
        }
        
        public async Task LevelBenchmark()
        {
            var levelLoader = Locator.Current.GetService<LevelLoader>();
            if (levelLoader != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var levels = await levelLoader.GetCustomLevelsAsync();
                stopwatch.Stop();
                var time = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"All levels load time: {time}ms");

                if (levels.Count > 0)
                {
                    stopwatch.Reset();
                    var level = levels.First().Value;
                    stopwatch.Start();
                    var levelData = await level.GetLevelDataAsync();
                    var cover = await levelData!.GetCoverImageAsync();
                    stopwatch.Stop();
                    time = stopwatch.ElapsedMilliseconds;
                    Console.WriteLine($"Level parse time: {time}ms");
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