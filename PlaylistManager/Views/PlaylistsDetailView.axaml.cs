using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;
using ReactiveUI;
using Splat;

namespace PlaylistManager.Views
{
    public class PlaylistsDetailView : UserControl
    {
        private NavigationPanel? navigationPanel;
        private NavigationPanel? NavigationPanel => navigationPanel ??= Locator.Current.GetService<NavigationPanel>("PlaylistsTab");

        private PlaylistsDetailViewModel? viewModel;

        public PlaylistsDetailViewModel? ViewModel
        {
            get => viewModel;
            set
            {
                viewModel = value;
                DataContext = value;
            }
        }
        
        public PlaylistsDetailView()
        {
            InitializeComponent();
#if DEBUG
            var utils = Locator.Current.GetService<PlaylistLibUtils>();
            var playlist = utils?.PlaylistManager.GetPlaylist("monterwook_s_speed_practice.json");
            ViewModel = new PlaylistsDetailViewModel(playlist!);
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnBackClick(object? sender, RoutedEventArgs e) => NavigationPanel?.Pop();
    }

    public class PlaylistsDetailViewModel : ViewModelBase
    {
        private readonly IPlaylist playlist;
        private readonly LevelLookup? levelLookup;
        private int ownedSongs;
        private bool songsLoaded;
        private Bitmap? coverImage;
        private CoverImageLoader? coverImageLoader;
        
        public PlaylistsDetailViewModel(IPlaylist playlist)
        {
            this.playlist = playlist;
            levelLookup = Locator.Current.GetService<LevelLookup>();
            _ = FetchSongs();
        }

        public string Title => playlist.Title;
        public string Author => playlist.Author ?? "Unknown";
        public string? Description => playlist.Description;
        public string NumSongs => $"{playlist.Count} song{(playlist.Count != 1 ? "s" : "")} {(songsLoaded ? $"({ownedSongs} owned)" : "")}";
        public ObservableCollection<LevelListItemViewModel> Levels { get; } = new();

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

        private async Task FetchSongs()
        {
            if (levelLookup != null)
            {
                foreach (var playlistSong in playlist)
                {
                    if (playlistSong.TryGetIdentifierForPlaylistSong(out var identifier, out var identifierType))
                    {
                        var levelData = identifierType == Identifier.Hash ? await levelLookup.GetLevelByHash(identifier!) :
                                identifierType == Identifier.Key ? await levelLookup.GetLevelByKey(identifier!) : null;
                        if (levelData != null)
                        {
                            Levels.Add(new LevelListItemViewModel(new PlaylistSongWrapper(playlistSong, levelData)));
                            if (levelData.Downloaded)
                            {
                                ownedSongs++;
                            }
                        }
                    }
                }
                songsLoaded = true;
                NotifyPropertyChanged(nameof(NumSongs));
            }
        }
    }
}