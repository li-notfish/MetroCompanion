using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using MetroCompanion.Behaviores;
using Microsoft.Maui.Dispatching;

namespace MetroCompanion.Controls;

/// <summary>
/// 还原 Windows Phone 8.1 Pivot 行为与外观的 MAUI 控件：
/// 整页横滑、始终吸附到某一页、表头以视差方式随滚动移动、点击表头跳转。
/// 外观参数均为 BindableProperty，默认值取 WP8.1 原始值。
/// </summary>
[ContentProperty(nameof(Items))]
public partial class PivotView : ContentView
{
    private Label? _titleLabel;
    private ScrollView? _headersClip;
    private HorizontalStackLayout? _headersPanel;
    private ScrollView? _scrollView;
    private HorizontalStackLayout? _itemsContainer;

    private IDispatcherTimer? _snapTimer;
    private DateTime _lastScrollCommandTime = DateTime.MinValue;
    private bool _isAnimating;
    private bool _isSyncingSelection;
    private int _currentIndex;
    private double _pageWidth;
    // 表头条带期望宽度是否需要重新测量（增删表头、字号/间距/边距变化时）
    private bool _headerExtentDirty = true;

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(PivotView), string.Empty, propertyChanged: OnTitleChanged);
    public static readonly BindableProperty TitleFontSizeProperty = BindableProperty.Create(nameof(TitleFontSize), typeof(double), typeof(PivotView), Styles.MetroTokens.PivotTitleFontSize);
    public static readonly BindableProperty TitleForegroundProperty = BindableProperty.Create(nameof(TitleForeground), typeof(Color), typeof(PivotView), Styles.MetroTokens.ForegroundColor);

    public static readonly BindableProperty HeaderFontSizeProperty = BindableProperty.Create(nameof(HeaderFontSize), typeof(double), typeof(PivotView), Styles.MetroTokens.PivotHeaderItemFontSize, propertyChanged: OnHeaderAppearanceChanged);
    public static readonly BindableProperty HeaderForegroundProperty = BindableProperty.Create(nameof(HeaderForeground), typeof(Color), typeof(PivotView), Styles.MetroTokens.ForegroundColor, propertyChanged: OnHeaderAppearanceChanged);
    public static readonly BindableProperty UnselectedHeaderOpacityProperty = BindableProperty.Create(nameof(UnselectedHeaderOpacity), typeof(double), typeof(PivotView), Styles.MetroTokens.UnselectedHeaderOpacity);
    public static readonly BindableProperty HeaderMarginProperty = BindableProperty.Create(nameof(HeaderMargin), typeof(Thickness), typeof(PivotView), new Thickness(Styles.MetroTokens.PageMargin, 0, 0, 0), propertyChanged: OnHeaderLayoutChanged);
    public static readonly BindableProperty HeaderSpacingProperty = BindableProperty.Create(nameof(HeaderSpacing), typeof(double), typeof(PivotView), Styles.MetroTokens.PivotHeaderItemSpacing, propertyChanged: OnHeaderLayoutChanged);

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(PivotView), 0, BindingMode.TwoWay, coerceValue: CoerceIndex, propertyChanged: OnSelectedIndexChanged);
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(PivotView), null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public double TitleFontSize { get => (double)GetValue(TitleFontSizeProperty); set => SetValue(TitleFontSizeProperty, value); }
    public Color TitleForeground { get => (Color)GetValue(TitleForegroundProperty); set => SetValue(TitleForegroundProperty, value); }

    public double HeaderFontSize { get => (double)GetValue(HeaderFontSizeProperty); set => SetValue(HeaderFontSizeProperty, value); }
    public Color HeaderForeground { get => (Color)GetValue(HeaderForegroundProperty); set => SetValue(HeaderForegroundProperty, value); }
    public double UnselectedHeaderOpacity { get => (double)GetValue(UnselectedHeaderOpacityProperty); set => SetValue(UnselectedHeaderOpacityProperty, value); }
    public Thickness HeaderMargin { get => (Thickness)GetValue(HeaderMarginProperty); set => SetValue(HeaderMarginProperty, value); }
    public double HeaderSpacing { get => (double)GetValue(HeaderSpacingProperty); set => SetValue(HeaderSpacingProperty, value); }

    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }
    public object SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

    public ObservableCollection<PivotItem> Items { get; } = new();

    public event EventHandler<PivotSelectionChangedEventArgs>? SelectionChanged;

    public PivotView()
    {
        InitializeComponent();
        Items.CollectionChanged += OnItemsChanged;
        SizeChanged += (s, e) => UpdateLayout();
        Unloaded += OnPivotUnloaded;

        if (Application.Current != null)
        {
            _snapTimer = Application.Current.Dispatcher.CreateTimer();
            _snapTimer.Interval = TimeSpan.FromMilliseconds(300);
            _snapTimer.Tick += OnSnapTimerTick;
        }
    }

    private void OnPivotUnloaded(object? sender, EventArgs e)
    {
        _snapTimer?.Stop();
        _snapTimer = null;

        if (_scrollView != null)
            _scrollView.Scrolled -= OnScrolled;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _titleLabel = GetTemplateChild("PART_Title") as Label;
        _headersClip = GetTemplateChild("PART_HeadersClip") as ScrollView;
        _headersPanel = GetTemplateChild("PART_HeadersPanel") as HorizontalStackLayout;
        _scrollView = GetTemplateChild("PART_ScrollView") as ScrollView;
        _itemsContainer = GetTemplateChild("PART_ItemsContainer") as HorizontalStackLayout;

        if (_titleLabel != null)
        {
            // WinRT 8.1 大标题使用 Light 字重
            _titleLabel.FontFamily = Styles.MetroTokens.LightFontFamily;
            _titleLabel.IsVisible = !string.IsNullOrWhiteSpace(Title);
        }

        if (_scrollView != null)
            _scrollView.Scrolled += OnScrolled;
#if WINDOWS
        // 滚轮/方向键翻页行为以代码挂载：模板内 OnPlatform<Behavior> 在
        // Release AOT 下会尝试实例化抽象 Behavior 导致崩溃
        if (_scrollView != null && _scrollView.Behaviors.Count == 0)
            _scrollView.Behaviors.Add(new HubHorizontalScrollBehavior { StepByPage = true });
#endif

        RefreshItems();

        Dispatcher.Dispatch(() => UpdateLayout());
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => _ = PlayEntranceAnimation());
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshItems();

    private void RefreshItems()
    {
        if (_itemsContainer == null || _headersPanel == null) return;

        _itemsContainer.Children.Clear();
        foreach (var item in Items)
        {
            item.VerticalOptions = LayoutOptions.Fill;
            _itemsContainer.Children.Add(item);
        }

        RefreshHeaders();

        int index = Items.Count > 0 ? Math.Clamp(_currentIndex, 0, Items.Count - 1) : 0;
        SyncSelection(index, raiseEvent: false);

        Dispatcher.Dispatch(() => UpdateLayout());
    }

    private void RefreshHeaders()
    {
        if (_headersPanel == null) return;

        _headersPanel.Children.Clear();
        _headersPanel.Spacing = HeaderSpacing;
        _headersPanel.Padding = HeaderMargin;

        foreach (var item in Items)
            _headersPanel.Children.Add(CreateHeaderView(item));

        _headerExtentDirty = true;
        UpdateHeaderStrip(_scrollView?.ScrollX ?? 0);
    }

    private View CreateHeaderView(PivotItem item)
    {
        View headerView;
        if (item.HeaderTemplate != null)
        {
            headerView = (View)item.HeaderTemplate.CreateContent();
            headerView.BindingContext = item.Header;
        }
        else
        {
            headerView = new Label
            {
                Text = item.Header?.ToString() ?? string.Empty,
                FontSize = HeaderFontSize,
                // PivotHeaderItemThemeFontWeight = SemiLight
                FontFamily = Styles.MetroTokens.SemilightFontFamily,
                TextColor = HeaderForeground,
                // StackLayout 会用"剩余宽度"约束测量子元素：末表头在手机上
                // 只剩一个字的剩余空间，默认 WordWrap 会折行后被条带行高裁掉，
                // 强制单行保证测量宽度即完整文字宽度
                LineBreakMode = LineBreakMode.NoWrap,
            };
        }

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => OnHeaderTapped(item);
        headerView.GestureRecognizers.Add(tap);

        return headerView;
    }

    private void OnHeaderTapped(PivotItem item)
    {
        int index = Items.IndexOf(item);
        if (index < 0) return;

        NavigateTo(index);
    }

    private void UpdateLayout()
    {
        if (Width <= 0 || _itemsContainer == null) return;

        _pageWidth = Width;
        foreach (var child in _itemsContainer.Children)
        {
            if (child is PivotItem item)
                item.WidthRequest = _pageWidth;
        }

        _itemsContainer.InvalidateMeasure();

        // 布局完成后保持当前选中页（尺寸变化时无动画复位）
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
        {
            if (_scrollView == null || _pageWidth <= 0 || Items.Count == 0) return;
            double targetX = Math.Clamp(_currentIndex, 0, Items.Count - 1) * _pageWidth;
            if (!_isAnimating)
                _scrollView.ScrollToAsync(targetX, 0, false);
            UpdateHeaderStrip(_scrollView.ScrollX);
        });
    }

    private void OnScrolled(object? sender, ScrolledEventArgs e)
    {
        UpdateHeaderStrip(e.ScrollX);

        if (_isAnimating || DateTime.Now - _lastScrollCommandTime < TimeSpan.FromMilliseconds(200))
            return;

        _snapTimer?.Stop();
        _snapTimer?.Start();
    }

    /// <summary>
    /// WP8.1 Pivot 表头行为：条带尽量静止，只有当选中表头会超出视口时才移动刚好够的距离
    /// （即 UWP/WP8.1 Pivot 的 scroll-into-view）；滚动过程中两邻接表头交叉淡化。
    /// </summary>
    private void UpdateHeaderStrip(double scrollX)
    {
        if (_headersPanel == null || _headersPanel.Children.Count == 0) return;

        int count = _headersPanel.Children.Count;
        double t = _pageWidth > 0 ? scrollX / _pageWidth : 0;

        // 位移：对页 k 求让表头 k 完整可见所需的最小左移量，再在两页之间线性插值
        double tc = Math.Clamp(t, 0, count - 1);
        int k = (int)Math.Floor(tc);
        double f = tc - k;
        double viewportWidth = _headersClip?.Width ?? 0;
        double marginLeft = HeaderMargin.Left;
        double maxShift = Math.Max(0, EnsureHeaderStripWidth(viewportWidth) + marginLeft - viewportWidth);
        double shiftA = GetHeaderShift(k, viewportWidth, marginLeft, maxShift);
        double shiftB = GetHeaderShift(Math.Min(k + 1, count - 1), viewportWidth, marginLeft, maxShift);
        _headersPanel.TranslationX = -(shiftA + (shiftB - shiftA) * f);

        // 交叉淡化：滚动进度落在 i 与 i+1 之间时，二者在 1 与 UnselectedHeaderOpacity 间过渡
        double unselected = UnselectedHeaderOpacity;
        for (int i = 0; i < count; i++)
        {
            double d = Math.Abs(t - i);
            double opacity = d >= 1 ? unselected : 1.0 + (unselected - 1.0) * d;
            if (_headersPanel.Children[i] is VisualElement v)
                v.Opacity = opacity;
        }
    }

    private double GetHeaderPosition(int index)
        => _headersPanel != null && _headersPanel.Children[index] is VisualElement v ? v.X : 0;

    /// <summary>
    /// 让表头 <paramref name="index"/> 完整落在视口内所需的最小条带左移量。
    /// 目标：表头右缘对齐视口右栅格（右缘 = 视口宽 - 左边距）。
    /// </summary>
    private double GetHeaderShift(int index, double viewportWidth, double marginLeft, double maxShift)
    {
        if (viewportWidth <= 0 || _headersPanel == null || index >= _headersPanel.Children.Count)
            return 0;

        double pos = GetHeaderPosition(index);
        double width = _headersPanel.Children[index] is VisualElement v ? v.Width : 0;

        double shift = Math.Max(0, pos + width + marginLeft - viewportWidth);
        // 条带右端（含右边距）最多收到视口右栅格：WP8.1 在最后一页时末表头
        // 完整可见，前面的表头被推出视口左缘（条带比视口宽时自然让位）
        return Math.Min(shift, maxShift);
    }

    /// <summary>
    /// 表头条带的总期望宽度（无约束测量），并把面板显式撑宽到该值。
    /// Orientation=Neither 的 ScrollView 会以视口宽约束测量内容，StackLayout
    /// 又把"剩余宽度"传给末尾子元素——条带比视口宽时，末表头被截短、面板本地
    /// 宽度被裁到视口宽，Android 默认 clipChildren 会裁掉面板边界外的部分，
    /// TranslationX 平移的是面板自身，永远无法露出边界外的内容。
    /// </summary>
    private double EnsureHeaderStripWidth(double viewportWidth)
    {
        if (_headersPanel == null || _headersPanel.Children.Count == 0 || viewportWidth <= 0)
            return 0;

        if (_headerExtentDirty)
        {
            double extent = _headersPanel.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
            if (extent > 0 && Math.Abs(_headersPanel.WidthRequest - extent) > 1)
                _headersPanel.WidthRequest = extent;
            _headerExtentDirty = false;
        }

        return _headersPanel.WidthRequest;
    }

    private void OnSnapTimerTick(object? sender, EventArgs e)
    {
        _snapTimer?.Stop();
        if (_scrollView == null || Items.Count == 0 || _pageWidth <= 0) return;

        double scrollX = _scrollView.ScrollX;
        // Pivot 始终吸附：四舍五入到最近的整页
        int targetIndex = (int)Math.Clamp(Math.Round(scrollX / _pageWidth), 0, Items.Count - 1);
        double targetX = targetIndex * _pageWidth;

        SyncSelection(targetIndex);
        if (Math.Abs(targetX - scrollX) >= 0.5)
            _ = AnimateScrollTo(targetX);
    }

    private void NavigateTo(int index)
    {
        if (Items.Count == 0 || _pageWidth <= 0 || _scrollView == null)
        {
            _currentIndex = Math.Max(0, index);
            return;
        }

        index = Math.Clamp(index, 0, Items.Count - 1);
        SyncSelection(index);
        _ = AnimateScrollTo(index * _pageWidth);
    }

    /// <summary>
    /// 同步运行时选中状态到 _currentIndex 与两个 BindableProperty，并按需触发 SelectionChanged。
    /// </summary>
    private void SyncSelection(int index, bool raiseEvent = true)
    {
        index = Items.Count > 0 ? Math.Clamp(index, 0, Items.Count - 1) : 0;
        bool changed = index != _currentIndex;
        int previousIndex = _currentIndex;
        var previousItem = previousIndex >= 0 && previousIndex < Items.Count ? Items[previousIndex] : null;

        _currentIndex = index;

        _isSyncingSelection = true;
        try
        {
            SetValue(SelectedIndexProperty, index);
            SetValue(SelectedItemProperty, index < Items.Count ? Items[index] : null);
        }
        finally
        {
            _isSyncingSelection = false;
        }

        if (changed && raiseEvent && index < Items.Count)
        {
            SelectionChanged?.Invoke(this, new PivotSelectionChangedEventArgs(
                index, Items[index], previousIndex, previousItem));
        }
    }

    private async Task AnimateScrollTo(double targetX)
    {
        if (_isAnimating || _scrollView == null) return;

        _isAnimating = true;
        _lastScrollCommandTime = DateTime.Now;

        double startX = _scrollView.ScrollX;
        if (Math.Abs(targetX - startX) < 0.5)
        {
            _isAnimating = false;
            return;
        }

        // 跳页越多动画越长：单页吸附保持 HubView 的 150ms 节奏
        double pages = Math.Abs(targetX - startX) / Math.Max(1.0, _pageWidth);
        int durationMs = (int)Math.Clamp(150 + pages * 150, 150, 600);
        const int frameDelayMs = 16;
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < durationMs)
        {
            double progress = Math.Min((double)sw.ElapsedMilliseconds / durationMs, 1.0);
            double eased = CubicOut(progress);
            _ = _scrollView.ScrollToAsync(startX + (targetX - startX) * eased, 0, false);
            await Task.Delay(frameDelayMs);
        }

        _ = _scrollView.ScrollToAsync(targetX, 0, false);
        UpdateHeaderStrip(targetX);
        _isAnimating = false;
    }

    private static double CubicOut(double t)
    {
        return 1.0 - Math.Pow(1.0 - t, 3);
    }

    /// <summary>
    /// WP8.1 页面载入节奏：大标题先从右侧滑入，表头条随后，内容最后。
    /// </summary>
    public async Task PlayEntranceAnimation()
    {
        var tasks = new List<Task>();
        if (_titleLabel != null && _titleLabel.IsVisible)
            tasks.Add(SlideAndFadeIn(_titleLabel, 120, 500, 0));
        if (_headersClip != null)
            tasks.Add(SlideAndFadeIn(_headersClip, 90, 450, 80));
        if (_scrollView != null)
            tasks.Add(SlideAndFadeIn(_scrollView, 40, 400, 160));
        await Task.WhenAll(tasks);
    }

    private static async Task SlideAndFadeIn(VisualElement element, double offsetX, uint duration, int delayMs)
    {
        element.TranslationX = offsetX;
        element.Opacity = 0;
        if (delayMs > 0)
            await Task.Delay(delayMs);
        await Task.WhenAll(
            element.TranslateToAsync(0, 0, duration, Easing.CubicOut),
            element.FadeToAsync(1, duration, Easing.CubicOut));
    }

    private static object CoerceIndex(BindableObject bindable, object value)
        => (int)value < 0 ? 0 : value;

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PivotView pivot && pivot._titleLabel != null)
            pivot._titleLabel.IsVisible = !string.IsNullOrWhiteSpace((string)newValue);
    }

    private static void OnHeaderAppearanceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PivotView pivot && pivot._headersPanel != null)
        {
            foreach (var child in pivot._headersPanel.Children)
            {
                if (child is Label label)
                {
                    label.FontSize = pivot.HeaderFontSize;
                    label.FontFamily = Styles.MetroTokens.SemilightFontFamily;
                    label.TextColor = pivot.HeaderForeground;
                }
            }
            pivot._headerExtentDirty = true;
        }
    }

    private static void OnHeaderLayoutChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PivotView pivot && pivot._headersPanel != null)
        {
            pivot._headersPanel.Spacing = pivot.HeaderSpacing;
            pivot._headersPanel.Padding = pivot.HeaderMargin;
            pivot._headerExtentDirty = true;
            pivot.UpdateHeaderStrip(pivot._scrollView?.ScrollX ?? 0);
        }
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PivotView pivot && !pivot._isSyncingSelection && newValue is int index)
            pivot.NavigateTo(index);
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PivotView pivot && !pivot._isSyncingSelection)
        {
            int index = newValue is PivotItem item ? pivot.Items.IndexOf(item) : -1;
            if (index >= 0)
                pivot.NavigateTo(index);
        }
    }
}
