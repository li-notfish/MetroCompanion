using System.Windows.Input;
using MetroCompanion.Styles;

namespace MetroCompanion.Controls;

/// <summary>
/// WP8 应用栏图标按钮：48×48 图标 + caption（caption 仅在应用栏展开时显示，同真机）。
/// 禁用态为 40% 不透明（PhoneDisabledColor 语义）。
/// </summary>
public partial class AppBarButton : ContentView
{
    private Image? _iconImage;
    private Label? _captionLabel;

    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(AppBarButton), string.Empty, propertyChanged: OnTextChanged);
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(AppBarButton), propertyChanged: OnIconChanged);
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(AppBarButton));
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(AppBarButton));

    /// <summary>图标下方 caption，展开时显示。</summary>
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    /// <summary>官方规格图标（48×48 PNG，透明边距内含 glyph）。</summary>
    public ImageSource Icon { get => (ImageSource)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    /// <summary>按钮被激活（点击）时触发；宿主 <see cref="ApplicationBar"/> 借此收起菜单。</summary>
    public event EventHandler? Activated;

    public AppBarButton()
    {
        ControlTemplate = new ControlTemplate(() =>
        {
            var stack = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 1
            };

            _iconImage = new Image
            {
                Source = Icon,
                WidthRequest = MetroTokens.AppBarIconSize,
                HeightRequest = MetroTokens.AppBarIconSize,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.End
            };

            _captionLabel = new Label
            {
                Text = Text,
                FontSize = MetroTokens.AppBarCaptionFontSize,
                FontFamily = MetroTokens.FontFamily,
                TextColor = MetroTokens.AppBarForeground,
                HorizontalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.NoWrap,
                IsVisible = false
            };

            stack.Children.Add(_iconImage);
            stack.Children.Add(_captionLabel);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnActivated();
            stack.GestureRecognizers.Add(tap);
            return stack;
        });
    }

    /// <summary>应用栏展开/收起时由宿主调用：caption 仅在展开时可见（WP8 行为）。</summary>
    internal void SetCaptionVisible(bool visible)
    {
        if (_captionLabel != null)
            _captionLabel.IsVisible = visible;
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AppBarButton button && button._captionLabel != null)
            button._captionLabel.Text = (string)newValue;
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AppBarButton button && button._iconImage != null)
            button._iconImage.Source = (ImageSource)newValue;
    }

    protected override void OnPropertyChanged(string? propertyName)
    {
        base.OnPropertyChanged(propertyName);

        // WP8 禁用态：前景 40% 不透明
        if (propertyName == nameof(IsEnabled))
            Opacity = IsEnabled ? 1 : 0.4;
    }

    private void OnActivated()
    {
        if (!IsEnabled)
            return;

        Command?.Execute(CommandParameter);
        Activated?.Invoke(this, EventArgs.Empty);
    }
}
