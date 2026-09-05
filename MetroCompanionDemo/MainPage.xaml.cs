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
            if (sender is BindableObject { BindingContext: DemoItem item })
                await Shell.Current.GoToAsync($"{nameof(PivotDemoPage)}?item={Uri.EscapeDataString(item.Title)}");
        }
    }
}
