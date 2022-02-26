using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.Models
{
    public class ConfigModel
    {
        private const string kConfigPath = "PlaylistManager.json";
        private const string kImagePath = "CoverImage";
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
        
        [JsonIgnore]
        public Bitmap? coverImage;

        public static ConfigModel Factory()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kConfigPath);

            ConfigModel configModel;
            
            if (!File.Exists(configPath))
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
            
            configModel.LoadImage();

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += delegate(object? sender, ControlledApplicationLifetimeExitEventArgs args)
                {
                    configModel.Save();
                };
            }
            
            return configModel;
        }

        private void LoadImage()
        {
            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kImagePath);
            if (File.Exists(imagePath))
            {
                using Stream imageStream = File.Open(imagePath, FileMode.Open);
                coverImage = Bitmap.DecodeToHeight(imageStream, 512);
            }
            else
            {
                var assembly = Locator.Current.GetService<Assembly>();
                using Stream? imageStream = assembly?.GetManifestResourceStream("PlaylistManager.Icons.DefaultIcon.png");
                coverImage = Bitmap.DecodeToHeight(imageStream, 512);
            }
        }

        public void Save()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kConfigPath);
            using Stream configStream = File.Create(configPath);
            Utils.Serialize(this, configStream);
            
            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, kImagePath);
            coverImage?.Save(imagePath);
        }
    }
}