using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MetroCompanion.Styles;

namespace MetroCompanion.Controls;

/// <summary>应用栏模式：Default 显示图标按钮，Minimized 仅显示"…"（WP8 ApplicationBar.Mode 语义）。</summary>
public enum AppBarMode
{
    Default,
    Minimized
}

/// <summary>
/// 还原 WP8 ApplicationBar 的 MAUI 控件：72px 高栏 + 至多四个图标按钮 + "…" 展开菜单。
/// 展开时"…"旋转 90°、按钮 caption 与菜单项淡入；激活任一命令后自动收起。
/// 栏底色为 PhoneChromeColor（暗 #FF1F1F1F / 亮 #FFDDDDDD）。
/// </summary>
/// <remarks>
/// 用法：放进页面底部 Grid 行（MAUI 无页面级 chrome），如
/// <c>&lt;Grid RowDefinitions="*, Auto"&gt; … &lt;mcm:ApplicationBar /&gt;&lt;/Grid&gt;</c>。
/// 点击页面其余区域收起菜单由使用方处理（对页面内容挂 TapGestureRecognizer 调 <see cref="Close"/>）。
/// </remarks>
[ContentProperty(nameof(Buttons))]
public partial class ApplicationBar : ContentView
{
    private Grid? _root;
    private VerticalStackLayout? _menuPanel;
    private Grid? _barRow;
    private Grid? _buttonGrid;
    private Grid? _ellipsisButton;
    private HorizontalStackLayout? _ellipsisDots;

    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(ApplicationBar), false, BindingMode.TwoWay, propertyChanged: OnIsOpenChanged);
    public static readonly BindableProperty ModeProperty = BindableProperty.Create(nameof(Mode), typeof(AppBarMode), typeof(ApplicationBar), AppBarMode.Default, propertyChanged: OnModeChanged);

    /// <summary>"…" 菜单展开状态（TwoWay）。</summary>
    public bool IsOpen { get => (bool)GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    /// <summary>Default：图标按钮 + "…"；Minimized：仅"…"（WP8 Mode 语义）。</summary>
    public AppBarMode Mode { get => (AppBarMode)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }

    /// <summary>图标按钮（WP8 至多四个；超出的仍会渲染，但不符合真机规格）。</summary>
    public ObservableCollection<AppBarButton> Buttons { get; } = new();
    /// <summary>"…" 展开的菜单项。</summary>
    public ObservableCollection<AppBarMenuItem> MenuItems { get; } = new();

    public ApplicationBar()
    {
        InitializeComponent();
        Buttons.CollectionChanged += (_, _) => RefreshButtons();
        MenuItems.CollectionChanged += (_, _) => RefreshMenuItems();
        Unloaded += OnBarUnloaded;
    }

    private static readonly Dictionary<BindableObject, (PointerGestureRecognizer Gesture, IDispatcherTimer Timer)> _menuTriggers = new();

    /// <summary>
    /// 把应用栏菜单挂到页面内容元素：Windows 上单击鼠标右键弹出/收回（WinRT 桌面应用栏
    /// 约定），移动端长按约 1 秒弹出（WP8 按住行为）。值为宿主 <see cref="ApplicationBar"/>：
    /// <c>&lt;Grid mcm:ApplicationBar.MenuTrigger="{x:Reference Bar}"&gt;…&lt;/Grid&gt;</c>
    /// </summary>
    public static readonly BindableProperty MenuTriggerProperty = BindableProperty.CreateAttached(
        "MenuTrigger", typeof(ApplicationBar), typeof(VisualElement), null, propertyChanged: OnMenuTriggerChanged);

    public static ApplicationBar? GetMenuTrigger(BindableObject bindable) => (ApplicationBar?)bindable.GetValue(MenuTriggerProperty);
    public static void SetMenuTrigger(BindableObject bindable, ApplicationBar? value) => bindable.SetValue(MenuTriggerProperty, value);

    private static void OnMenuTriggerChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View element)
            return;

        if (_menuTriggers.Remove(bindable, out var old))
        {
            element.GestureRecognizers.Remove(old.Gesture);
            old.Timer.Stop();
        }

        if (newValue is not ApplicationBar bar)
            return;

        // 移动端长按弹出：按下计时，约 1 秒未松开/未移动则呼出（WP8 行为）。
        // 拖动（滚动）或提前松开都会取消。
        var timer = Application.Current?.Dispatcher.CreateTimer();
        if (timer != null)
        {
            timer.Interval = TimeSpan.FromMilliseconds(900);
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (!bar.IsOpen)
                    bar.IsOpen = true;
            };
        }

        var gesture = new PointerGestureRecognizer();
        gesture.PointerPressed += (_, _) => timer?.Start();
        gesture.PointerMoved += (_, _) => timer?.Stop();
        gesture.PointerReleased += (_, _) => timer?.Stop();
        gesture.PointerExited += (_, _) => timer?.Stop();
        element.GestureRecognizers.Add(gesture);

#if WINDOWS
        // 鼠标右键弹出/收回以代码挂载：模板/XAML 内 OnPlatform<Behavior> 在 Release AOT 下
        // 会尝试实例化抽象 Behavior 导致崩溃
        element.Behaviors.Add(new Behaviores.AppBarRightClickBehavior { Bar = bar });
