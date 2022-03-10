using System.Reactive;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PlaylistManager.Models;
using ReactiveUI;

namespace PlaylistManager.UserControls
{
    public class ProgressIcon : TemplatedControl
    {
        public ProgressIcon()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<Geometry> IconProperty =
            AvaloniaProperty.Register<ProgressIcon, Geometry>(nameof(Icon), defaultBindingMode: BindingMode.TwoWay);

        public Geometry Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly StyledProperty<bool> AnimateProperty = 
            AvaloniaProperty.Register<ProgressIcon, bool>(nameof(Animate), defaultBindingMode: BindingMode.TwoWay);
        
        public bool Animate
        {
            get => GetValue(AnimateProperty);
            set => SetValue(AnimateProperty, value);
        }
    }
}