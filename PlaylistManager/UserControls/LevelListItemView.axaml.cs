using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Aura.UI.Services;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PlaylistManager.Clipboard;
using PlaylistManager.Models;
using PlaylistManager.Utilities;
using PlaylistManager.Views;
using PlaylistManager.Windows;
using ReactiveUI;
using Splat;
using Difficulty = PlaylistManager.Models.Difficulty;

namespace PlaylistManager.UserControls
{
    public class LevelListItemView : UserControl
    {
        private readonly ContextMenu contextMenu;

        private PlaylistsDetailView? playlistsDetailView;
        
        private PlaylistsDetailView PlaylistsDetailView =>
            (playlistsDetailView ??= Locator.Current.GetService<PlaylistsDetailView>())!;
        
        public LevelListItemView()
        {
            AvaloniaXamlLoader.Load(this);
            AddHandler(DragDrop.DragOverEvent, DragOver!);
            AddHandler(DragDrop.DropEvent, Drop!);
            contextMenu = this.Find<ContextMenu>("ContextMenu");
        }

        #region Drag and Drop

        public const string kPlaylistSongData = "application/com.rithik-b.PlaylistManager.PlaylistSong";
        private bool pointerHeld;

        private async void DoDrag(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            pointerHeld = true;

            await Task.Delay(Utils.kHoldDelay);
            if (!pointerHeld)
            {
                return;
            }

            if (DataContext is LevelListItemViewModel viewModel)
            {
                var dragData = new DataObject();
                dragData.Set(kPlaylistSongData, viewModel);
                var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
            }
        }

        // Tracks if pointer is released to prevent a drag operation
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) => pointerHeld = false;
        
