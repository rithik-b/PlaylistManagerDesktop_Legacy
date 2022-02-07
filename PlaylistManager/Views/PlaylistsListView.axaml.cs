using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PlaylistManager.Types;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.Views
{
    public class PlaylistsListView : UserControl
    {
        public PlaylistsListView()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public class ViewModel : ViewModelBase
        {
            private readonly PlaylistLibUtils? playlistLibUtils = Locator.Current.GetService<PlaylistLibUtils>();
            private PlaylistCoverViewModel? selectedPlaylist;

            public ObservableCollection<PlaylistCoverViewModel> SearchResults { get; } = new();
            
            public PlaylistCoverViewModel? SelectedPlaylist
            {
                get => selectedPlaylist;
                set
                {
                    selectedPlaylist = value;
                    NotifyPropertyChanged();
                }
            }
            
            public ViewModel()
            {
                if (playlistLibUtils != null)
                {
                    var playlists = playlistLibUtils.PlaylistManager.GetAllPlaylists();
                    foreach (var playlist in playlists)
                    {
                        SearchResults.Add(new PlaylistCoverViewModel(playlist));
                    }
                }
            }
        }
    }
}