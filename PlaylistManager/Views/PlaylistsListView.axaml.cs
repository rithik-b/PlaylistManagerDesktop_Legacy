using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
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
        public readonly ViewModel viewModel;
        
        private NavigationPanel? navigationPanel;
        private NavigationPanel? NavigationPanel => navigationPanel ??= Locator.Current.GetService<NavigationPanel>("PlaylistsTab");

        private PlaylistsDetailView? playlistsDetailView;
        private PlaylistsDetailView? PlaylistsDetailView => playlistsDetailView ??= Locator.Current.GetService<PlaylistsDetailView>();

        public PlaylistsListView()
        {
            InitializeComponent();
            viewModel = new ViewModel();
            DataContext = viewModel;
            Locator.CurrentMutable.RegisterConstant(this, typeof(PlaylistsListView));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var copyTarget = this.Find<Border>("CopyTarget");
            copyTarget.AddHandler(DragDrop.DragOverEvent, DragOver!);
            copyTarget.AddHandler(DragDrop.DropEvent, Drop!);
        }

        #region Drag and Drop

        private void DragOver(object? sender, DragEventArgs e)
        {
            if (viewModel.CurrentManager != null 
                && e.Data.Contains(DataFormats.FileNames)
                && !e.Data.Contains(PlaylistCoverView.kPlaylistData)
                && viewModel.CurrentManager.SupportsExtension(Path.GetExtension(e.Data.GetFileNames()?.FirstOrDefault() ?? "")))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else if (!e.Data.Contains(PlaylistCoverView.kPlaylistData))
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private async void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames) && !e.Data.Contains(PlaylistCoverView.kPlaylistData) && viewModel is {CurrentManager:{}})
            {
                await PlaylistLibUtils.OnPlaylistFileCopy(e.Data.GetFileNames()!, viewModel.CurrentManager);
                viewModel.CurrentManager.RequestRefresh("PlaylistManager (desktop)");
                e.DragEffects = DragDropEffects.Copy;
            }
        }

        #endregion

        private void OnBackClick(object? sender, RoutedEventArgs e)
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

        private void OnDoubleTapped(object? sender, RoutedEventArgs e) => OpenSelectedPlaylistOrManager();

        private void LoseFocus(object? sender, PointerPressedEventArgs e)
        {
            if (viewModel.SelectedPlaylistOrManager != null)
            {
                viewModel.SelectedPlaylistOrManager.IsRenaming = false;
            }
            viewModel.SelectedPlaylistOrManager = null;
        }
        
        private async void OnShortcut(object? sender, KeyEventArgs e)
        {
            if (viewModel.SelectedPlaylistOrManager != null)
            {
                if (e.KeyModifiers == KeyModifiers.Control)
                {
                    switch (e.Key)
                    {
                        case Key.X:
                            await viewModel.SelectedPlaylistOrManager.Cut();
                            break;
                        case Key.C:
                            await viewModel.SelectedPlaylistOrManager.Copy();
                            break;
                    }
                }
                else if (e.Key == Key.Delete)
                {
                    viewModel.SelectedPlaylistOrManager.Delete();
                }
            }
        }

        public void OpenSelectedPlaylistOrManager()
        {
            if (viewModel.SelectedPlaylistOrManager is {isPlaylist: false})
            {
                viewModel.CurrentManager = viewModel.SelectedPlaylistOrManager.playlistManager;
            }
            else if (viewModel.SelectedPlaylistOrManager is {isPlaylist: true, playlist:{}})
            {
                if (PlaylistsDetailView != null)
                {
                    PlaylistsDetailView.ViewModel = new PlaylistsDetailViewModel(viewModel.SelectedPlaylistOrManager.playlist);
                    NavigationPanel?.Push(PlaylistsDetailView);
                }
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
                    if (currentManager != null)
                    {
                        currentManager.PlaylistsRefreshRequested -= OnRefresh;
                    }
                    currentManager = value;
                    if (value != null)
                    {
                        value.PlaylistsRefreshRequested += OnRefresh;
                    }
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
                    if (selectedPlaylistOrManager != null)
                    {
                        selectedPlaylistOrManager.IsRenaming = false;
                    }
                    selectedPlaylistOrManager = value;
                    NotifyPropertyChanged();
                }
            }
            
            public ViewModel()
            {
                playlistLibUtils = Locator.Current.GetService<PlaylistLibUtils>();
                CurrentManager = playlistLibUtils?.PlaylistManager;
                
                playlistLibUtils!.PlaylistManagerChanged += manager =>
                {
                    CurrentManager = manager;
                };
            }

            private void OnRefresh(object? args, string source) => _ = LoadPlaylistsAsync();

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