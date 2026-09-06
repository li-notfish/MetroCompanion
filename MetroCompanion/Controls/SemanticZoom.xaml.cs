using MetroCompanion.Behaviores;
using MetroCompanion.Styles;
using Microsoft.Maui.Dispatching;

namespace MetroCompanion.Controls;

/// <summary>
/// 还原 WinRT SemanticZoom 的 MAUI 控件：叠放放大/缩小两个视图，
/// 捏合、Ctrl+滚轮（Windows，经 <see cref="SemanticZoomWheelBehavior"/> 挂载）或
/// 右下角缩放按钮切换，切换为约 200ms 交叉淡化 + 轻微缩放（对应 SDK 的
/// FadeIn/FadeOut 视觉状态）。
/// </summary>
/// <remarks>
/// 缩放按钮按 Win8 模板规格：21×21、1px 边框、右下 Margin 0,0,7,24，
/// 静止 3 秒自动淡出、交互时重新显示。放大视图内的定位（如点字母滚到分组）
/// 由内容自行实现。
/// </remarks>
[ContentProperty(nameof(ZoomedInView))]
public partial class SemanticZoom : ContentView
{
    private ContentPresenter? _zoomedInPresenter;
    private ContentPresenter? _zoomedOutPresenter;
    private Border? _zoomButton;
    private Label? _zoomGlyph;

    private IDispatcherTimer? _hideButtonTimer;
    private bool _isAnimatingZoom;

    public static readonly BindableProperty ZoomedInViewProperty = BindableProperty.Create(nameof(ZoomedInView), typeof(ContentView), typeof(SemanticZoom), propertyChanged: OnZoomedViewChanged);
    public static readonly BindableProperty ZoomedOutViewProperty = BindableProperty.Create(nameof(ZoomedOutView), typeof(ContentView), typeof(SemanticZoom), propertyChanged: OnZoomedViewChanged);
    public static readonly BindableProperty IsZoomedOutActiveProperty = BindableProperty.Create(nameof(IsZoomedOutActive), typeof(bool), typeof(SemanticZoom), false, BindingMode.TwoWay, propertyChanged: OnIsZoomedOutActiveChanged);
    public static readonly BindableProperty CanChangeViewsProperty = BindableProperty.Create(nameof(CanChangeViews), typeof(bool), typeof(SemanticZoom), true);
    public static readonly BindableProperty IsZoomButtonVisibleProperty = BindableProperty.Create(nameof(IsZoomButtonVisible), typeof(bool), typeof(SemanticZoom), true, propertyChanged: OnIsZoomButtonVisibleChanged);

    /// <summary>放大视图（默认内容，ContentProperty）。</summary>
    public ContentView ZoomedInView { get => (ContentView)GetValue(ZoomedInViewProperty); set => SetValue(ZoomedInViewProperty, value); }
    /// <summary>缩小视图（Win8 起始屏幕风格通常为字母/分组网格）。</summary>
    public ContentView ZoomedOutView { get => (ContentView)GetValue(ZoomedOutViewProperty); set => SetValue(ZoomedOutViewProperty, value); }
    /// <summary>当前是否处于缩小视图（TwoWay）。</summary>
    public bool IsZoomedOutActive { get => (bool)GetValue(IsZoomedOutActiveProperty); set => SetValue(IsZoomedOutActiveProperty, value); }
    /// <summary>是否允许手势/按钮切换视图（程序直接赋值不受限）。</summary>
    public bool CanChangeViews { get => (bool)GetValue(CanChangeViewsProperty); set => SetValue(CanChangeViewsProperty, value); }
    /// <summary>是否显示 Win8 风格右下角缩放按钮。</summary>
    public bool IsZoomButtonVisible { get => (bool)GetValue(IsZoomButtonVisibleProperty); set => SetValue(IsZoomButtonVisibleProperty, value); }

