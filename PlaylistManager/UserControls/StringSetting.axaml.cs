using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace PlaylistManager.UserControls
{
    public class StringSetting : TemplatedControl
    {
        public StringSetting()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<string> SettingNameProperty =
            AvaloniaProperty.Register<StringSetting, string>(nameof(SettingName));

        public string SettingName
        {
            get => GetValue(SettingNameProperty);
            set => SetValue(SettingNameProperty, value);
        }
            
        public static readonly StyledProperty<string> SettingValueProperty =
            AvaloniaProperty.Register<StringSetting, string>(nameof(SettingValue), defaultBindingMode: BindingMode.TwoWay);
        
        public string SettingValue
        {
            get => GetValue(SettingValueProperty);
            set => SetValue(SettingValueProperty, value);
        }
    }
}