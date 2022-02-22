using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BeatSaber.SongHashing;
using PlaylistManager.Models;

namespace PlaylistManager.Utilities
{
    public class LevelLoader
    {
        // TODO (Maybe): Add support for SongCore custom directories and ignore WIP directories
        private string kBeatSaberDataDir = "Beat Saber_Data";
        private string kCustomLevelsDir = "CustomLevels";
        
        private readonly ConcurrentDictionary<string, CustomLevel> customLevels;
        private readonly Hasher hasher;
        private readonly ConfigModel configModel;
        private bool needsRefresh;

        public LevelLoader(ConfigModel configModel)
        {
            customLevels = new ConcurrentDictionary<string, CustomLevel>();
            hasher = new Hasher();
            this.configModel = configModel;
            needsRefresh = true;
            configModel.DirectoryChanged += s => needsRefresh = true;
        }
        
        /// <summary>
        /// Loads all levels in the Custom Levels directory
        /// </summary>
        /// <param name="needsRefresh">If the levels need to be loaded again</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A dictionary of hash mapped to levels</returns>
        public async Task<ConcurrentDictionary<string, CustomLevel>> GetCustomLevelsAsync(bool needsRefresh = false, CancellationToken? cancellationToken = null)
        {
            if (needsRefresh || this.needsRefresh)
            {
                this.needsRefresh = false;
                customLevels.Clear();
                await Task.Run(() =>
                {
                    var songDirectories = Directory.GetDirectories(Path.Combine(configModel.BeatSaberDir, kBeatSaberDataDir, kCustomLevelsDir));
                    foreach (var songDirectory in songDirectories)
                    {
                        var hash = hasher.HashDirectory(songDirectory, cancellationToken ?? CancellationToken.None);
                        if (hash.Hash != null && hash.ResultType is HashResultType.Success or HashResultType.Warn)
                        {
                            customLevels[hash.Hash] = new CustomLevel(hash.Hash, songDirectory);
                        }
                    }
                }, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
            }

            return customLevels;
        }
    }
}