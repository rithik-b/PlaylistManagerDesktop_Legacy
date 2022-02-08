using System;
using System.IO;
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
            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);
        }

        #region Drag and Drop
        
        private const string kPlaylistData = "application/com.rithik-b.PlaylistManager.Playlist";
        
        private async void DoDrag(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var dragData = new DataObject();
            dragData.Set(kPlaylistData, DataContext);
            await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(kPlaylistData) && DataContext is PlaylistCoverViewModel current && !current.isPlaylist
                && current is {playlistManager: { }})
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
            if (e.Data.Contains(kPlaylistData) && DataContext is PlaylistCoverViewModel current && !current.isPlaylist
                && current is {playlistManager: { }})
            {
                e.DragEffects = DragDropEffects.Move;
                if (e.Data.Get(kPlaylistData) is PlaylistCoverViewModel drag)
                {
                    if (drag.isPlaylist && drag is {playlist: { }})
                    {
                        playlistsListView ??= Locator.Current.GetService<PlaylistsListView>();
                        if (playlistsListView is {viewModel: {CurrentManager: { }}})
                        {
                            drag.playlist.MovePlaylist(playlistsListView.viewModel.CurrentManager,current.playlistManager);
                            playlistsListView.viewModel.SearchResults.Remove(drag);
                            await current.LoadPlaylistsAsync();
                        }
                    }
                }
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
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
        
        public bool AllowDrop => !isPlaylist;
        
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