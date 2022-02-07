using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PlaylistManager.Types;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.Views
{
    public class PlaylistsListView : UserControl
    {
        private readonly ViewModel viewModel;
        
        public PlaylistsListView()
        {
            InitializeComponent();
            viewModel = new ViewModel();
            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnClick(object? sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentManager != null)
            {
                var parentManager = viewModel.CurrentManager.Parent;
                if (parentManager != null)
                {
                    viewModel.CurrentManager = parentManager;
                }
            }
        }
        
        private void OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedPlaylistOrManager is {isPlaylist: false})
            {
                viewModel.CurrentManager = viewModel.SelectedPlaylistOrManager.playlistManager;
            }
        }

        public class ViewModel : ViewModelBase
        {
            private readonly PlaylistLibUtils? playlistLibUtils;
            private BeatSaberPlaylistsLib.PlaylistManager? currentManager;
            private PlaylistCoverViewModel? selectedPlaylistOrManager;
            
            public ObservableCollection<PlaylistCoverViewModel> SearchResults { get; } = new();
            public bool IsSubfolder => currentManager?.Parent != null;
            public string FolderTitle => Path.GetFileName(currentManager?.PlaylistPath) ?? "";
            
            public BeatSaberPlaylistsLib.PlaylistManager? CurrentManager
            {
                get => currentManager;
                set
                {
                    currentManager = value;
                    _ = LoadPlaylistsAsync();
                    NotifyPropertyChanged(nameof(IsSubfolder));
                    NotifyPropertyChanged(nameof(FolderTitle));
                }
            }
            
            public PlaylistCoverViewModel? SelectedPlaylistOrManager
            {
                get => selectedPlaylistOrManager;
                set
                {
                    selectedPlaylistOrManager = value;
                    NotifyPropertyChanged();
                }
            }
            
            public ViewModel()
            {
                playlistLibUtils = Locator.Current.GetService<PlaylistLibUtils>();
                currentManager = playlistLibUtils?.PlaylistManager;
                _ = LoadPlaylistsAsync();
            }

            private async Task LoadPlaylistsAsync()
            {
                if (playlistLibUtils != null && currentManager != null)
                {
                    SearchResults.Clear();
                    var folders = currentManager.GetChildManagers();
                    var playlists = await playlistLibUtils.GetPlaylistsAsync(currentManager);
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