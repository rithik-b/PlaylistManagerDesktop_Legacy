using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;
using PlaylistManager.Utilities;
using ReactiveUI;
using Splat;

namespace PlaylistManager.Windows
{
    public class PlaylistEditWindow : Window
    {
        public PlaylistEditWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Closing += (sender, args) =>
            {
                args.Cancel = true;
                Hide();
            };
        }

        public void EditPlaylist(Window parent, IPlaylist playlist)
        {
            DataContext = new PlaylistEditWindowModel(playlist);
            _ = ShowDialog(parent);
        }
    }

    public class PlaylistEditWindowModel : ViewModelBase
    {
        public readonly IPlaylist playlist;
        private Bitmap? coverImage;
        private CoverImageLoader? coverImageLoader;
        
        public PlaylistEditWindowModel(IPlaylist playlist)
        {
            this.playlist = playlist;
        }
        
        public string Title => playlist.Title;
        public string Author => playlist.Author ?? "Unknown";
        public string? Description => playlist.Description;
        
        public Bitmap? CoverImage
        {
            get
            {
                if (coverImage != null)
                {
                    return coverImage;
                }
                coverImageLoader ??= Locator.Current.GetService<CoverImageLoader>();
                _ = LoadCoverAsync();
                return coverImageLoader?.LoadingImage;
            }
            set
            {
                coverImage = value;
                NotifyPropertyChanged();
            }
        }
        
        private async Task LoadCoverAsync()
        {
            await using var imageStream = playlist.GetCoverStream();
            var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 512));
            if (bitmap != null)
            {
                RxApp.MainThreadScheduler.Schedule(() => CoverImage = bitmap);
            }
        }
    }
}