using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PlaylistManager.Models;
using PlaylistManager.UserControls;
using PlaylistManager.Utilities;
using PlaylistManager.Views;
using ReactiveUI;
using Splat;

namespace PlaylistManager.Windows
{
    public class LevelSearchWindow : Window
    {
        private readonly TextBox searchBox;
        
        public LevelSearchWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            searchBox = this.FindControl<TextBox>("SearchBox");
            DataContext = new LevelSearchWindowModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            searchBox.Focus();
        }
        
        private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }
    }

    public class LevelSearchWindowModel : ViewModelBase
    {
        private CancellationTokenSource? tokenSource;
        private LevelMatcher? levelMatcher;
        private readonly List<ILevelEncodedIDProtocol> levelEncodedIDProtocols = new()
        {
            new BeatSaverIDProtocol(),
            new BeastSaberIDProtocol(),
            new IDProtocol()
        };

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
            SearchResults.Clear();
            foreach (var levelEncodedIDProtocol in levelEncodedIDProtocols)
            {
                var searchResult = await levelEncodedIDProtocol.Result(searchText, tokenSource.Token);
                if (searchResult != null)
                {
                    levelMatcher ??= Locator.Current.GetService<LevelMatcher>();
                    if (levelMatcher != null)
                    {
                        ICustomLevelData? level = null;
                        if (searchResult.Value.Type == IDType.Key)
                        {
                            level = await levelMatcher.GetLevelByKey(searchResult.Value.ID);
                        }
                        else
                        {
                            level = await levelMatcher.GetLevelByHash(searchResult.Value.ID);
                        }
                        
                        if (level != null)
                        {
                            SearchResults.Add(new SearchItemViewModel(level));
                        }
                    }
                    
                    break;
                }
            }
        }
    }
}