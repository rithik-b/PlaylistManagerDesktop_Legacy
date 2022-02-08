using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PlaylistManager.Utilities;

namespace PlaylistManager.Types
{
    public class ConfigModel
    {
        private const string kConfigPath = "PlaylistManager.json";
        public string BeatSaberDir { get; set; }

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