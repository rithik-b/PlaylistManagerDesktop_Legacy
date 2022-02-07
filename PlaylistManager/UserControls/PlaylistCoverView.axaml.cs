using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Types;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.UserControls
{
    public class PlaylistCoverView : UserControl
    {
        public PlaylistCoverView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    
    public class PlaylistCoverViewModel : ViewModelBase
    {
        public readonly IPlaylist playlist;
        private readonly CoverImageLoader? coverImageLoader;
        private Bitmap? coverImage;
        
        public PlaylistCoverViewModel(IPlaylist playlist)
        {
            this.playlist = playlist;
            coverImageLoader = Locator.Current.GetService<CoverImageLoader>();
        }

        public string Title => playlist.Title;
        public string Author => playlist.Author ?? "";

        public Bitmap? CoverImage
        {
            get
            {
                if (coverImage != null)
                {
                    return coverImage;
                }
                _ = LoadCover();
                return coverImageLoader?.LoadingImage;
            }
            set
            {
                coverImage = value;
                NotifyPropertyChanged();
            }
        }
        
        public async Task LoadCover()
        {
            await using var imageStream = playlist.GetCoverStream();
            var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            if (bitmap != null)
            {
                CoverImage = bitmap;
            }
        }
    }
}