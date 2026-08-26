using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MetroCompanion.Controls;

[ContentProperty(nameof(Sections))]
public partial class HubView : ContentView
{
    private Image _parallaxBg;
    private ScrollView _scrollView;
    private HorizontalStackLayout _sectionsContainer;

    public static readonly BindableProperty BackgroundSourceProperty = BindableProperty.Create(nameof(BackgroundSource), typeof(ImageSource), typeof(HubView));
    public static readonly BindableProperty IsParallaxEnabledProperty = BindableProperty.Create(nameof(IsParallaxEnabled), typeof(bool), typeof(HubView), true);

    public ImageSource BackgroundSource { get => (ImageSource)GetValue(BackgroundSourceProperty); set => SetValue(BackgroundSourceProperty, value); }
    public bool IsParallaxEnabled { get => (bool)GetValue(IsParallaxEnabledProperty); set => SetValue(IsParallaxEnabledProperty, value); }

    public ObservableCollection<HubSection> Sections { get; } = new();

    public HubView()
    {
        InitializeComponent();
        Sections.CollectionChanged += OnSectionsChanged;
        SizeChanged += (s, e) => UpdateLayout();

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

    private void OnSectionsChanged(object sender, NotifyCollectionChangedEventArgs e) => RefreshSections();

    private void RefreshSections()
    {
        if (_sectionsContainer == null) return;
        _sectionsContainer.Children.Clear();
        foreach (var s in Sections) _sectionsContainer.Children.Add(s);

        // 动画触发
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => _ = PlayEntranceAnimation());
    }

    private void UpdateLayout()
    {
        if (Width <= 0 || _sectionsContainer == null) return;

        // 保持 1.5 Section 的视觉逻辑
        double ratio = DeviceInfo.Current.Idiom == DeviceIdiom.Phone ? 0.85 : 0.65;
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
            _parallaxBg.TranslationX = -e.ScrollX * 0.15;
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