    public SemanticZoom()
    {
        InitializeComponent();

        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnPinchUpdated;
        GestureRecognizers.Add(pinch);

        if (Application.Current != null)
        {
            _hideButtonTimer = Application.Current.Dispatcher.CreateTimer();
            _hideButtonTimer.Interval = MetroTokens.SemanticZoomButtonHideDelay;
            _hideButtonTimer.Tick += OnHideButtonTimerTick;
        }

        Unloaded += OnZoomUnloaded;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _zoomedInPresenter = GetTemplateChild("PART_ZoomedInPresenter") as ContentPresenter;
        _zoomedOutPresenter = GetTemplateChild("PART_ZoomedOutPresenter") as ContentPresenter;
        _zoomButton = GetTemplateChild("PART_ZoomButton") as Border;
        _zoomGlyph = GetTemplateChild("PART_ZoomButtonGlyph") as Label;

        ApplyButtonTheme();

        if (_zoomButton != null)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => ToggleZoom();
            _zoomButton.GestureRecognizers.Add(tap);

            // 按下交换底色/前景（SemanticZoomButtonPressed* 资源语义）
            var pointer = new PointerGestureRecognizer();
            pointer.PointerPressed += (_, _) => SetButtonPressed(true);
            pointer.PointerReleased += (_, _) => SetButtonPressed(false);
            pointer.PointerExited += (_, _) => SetButtonPressed(false);
            _zoomButton.GestureRecognizers.Add(pointer);
        }

#if WINDOWS
        // Ctrl+滚轮切换视图以代码挂载：模板内 OnPlatform<Behavior> 在 Release AOT 下
        // 会尝试实例化抽象 Behavior 导致崩溃（同 HubView/PivotView）
        if (Behaviors.Count == 0)
            Behaviors.Add(new SemanticZoomWheelBehavior());
#endif

