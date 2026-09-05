namespace MetroCompanionDemo
{
    [QueryProperty(nameof(ItemText), "item")]
    public partial class PivotDemoPage : ContentPage
    {
        public PivotDemoPage()
        {
            InitializeComponent();

            WordsList.ItemsSource = DemoData.Words;
        }

        /// <summary>
        /// 从 Hub 列表点击传入的词条，作为 Pivot 大标题与概览内容。
        /// </summary>
        public string ItemText
        {
            set
            {
                Pivot.Title = value;
                OverviewLabel.Text = value;
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
