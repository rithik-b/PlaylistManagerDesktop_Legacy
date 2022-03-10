using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Aura.UI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BeatSaberPlaylistsLib.Types;
using PlaylistManager.Clipboard;
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
        private NavigationPanel NavigationPanel => navigationPanel ??= Locator.Current.GetService<NavigationPanel>("PlaylistsTab")!;

        private MainWindow? mainWindow;
        private MainWindow MainWindow => mainWindow ??= Locator.Current.GetService<MainWindow>()!;
        
        private LevelSearchWindow? levelSearchWindow;
        private LevelSearchWindow LevelSearchWindow => levelSearchWindow ??= Locator.Current.GetService<LevelSearchWindow>()!;
        
        private PlaylistEditWindow? playlistEditWindow;
        private PlaylistEditWindow PlaylistEditWindow => playlistEditWindow ??= Locator.Current.GetService<PlaylistEditWindow>()!;
        
        private IClipboardHandler? clipboardHandler;
        private IClipboardHandler ClipboardHandler => clipboardHandler ??= Locator.Current.GetService<IClipboardHandler>()!;
        
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
        private readonly ListBox listBox;
        
        public PlaylistsDetailView()
        {
            AvaloniaXamlLoader.Load(this);
            floatingButtonBar = this.FindControl<FloatingButtonBar>("FloatingButtonBar");
            listBox = this.Find<ListBox>("ListBox");
            listBox.AddHandler(DragDrop.DragOverEvent, DragOverList!);
#if DEBUG
            var utils = Locator.Current.GetService<PlaylistLibUtils>()!;
            var playlist = utils.PlaylistManager.GetPlaylist("monterwook_s_speed_practice.json");
            ViewModel = new PlaylistsDetailViewModel(playlist!, utils.PlaylistManager);
#endif
        }

        private void DragOverList(object sender, DragEventArgs e)
        {
            if (e.GetPosition(listBox).Y <= 40)
            {
                Scroll(-0.2);
            }
            else if(listBox.Scroll != null && (e.GetPosition(listBox).Y / listBox.Scroll.Viewport.Height) >= 70)
            {
                Scroll(0.2);
            }
        }

        public void Scroll(double offset)
        {
            if (listBox.Scroll != null)
            {
                var x = listBox.Scroll.Offset.X;
                var y = listBox.Scroll.Offset.Y;
                listBox.Scroll.Offset = new Vector(x, y + offset);
            }
        }

        private void BackClick(object? sender, RoutedEventArgs e)
        {
            ViewModel?.Save();
            NavigationPanel.Pop();
        }

        private async void AddClick(object? sender, RoutedEventArgs? e)
        {
            if (ViewModel != null)
            {
                var searchedSong = await LevelSearchWindow.SearchSong(MainWindow);
                if (searchedSong is {level: { }})
                {
                    var levelToAdd = searchedSong.level;
                    var playlistSong = ViewModel.playlist.Add(levelToAdd.Hash, levelToAdd.SongName, await levelToAdd.GetKeyAsync(),
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
        
        private async void EditClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await PlaylistEditWindow.EditPlaylist(MainWindow, ViewModel.playlist, ViewModel.parentManager);
                ViewModel.UpdateMetadata();
            }
        }

        private void LoseFocus(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.SelectedLevel = null;
            }
            Focus();
        }

        private async void OnCellShortcut(object? sender, KeyEventArgs e)
        {
            if (ViewModel is {SelectedLevel: { }})
            {
                if (e.KeyModifiers == KeyModifiers.Control)
                {
                    switch (e.Key)
                    {
                        case Key.X:
                            await ViewModel.SelectedLevel.Cut();
                            break;
                        case Key.C:
                            await ViewModel.SelectedLevel.Copy();
                            break;
                    }
                }
                else if (e.Key == Key.Delete)
                {
                    ViewModel.SelectedLevel.Remove();
                }
            }
        }
        
        private void OnShortcut(object? sender, KeyEventArgs e)
        {
            if (e.KeyModifiers == KeyModifiers.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        AddClick(this, null);
                        break;
                    case Key.V:
                        PasteClick(this, null);
                        break;
                }
            }
        }
        
        private async void PasteClick(object? sender, RoutedEventArgs? e)
        {
            var playlistSongWrappers = await ClipboardHandler.PastePlaylistSongWrappers();
            if (playlistSongWrappers != null && ViewModel != null)
            {
                listBox.Selection.Clear();
                foreach (var playlistSongWrapper in playlistSongWrappers)
                {
                    ViewModel.playlist.Add(playlistSongWrapper.playlistSong);
                    var cell = new LevelListItemViewModel(playlistSongWrapper);
                    ViewModel.Levels.Add(cell);
                    listBox.Selection.Select(ViewModel.Levels.Count - 1);
                }
                ViewModel.UpdateNumSongs();
            }
        }
    }

    public class PlaylistsDetailViewModel : ViewModelBase
    {
        public readonly IPlaylist playlist;
        public readonly BeatSaberPlaylistsLib.PlaylistManager parentManager;
        private readonly LevelMatcher levelMatcher;
        private bool songsLoaded;
        private Bitmap? coverImage;
        private CoverImageLoader? coverImageLoader;
        
        public PlaylistsDetailViewModel(IPlaylist playlist, BeatSaberPlaylistsLib.PlaylistManager parentManager)
        {
            this.playlist = playlist;
            this.parentManager = parentManager;
            levelMatcher = Locator.Current.GetService<LevelMatcher>()!;
            _ = FetchSongs();
        }

        public string Title => playlist.Title;
        public string Author => string.IsNullOrWhiteSpace(playlist.Author) ? "Unknown" : playlist.Author;
        public string? Description => playlist.Description;
        public int OwnedSongs => Levels.Count(l => l.playlistSongWrapper.Downloaded);
        public string NumSongs => $"{playlist.Count} song{(playlist.Count != 1 ? "s" : "")} {(songsLoaded ? $"({OwnedSongs} downloaded)" : "")}";
        private bool DownloadableLevelsExist => Levels.Any(x => !x.Downloaded && x.Key != null);
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

        public List<LevelListItemViewModel> SelectedLevels { get; } = new();

        public Bitmap? CoverImage
        {
            get
            {
                if (coverImage != null)
                {
                    return coverImage;
                }
                coverImageLoader ??= Locator.Current.GetService<CoverImageLoader>()!;
                _ = LoadCoverAsync();
                return coverImageLoader.LoadingImage;
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

        public void UpdateMetadata()
        {
            NotifyPropertyChanged(nameof(Title));
            NotifyPropertyChanged(nameof(Author));
            NotifyPropertyChanged(nameof(Description));
            _ = LoadCoverAsync();
        }

        public void Save() => parentManager.StorePlaylist(playlist);

        public void UpdateNumSongs()
        {
            NotifyPropertyChanged(nameof(NumSongs));
            NotifyPropertyChanged(nameof(DownloadableLevelsExist));
        }

        private async Task FetchSongs()
        {
            foreach (var playlistSong in playlist)
            {
                if (playlistSong.TryGetIdentifierForPlaylistSong(out var identifier, out var identifierType))
                {
                    var levelData = identifierType == Identifier.Hash ? await levelMatcher.GetLevelByHash(identifier) :
                        identifierType == Identifier.Key ? await levelMatcher.GetLevelByKey(identifier) : null;
                    if (levelData != null)
                    {
                        Levels.Add(new LevelListItemViewModel(new PlaylistSongWrapper(playlistSong, levelData)));
                    }
                    else
                    {
                        Levels.Add(new LevelListItemViewModel(new PlaylistSongWrapper(playlistSong)));
                    }
                }
            }
            songsLoaded = true;
            NotifyPropertyChanged(nameof(SongsLoading));
            UpdateNumSongs();
        }
        
        public void MoveLevel(LevelListItemViewModel source, LevelListItemViewModel destination)
        {
            playlist.Remove(source.playlistSongWrapper.playlistSong);
            playlist.Insert(playlist.IndexOf(destination.playlistSongWrapper.playlistSong), source.playlistSongWrapper.playlistSong);
            Levels.Move(Levels.IndexOf(source), Levels.IndexOf(destination));
            SelectedLevels.Clear();
            SelectedLevel = source;
        }

        #region Download
        
        private double downloadProgress;
        private double DownloadProgress
        {
            get => downloadProgress;
            set
            {
                downloadProgress = value;
                NotifyPropertyChanged();
            }
        }

        private bool isDownloading;
        private bool IsDownloading
        {
            get => isDownloading;
            set
            {
                isDownloading = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(DownloadTip));
            }
        }

        private string DownloadTip => IsDownloading ? "Stop download" : "Download all levels";
        
        private bool isIndeterminate;
        private bool IsIndeterminate
        {
            get => isIndeterminate;
            set
            {
                isIndeterminate = value;
                NotifyPropertyChanged();
            }
        }

        private async Task ToggleDownloadLevels()
        {
            if (IsDownloading)
            {
                IsDownloading = false;
                foreach (var level in Levels)
                {
                    if (!level.Downloaded && !level.IsDownloading && await level.playlistSongWrapper.GetKeyAsync() != null)
                    {
                        level.IsIndeterminate = false;
                    }
                    else if (level.IsDownloading)
                    {
                        
                        level.IsIndeterminate = false;
                        await level.ToggleDownload();
                    }
                }
                return;
            }

            IsDownloading = true;
            IsIndeterminate = true;
            DownloadProgress = 0;
            
            foreach (var level in Levels)
            {
                if (!level.Downloaded && !level.IsDownloading && await level.playlistSongWrapper.GetKeyAsync() != null)
                {
                    level.IsIndeterminate = true;
                }
            }

            IsIndeterminate = false;

            for (int i = 0; i < Levels.Count; i++)
            {
                var level = Levels[i];
                if (!level.Downloaded && !level.IsDownloading && level.IsIndeterminate)
                {
                    await level.ToggleDownload();
                }
                DownloadProgress = (float) i / Levels.Count;
            }

            IsDownloading = false;
        }

        #endregion

        #region Sync

        private bool IsSyncable => playlist.TryGetCustomData("syncURL", out var _);

        private async Task SyncPlaylist()
        {
            songsLoaded = false;
            NotifyPropertyChanged(nameof(SongsLoading));
            await playlist.Sync(parentManager);
            Levels.Clear();
            _ = FetchSongs();
        }

        #endregion
    }
}