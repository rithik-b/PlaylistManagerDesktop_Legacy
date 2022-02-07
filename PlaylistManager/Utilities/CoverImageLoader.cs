using System.IO;
using System.Reflection;
using Avalonia.Media.Imaging;

namespace PlaylistManager.Utilities
{
    public class CoverImageLoader
    {
        private readonly Assembly assembly;
        private const string kLoadingPath = "PlaylistManager.Icons.LoadingIcon.png";
        private const string kFolderPath = "PlaylistManager.Icons.FolderIcon.png";
        private Bitmap? loadingImage;
        private Bitmap? folderImage;
        
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
                using Stream? imageStream = assembly.GetManifestResourceStream(kLoadingPath);
                loadingImage = Bitmap.DecodeToWidth(imageStream, 512);
                return loadingImage;
            }
        }
        
        public Bitmap FolderImage
        {
            get
            {
                if (folderImage != null)
                {
                    return folderImage;
                }
                using Stream? imageStream = assembly.GetManifestResourceStream(kFolderPath);
                folderImage = Bitmap.DecodeToWidth(imageStream, 512);
                return folderImage;
            }
        }
    }
}