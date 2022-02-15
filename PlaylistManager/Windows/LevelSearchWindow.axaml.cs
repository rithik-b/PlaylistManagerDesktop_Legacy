using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PlaylistManager.Models;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;
using ReactiveUI;
using Splat;

namespace PlaylistManager.Windows
{
    public class LevelSearchWindow : Window
    {
        private readonly LevelSearchWindowModel viewModel;
        private readonly TextBox searchBox;

        public PlaylistSongWrapper? searchedSong { get; private set; }
        
        public LevelSearchWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            viewModel = new LevelSearchWindowModel();
            DataContext = viewModel;
            searchBox = this.FindControl<TextBox>("SearchBox");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            searchedSong = null;
            searchBox.Focus();
        }
        
        private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
            else if (e.Key == Key.Enter && viewModel.SelectedResult != null)
            {
                Hide();
            }
        }
    }

    public class LevelSearchWindowModel : ViewModelBase
    {
        private CancellationTokenSource? tokenSource; 
        private readonly List<ILevelEncodedIDProtocol> levelEncodedIDProtocols = new()
        {
            new BeatSaverIDProtocol(),
            new BeastSaberIDProtocol(),
            new ScoreSaberIDProtocol(),
            new IDProtocol()
        };
        
        private LevelMatcher? levelMatcher;
        private LevelMatcher? LevelMatcher => levelMatcher ??= Locator.Current.GetService<LevelMatcher>();

        private string searchText = "";
        private string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                NotifyPropertyChanged();
            }
        }

        private SearchItemViewModel? selectedResult;
        public SearchItemViewModel? SelectedResult
        {
            get => selectedResult;
            set
            {
                selectedResult = value;
                NotifyPropertyChanged();
            }
        }
        
        private ObservableCollection<SearchItemViewModel> SearchResults { get; } = new();

        public LevelSearchWindowModel()
        {
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(DoSearch!);
        }

        private async void DoSearch(string searchText)
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();
            SearchResults.Clear();

            if (LevelMatcher != null)
            {
                // Do smart ID parsing
                foreach (var levelEncodedIDProtocol in levelEncodedIDProtocols)
                {
                    var searchResult = await levelEncodedIDProtocol.Result(searchText, tokenSource.Token);
                    if (searchResult != null)
                    {
                        ICustomLevelData? level = null;
                        if (searchResult.Value.Type == IDType.Key)
                        {
                            level = await LevelMatcher.GetLevelByKey(searchResult.Value.ID);
                        }
                        else
                        {
                            level = await LevelMatcher.GetLevelByHash(searchResult.Value.ID);
                        }

                        if (level != null)
                        {
                            var resultToAdd = new SearchItemViewModel(level);
                            SearchResults.Add(resultToAdd);
                            SelectedResult = resultToAdd;
                            break;
                        }
                    }
                }
                
                // Perform search
                var searchResults = await LevelMatcher.SearchLevelsAsync(searchText, tokenSource.Token);
                foreach (var searchResult in searchResults)
                {
                    SearchResults.Add(new SearchItemViewModel(searchResult));
                }
                
                // Select a map if not selected already
                if (SelectedResult == null)
                {
                    SelectedResult = SearchResults.FirstOrDefault();
                }
            }
        }
    }
}