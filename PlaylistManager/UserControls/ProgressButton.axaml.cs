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
    public class ProgressButton : TemplatedControl
    {
        public ProgressButton()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<Geometry> IconProperty =
            AvaloniaProperty.Register<ProgressButton, Geometry>(nameof(Icon), defaultBindingMode: BindingMode.TwoWay);

        public Geometry Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DirectProperty<ProgressButton, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<ProgressButton, ICommand>(nameof(Command), button => button.Command, 
                (button, command) => button.Command = command, enableDataValidation: true);

        private ICommand Command { get; set; }

        public static readonly StyledProperty<bool> AnimateProperty = 
            AvaloniaProperty.Register<ProgressButton, bool>(nameof(Animate), defaultBindingMode: BindingMode.TwoWay);
        
        public bool Animate
        {
            get => GetValue(AnimateProperty);
            set => SetValue(AnimateProperty, value);
        }
    }
}