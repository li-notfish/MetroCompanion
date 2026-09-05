using System.Windows.Input;
using MetroCompanion.Styles;

namespace MetroCompanion.Controls;

public partial class HubSection : ContentView
{
    private Label _titleLabel;
    private BoxView _accentBar;

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(HubSection), "Section");
    public static readonly BindableProperty HeaderFontSizeProperty = BindableProperty.Create(nameof(HeaderFontSize), typeof(double), typeof(HubSection), MetroTokens.HubSectionHeaderFontSize);

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public double HeaderFontSize { get => (double)GetValue(HeaderFontSizeProperty); set => SetValue(HeaderFontSizeProperty, value); }

    internal bool IsPanoramaMode { get; set; }

    public HubSection()
    {
        VerticalOptions = LayoutOptions.Fill;
        ControlTemplate = new ControlTemplate(() => {
            double pageMargin = MetroTokens.PageMargin;
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star)
                },
                Margin = new Thickness(0, 0, 60, 0)
            };

            // 原版 Hub 节头无竖条，这里作为本库装饰保留，尺寸随节标题等比缩小（Metro 直角）
            _accentBar = new BoxView
            {
                Color = MetroTokens.AccentColor,
                WidthRequest = 4,
                CornerRadius = 0,
                HeightRequest = 28,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(pageMargin, 50, 0, 0)
            };

            _titleLabel = new Label
            {
                FontSize = MetroTokens.HubSectionHeaderFontSize,
                FontFamily = MetroTokens.SemiboldFontFamily,
                TextColor = MetroTokens.ForegroundColor,
                Margin = new Thickness(pageMargin + 12, 48, 0, 8)
            };
            _titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

            var content = new ContentPresenter();
            content.Padding = new Thickness(pageMargin, 0, 0, 0);
            Grid.SetRow(content, 1);

            grid.Children.Add(_accentBar);
            grid.Children.Add(_titleLabel);
            grid.Children.Add(content);
            return grid;
        });
    }

    internal void ApplyPanoramaStyle()
    {
        if (_titleLabel == null) return;

        // WP8 真机 Panorama 标题约 165px Light、约 0.64 透明度
        _titleLabel.FontSize = MetroTokens.PanoramaTitleFontSize;
        _titleLabel.FontFamily = MetroTokens.LightFontFamily;
        _titleLabel.Opacity = 0.65;
        _titleLabel.Margin = new Thickness(MetroTokens.PageMargin - 4, 24, 0, 0);

        if (_accentBar != null)
            _accentBar.IsVisible = false;
    }

    internal void ApplyHubStyle()
    {
        if (_titleLabel == null) return;

        // HubSectionHeaderThemeFontSize：26.667 Semibold
        _titleLabel.FontSize = HeaderFontSize;
        _titleLabel.FontFamily = MetroTokens.SemiboldFontFamily;
        _titleLabel.Opacity = 1.0;
        _titleLabel.Margin = new Thickness(MetroTokens.PageMargin + 12, 48, 0, 8);

        if (_accentBar != null)
            _accentBar.IsVisible = true;
    }
}
