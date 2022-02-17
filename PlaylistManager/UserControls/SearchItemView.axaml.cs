using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PlaylistManager.Models;
using PlaylistManager.Utilities;
using ReactiveUI;
using Splat;

namespace PlaylistManager.UserControls
{
    public class SearchItemView : UserControl
    {
        public SearchItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    public class SearchItemViewModel : ViewModelBase
    {
        public readonly ICustomLevelData? level;
        
        public string Name { get; }
        public string SubName { get; }
        
        private Bitmap? image;
        private bool HasImage => imageFactory != null;
        private readonly Task<Bitmap?>? imageFactory;
        private readonly CancellationTokenSource? loadingTokenSource;
        private CoverImageLoader? coverImageLoader;
        public Bitmap? Image
        {
            get
            {
                if (image != null)
                {
                    return image;
                }
                coverImageLoader ??= Locator.Current.GetService<CoverImageLoader>();
                _ = LoadImageAsync();
                return coverImageLoader?.LoadingImage;
            }
            private set
            {
                image = value;
                NotifyPropertyChanged();
            }
        }
        
        public SearchItemViewModel(string name, string subName, Task<Bitmap?>? imageFactory = null)
        {
            Name = name;
            SubName = subName;
            this.imageFactory = imageFactory;
        }

        public SearchItemViewModel(ICustomLevelData level)
        {
            Name = $"{level.SongName} {level.SongSubName}";
            SubName = $"{level.SongAuthorName} [{level.LevelAuthorName}]";
            this.level = level;
            loadingTokenSource = new CancellationTokenSource();
            imageFactory = level.GetCoverImageAsync(loadingTokenSource.Token);
        }

        private async Task LoadImageAsync()
        {
            if (imageFactory != null)
            {
                var bitmap = await imageFactory;
                if (bitmap != null)
                {
                    RxApp.MainThreadScheduler.Schedule(() => Image = bitmap);
                }
            }
        }

        // Finalizer
        ~SearchItemViewModel() => loadingTokenSource?.Cancel();
    }
}