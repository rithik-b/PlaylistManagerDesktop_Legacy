using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PlaylistManager.Models;

namespace PlaylistManager.Windows
{
    public class MainWindow : Window
    {
        public readonly MainWindowModel viewModel;
        public Action? ClickOffEvent;
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            viewModel = new MainWindowModel();
            DataContext = viewModel;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void ClickOff(object? sender, PointerPressedEventArgs e) => ClickOffEvent?.Invoke();
    }

    public class MainWindowModel : ViewModelBase
    {
        private bool modalShown;
        public bool ModalShown
        {
            get => modalShown;
            set
            {
                modalShown = value;
                NotifyPropertyChanged();
            }
        }
    }
}