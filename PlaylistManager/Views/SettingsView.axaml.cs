using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
        }
    }
}