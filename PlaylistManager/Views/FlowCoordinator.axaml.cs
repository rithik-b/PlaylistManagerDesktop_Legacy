using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PlaylistManager.Models;

namespace PlaylistManager.Views
{
    public class FlowCoordinator : UserControl
    {
        public FlowCoordinator()
        {
            InitializeComponent();
            DataContext = new FlowCoordinatorModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public class FlowCoordinatorModel : ViewModelBase
        {
            // TODO: Prettify Side Panel
            // TODO: Find a way to tab switch dynamically
            private ObservableCollection<string> MenuItems { get; } = new()
            {
                "Playlists",
                "Settings"
            };
            
            private int selectedIndex = 0;
            public int SelectedIndex
            {
                get => selectedIndex;
                set
                {
                    selectedIndex = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(PlaylistsVisible));
                    NotifyPropertyChanged(nameof(SettingsVisible));
                }
            }

            public bool PlaylistsVisible => SelectedIndex == 0;
            public bool SettingsVisible => SelectedIndex == 1;
        }
    }
}