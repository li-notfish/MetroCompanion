using MetroCompanion.Styles;

namespace MetroCompanion.Controls;

public partial class HubSection : ContentView
{
    private Label? _titleLabel;
    private BoxView? _accentBar;

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(HubSection), "Section", propertyChanged: OnTitleChanged);
    public static readonly BindableProperty HeaderFontSizeProperty = BindableProperty.Create(nameof(HeaderFontSize), typeof(double), typeof(HubSection), MetroTokens.HubSectionHeaderFontSize);

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public double HeaderFontSize { get => (double)GetValue(HeaderFontSizeProperty); set => SetValue(HeaderFontSizeProperty, value); }

    // Panorama 模式下首面板显示的是控件级 PanoramaTitle 而非自身 Title
    private bool _showingPanoramaTitle;

    public HubSection()
    {
        VerticalOptions = LayoutOptions.Fill;
        ControlTemplate = new ControlTemplate(() =>
        {
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
                // HubSectionHeaderThemeFontWeight = Normal
                FontFamily = MetroTokens.FontFamily,
                TextColor = MetroTokens.ForegroundColor,
                CharacterSpacing = MetroTokens.ToCharacterSpacing(
                    MetroTokens.HubSectionHeaderFontSize, MetroTokens.HubSectionHeaderCharacterSpacing),
                // 横向对齐页面栅格（结构校准）；标题下方 31.5 为 SDK HubSectionHeaderMarginThickness
                Margin = new Thickness(pageMargin + 12, 48, 0, 31.5),
                Text = Title
            };

            var content = new ContentPresenter();
            content.Padding = new Thickness(pageMargin, 0, 0, 0);
            Grid.SetRow(content, 1);

            grid.Children.Add(_accentBar);
            grid.Children.Add(_titleLabel);
            grid.Children.Add(content);
            return grid;
        });
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var section = (HubSection)bindable;
        if (!section._showingPanoramaTitle && section._titleLabel != null)
            section._titleLabel.Text = (string)newValue;
    }

    internal void ApplyPanoramaStyle(bool isFirst, string panoramaTitle)
    {
        if (_titleLabel == null) return;

        if (isFirst && !string.IsNullOrWhiteSpace(panoramaTitle))
        {
            // WP8 真机 Panorama 大标题约 165px Light、约 0.64 透明度，仅出现在第一面板
            _showingPanoramaTitle = true;
            _titleLabel.Text = panoramaTitle;
            _titleLabel.FontSize = MetroTokens.PanoramaTitleFontSize;
            _titleLabel.FontFamily = MetroTokens.LightFontFamily;
            _titleLabel.Opacity = 0.65;
            _titleLabel.Margin = new Thickness(MetroTokens.PageMargin - 4, 24, 0, 0);
        }
        else
        {
            // 后续面板只保留 PanoramaItem 小表头（Silverlight PanoramaItemHeaderFontSize = 66，
            // 50pt Semelight，字距 -35；无竖条）
            _showingPanoramaTitle = false;
            _titleLabel.Text = Title;
            _titleLabel.FontSize = MetroTokens.PanoramaItemHeaderFontSize;
            _titleLabel.FontFamily = MetroTokens.SemilightFontFamily;
            _titleLabel.CharacterSpacing = MetroTokens.ToCharacterSpacing(
                MetroTokens.PanoramaItemHeaderFontSize, MetroTokens.PanoramaItemHeaderCharacterSpacing);
            _titleLabel.Opacity = 1.0;
            _titleLabel.Margin = new Thickness(MetroTokens.PageMargin, 24, 0, 0);
        }

        if (_accentBar != null)
            _accentBar.IsVisible = false;
    }

    /// <summary>Panorama 滚动时首面板大标题的淡出；仅当该面板正显示 PanoramaTitle 时生效。</summary>
    internal void SetPanoramaTitleOpacity(double opacity)
    {
        if (_titleLabel == null || !_showingPanoramaTitle) return;
        _titleLabel.Opacity = Math.Clamp(opacity, 0, 1);
    }

    internal void ApplyHubStyle()
    {
        if (_titleLabel == null) return;

        _showingPanoramaTitle = false;
        _titleLabel.Text = Title;

        // HubSectionHeaderThemeFontSize：桌面 26.667 / 手机 19，字重 Normal，字距 -10
        _titleLabel.FontSize = HeaderFontSize;
        _titleLabel.FontFamily = MetroTokens.FontFamily;
        _titleLabel.CharacterSpacing = MetroTokens.ToCharacterSpacing(
            HeaderFontSize, MetroTokens.HubSectionHeaderCharacterSpacing);
        _titleLabel.Opacity = 1.0;
        _titleLabel.Margin = new Thickness(MetroTokens.PageMargin + 12, 48, 0, 31.5);

        if (_accentBar != null)
            _accentBar.IsVisible = true;
    }
}
