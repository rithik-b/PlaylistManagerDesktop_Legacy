using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PlaylistManager.Models;
using PlaylistManager.Utilities;
using PlaylistManager.Windows;
using ReactiveUI;
using Splat;

namespace PlaylistManager.UserControls
{
    public class LevelListItemView : UserControl
    {
        private const string kRabbitPreviewerIHardlyKnowHer = "https://skystudioapps.com/bs-viewer/?id=";
        
        public LevelListItemView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void PreviewClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is LevelListItemViewModel {Key: { }} viewModel)
            {
                Utils.OpenBrowser(kRabbitPreviewerIHardlyKnowHer + viewModel.Key);
            }
        }
    }

    public class LevelListItemViewModel : ViewModelBase
    {
        private readonly PlaylistSongWrapper playlistSong;
        private Bitmap? coverImage;
        private CoverImageLoader? coverImageLoader;
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
        public string? Key => playlistSong.customLevelData.Key;
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

                    EasyHighlighted = false;
                    NormalHighlighted = false;
                    HardHighlighted = false;
                    ExpertHighlighted = false;
                    ExpertPlusHighlighted = false;
                    
                    if (playlistSong.playlistSong.Difficulties != null)
                    {
                        foreach (var difficulty in playlistSong.playlistSong.Difficulties)
                        {
                            if (difficulty.Characteristic.Equals(SelectedCharacteristic, StringComparison.OrdinalIgnoreCase))
                            {
                                switch (difficulty.DifficultyValue)
                                {
                                    case (int)Difficulty.Easy:
                                        EasyHighlighted = true;
                                        break;
                                    case (int)Difficulty.Normal:
                                        NormalHighlighted = true;
                                        break;
                                    case (int)Difficulty.Hard:
                                        HardHighlighted = true;
                                        break;
                                    case (int)Difficulty.Expert:
                                        ExpertHighlighted = true;
                                        break;
                                    case (int)Difficulty.ExpertPlus:
                                        ExpertPlusHighlighted = true;
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
            }
        }
        public bool MultipleCharacteristics => playlistSong.customLevelData.Difficulties.Keys.Count > 1;

        #region Difficulties
        
        private const string kEasyBackground = "#8067AC5B";
        private const string kNormalBackground = "#804795EC";
        private const string kHardBackground = "#80F19C38";
        private const string kExpertBackground = "#80E15241";
        private const string kExpertPlusBackground = "#808F34AA";
        
        private const string kEasyHighlightedBackground = "#FF67AC5B";
        private const string kNormalHighlightedBackground = "#FF4795EC";
        private const string kHardHighlightedBackground = "#FFF19C38";
        private const string kExpertHighlightedBackground = "#FFE15241";
        private const string kExpertPlusHighlightedBackground = "#FF8F34AA";

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

        private void HighlightDifficulty(Difficulty difficulty)
        {
            if (SelectedCharacteristic != null)
            {
                playlistSong.playlistSong.Difficulties ??= new List<BeatSaberPlaylistsLib.Types.Difficulty>();
                var diffToHighlight = new BeatSaberPlaylistsLib.Types.Difficulty();
                diffToHighlight.Characteristic = SelectedCharacteristic;
                diffToHighlight.Name = difficulty.ToString();
                playlistSong.playlistSong.Difficulties.Add(diffToHighlight);
            }
        }
        
        private void UnHighlightDifficulty(Difficulty difficulty)
        {
            if (playlistSong.playlistSong.Difficulties != null && SelectedCharacteristic != null)
            {
                var diffToDelete = playlistSong.playlistSong.Difficulties.FindIndex(d =>
                    d.Characteristic.Equals(SelectedCharacteristic, StringComparison.OrdinalIgnoreCase) &&
                    d.DifficultyValue == (int) difficulty);
                playlistSong.playlistSong.Difficulties.RemoveAt(diffToDelete);
            }
        }

        #endregion

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