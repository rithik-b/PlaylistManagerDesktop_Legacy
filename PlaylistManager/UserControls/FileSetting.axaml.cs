using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PlaylistManager.Windows;
using Splat;

namespace PlaylistManager.UserControls
{
    public class FileSetting : TemplatedControl
    {
        private Window? mainWindow;
        private OpenFileDialog? openFileDialog;
        private SaveFileDialog? saveFileDialog;
        private OpenFolderDialog? openFolderDialog;
        
        public FileSetting()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<string> SettingNameProperty =
            AvaloniaProperty.Register<FileSetting, string>(nameof(SettingName));

        public string SettingName
        {
            get => GetValue(SettingNameProperty);
            set => SetValue(SettingNameProperty, value);
        }
            
        public static readonly StyledProperty<string> SettingValueProperty =
            AvaloniaProperty.Register<FileSetting, string>(nameof(SettingValue), defaultBindingMode: BindingMode.TwoWay);
        
        public string SettingValue
        {
            get => GetValue(SettingValueProperty);
            set => SetValue(SettingValueProperty, value);
        }
        
        public static readonly StyledProperty<Mode> ModeValueProperty =
            AvaloniaProperty.Register<FileSetting, Mode>(nameof(ModeValue));

        public Mode ModeValue
        {
            get => GetValue(ModeValueProperty);
            set => SetValue(ModeValueProperty, value);
        }
        
        private async void OnClick(object? sender, RoutedEventArgs e)
        {
            mainWindow ??= Locator.Current.GetService<MainWindow>();
            switch (ModeValue)
            {
                case Mode.FileRead:
                {
                    openFileDialog ??= new OpenFileDialog();
                    var paths = await openFileDialog.ShowAsync(mainWindow);
                    if (paths != null && paths.Length != 0)
                    {
                        SettingValue = paths.First();
                    }
                    break;
                }
                case Mode.FileWrite:
                {
                    saveFileDialog ??= new SaveFileDialog();
                    var path = await saveFileDialog.ShowAsync(mainWindow);
                    if (path != null)
                    {
                        SettingValue = path;
                    }
                    break;
                }
                case Mode.FolderRead:
                {
                    openFolderDialog ??= new OpenFolderDialog();
                    var path = await openFolderDialog.ShowAsync(mainWindow);
                    if (path != null)
                    {
                        SettingValue = path;
                    }
                    break;
                }
            }
        }
        
        public enum Mode
        {
            FileRead,
            FileWrite,
            FolderRead
        }
    }
}