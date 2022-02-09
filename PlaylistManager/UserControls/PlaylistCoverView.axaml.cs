using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
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
    }
    
    public class PlaylistCoverViewModel : ViewModelBase
    {
        public readonly IPlaylist? playlist;
        public readonly BeatSaberPlaylistsLib.PlaylistManager? playlistManager;
        public readonly bool isPlaylist;
        private readonly CoverImageLoader? coverImageLoader;
        private readonly PlaylistLibUtils? playlistLibUtils;
        private int? numPlaylists;
        private Bitmap? coverImage;

        public PlaylistCoverViewModel(IPlaylist playlist)
        {
            this.playlist = playlist;
            isPlaylist = true;
            coverImageLoader = Locator.Current.GetService<CoverImageLoader>();
        }
        
        public PlaylistCoverViewModel(BeatSaberPlaylistsLib.PlaylistManager playlistManager)
        {
            this.playlistManager = playlistManager;
            isPlaylist = false;
            coverImageLoader = Locator.Current.GetService<CoverImageLoader>();
            playlistLibUtils = Locator.Current.GetService<PlaylistLibUtils>();
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
            if (playlistLibUtils != null && playlistManager != null)
            {
                numPlaylists = (await playlistLibUtils.GetPlaylistsAsync(playlistManager)).Length;
                NotifyPropertyChanged(nameof(Author));
            }
        }
    }
}