#endif

        _menuTriggers[bindable] = (gesture, timer!);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = GetTemplateChild("PART_Root") as Grid;
        _menuPanel = GetTemplateChild("PART_MenuPanel") as VerticalStackLayout;
        _barRow = GetTemplateChild("PART_BarRow") as Grid;
        _buttonGrid = GetTemplateChild("PART_ButtonGrid") as Grid;
        _ellipsisButton = GetTemplateChild("PART_EllipsisButton") as Grid;
        _ellipsisDots = GetTemplateChild("PART_EllipsisDots") as HorizontalStackLayout;

        // PhoneChromeColor 栏底 + PhoneForegroundColor 前景
        var barColor = MetroTokens.AppBarBackground;
        if (_barRow != null) _barRow.BackgroundColor = barColor;
        if (_menuPanel != null) _menuPanel.BackgroundColor = barColor;
        if (_ellipsisDots != null)
            foreach (var dot in _ellipsisDots.Children.OfType<BoxView>())
                dot.Color = MetroTokens.AppBarForeground;

        if (_ellipsisButton != null)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => Toggle();
            _ellipsisButton.GestureRecognizers.Add(tap);
        }

        RefreshButtons();
        RefreshMenuItems();
        UpdateOpenState(animate: false);
    }

    private void OnBarUnloaded(object? sender, EventArgs e)
    {
        // 淡入动画可能仍在进行，中断引用避免泄露
        _menuPanel = null;
        _barRow = null;
        _buttonGrid = null;
        _ellipsisButton = null;
        _ellipsisDots = null;
        _root = null;
    }

    /// <summary>收起展开的菜单。</summary>
    public void Close() => IsOpen = false;

    /// <summary>切换菜单展开/收起。</summary>
    public void Toggle() => IsOpen = !IsOpen;

    protected override void OnPropertyChanged(string? propertyName)
    {
        base.OnPropertyChanged(propertyName);

        // WP8 Opacity 语义：0 = 整栏隐藏且不占位
        if (propertyName == nameof(Opacity))
            IsVisible = Math.Abs(Opacity) > 0.001;
    }

    private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ApplicationBar bar)
            bar.UpdateOpenState(animate: true);
    }

    private static void OnModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ApplicationBar bar)
            bar.UpdateOpenState(animate: false);
    }

    private void RefreshButtons()
    {
        if (_buttonGrid == null) return;

        _buttonGrid.Children.Clear();
        var columns = new ColumnDefinitionCollection();
        for (int i = 0; i < Math.Max(Buttons.Count, 1); i++)
            columns.Add(new ColumnDefinition(GridLength.Star));
        _buttonGrid.ColumnDefinitions = columns;

        for (int i = 0; i < Buttons.Count; i++)
        {
            var button = Buttons[i];
            Grid.SetColumn(button, i);
            // RefreshButtons 可能被模板应用与集合变化多次触发，防重复订阅
            button.Activated -= OnItemActivated;
            button.Activated += OnItemActivated;
            _buttonGrid.Children.Add(button);
        }

        _buttonGrid.IsVisible = Mode != AppBarMode.Minimized;
    }

    private void RefreshMenuItems()
    {
        if (_menuPanel == null) return;

        _menuPanel.Children.Clear();
        foreach (var item in MenuItems)
        {
            item.Activated -= OnItemActivated;
            item.Activated += OnItemActivated;
            _menuPanel.Children.Add(item);
        }
    }

    private void OnItemActivated(object? sender, EventArgs e)
    {
        // WP8：激活任一图标按钮/菜单项后菜单收起
        IsOpen = false;
    }

    private void UpdateOpenState(bool animate)
    {
        if (_buttonGrid == null || _menuPanel == null || _ellipsisDots == null || _ellipsisButton == null)
            return;

        bool open = IsOpen;
        _buttonGrid.IsVisible = Mode != AppBarMode.Minimized;

        if (open)
        {
            _menuPanel.IsVisible = MenuItems.Count > 0;
            if (animate)
                _ = AnimateMenuIn();
        }
        else
        {
            _menuPanel.IsVisible = false;
            // 复位淡入起点
            foreach (var child in _menuPanel.Children.OfType<VisualElement>())
            {
                child.Opacity = 1;
                child.TranslationY = 0;
            }
        }

        if (animate)
            _ = _ellipsisButton.RotateToAsync(open ? 90 : 0, 120, Easing.CubicOut);
        else
            _ellipsisButton.Rotation = open ? 90 : 0;

        // WP8：caption 仅在展开时显示
        foreach (var button in Buttons)
            button.SetCaptionVisible(open);
    }

    private async Task AnimateMenuIn()
    {
        if (_menuPanel == null) return;

        var items = _menuPanel.Children.OfType<VisualElement>().ToList();
        foreach (var item in items)
        {
            item.Opacity = 0;
            item.TranslationY = 12;
        }

        var tasks = new List<Task>();
        foreach (var item in items)
            tasks.Add(AnimateItemIn(item));
        await Task.WhenAll(tasks);
    }

    private static async Task AnimateItemIn(VisualElement item)
    {
        await Task.WhenAll(
            item.FadeToAsync(1, 150, Easing.CubicOut),
            item.TranslateToAsync(0, 0, 150, Easing.CubicOut));
    }
}
