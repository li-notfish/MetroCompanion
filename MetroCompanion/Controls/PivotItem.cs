namespace MetroCompanion.Controls;

/// <summary>
/// WP8.1 Pivot 页容器，表头由 <see cref="PivotView"/> 统一渲染。
/// </summary>
public partial class PivotItem : ContentView
{
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(nameof(Header), typeof(object), typeof(PivotItem));
    public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(PivotItem));

    public object Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public DataTemplate HeaderTemplate { get => (DataTemplate)GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }

    public PivotItem()
    {
        VerticalOptions = LayoutOptions.Fill;
    }
}

/// <summary>
/// <see cref="PivotView.SelectionChanged"/> 的事件参数。
/// </summary>
public class PivotSelectionChangedEventArgs : EventArgs
{
    public int SelectedIndex { get; }
    public PivotItem SelectedItem { get; }
    public int PreviousIndex { get; }
    public PivotItem? PreviousItem { get; }

    public PivotSelectionChangedEventArgs(int selectedIndex, PivotItem selectedItem, int previousIndex, PivotItem? previousItem)
    {
        SelectedIndex = selectedIndex;
        SelectedItem = selectedItem;
        PreviousIndex = previousIndex;
        PreviousItem = previousItem;
    }
}
