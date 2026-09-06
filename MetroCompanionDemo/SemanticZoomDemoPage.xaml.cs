namespace MetroCompanionDemo
{
    /// <summary>缩小视图字母块绑定的分组对象（参考 WPToolkit AlphaKeyGroup 模式）。</summary>
    public class LetterGroup : List<AppInfo>
    {
        public string Key { get; }

        public LetterGroup(string key, IEnumerable<AppInfo> items) : base(items) => Key = key;
    }

    /// <summary>语义缩放演示的应用条目。</summary>
    public record AppInfo(string Name);

    /// <summary>
    /// Win8 起始屏幕风格的语义缩放演示：
    /// 放大视图为按字母分组的应用磁贴，缩小视图为字母网格，点字母回到对应分组。
    /// </summary>
    public partial class SemanticZoomDemoPage : ContentPage
    {
        private static readonly string[] AppNames =
        {
            "Alarms", "Banking", "Bing", "Calculator", "Calendar", "Camera",
            "Clock", "Contacts", "Documents", "Drive", "Email", "Feedback",
            "Finance", "Games", "Health", "Help", "Internet Explorer", "Jokes",
            "Keyboard", "Lights", "Maps", "Media", "Music", "News",
            "Notes", "OneNote", "Paint", "Photos", "Podcasts", "Reader",
            "Settings", "SkyDrive", "Sports", "Store", "Travel", "Video",
            "Weather", "Xbox", "Yahoo", "Zune"
        };

        public IReadOnlyList<LetterGroup> Groups { get; }

        public SemanticZoomDemoPage()
        {
            InitializeComponent();

            Groups = AppNames
                .GroupBy(n => n[0].ToString())
                .OrderBy(g => g.Key)
                .Select(g => new LetterGroup(g.Key, g.Select(n => new AppInfo(n)).OrderBy(n => n.Name)))
                .ToList();

            BindingContext = this;
        }

        private async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");

        private void OnLetterTapped(object sender, TappedEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: LetterGroup group })
                return;

            // 缩回放大视图并滚动到对应分组（Win8 起始屏幕行为）
            Zoom.ZoomIn();

            var first = group.FirstOrDefault();
            if (first != null)
                AppsList.ScrollTo(first, group, ScrollToPosition.Start, animate: true);
        }
    }
}
