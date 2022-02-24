using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PlaylistManager.Models;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.Views
{
    public class PlaylistsListView : UserControl
    {
        public readonly ViewModel viewModel;
        
        private NavigationPanel? navigationPanel;
        private NavigationPanel NavigationPanel => navigationPanel ??= Locator.Current.GetService<NavigationPanel>("PlaylistsTab")!;

        private PlaylistsDetailView? playlistsDetailView;
        private PlaylistsDetailView PlaylistsDetailView => playlistsDetailView ??= Locator.Current.GetService<PlaylistsDetailView>()!;

        private PlaylistLibUtils? playlistLibUtils;
        private PlaylistLibUtils PlaylistLibUtils => playlistLibUtils ??= Locator.Current.GetService<PlaylistLibUtils>()!;

        public PlaylistsListView()
        {
            AvaloniaXamlLoader.Load(this);
            var copyTarget = this.Find<Border>("CopyTarget");
            copyTarget.AddHandler(DragDrop.DragOverEvent, DragOver!);
            copyTarget.AddHandler(DragDrop.DropEvent, Drop!);
            
            viewModel = new ViewModel(this.FindControl<ListBox>("ListBox"));
            DataContext = viewModel;
            Locator.CurrentMutable.RegisterConstant(this, typeof(PlaylistsListView));
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
            else if (viewModel.SelectedPlaylistOrManager is {isPlaylist: true, playlist:{}} && viewModel.CurrentManager != null)
            {
                PlaylistsDetailView.ViewModel = new PlaylistsDetailViewModel(viewModel.SelectedPlaylistOrManager.playlist, viewModel.CurrentManager);
                NavigationPanel.Push(PlaylistsDetailView);
            }
        }
        
        #region Context Menu
        
        private void NewPlaylistClick(object? sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentManager != null)
            {
                var playlist = PlaylistLibUtils.CreatePlaylistWithConfig("New Playlist", viewModel.CurrentManager);
                var playlistViewModel = new PlaylistCoverViewModel(playlist);
                viewModel.SearchResults.Add(playlistViewModel);
                viewModel.SelectedPlaylistOrManager = playlistViewModel;
                playlistViewModel.IsRenaming = true;
            }
        }
        
        private void NewFolderClick(object? sender, RoutedEventArgs e)
        {
            if (viewModel.CurrentManager != null)
            {
                var folder = viewModel.CurrentManager.CreateChildManager("New Folder");
                var playlistViewModel = new PlaylistCoverViewModel(folder);
                viewModel.SearchResults.Add(playlistViewModel);
                viewModel.SelectedPlaylistOrManager = playlistViewModel;
                playlistViewModel.IsRenaming = true;
            }
        }
        
        #endregion

        public class ViewModel : ViewModelBase
        {
            private readonly ListBox listBox;
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

            public List<PlaylistCoverViewModel> SelectedPlaylistsOrManagers { get; } = new();

            public ViewModel(ListBox listBox)
            {
                this.listBox = listBox;
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
                    var scrollable = listBox.Scroll;
                    var offset = scrollable?.Offset ?? new Vector(0,0);
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

                    await Task.Delay(1); // I have to wait a tick and do this I am sorry
                    
                    if (scrollable != null && scrollable.Extent.Height > offset.Y)
                    {
                        scrollable.Offset = offset;
                    }
                }
            }
        }
    }
}