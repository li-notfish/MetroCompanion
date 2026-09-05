using System.Collections.ObjectModel;

namespace MetroCompanionDemo
{
    public partial class MainPage : ContentPage
    {
        private bool _isPanorama;

        public MainPage()
        {
            InitializeComponent();

            this.BindingContext = this;

            foreach (string s in DemoData.Words)
            {
                Tg.Add(s);
            }
        }

        private void OnModeToggleClicked(object sender, EventArgs e)
        {
            _isPanorama = !_isPanorama;
            Hub.IsPanoramaMode = _isPanorama;
            ModeToggleBtn.Text = _isPanorama ? "切换到 Hub 模式" : "切换到 Panorama 模式";
        }

        private async void OnItemTapped(object sender, TappedEventArgs e)
        {
            if (sender is Label { BindingContext: string item })
                await Shell.Current.GoToAsync($"{nameof(PivotDemoPage)}?item={Uri.EscapeDataString(item)}");
        }

        public ObservableCollection<string> Tg { get; set; } = new ObservableCollection<string>();
    }
}
