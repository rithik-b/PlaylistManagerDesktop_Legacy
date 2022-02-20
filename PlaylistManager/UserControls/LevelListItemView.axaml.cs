using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PlaylistManager.Models;
using PlaylistManager.Utilities;
using PlaylistManager.Views;
using ReactiveUI;
using Splat;
using Difficulty = PlaylistManager.Models.Difficulty;

namespace PlaylistManager.UserControls
{
    public class LevelListItemView : UserControl
    {
        private readonly ContextMenu contextMenu;

        private PlaylistsDetailView? playlistsDetailView;

        private PlaylistsDetailView? PlaylistsDetailView =>
            playlistsDetailView ??= Locator.Current.GetService<PlaylistsDetailView>();
        
        public LevelListItemView()
        {
            InitializeComponent();
            contextMenu = this.Find<ContextMenu>("ContextMenu");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            AddHandler(DragDrop.DragOverEvent, DragOver!);
            AddHandler(DragDrop.DropEvent, Drop!);
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
                PlaylistsDetailView?.ViewModel?.MoveLevel(source, destination);
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
        private const string kRabbitPreviewerIHardlyKnowHer = "https://skystudioapps.com/bs-viewer/?id=";

        public readonly PlaylistSongWrapper playlistSong;
        private Bitmap? coverImage;
        private CoverImageLoader? coverImageLoader;
        private string? key;
        private string? selectedCharacteristic;
        private List<Difficulty>? difficulties;
        
        public LevelListItemViewModel(PlaylistSongWrapper playlistSong)
        {
            this.playlistSong = playlistSong;
            
            foreach (var characteristic in playlistSong.customLevelData.Difficulties.Keys)
            {
                Characteristics.Add(characteristic);
            }
            
            SelectedCharacteristic = Characteristics.FirstOrDefault();
        }

        public string SongName => $"{playlistSong.customLevelData.SongName} {playlistSong.customLevelData.SongSubName}";
        public string AuthorName => $"{playlistSong.customLevelData.SongAuthorName} [{playlistSong.customLevelData.LevelAuthorName}]";
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
        public float Opacity => playlistSong.customLevelData.Downloaded ? 1f: 0.5f;
        public List<string> Characteristics { get; } = new();
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
                if (selectedCharacteristic != null && playlistSong.customLevelData.Difficulties.TryGetValue(selectedCharacteristic, out var difficulties))
                {
                    this.difficulties = difficulties;

                    easyHighlighted = false;
                    normalHighlighted = false;
                    hardHighlighted = false;
                    expertHighlighted = false;
                    expertPlusHighlighted = false;
                    
                    if (playlistSong.playlistSong.Difficulties != null)
                    {
                        foreach (var difficulty in playlistSong.playlistSong.Difficulties)
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
        public bool MultipleCharacteristics => playlistSong.customLevelData.Difficulties.Keys.Count > 1;

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
                playlistSong.playlistSong.Difficulties ??= new List<BeatSaberPlaylistsLib.Types.Difficulty>();
                var diffToHighlight = new BeatSaberPlaylistsLib.Types.Difficulty
                {
                    Characteristic = SelectedCharacteristic,
                    Name = difficulty.ToString()
                };
                playlistSong.playlistSong.Difficulties.Add(diffToHighlight);
            }
        }
        
        private void UnHighlightDifficulty(Difficulty difficulty)
        {
            if (playlistSong.playlistSong.Difficulties != null && SelectedCharacteristic != null)
            {
                var diffToDelete = playlistSong.playlistSong.Difficulties.FindIndex(d =>
                    d.Characteristic.Equals(SelectedCharacteristic, StringComparison.OrdinalIgnoreCase) &&
                    d.Name.Equals(difficulty.ToString(), StringComparison.OrdinalIgnoreCase));
                if (diffToDelete != -1)
                {
                    playlistSong.playlistSong.Difficulties.RemoveAt(diffToDelete);
                }
            }
        }

        #endregion

        #region Action Buttons

        private void RemoveLevel()
        {
            // TODO: Show popup before deletion
            var detailView = Locator.Current.GetService<PlaylistsDetailView>();
            if (detailView is {ViewModel: { }})
            {
                var viewModel = detailView.ViewModel;
                viewModel.playlist.Remove(playlistSong.playlistSong);
                viewModel.Levels.Remove(this);
                viewModel.UpdateNumSongs();
            }
        }
        
        private void OpenPreview() => Utils.OpenBrowser(kRabbitPreviewerIHardlyKnowHer + Key);

        #endregion

        private async Task LoadKeyAsync()
        {
            var key = await playlistSong.customLevelData.GetKeyAsync();
            if (key != null)
            {
                RxApp.MainThreadScheduler.Schedule(() => Key = key);
            }
        }
        
        private async Task LoadCoverAsync()
        {
            var bitmap = await playlistSong.customLevelData.GetCoverImageAsync();
            if (bitmap != null)
            {
                RxApp.MainThreadScheduler.Schedule(() => CoverImage = bitmap);
            }
        }
    }
}