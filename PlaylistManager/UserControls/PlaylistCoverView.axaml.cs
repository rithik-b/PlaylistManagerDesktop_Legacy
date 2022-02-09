using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using PlaylistManager.Views;
using Splat;

namespace PlaylistManager.UserControls
{
    public class PlaylistCoverView : UserControl
    {
        private PlaylistsListView? playlistsListView;
        private TextBox? renameBox;
        
        public PlaylistCoverView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            AddHandler(DragDrop.DragOverEvent, DragOver!);
            AddHandler(DragDrop.DropEvent, Drop!);
        }

        #region Drag and Drop
        
        public const string kPlaylistData = "application/com.rithik-b.PlaylistManager.Playlist";
        private const int kHoldDelay = 300;
        private bool pointerHeld;
        
        private async void DoDrag(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            pointerHeld = true;

            await Task.Delay(kHoldDelay);
            if (!pointerHeld)
            {
                return;
            }
            
            playlistsListView ??= Locator.Current.GetService<PlaylistsListView>();
            if (DataContext is PlaylistCoverViewModel {isPlaylist: true} coverViewModel and {playlist: { }}
                && playlistsListView is {viewModel: {CurrentManager: { }}})
            {
                var playlistPath = coverViewModel.playlist.GetPlaylistPath(playlistsListView.viewModel.CurrentManager);
                var dragData = new DataObject();
                dragData.Set(kPlaylistData, coverViewModel.playlist);
                dragData.Set(DataFormats.FileNames, new string[1]
                {
                    playlistPath
                });
                
                // Need to keep file name as it will change when moving
                var oldFileName = coverViewModel.playlist.Filename;

                var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Link |
                                                                    DragDropEffects.Move | 
                                                                    DragDropEffects.Copy |
                                                                    DragDropEffects.None);
                if (result == DragDropEffects.Move)
                {
                    var newFileName = coverViewModel.playlist.Filename;
                    coverViewModel.playlist.Filename = oldFileName;
                    if (File.Exists(playlistPath))
                    {
                        playlistsListView.viewModel.CurrentManager.DeletePlaylist(coverViewModel.playlist);
                    }
                    coverViewModel.playlist.Filename = newFileName;
                    playlistsListView.viewModel.SearchResults.Remove(coverViewModel);
                }
            }
        }
        
        // Tracks if pointer is released to prevent a drag operation
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) => pointerHeld = false;

        private void DragOver(object sender, DragEventArgs e)
        {
            // If playlist is dragged (from within app) only move to other subfolders
            if (DataContext is PlaylistCoverViewModel current && !current.isPlaylist &&
                current is {playlistManager: { }} && e.Data.Contains(kPlaylistData))
            {
                e.DragEffects = DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }
        
        private async void Drop(object sender, DragEventArgs e)
        {
            if (DataContext is PlaylistCoverViewModel current && !current.isPlaylist && current is {playlistManager: { }})
            {
                if (e.Data.Contains(kPlaylistData) && e.Data.Get(kPlaylistData) is IPlaylist drag)
                {
                    PlaylistLibUtils.OnPlaylistMove(drag, current.playlistManager);
                    _ = current.LoadPlaylistsAsync();
                }
            }
        }
        #endregion

        #region Context Menu

        private void OpenClick(object? sender, RoutedEventArgs e)
        {
            playlistsListView ??= Locator.Current.GetService<PlaylistsListView>();
            if (playlistsListView != null)
            {
                playlistsListView.OpenSelectedPlaylistOrManager();
            }
        }
        
        private async void RenameClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is PlaylistCoverViewModel viewModel)
            {
                viewModel.IsRenaming = true;
                renameBox ??= this.Find<TextBox>("RenameBox");
                // I am sorry but I gotta wait a tick
                await Task.Delay(1);
                renameBox.Focus();
                renameBox.SelectionStart = 0;
                renameBox.SelectionEnd = Int32.MaxValue;
            }
        }

        private void RenameKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is PlaylistCoverViewModel viewModel)
            {
                viewModel.IsRenaming = false;
            }
        }

        #endregion
    }
    
    public class PlaylistCoverViewModel : ViewModelBase
    {
        public readonly IPlaylist? playlist;
        public readonly BeatSaberPlaylistsLib.PlaylistManager? playlistManager;
        public readonly bool isPlaylist;
        private CoverImageLoader? coverImageLoader;
        private PlaylistLibUtils? playlistLibUtils;
        private PlaylistsListView? playlistsListView;
        private int? numPlaylists;
        private Bitmap? coverImage;

        public PlaylistCoverViewModel(IPlaylist playlist)
        {
            this.playlist = playlist;
            isPlaylist = true;
        }
        
        public PlaylistCoverViewModel(BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            this.playlistManager = playlistManager;
            isPlaylist = false;
        }
        
        private bool AllowDrop => !isPlaylist;
        
        public string Title
        {
            get
            {
                if (isPlaylist)
                {
                    return playlist?.Title ?? "";
                }
                return Path.GetFileName(playlistManager?.PlaylistPath) ?? "";
            }
            set
            {
                if (isPlaylist && playlist != null)
                {
                    playlist.Title = value;
                    playlistsListView ??= Locator.Current.GetService<PlaylistsListView>();
                    playlistsListView?.viewModel.CurrentManager?.StorePlaylist(playlist);
                }
                else if (!isPlaylist && playlistManager != null)
                {
                    playlistManager.RenameManager(value);
                }
                NotifyPropertyChanged();
            }
        }

        public string Author
        {
            get
            {
                if (isPlaylist)
                {
                    return playlist?.Author ?? "";
                }
                
                if (numPlaylists == null)
                {
                    _ = LoadPlaylistsAsync();
                    return "";
                }
                
                return $"{numPlaylists} playlists";
            }
        }

        public Bitmap? CoverImage
        {
            get
            {
                if (coverImage != null)
                {
                    return coverImage;
                }
                coverImageLoader ??= Locator.Current.GetService<CoverImageLoader>();
                if (isPlaylist)
                {
                    _ = LoadCoverAsync();
                    return coverImageLoader?.LoadingImage;
                }
                return coverImageLoader?.FolderImage;
            }
            set
            {
                coverImage = value;
                NotifyPropertyChanged();
            }
        }
        
        private async Task LoadCoverAsync()
        {
            await using var imageStream = playlist?.GetCoverStream();
            var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            if (bitmap != null)
            {
                CoverImage = bitmap;
            }
        }
        
        public async Task LoadPlaylistsAsync()
        {
            playlistLibUtils ??= Locator.Current.GetService<PlaylistLibUtils>();
            if (playlistLibUtils != null && playlistManager != null)
            {
                numPlaylists = (await playlistLibUtils.GetPlaylistsAsync(playlistManager)).Length;
                NotifyPropertyChanged(nameof(Author));
            }
        }

        #region Context Menu
        
        private bool isRenaming;
        public bool IsRenaming
        {
            get => isRenaming;
            set
            {
                isRenaming = value;
                if (value)
                {
                    RenameTitle = Title;
                }
                else
                {
                    if (Title != RenameTitle && !string.IsNullOrWhiteSpace(RenameTitle))
                    {
                        Title = RenameTitle;
                    }
                }
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotRenaming));
            }
        }
        private bool IsNotRenaming => !isRenaming;

        private string renameTitle = "";
        public string RenameTitle
        {
            get => renameTitle;
            set
            {
                renameTitle = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}