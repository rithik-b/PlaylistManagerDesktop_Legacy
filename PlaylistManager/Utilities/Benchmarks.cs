using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SongDetailsCache;
using Splat;

namespace PlaylistManager.Utilities
{
    public class Benchmarks
    {
        public Benchmarks()
        {
            // _ = LevelBenchmark();
            // _ = SongDetailsBenchmark();
            // _ = LevelLookupBenchmark();
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

        public async Task SongDetailsBenchmark()
        {
            var songDetailsLoader = Locator.Current.GetService<SongDetailsLoader>();
            if (songDetailsLoader != null)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                await songDetailsLoader.Init();
                var result = songDetailsLoader.TryGetLevelByKey("25f", out var level);
                stopwatch.Stop();
                var time = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"SongDetails init and search took {time}ms");
            }
        }

        public async Task LevelLookupBenchmark()
        {
            var levelLookup = Locator.Current.GetService<LevelLookup>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (levelLookup != null)
            {
                var level = await levelLookup.GetLevelByKey("25f");
            }
            stopwatch.Stop();
            var time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Key lookup on owned level took {time}ms");
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