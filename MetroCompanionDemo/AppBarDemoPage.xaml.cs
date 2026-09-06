namespace MetroCompanionDemo
{
    public partial class AppBarDemoPage : ContentPage
    {
        private bool _minimized;
        private double _opacity = 1.0;

        public AppBarDemoPage()
        {
            InitializeComponent();
            ContentList.ItemsSource = DemoData.Mine;
        }

        private async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        private void OnModeToggleClicked(object sender, EventArgs e)
        {
            _minimized = !_minimized;
            Bar.Mode = _minimized ? MetroCompanion.Controls.AppBarMode.Minimized : MetroCompanion.Controls.AppBarMode.Default;
            ModeBtn.Text = _minimized ? "Default 模式" : "Minimized 模式";
            StatusLabel.Text = $"Mode = {Bar.Mode}";
        }

        private void OnOpacityToggleClicked(object sender, EventArgs e)
        {
            // WP8 Opacity 语义：0.5 半透明 / 1 不透明
            _opacity = _opacity == 1.0 ? 0.5 : 1.0;
            Bar.Opacity = _opacity;
            OpacityBtn.Text = _opacity == 1.0 ? "Opacity 0.5" : "Opacity 1.0";
            StatusLabel.Text = $"Opacity = {_opacity}";
        }

        private void OnPageTapped(object sender, TappedEventArgs e)
        {
            // 点击页面其余区域收起"…"菜单（WP8 行为）
            if (Bar.IsOpen)
                Bar.Close();
        }

        private void OnAddClicked(object sender, EventArgs e) => StatusLabel.Text = "命令：添加";
        private void OnEditClicked(object sender, EventArgs e) => StatusLabel.Text = "命令：编辑";
        private void OnDeleteClicked(object sender, EventArgs e) => StatusLabel.Text = "命令：删除（禁用态示例，不应出现）";
        private void OnSettingsClicked(object sender, EventArgs e) => StatusLabel.Text = "菜单：设置";
        private void OnAboutClicked(object sender, EventArgs e) => StatusLabel.Text = "菜单：关于";
    }
}