        ApplyZoomState(animate: false);
        ShowZoomButton();
    }

    private void OnZoomUnloaded(object? sender, EventArgs e)
    {
        _hideButtonTimer?.Stop();
        _hideButtonTimer = null;
        _zoomedInPresenter = null;
        _zoomedOutPresenter = null;
        _zoomButton = null;
        _zoomGlyph = null;
    }

    /// <summary>切回放大视图。</summary>
    public void ZoomIn()
    {
        if (CanChangeViews && IsZoomedOutActive)
            IsZoomedOutActive = false;
    }

    /// <summary>
    /// 经 ContentPresenter + TemplateBinding 挂入的内容视图不会被设置逻辑父级，
    /// BindingContext 无法沿树继承——控件显式向两个视图传播。
    /// </summary>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (ZoomedInView != null)
            ZoomedInView.BindingContext = BindingContext;
        if (ZoomedOutView != null)
            ZoomedOutView.BindingContext = BindingContext;
    }

    private static void OnZoomedViewChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SemanticZoom zoom && newValue is ContentView view)
            view.BindingContext = zoom.BindingContext;
    }

    /// <summary>切到缩小视图。</summary>
    public void ZoomOut()
    {
        if (CanChangeViews && !IsZoomedOutActive)
            IsZoomedOutActive = true;
    }

    /// <summary>在放大/缩小视图间切换。</summary>
    public void ToggleZoom()
    {
        if (CanChangeViews)
            IsZoomedOutActive = !IsZoomedOutActive;
    }

    /// <summary>
    /// 内容/宿主通知发生用户交互（滚轮、指针移动等）：缩放按钮重新显示，
    /// 并重新计时 3 秒自动淡出（Win8 ZoomOutButtonStates 语义）。
    /// Windows 滚轮行为内部会调用。
    /// </summary>
    public void NotifyUserInteraction() => ShowZoomButton();

    private static void OnIsZoomedOutActiveChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SemanticZoom zoom)
        {
            _ = zoom.ApplyZoomStateAsync(animate: true);
            zoom.ShowZoomButton();
        }
    }

    private static void OnIsZoomButtonVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SemanticZoom zoom && zoom._zoomButton != null)
        {
            zoom._zoomButton.IsVisible = (bool)newValue;
            if (zoom._zoomButton.IsVisible)
                zoom.ShowZoomButton();
        }
    }

    private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        if (!CanChangeViews)
            return;

        ShowZoomButton();

        // 捏合结束时按累计方向判定：张开 → 放大视图，捏拢 → 缩小视图
        if (e.Status != GestureStatus.Completed && e.Status != GestureStatus.Canceled)
            return;

        if (e.Scale > 1.1 && IsZoomedOutActive)
            IsZoomedOutActive = false;
        else if (e.Scale < 0.9 && !IsZoomedOutActive)
            IsZoomedOutActive = true;
    }

    private void ShowZoomButton()
    {
        if (_zoomButton == null || !IsZoomButtonVisible)
            return;

        _hideButtonTimer?.Stop();
        _hideButtonTimer?.Start();

        if (_zoomButton.Opacity < 1)
            _ = _zoomButton.FadeToAsync(1, 150, Easing.CubicOut);
    }

    private void OnHideButtonTimerTick(object? sender, EventArgs e)
    {
        _hideButtonTimer?.Stop();
        if (_zoomButton != null)
            _ = _zoomButton.FadeToAsync(0, 150, Easing.CubicOut);
    }

    private void ApplyButtonTheme()
    {
        if (_zoomButton == null || _zoomGlyph == null)
            return;

        _zoomButton.BackgroundColor = MetroTokens.SemanticZoomButtonBackground;
        _zoomButton.Stroke = MetroTokens.SemanticZoomButtonForeground;
        _zoomGlyph.TextColor = MetroTokens.SemanticZoomButtonForeground;
        _zoomGlyph.FontSize = MetroTokens.SemanticZoomButtonFontSize;
        _zoomGlyph.FontFamily = MetroTokens.IsPhone ? MetroTokens.FontFamily : "Segoe UI Symbol";
        _zoomButton.Margin = MetroTokens.SemanticZoomButtonMargin;
    }

    private void SetButtonPressed(bool pressed)
    {
        if (_zoomButton == null || _zoomGlyph == null)
            return;

        _zoomButton.BackgroundColor = pressed
            ? MetroTokens.SemanticZoomButtonPressedBackground
            : MetroTokens.SemanticZoomButtonBackground;
        _zoomGlyph.TextColor = pressed
            ? MetroTokens.SemanticZoomButtonPressedForeground
            : MetroTokens.SemanticZoomButtonForeground;
    }

    private void ApplyZoomState(bool animate)
    {
        if (_zoomedInPresenter == null || _zoomedOutPresenter == null)
            return;

        bool zoomedOut = IsZoomedOutActive;
        var appearing = zoomedOut ? _zoomedOutPresenter : _zoomedInPresenter;
        var disappearing = zoomedOut ? _zoomedInPresenter : _zoomedOutPresenter;

        appearing.IsVisible = true;
        appearing.InputTransparent = false;
        appearing.Opacity = 1;
        appearing.Scale = 1;

        disappearing.IsVisible = false;
        disappearing.InputTransparent = true;
        disappearing.Opacity = 1;
        disappearing.Scale = 1;
        _ = animate; // 无动画路径：直接置位
    }

    private async Task ApplyZoomStateAsync(bool animate)
    {
        if (_zoomedInPresenter == null || _zoomedOutPresenter == null)
            return;

        if (!animate || _isAnimatingZoom)
        {
            ApplyZoomState(animate: false);
            return;
        }

        _isAnimatingZoom = true;
        try
        {
            bool zoomedOut = IsZoomedOutActive;
            var appearing = zoomedOut ? _zoomedOutPresenter : _zoomedInPresenter;
            var disappearing = zoomedOut ? _zoomedInPresenter : _zoomedOutPresenter;

            // SDK 语义：两视图交叉淡化，出现方带轻微缩放
            double fromScale = zoomedOut ? 1.15 : 0.85;
            appearing.IsVisible = true;
            appearing.InputTransparent = false;
            appearing.Opacity = 0;
            appearing.Scale = fromScale;

            await Task.WhenAll(
                appearing.FadeToAsync(1, MetroTokens.SemanticZoomTransitionMs, Easing.CubicOut),
                appearing.ScaleToAsync(1, MetroTokens.SemanticZoomTransitionMs, Easing.CubicOut),
                disappearing.FadeToAsync(0, MetroTokens.SemanticZoomTransitionMs, Easing.CubicOut));

            disappearing.IsVisible = false;
            disappearing.InputTransparent = true;
            disappearing.Opacity = 1;
            disappearing.Scale = 1;
        }
        finally
        {
            _isAnimatingZoom = false;
        }
    }
}
