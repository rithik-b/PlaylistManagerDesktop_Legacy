using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
}