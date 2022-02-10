using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using PlaylistManager.Utilities;

namespace PlaylistManager.Models
{
    public class ConfigModel
    {
        private const string kConfigPath = "PlaylistManager.json";
        public event Action<string>? DirectoryChanged;
        
        private string beatSaberDir = "";
        public string BeatSaberDir
        {
            get => beatSaberDir;
            set
            {
                beatSaberDir = value;
                DirectoryChanged?.Invoke(value);
            }
        }

        public static ConfigModel Factory()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kConfigPath);
            
            if (!File.Exists(path))
            {
                var configModel = new ConfigModel();
                configModel.Save();
                return configModel;
            }
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(kConfigPath).Build();
            return builder.Get<ConfigModel>();
        }

        public void Save()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kConfigPath);
            using Stream fileStream = File.Create(path);
            Utils.Serialize(this, fileStream);
        }
    }
}