using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
                Task.Run(LoadPlaylistsAsync);
            }

            private async Task LoadPlaylistsAsync()
            {
                if (playlistLibUtils != null)
                {
                    var folders = playlistLibUtils.PlaylistManager.GetChildManagers();
                    var playlists = await playlistLibUtils.GetPlaylistsAsync(playlistLibUtils.PlaylistManager);
                    foreach (var folder in folders)
                    {
                        SearchResults.Add(new PlaylistCoverViewModel(folder));
                    }
                    foreach (var playlist in playlists)
                    {
                        SearchResults.Add(new PlaylistCoverViewModel(playlist));
                    }
                }
            }
        }
    }
}