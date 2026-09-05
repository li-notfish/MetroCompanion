namespace MetroCompanionDemo
{
    [QueryProperty(nameof(ItemText), "item")]
    public partial class PivotDemoPage : ContentPage
    {
        public PivotDemoPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 从 Hub 点击传入的条目标题：作为 Pivot 大标题，并加载该条目
        /// 所在分组的全部条目作为"内容"页列表。
        /// </summary>
        public string ItemText
        {
            set
            {
                Pivot.Title = value;

                var item = DemoData.Find(value);
                OverviewLabel.Text = value;
                OverviewSubLabel.Text = item == null
                    ? "这是 WP8.1 风格的 Pivot 页。"
                    : string.IsNullOrEmpty(item.Detail)
                        ? item.Subtitle
                        : $"{item.Subtitle} —— {item.Detail}";

                ContentList.ItemsSource = DemoData.SectionOf(value);
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
