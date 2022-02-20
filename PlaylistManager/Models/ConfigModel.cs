using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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

        public string AuthorName { get; set; } = nameof(PlaylistManager);

        public static ConfigModel Factory()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kConfigPath);

            ConfigModel configModel;
            
            if (!File.Exists(path))
            {
                configModel = new ConfigModel();
                configModel.Save();
                return configModel;
            }
            else
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(kConfigPath).Build();
                configModel = builder.Get<ConfigModel>();
            }

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += delegate(object? sender, ControlledApplicationLifetimeExitEventArgs args)
                {
                    configModel.Save();
                };
            }
            
            return configModel;
        }

        public void Save()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kConfigPath);
            using Stream fileStream = File.Create(path);
            Utils.Serialize(this, fileStream);
        }
    }
}