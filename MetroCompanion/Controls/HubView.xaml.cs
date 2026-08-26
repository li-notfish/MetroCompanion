using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Dispatching;

namespace MetroCompanion.Controls;

[ContentProperty(nameof(Sections))]
public partial class HubView : ContentView
{
    private Image _parallaxBg;
    private ScrollView _scrollView;
    private HorizontalStackLayout _sectionsContainer;

    private bool _isSnapping;
    private IDispatcherTimer _snapTimer;

    public static readonly BindableProperty BackgroundSourceProperty = BindableProperty.Create(nameof(BackgroundSource), typeof(ImageSource), typeof(HubView));
    public static readonly BindableProperty IsParallaxEnabledProperty = BindableProperty.Create(nameof(IsParallaxEnabled), typeof(bool), typeof(HubView), true);
    public static readonly BindableProperty IsPanoramaModeProperty = BindableProperty.Create(nameof(IsPanoramaMode), typeof(bool), typeof(HubView), false, propertyChanged: OnIsPanoramaModeChanged);

    public ImageSource BackgroundSource { get => (ImageSource)GetValue(BackgroundSourceProperty); set => SetValue(BackgroundSourceProperty, value); }
    public bool IsParallaxEnabled { get => (bool)GetValue(IsParallaxEnabledProperty); set => SetValue(IsParallaxEnabledProperty, value); }
    public bool IsPanoramaMode { get => (bool)GetValue(IsPanoramaModeProperty); set => SetValue(IsPanoramaModeProperty, value); }

    public ObservableCollection<HubSection> Sections { get; } = new();

    public HubView()
    {
        InitializeComponent();
        Sections.CollectionChanged += OnSectionsChanged;
        SizeChanged += (s, e) => UpdateLayout();
        this.Unloaded += OnUnloaded;

        _snapTimer = Application.Current.Dispatcher.CreateTimer();
        _snapTimer.Interval = TimeSpan.FromMilliseconds(300);
        _snapTimer.Tick += OnSnapTimerTick;
    }

    private void OnUnloaded(object sender, EventArgs e)
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

        if (_scrollView != null) _scrollView.Scrolled += OnScrolled;
        RefreshSections();

        Dispatcher.Dispatch(() => UpdateLayout());
    }

    private static void OnIsPanoramaModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HubView hub)
        {
            bool isPanorama = (bool)newValue;
            foreach (var section in hub.Sections)
            {
                if (isPanorama)
                    section.ApplyPanoramaStyle();
                else
                    section.ApplyHubStyle();
            }
        }
    }

    private void OnSectionsChanged(object sender, NotifyCollectionChangedEventArgs e) => RefreshSections();

    private void RefreshSections()
    {
        if (_sectionsContainer == null) return;
        _sectionsContainer.Children.Clear();
        foreach (var s in Sections)
        {
            if (IsPanoramaMode)
                s.ApplyPanoramaStyle();
            else
                s.ApplyHubStyle();
            _sectionsContainer.Children.Add(s);
        }

        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => _ = PlayEntranceAnimation());
    }

    private void UpdateLayout()
    {
        if (Width <= 0 || _sectionsContainer == null) return;

        double ratio = DeviceInfo.Current.Idiom == DeviceIdiom.Phone ? 0.80 : 0.65;
        double targetWidth = Width * ratio;

        foreach (var child in _sectionsContainer.Children)
        {
            if (child is HubSection s) s.WidthRequest = targetWidth;
        }

        _sectionsContainer.InvalidateMeasure();

        if (_parallaxBg != null)
            _parallaxBg.WidthRequest = Width + (_sectionsContainer.Width * 0.2);
    }

    private void OnScrolled(object sender, ScrolledEventArgs e)
    {
        if (IsParallaxEnabled && _parallaxBg != null)
        {
            double parallaxOffset = -e.ScrollX * 0.1;
            _parallaxBg.TranslationX = parallaxOffset;
        }

        if (_isSnapping || _snapTimer == null) return;

        _snapTimer.Stop();
        _snapTimer.Start();
    }

    private void OnSnapTimerTick(object sender, EventArgs e)
    {
        if (_snapTimer != null) _snapTimer.Stop();

        if (_scrollView == null || _sectionsContainer == null) return;

        double scrollX = _scrollView.ScrollX;
        double targetX = CalculateNearestSection(scrollX);

        if (Math.Abs(targetX - scrollX) < 1) return;

        _isSnapping = true;
        _scrollView.ScrollToAsync(targetX, 0, false);

        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () => _isSnapping = false);
    }

    private double CalculateNearestSection(double currentScrollX)
    {
        if (_sectionsContainer == null || _sectionsContainer.Children.Count == 0)
            return 0;

        double accumulated = 0;
        double bestX = 0;
        double bestDistance = double.MaxValue;

        foreach (var child in _sectionsContainer.Children)
        {
            if (child is HubSection section)
            {
                double distance = Math.Abs(currentScrollX - accumulated);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestX = accumulated;
                }
                accumulated += section.WidthRequest;
            }
        }

        double maxScroll = accumulated - (_scrollView?.Width ?? 0);
        if (maxScroll < 0) maxScroll = 0;

        return Math.Clamp(bestX, 0, maxScroll);
    }

    public async Task PlayEntranceAnimation()
    {
        if (_sectionsContainer == null) return;
        int delay = 0;
        foreach (var child in _sectionsContainer.Children)
        {
            if (child is VisualElement el)
            {
                el.Opacity = 0;
                el.TranslationX = 200;
                _ = Task.Run(async () => {
                    await Task.Delay(delay);
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await Task.WhenAll(
                            el.TranslateTo(0, 0, 800, Easing.CubicOut),
                            el.FadeTo(1, 800, Easing.CubicOut)
                        );
                    });
                });
                delay += 100;
            }
        }
    }
}