        private void DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(kPlaylistSongData))
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
            if (DataContext is LevelListItemViewModel destination && e.Data.Get(kPlaylistSongData) is LevelListItemViewModel source && source != destination)
            {
                PlaylistsDetailView.ViewModel?.MoveLevel(source, destination);
            }
        }

        #endregion

        private void ContextButtonClick(object? sender, RoutedEventArgs e)
        {
            contextMenu.Open();
        }
    }

    public class LevelListItemViewModel : ViewModelBase
    {
        private const string kBeatSaverURL = "https://beatsaver.com/maps/";
        private const string kRabbitPreviewerIHardlyKnowHer = "https://skystudioapps.com/bs-viewer/?id=";

        public readonly PlaylistSongWrapper playlistSongWrapper;
        private Bitmap? coverImage;
        private CoverImageLoader? coverImageLoader;
        private string? key;
        private string? selectedCharacteristic;
        private List<Difficulty>? difficulties;

        private IClipboardHandler? clipboardHandler;
        private IClipboardHandler ClipboardHandler => clipboardHandler ??=  Locator.Current.GetService<IClipboardHandler>()!;
        
        private PlaylistsDetailView? playlistsDetailView;
        private PlaylistsDetailView PlaylistsDetailView => playlistsDetailView ??= Locator.Current.GetService<PlaylistsDetailView>()!;
        
        private MainWindow? mainWindow;
        private MainWindow MainWindow => mainWindow ??= Locator.Current.GetService<MainWindow>()!;
        
        public LevelListItemViewModel(PlaylistSongWrapper playlistSongWrapper)
        {
            this.playlistSongWrapper = playlistSongWrapper;
            
            foreach (var characteristic in playlistSongWrapper.customLevelData.Difficulties.Keys)
            {
                Characteristics.Add(characteristic);
            }
            
            SelectedCharacteristic = Characteristics.FirstOrDefault();
        }

        public string SongName => $"{playlistSongWrapper.customLevelData.SongName} {playlistSongWrapper.customLevelData.SongSubName}";
        public string AuthorName => $"{playlistSongWrapper.customLevelData.SongAuthorName} [{playlistSongWrapper.customLevelData.LevelAuthorName}]";
        public string? Key
        {
            get
            {
                if (key == null)
                {
                    _ = LoadKeyAsync();
                }
                return key;
            }
            private set
            {
                key = value;
                NotifyPropertyChanged();
            }
        }
        public float Opacity => playlistSongWrapper.customLevelData.Downloaded ? 1f: 0.5f;
        public List<string> Characteristics { get; } = new();
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
            private set
            {
                coverImage = value;
                NotifyPropertyChanged();
            }
        }
        public string? SelectedCharacteristic
        {
            get => selectedCharacteristic;
            set
            {
                selectedCharacteristic = value;
                if (selectedCharacteristic != null && playlistSongWrapper.customLevelData.Difficulties.TryGetValue(selectedCharacteristic, out var difficulties))
                {
                    this.difficulties = difficulties;

                    easyHighlighted = false;
                    normalHighlighted = false;
                    hardHighlighted = false;
                    expertHighlighted = false;
                    expertPlusHighlighted = false;
                    
                    if (playlistSongWrapper.playlistSong.Difficulties != null)
                    {
                        foreach (var difficulty in playlistSongWrapper.playlistSong.Difficulties)
                        {
                            if (difficulty.Characteristic.Equals(SelectedCharacteristic, StringComparison.OrdinalIgnoreCase))
                            {
                                switch (difficulty.GetDifficulty())
                                {
                                    case Difficulty.Easy:
                                        easyHighlighted = true;
                                        break;
                                    case Difficulty.Normal:
                                        normalHighlighted = true;
                                        break;
                                    case Difficulty.Hard:
                                        hardHighlighted = true;
                                        break;
                                    case Difficulty.Expert:
                                        expertHighlighted = true;
                                        break;
                                    case Difficulty.ExpertPlus:
                                        expertPlusHighlighted = true;
                                        break;
                                }
                            }
                        }
                    }
                }
                
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsEasy));
                NotifyPropertyChanged(nameof(IsNormal));
                NotifyPropertyChanged(nameof(IsHard));
                NotifyPropertyChanged(nameof(IsExpert));
                NotifyPropertyChanged(nameof(IsExpertPlus));
                
                NotifyPropertyChanged(nameof(EasyBackground));
                NotifyPropertyChanged(nameof(NormalBackground));
                NotifyPropertyChanged(nameof(HardBackground));
                NotifyPropertyChanged(nameof(ExpertBackground));
                NotifyPropertyChanged(nameof(ExpertPlusBackground));
            }
        }
        public bool MultipleCharacteristics => playlistSongWrapper.customLevelData.Difficulties.Keys.Count > 1;

        #region Difficulties
        
        private const string kEasyBackground = "#6067AC5B";
        private const string kNormalBackground = "#604795EC";
        private const string kHardBackground = "#60F19C38";
        private const string kExpertBackground = "#60E15241";
        private const string kExpertPlusBackground = "#60BF40BF";
        
        private const string kEasyHighlightedBackground = "#FF67AC5B";
        private const string kNormalHighlightedBackground = "#FF4795EC";
        private const string kHardHighlightedBackground = "#FFF19C38";
        private const string kExpertHighlightedBackground = "#FFE15241";
        private const string kExpertPlusHighlightedBackground = "#FFBF40BF";

        public bool IsEasy => difficulties != null && difficulties.Contains(Difficulty.Easy);
        public bool IsNormal => difficulties != null && difficulties.Contains(Difficulty.Normal);
        public bool IsHard => difficulties != null && difficulties.Contains(Difficulty.Hard);
        public bool IsExpert => difficulties != null && difficulties.Contains(Difficulty.Expert);
        public bool IsExpertPlus => difficulties != null && difficulties.Contains(Difficulty.ExpertPlus);
        
        private bool easyHighlighted;
        private bool EasyHighlighted
        {
            get => easyHighlighted;
            set
            {
                if (easyHighlighted != value)
                {
                    easyHighlighted = value;
                    if (value)
                    {
                        HighlightDifficulty(Difficulty.Easy);
                    }
                    else
                    {
                        UnHighlightDifficulty(Difficulty.Easy);
                    }
                    NotifyPropertyChanged(nameof(EasyBackground));
                }
            }
        }
        
        private bool normalHighlighted;
        private bool NormalHighlighted
        {
            get => normalHighlighted;
            set
            {
                if (normalHighlighted != value)
                {
                    normalHighlighted = value;
                    if (value)
                    {
                        HighlightDifficulty(Difficulty.Normal);
                    }
                    else
                    {
                        UnHighlightDifficulty(Difficulty.Normal);
                    }
                    NotifyPropertyChanged(nameof(NormalBackground));
                }
            }
        }
        
        private bool hardHighlighted;
        private bool HardHighlighted
        {
            get => hardHighlighted;
            set
            {
                if (hardHighlighted != value)
                {
                    hardHighlighted = value;
                    if (value)
                    {
                        HighlightDifficulty(Difficulty.Hard);
                    }
                    else
                    {
                        UnHighlightDifficulty(Difficulty.Hard);
                    }
                    NotifyPropertyChanged(nameof(HardBackground));
                }
            }
        }
        
        private bool expertHighlighted;
        private bool ExpertHighlighted
        {
            get => expertHighlighted;
            set
            {
                if (expertHighlighted != value)
                {
                    expertHighlighted = value;
                    if (value)
                    {
                        HighlightDifficulty(Difficulty.Expert);
                    }
                    else
                    {
                        UnHighlightDifficulty(Difficulty.Expert);
                    }
                    NotifyPropertyChanged(nameof(ExpertBackground));
                }
            }
        }
        
        private bool expertPlusHighlighted;
        private bool ExpertPlusHighlighted
        {
            get => expertPlusHighlighted;
            set
            {
                if (expertPlusHighlighted != value)
                {
                    expertPlusHighlighted = value;
                    if (value)
                    {
                        HighlightDifficulty(Difficulty.ExpertPlus);
                    }
                    else
                    {
                        UnHighlightDifficulty(Difficulty.ExpertPlus);
                    }
                    NotifyPropertyChanged(nameof(ExpertPlusBackground));
                }
            }
        }

        public string EasyBackground => EasyHighlighted ? kEasyHighlightedBackground : kEasyBackground;
        public string NormalBackground => NormalHighlighted ? kNormalHighlightedBackground : kNormalBackground;
        public string HardBackground => HardHighlighted ? kHardHighlightedBackground : kHardBackground;
        public string ExpertBackground => ExpertHighlighted ? kExpertHighlightedBackground : kExpertBackground;
        public string ExpertPlusBackground => ExpertPlusHighlighted ? kExpertPlusHighlightedBackground :  kExpertPlusBackground;

        private void ToggleEasy() => EasyHighlighted = !EasyHighlighted; 
        private void ToggleNormal() => NormalHighlighted = !NormalHighlighted; 
        private void ToggleHard() => HardHighlighted = !HardHighlighted; 
        private void ToggleExpert() => ExpertHighlighted = !ExpertHighlighted; 
        private void ToggleExpertPlus() => ExpertPlusHighlighted = !ExpertPlusHighlighted;

        private void HighlightDifficulty(Difficulty difficulty)
        {
            if (SelectedCharacteristic != null)
            {
                playlistSongWrapper.playlistSong.Difficulties ??= new List<BeatSaberPlaylistsLib.Types.Difficulty>();
                var diffToHighlight = new BeatSaberPlaylistsLib.Types.Difficulty
                {
                    Characteristic = SelectedCharacteristic,
                    Name = difficulty.ToString()
                };
                playlistSongWrapper.playlistSong.Difficulties.Add(diffToHighlight);
            }
        }
        
        private void UnHighlightDifficulty(Difficulty difficulty)
        {
            if (playlistSongWrapper.playlistSong.Difficulties != null && SelectedCharacteristic != null)
            {
                var diffToDelete = playlistSongWrapper.playlistSong.Difficulties.FindIndex(d =>
                    d.Characteristic.Equals(SelectedCharacteristic, StringComparison.OrdinalIgnoreCase) &&
                    d.Name.Equals(difficulty.ToString(), StringComparison.OrdinalIgnoreCase));
                if (diffToDelete != -1)
                {
                    playlistSongWrapper.playlistSong.Difficulties.RemoveAt(diffToDelete);
                }
            }
        }

        #endregion

        #region Action Buttons

        private void OpenBeatSaver() => Utils.OpenURL(kBeatSaverURL + Key);
        
        private void OpenPreview() => Utils.OpenURL(kRabbitPreviewerIHardlyKnowHer + Key);

        public async Task Cut()
        {
            if (PlaylistsDetailView.ViewModel != null)
            {
                var playlistSongs = new List<PlaylistSongWrapper>();
                var selectedItems = PlaylistsDetailView.ViewModel.SelectedLevels.ToArray();
                
                foreach (var selectedLevel in selectedItems)
                {
                    playlistSongs.Add(selectedLevel.playlistSongWrapper);
                    PlaylistsDetailView.ViewModel.Levels.Remove(selectedLevel);
                }
                
                PlaylistsDetailView.ViewModel.UpdateNumSongs();
                await ClipboardHandler.Copy(playlistSongs);
            }
        }

        public async Task Copy()
        {
            if (PlaylistsDetailView.ViewModel != null)
            {
                var playlistSongs = new List<PlaylistSongWrapper>();
                foreach (var selectedLevel in PlaylistsDetailView.ViewModel.SelectedLevels)
                {
                    playlistSongs.Add(selectedLevel.playlistSongWrapper);
                }

                await ClipboardHandler.Copy(playlistSongs);
            }
        }
        
        public void Remove()
        {
            var removeMessage = GetRemoveMessage();
            if (PlaylistsDetailView.ViewModel != null && removeMessage != null)
            {
                MainWindow.NewContentDialog(removeMessage, (sender, e) =>
                {
                    var selectedItems = PlaylistsDetailView.ViewModel.SelectedLevels.ToArray();

                    foreach (var level in selectedItems)
                    {
                        PlaylistsDetailView.ViewModel.playlist.Remove(level.playlistSongWrapper.playlistSong);
                        PlaylistsDetailView.ViewModel.Levels.Remove(level);
                    }
                
                    PlaylistsDetailView.ViewModel.UpdateNumSongs();
                }, null, "Yes", "No");
            }
        }

        private string? GetRemoveMessage()
        {
            if (PlaylistsDetailView.ViewModel != null)
            {
                var numLevels = PlaylistsDetailView.ViewModel.SelectedLevels.Count;
                return numLevels > 0 ? $"Are you sure you want to remove {numLevels} level{(numLevels != 1 ? "s" : "")} from the playlist?" : null;
            }
            return null;
        }
        
        #endregion

        private async Task LoadKeyAsync()
        {
            var key = await playlistSongWrapper.customLevelData.GetKeyAsync();
            if (key != null)
            {
                RxApp.MainThreadScheduler.Schedule(() => Key = key);
            }
        }
        
        private async Task LoadCoverAsync()
        {
            var bitmap = await playlistSongWrapper.customLevelData.GetCoverImageAsync();
            if (bitmap != null)
            {
                RxApp.MainThreadScheduler.Schedule(() => CoverImage = bitmap);
            }
        }
    }
}