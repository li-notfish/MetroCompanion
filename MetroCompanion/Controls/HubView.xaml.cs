using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using MetroCompanion.Behaviores;
using Microsoft.Maui.Dispatching;

namespace MetroCompanion.Controls;

[ContentProperty(nameof(Sections))]
public partial class HubView : ContentView
{
    private Image? _parallaxBg;
    private ScrollView? _scrollView;
    private HorizontalStackLayout? _sectionsContainer;

    private IDispatcherTimer? _snapTimer;
    private DateTime _lastSnapTime = DateTime.MinValue;
    private bool _isAnimating;

    public static readonly BindableProperty BackgroundSourceProperty = BindableProperty.Create(nameof(BackgroundSource), typeof(ImageSource), typeof(HubView));
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(nameof(Header), typeof(View), typeof(HubView), propertyChanged: OnHeaderChanged);
    public static readonly BindableProperty IsParallaxEnabledProperty = BindableProperty.Create(nameof(IsParallaxEnabled), typeof(bool), typeof(HubView), true);
    public static readonly BindableProperty IsPanoramaModeProperty = BindableProperty.Create(nameof(IsPanoramaMode), typeof(bool), typeof(HubView), false, propertyChanged: OnIsPanoramaModeChanged);
    public static readonly BindableProperty PanoramaTitleProperty = BindableProperty.Create(nameof(PanoramaTitle), typeof(string), typeof(HubView), propertyChanged: OnPanoramaTitleChanged);
    public static readonly BindableProperty SnapThresholdProperty = BindableProperty.Create(nameof(SnapThreshold), typeof(double), typeof(HubView), 0.2);

