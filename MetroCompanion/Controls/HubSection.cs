using System.Windows.Input;

namespace MetroCompanion.Controls;

public partial class HubSection : ContentView
{
    private Label _titleLabel;
    private BoxView _accentBar;

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(HubSection), "Section");
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    internal bool IsPanoramaMode { get; set; }

    public HubSection()
    {
        VerticalOptions = LayoutOptions.Fill;
        ControlTemplate = new ControlTemplate(() => {
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star)
                },
                Margin = new Thickness(0, 0, 60, 0)
            };

            _accentBar = new BoxView
            {
                Color = Color.FromArgb("#0078D4"),
                WidthRequest = 4,
                CornerRadius = 2,
                HeightRequest = 40,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(24, 80, 0, 0)
            };

            _titleLabel = new Label
            {
                FontSize = 48,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                Margin = new Thickness(36, 80, 0, 16)
            };
            _titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

            var content = new ContentPresenter();
            content.Padding = new Thickness(36, 0, 0, 0);
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

        _titleLabel.FontSize = 64;
        _titleLabel.Opacity = 0.65;
        _titleLabel.Margin = new Thickness(20, 40, 0, 0);

        if (_accentBar != null)
            _accentBar.IsVisible = false;
    }

    internal void ApplyHubStyle()
    {
        if (_titleLabel == null) return;

        _titleLabel.FontSize = 48;
        _titleLabel.Opacity = 1.0;
        _titleLabel.Margin = new Thickness(36, 80, 0, 16);

        if (_accentBar != null)
            _accentBar.IsVisible = true;
    }
}