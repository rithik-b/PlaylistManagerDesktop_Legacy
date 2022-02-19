using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Aura.UI.Controls;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Models;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;
using PlaylistManager.Windows;
using ReactiveUI;
using Splat;

namespace PlaylistManager.Views
{
    public class PlaylistsDetailView : UserControl
    {
        private NavigationPanel? navigationPanel;
        private NavigationPanel? NavigationPanel => navigationPanel ??= Locator.Current.GetService<NavigationPanel>("PlaylistsTab");

        private MainWindow? mainWindow;
        private MainWindow? MainWindow => mainWindow ??= Locator.Current.GetService<MainWindow>();
        
        private LevelSearchWindow? levelSearchWindow;
        private LevelSearchWindow? LevelSearchWindow => levelSearchWindow ??= Locator.Current.GetService<LevelSearchWindow>();
        
                
        private PlaylistEditWindow? playlistEditWindow;
        private PlaylistEditWindow? PlaylistEditWindow => playlistEditWindow ??= Locator.Current.GetService<PlaylistEditWindow>();
        
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

        private readonly FloatingButtonBar floatingButtonBar;
        
        public PlaylistsDetailView()
        {
            InitializeComponent();
            floatingButtonBar = this.FindControl<FloatingButtonBar>("FloatingButtonBar");
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

        private async void OnAddClick(object? sender, RoutedEventArgs e)
        {
            if (LevelSearchWindow != null && MainWindow != null && ViewModel != null)
            {
                var searchedSong = await LevelSearchWindow.SearchSong(MainWindow);
                if (searchedSong is {level: { }})
                {
                    var levelToAdd = searchedSong.level;
                    var playlistSong = ViewModel.playlist.Add(levelToAdd.Hash, levelToAdd.SongName, levelToAdd.Key,
                        levelToAdd.LevelAuthorName);
                    if (playlistSong != null)
                    {
                        var modelToAdd = new LevelListItemViewModel(new PlaylistSongWrapper(playlistSong, levelToAdd));
                        ViewModel.Levels.Add(modelToAdd);
                        ViewModel.SelectedLevel = modelToAdd;   
                        ViewModel.UpdateNumSongs();
                    }
                }
            }
        }
        
        private void OnEditClick(object? sender, RoutedEventArgs e)
        {
            if (MainWindow != null && viewModel != null)
            {
                PlaylistEditWindow?.EditPlaylist(MainWindow, viewModel.playlist);
            }
        }

        private void FloatingBarHoverStart(object? sender, PointerEventArgs e)
        {
            floatingButtonBar.IsExpanded = true;
        }

        private void FloatingBarHoverLeave(object? sender, PointerEventArgs e)
        {
            floatingButtonBar.IsExpanded = false;
        }
    }

    public class PlaylistsDetailViewModel : ViewModelBase
    {
        public readonly IPlaylist playlist;
        private readonly LevelMatcher? levelMatcher;
        private bool songsLoaded;
        private Bitmap? coverImage;
        private CoverImageLoader? coverImageLoader;
        
        public PlaylistsDetailViewModel(IPlaylist playlist)
        {
            this.playlist = playlist;
            levelMatcher = Locator.Current.GetService<LevelMatcher>();
            _ = FetchSongs();
        }

        public string Title => playlist.Title;
        public string Author => playlist.Author ?? "Unknown";
        public string? Description => playlist.Description;
        public int OwnedSongs => Levels.Count(l => l.playlistSong.customLevelData.Downloaded);
        public string NumSongs => $"{playlist.Count} song{(playlist.Count != 1 ? "s" : "")} {(songsLoaded ? $"({OwnedSongs} owned)" : "")}";
        public bool SongsLoading => !songsLoaded;
        public ObservableCollection<LevelListItemViewModel> Levels { get; } = new();

        private LevelListItemViewModel? selectedLevel;
        public LevelListItemViewModel? SelectedLevel
        {
            get => selectedLevel;
            set
            {
                selectedLevel = value;
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
        
        private async Task LoadCoverAsync()
        {
            await using var imageStream = playlist.GetCoverStream();
            var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 512));
            if (bitmap != null)
            {
                RxApp.MainThreadScheduler.Schedule(() => CoverImage = bitmap);
            }
        }
        
        public void UpdateNumSongs() => NotifyPropertyChanged(nameof(NumSongs));

        private async Task FetchSongs()
        {
            if (levelMatcher != null)
            {
                foreach (var playlistSong in playlist)
                {
                    if (playlistSong.TryGetIdentifierForPlaylistSong(out var identifier, out var identifierType))
                    {
                        var levelData = identifierType == Identifier.Hash ? await levelMatcher.GetLevelByHash(identifier!) :
                                identifierType == Identifier.Key ? await levelMatcher.GetLevelByKey(identifier!) : null;
                        if (levelData != null)
                        {
                            Levels.Add(new LevelListItemViewModel(new PlaylistSongWrapper(playlistSong, levelData)));
                        }
                    }
                }
                songsLoaded = true;
                NotifyPropertyChanged(nameof(SongsLoading));
                UpdateNumSongs();
            }
        }
    }
}