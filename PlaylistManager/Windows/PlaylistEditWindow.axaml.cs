using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        private readonly SemaphoreSlim openSemaphore;
        private readonly OpenFileDialog openFileDialog;
        private readonly MainWindow? mainWindow;
        private readonly PlaylistLibUtils? playlistLibUtils;
        
        private PlaylistEditWindowModel? viewModel;
        public PlaylistEditWindowModel? ViewModel
        {
            get => viewModel;
            set
            {
                viewModel = value;
                DataContext = value;
            }
        }

        public PlaylistEditWindow(MainWindow mainWindow, PlaylistLibUtils playlistLibUtils) : this()
        {
            this.mainWindow = mainWindow;
            this.playlistLibUtils = playlistLibUtils;
        }
        
        public PlaylistEditWindow()
        {
            AvaloniaXamlLoader.Load(this);
            Closing += (sender, args) =>
            {
                args.Cancel = true;
                openSemaphore?.Release();
            };
#if DEBUG
            var utils = Locator.Current.GetService<PlaylistLibUtils>();
            var playlist = utils?.PlaylistManager.GetPlaylist("monterwook_s_speed_practice.json");
            ViewModel = new PlaylistEditWindowModel(playlist!);
#endif
#if DEBUG
            this.AttachDevTools();
#endif
            openSemaphore = new SemaphoreSlim(0, 1);
            openFileDialog = new OpenFileDialog()
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>()
                {
                    new FileDialogFilter()
                    {
                        Extensions = new List<string>()
                        {
                            "png",
                            "jpg",
                            "jpeg"
                        }
                    }
                }
            };
        }

        public async Task EditPlaylist(Window parent, IPlaylist playlist)
        {
            ViewModel = new PlaylistEditWindowModel(playlist);
            _ = ShowDialog(parent);
            await openSemaphore.WaitAsync();
            Hide();
            playlistLibUtils?.PlaylistManager.RequestRefresh("PlaylistManager (desktop)");
        }

        private async void EditButtonClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null && mainWindow != null)
            {
                var filePaths = await openFileDialog.ShowAsync(mainWindow);
                if (filePaths is {Length: > 0})
                {
                    var filePath = filePaths.First();
                    await using var imageStream = File.Open(filePath, FileMode.Open);
                    ViewModel.playlist.SetCover(imageStream);
                    _ = ViewModel.LoadCoverAsync();
                }
            }
        }

        private void LoseFocus(object? sender, PointerPressedEventArgs e) => Focus();
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

        public string Title
        {
            get => playlist.Title;
            set
            {
                playlist.Title = value;
                NotifyPropertyChanged();
            }
        }
        public string Author
        {
            get => playlist.Author ?? "";
            set
            {
                playlist.Author = value;
                NotifyPropertyChanged();
            }
        }
        public string? Description
        {
            get => playlist.Description;
            set
            {
                playlist.Description = value;
                NotifyPropertyChanged();
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
                _ = LoadCoverAsync();
                return coverImageLoader?.LoadingImage;
            }
            set
            {
                coverImage = value;
                NotifyPropertyChanged();
            }
        }
        
        public async Task LoadCoverAsync()
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