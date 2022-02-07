using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;

namespace PlaylistManager.Utilities
{
    public class CoverImageLoader
    {
        private readonly Assembly assembly;
        private const string LOADING_PATH = "PlaylistManager.Icons.LoadingIcon.png";
        private Bitmap? loadingImage;
        
        public CoverImageLoader(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public Bitmap LoadingImage
        {
            get
            {
                if (loadingImage != null)
                {
                    return loadingImage;
                }
                using Stream? imageStream = assembly.GetManifestResourceStream(LOADING_PATH);
                loadingImage = Bitmap.DecodeToWidth(imageStream, 512);
                return loadingImage;
            }
        }
    }
}