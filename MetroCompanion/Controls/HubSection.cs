using System.Windows.Input;

namespace MetroCompanion.Controls;

public partial class HubSection : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(HubSection), "Section");
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    public HubSection()
    {
        VerticalOptions = LayoutOptions.Fill;
        ControlTemplate = new ControlTemplate(() => {
            var grid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }, Margin = new Thickness(0, 0, 80, 0) };

            var title = new Label { FontSize = 42, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, Margin = new Thickness(40, 60, 0, 20) };
            title.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

            var content = new ContentPresenter();
            content.Padding = new Thickness(40, 0, 0, 0);
            Grid.SetRow(content, 1);

            grid.Children.Add(title);
            grid.Children.Add(content);
            return grid;
        });
    }
}