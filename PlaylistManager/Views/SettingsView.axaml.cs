using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PlaylistManager.Models;
using Splat;

namespace PlaylistManager.Views
{
    public class SettingsView : UserControl
    {
        private readonly ViewModel viewModel;
        
        public SettingsView()
        {
            InitializeComponent();
            viewModel = new ViewModel();
            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public class ViewModel : ViewModelBase
        {
            private readonly ConfigModel configModel;

            public ViewModel()
            {
                configModel = Locator.Current.GetService<ConfigModel>()!;
            }

            public string BeatSaberDir
            {
                get => configModel.BeatSaberDir;
                set
                {
                    configModel.BeatSaberDir = value;
                    NotifyPropertyChanged();
                }
            }

            public Bitmap CoverImage
            {
                get => configModel.coverImage;
                set
                {
                    configModel.coverImage = value;
                    NotifyPropertyChanged();
                }
            }

            public string AuthorName
            {
                get => configModel.AuthorName;
                set
                {
                    configModel.AuthorName = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}