namespace MetroCompanionDemo
{
    public partial class MainPage : ContentPage
    {
        private bool _isPanorama;

        public MainPage()
        {
            InitializeComponent();

            this.BindingContext = this;
        }

        public IReadOnlyList<DemoItem> Trips => DemoData.Trips;
        public IReadOnlyList<DemoItem> Diary => DemoData.Diary;
        public IReadOnlyList<DemoItem> Feed => DemoData.Feed;
        public IReadOnlyList<DemoItem> Mine => DemoData.Mine;

        private void OnModeToggleClicked(object sender, EventArgs e)
        {
            _isPanorama = !_isPanorama;
            Hub.IsPanoramaMode = _isPanorama;
            ModeToggleBtn.Text = _isPanorama ? "切换到 Hub 模式" : "切换到 Panorama 模式";
        }

        private async void OnItemTapped(object sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: DemoItem item })
                return;

            // 新控件演示入口磁贴
            if (item.Title == "应用栏")
            {
                await Shell.Current.GoToAsync(nameof(AppBarDemoPage));
                return;
            }
            if (item.Title == "语义缩放")
            {
                await Shell.Current.GoToAsync(nameof(SemanticZoomDemoPage));
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(PivotDemoPage)}?item={Uri.EscapeDataString(item.Title)}");
        }
    }
}
