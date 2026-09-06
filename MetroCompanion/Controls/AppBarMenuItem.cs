using System.Windows.Input;
using MetroCompanion.Styles;

namespace MetroCompanion.Controls;

/// <summary>
/// WP8 应用栏菜单项：19pt（25.333px）文本行，点击后由宿主收起菜单。
/// 规格参照 WPToolkit ContextMenu MenuItem（左缩进 25、纵向 10 padding）与系统菜单项字号。
/// </summary>
public partial class AppBarMenuItem : ContentView
{
    private Label? _textLabel;

    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(AppBarMenuItem), string.Empty, propertyChanged: OnTextChanged);
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(AppBarMenuItem));
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(AppBarMenuItem));

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    /// <summary>菜单项被激活（点击）时触发；宿主 <see cref="ApplicationBar"/> 借此收起菜单。</summary>
    public event EventHandler? Activated;

    public AppBarMenuItem()
    {
        ControlTemplate = new ControlTemplate(() =>
        {
            _textLabel = new Label
            {
                Text = Text,
                FontSize = MetroTokens.AppBarMenuItemFontSize,
                FontFamily = MetroTokens.FontFamily,
                TextColor = MetroTokens.AppBarForeground,
                Padding = new Thickness(MetroTokens.AppBarMenuItemIndent, 10, 25, 10),
                LineBreakMode = LineBreakMode.NoWrap,
                VerticalOptions = LayoutOptions.Center
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnActivated();
            _textLabel.GestureRecognizers.Add(tap);
            return _textLabel;
        });
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AppBarMenuItem item && item._textLabel != null)
            item._textLabel.Text = (string)newValue;
    }

    private void OnActivated()
    {
        Command?.Execute(CommandParameter);
        Activated?.Invoke(this, EventArgs.Empty);
    }
}
