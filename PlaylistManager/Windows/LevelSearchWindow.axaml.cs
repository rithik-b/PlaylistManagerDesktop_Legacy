using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        private readonly ListBox listBox;
        private readonly SemaphoreSlim openSemaphore;

        private SearchItemViewModel? SearchedSong => viewModel.SelectedResult;
        
        public LevelSearchWindow()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
            viewModel = new LevelSearchWindowModel();
            DataContext = viewModel;
            searchBox = this.FindControl<TextBox>("SearchBox");
            listBox = this.FindControl<ListBox>("ListBox");
            openSemaphore = new SemaphoreSlim(0, 1);
        }

        public async Task<SearchItemViewModel?> SearchSong(Window parent)
        {
            _ = ShowDialog(parent);
            await openSemaphore.WaitAsync();
            Hide();
            return SearchedSong;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            viewModel.SelectedResult = null;
            viewModel.SearchText = String.Empty;
            viewModel.SearchResults.Clear();
            searchBox.Focus();
        }
        
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    openSemaphore.Release();
                    break;
                case Key.Up:
                    listBox.SelectedIndex--;
                    break;
                case Key.Down:
                    listBox.SelectedIndex++;
                    break;
                case Key.Enter:
                    if (SearchedSong != null)
                    {
                        openSemaphore.Release();
                    }
                    break;
            }
        }

        private void OnDoubleClick(object? sender, RoutedEventArgs e)
        {
            if (SearchedSong != null)
            {
                openSemaphore.Release();
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
        private LevelMatcher LevelMatcher => levelMatcher ??= Locator.Current.GetService<LevelMatcher>()!;

        private string searchText = "";
        public string SearchText
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
        
        public ObservableCollection<SearchItemViewModel> SearchResults { get; } = new();

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

            await Task.Run(async () =>
            {
                SearchResults.Clear();

                // Do smart ID parsing
                foreach (var levelEncodedIDProtocol in levelEncodedIDProtocols)
                {
                    var searchResult = await levelEncodedIDProtocol.FindResultAsync(searchText, tokenSource.Token);
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
            }, tokenSource.Token).ConfigureAwait(false);
        }
    }
}