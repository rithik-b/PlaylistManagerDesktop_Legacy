using System;
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
            AddHandler(DragDrop.DragOverEvent, DragOver!);
            AddHandler(DragDrop.DropEvent, Drop!);
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            if (viewModel.CurrentManager != null 
                && e.Data.Contains(DataFormats.FileNames)
                && viewModel.CurrentManager.SupportsExtension(Path.GetExtension(e.Data.GetFileNames()?.FirstOrDefault() ?? "")))
            {
                e.DragEffects = DragDropEffects.Link;
            }
        }

        private async void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames) && !e.Data.Contains(PlaylistCoverView.kPlaylistData) && viewModel is {CurrentManager:{}})
            {
                await PlaylistLibUtils.OnPlaylistFileCopy(e.Data.GetFileNames()!, viewModel.CurrentManager);
                viewModel.CurrentManager.RequestRefresh("PlaylistManager (desktop)");
            }
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
        
        private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            viewModel.SelectedPlaylistOrManager = null;
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