    public ImageSource BackgroundSource { get => (ImageSource)GetValue(BackgroundSourceProperty); set => SetValue(BackgroundSourceProperty, value); }
    public View Header { get => (View)GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
    public bool IsParallaxEnabled { get => (bool)GetValue(IsParallaxEnabledProperty); set => SetValue(IsParallaxEnabledProperty, value); }
    public bool IsPanoramaMode { get => (bool)GetValue(IsPanoramaModeProperty); set => SetValue(IsPanoramaModeProperty, value); }
    /// <summary>Panorama 模式下显示在第一面板的控件级大标题。</summary>
    public string PanoramaTitle { get => (string)GetValue(PanoramaTitleProperty); set => SetValue(PanoramaTitleProperty, value); }
    public double SnapThreshold { get => (double)GetValue(SnapThresholdProperty); set => SetValue(SnapThresholdProperty, value); }

    public ObservableCollection<HubSection> Sections { get; } = new();

    public HubView()
    {
        InitializeComponent();
        Sections.CollectionChanged += OnSectionsChanged;
        SizeChanged += (s, e) => UpdateLayout();
        this.Unloaded += OnUnloaded;

        if (Application.Current != null)
        {
            _snapTimer = Application.Current.Dispatcher.CreateTimer();
            _snapTimer.Interval = TimeSpan.FromMilliseconds(300);
            _snapTimer.Tick += OnSnapTimerTick;
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        _snapTimer?.Stop();
        _snapTimer = null;

        if (_scrollView != null)
            _scrollView.Scrolled -= OnScrolled;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _parallaxBg = GetTemplateChild("PART_ParallaxBg") as Image;
        _scrollView = GetTemplateChild("PART_ScrollView") as ScrollView;
        _sectionsContainer = GetTemplateChild("PART_SectionsContainer") as HorizontalStackLayout;

        if (_scrollView != null)
        {
            _scrollView.Scrolled += OnScrolled;
#if WINDOWS
            // 滚轮行为以代码挂载：模板内 OnPlatform<Behavior> 在 Release AOT 下会
            // 尝试实例化抽象 Behavior 导致崩溃
            if (_scrollView.Behaviors.Count == 0)
                _scrollView.Behaviors.Add(new HubHorizontalScrollBehavior());
#endif
        }
        RefreshSections();

        Dispatcher.Dispatch(() => UpdateLayout());
    }

    private static void OnHeaderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Header 经 ContentPresenter + TemplateBinding 挂入，无逻辑父级，BindingContext 需显式传播
        if (bindable is HubView hub && newValue is View header)
            header.BindingContext = hub.BindingContext;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (Header != null)
            Header.BindingContext = BindingContext;
    }

    private static void OnIsPanoramaModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HubView hub)
            hub.ApplySectionStyles();
    }

    private static void OnPanoramaTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HubView hub && hub.IsPanoramaMode)
            hub.ApplySectionStyles();
    }
    private void ApplySectionStyles()
    {
        // WP8 Panorama：仅第一面板显示控件级大标题，其余面板为小表头
        for (int i = 0; i < Sections.Count; i++)
        {
            if (IsPanoramaMode)
                Sections[i].ApplyPanoramaStyle(isFirst: i == 0, PanoramaTitle);
            else
                Sections[i].ApplyHubStyle();
        }
    }

    private void OnSectionsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshSections();

    private void RefreshSections()
    {
        if (_sectionsContainer == null) return;
        _sectionsContainer.Children.Clear();
        ApplySectionStyles();
        foreach (var s in Sections)
            _sectionsContainer.Children.Add(s);

        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => _ = PlayEntranceAnimation());
    }

    private void UpdateLayout()
    {
        if (Width <= 0 || _sectionsContainer == null) return;

        double ratio = DeviceInfo.Current.Idiom == DeviceIdiom.Phone ? 0.80 : 0.65;
        double targetWidth = Width * ratio;

        foreach (var child in _sectionsContainer.Children)
        {
            // 真机 PanoramaItem 宽度可变：Panorama 模式下尊重调用方显式设置的宽度
            if (child is HubSection s && !(IsPanoramaMode && s.WidthRequest > 0))
                s.WidthRequest = targetWidth;
        }

        _sectionsContainer.InvalidateMeasure();

        if (_parallaxBg != null)
            _parallaxBg.WidthRequest = Width + (_sectionsContainer.Width * 0.2);
    }

    private void OnScrolled(object? sender, ScrolledEventArgs e)
    {
        if (IsParallaxEnabled && _parallaxBg != null)
        {
            double parallaxOffset = -e.ScrollX * 0.1;
            _parallaxBg.TranslationX = parallaxOffset;
        }

        // WP8 Panorama 首面板大标题：滚过约半个视口后完全淡出，回滚时恢复
        if (IsPanoramaMode && Sections.Count > 0 && Width > 0)
        {
            double fade = Math.Clamp(1 - e.ScrollX / (Width * 0.5), 0, 1);
            Sections[0].SetPanoramaTitleOpacity(0.65 * fade);
        }

        if (_isAnimating || DateTime.Now - _lastSnapTime < TimeSpan.FromMilliseconds(200))
            return;

        _snapTimer?.Stop();
        _snapTimer?.Start();
    }

    private void OnSnapTimerTick(object? sender, EventArgs e)
    {
        _snapTimer?.Stop();

        if (_scrollView == null || _sectionsContainer == null) return;

        double scrollX = _scrollView.ScrollX;
        var (snapTarget, _) = FindSnapTarget(scrollX);

        if (snapTarget < 0) return;

        _ = AnimateSnap(scrollX, snapTarget);
    }

    private (double targetX, double sectionWidth) FindSnapTarget(double scrollX)
    {
        if (_sectionsContainer == null || _sectionsContainer.Children.Count == 0)
            return (-1, 0);

        double accumulated = 0;
        double currentSectionStart = 0;
        double currentSectionWidth = 0;
        bool foundCurrent = false;

        foreach (var child in _sectionsContainer.Children)
        {
            if (child is HubSection section)
            {
                if (!foundCurrent && scrollX >= accumulated - 1)
                {
                    currentSectionStart = accumulated;
                    currentSectionWidth = section.WidthRequest;
                    foundCurrent = true;
                }
                accumulated += section.WidthRequest;
            }
        }

        double threshold = currentSectionWidth * SnapThreshold;
        double distToCurrent = Math.Abs(scrollX - currentSectionStart);
        double distToNext = Math.Abs(scrollX - (currentSectionStart + currentSectionWidth));

        if (distToNext < threshold)
            return (currentSectionStart + currentSectionWidth, currentSectionWidth);
        if (distToCurrent < threshold)
            return (currentSectionStart, currentSectionWidth);

        return (-1, currentSectionWidth);
    }

    private async Task AnimateSnap(double startX, double targetX)
    {
        if (_isAnimating || _scrollView == null) return;

        _isAnimating = true;
        _lastSnapTime = DateTime.Now;

        const int durationMs = 150;
        const int frameDelayMs = 16;
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < durationMs)
        {
            double progress = Math.Min((double)sw.ElapsedMilliseconds / durationMs, 1.0);
            double eased = CubicOut(progress);
            double currentX = startX + (targetX - startX) * eased;
            _ = _scrollView.ScrollToAsync(currentX, 0, false);
            await Task.Delay(frameDelayMs);
        }

        _ = _scrollView.ScrollToAsync(targetX, 0, false);
        _isAnimating = false;
    }

    private static double CubicOut(double t)
    {
        return 1.0 - Math.Pow(1.0 - t, 3);
    }

    public async Task PlayEntranceAnimation()
    {
        if (_sectionsContainer == null) return;
        int delay = 0;
        var tasks = new List<Task>();
        foreach (var child in _sectionsContainer.Children)
        {
            if (child is VisualElement el)
            {
                el.Opacity = 0;
                el.TranslationX = 200;
                tasks.Add(AnimateIn(el, delay));
                delay += 100;
            }
        }
        await Task.WhenAll(tasks);
    }

    private static async Task AnimateIn(VisualElement el, int delayMs)
    {
        if (delayMs > 0)
            await Task.Delay(delayMs);
        await Task.WhenAll(
            el.TranslateToAsync(0, 0, 800, Easing.CubicOut),
            el.FadeToAsync(1, 800, Easing.CubicOut));
    }
}
