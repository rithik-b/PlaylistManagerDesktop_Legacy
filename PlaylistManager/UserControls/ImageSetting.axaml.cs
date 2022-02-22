using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PlaylistManager.Windows;
using Splat;

namespace PlaylistManager.UserControls
{
    public class ImageSetting : TemplatedControl
    {
        private readonly OpenFileDialog openFileDialog;
        
        private MainWindow? mainWindow;
        private MainWindow MainWindow => (mainWindow ??= Locator.Current.GetService<MainWindow>())!;

        public ImageSetting()
        {
            AvaloniaXamlLoader.Load(this);
            openFileDialog = new OpenFileDialog()
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>()
                {
                    new FileDialogFilter()
                    {
                        Extensions = new List<string>()
                        {
                            "png",
                            "jpg",
                            "jpeg"
                        }
                    }
                }
            };
        }
        
        public static readonly StyledProperty<string> SettingNameProperty =
            AvaloniaProperty.Register<FileSetting, string>(nameof(SettingName));

        public string SettingName
        {
            get => GetValue(SettingNameProperty);
            set => SetValue(SettingNameProperty, value);
        }
            
        public static readonly StyledProperty<Bitmap> ImageProperty =
            AvaloniaProperty.Register<FileSetting, Bitmap>(nameof(Image), defaultBindingMode: BindingMode.TwoWay);
        
        public Bitmap Image
        {
            get => GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        private async void OpenImage(object? sender, RoutedEventArgs e)
        {
            var filePaths = await openFileDialog.ShowAsync(MainWindow);
            if (filePaths is {Length: > 0})
            {
                var filePath = filePaths.First();
                await using var imageStream = File.Open(filePath, FileMode.Open);
                Image = Bitmap.DecodeToHeight(imageStream, 512);
            }
        }
    }
}