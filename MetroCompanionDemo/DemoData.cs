namespace MetroCompanionDemo
{
    /// <summary>Demo 结构化示例条目（磁贴/列表共用）。</summary>
    public record DemoItem(string Title, string Subtitle, string Detail, string TileColor);

    /// <summary>
    /// Demo 共享示例数据。
    /// </summary>
    public static class DemoData
    {
        // Metro 磁贴底色：强调蓝与半透明黑（叠在壁纸上）
        private const string Accent = "#0078D7";
        private const string TileDark = "#B3000000";

        public static IReadOnlyList<DemoItem> Trips { get; } = new DemoItem[]
        {
            new("京都", "2024 · 春", "哲学之道樱花满开，雨后石阶映苔青。", Accent),
            new("巴黎", "2023 · 秋", "蒙马特高地看落日，咖啡与手风琴。", TileDark),
            new("冰岛", "2023 · 夏", "午夜太阳悬在黑沙滩与玄武岩之上。", TileDark),
            new("敦煌", "2022 · 冬", "莫高窟第 257 窟的九色鹿本生画。", Accent),
        };

        public static IReadOnlyList<DemoItem> Diary { get; } = new DemoItem[]
        {
            new("十月九日", "相册", "把旧照片扫描进云盘，翻到 2010 年的 MWC 门票。", TileDark),
            new("九月廿三", "系统", "重装 Windows 8.1，开始屏幕磁贴还能自己配色。", Accent),
            new("九月十七", "代码", "深夜写完 Panorama 控件的第一版模板。", TileDark),
            new("九月一日", "折腾", "给旧笔记本换上 SSD，开机只要 11 秒。", TileDark),
        };

        public static IReadOnlyList<DemoItem> Feed { get; } = new DemoItem[]
        {
            new("相册", "新增 48 张照片 · 冰岛环岛", "", TileDark),
            new("网盘", "同步完成 · 3.2 GB", "", TileDark),
            new("邮件", "Metro 设计语言十周年专题", "", TileDark),
            new("天气", "多云 21°C · 西北风三级", "", TileDark),
        };

        public static IReadOnlyList<DemoItem> Mine { get; } = new DemoItem[]
        {
            new("收藏", "128 项", "", Accent),
            new("相册", "2,046 张", "", TileDark),
            new("音乐", "17 张专辑", "", TileDark),
            new("阅读", "42 篇待读", "", TileDark),
            new("主题", "深色 · 强调蓝", "", TileDark),
            new("关于", "MetroCompanion 1.0", "", TileDark),
        };

        /// <summary>按标题在所有分组中查找条目。</summary>
        public static DemoItem? Find(string title) =>
            Trips.Concat(Diary).Concat(Feed).Concat(Mine).FirstOrDefault(i => i.Title == title);

        /// <summary>返回条目所在的分组（找不到时回退到"我的"）。</summary>
        public static IReadOnlyList<DemoItem> SectionOf(string title)
        {
            if (Trips.Any(i => i.Title == title)) return Trips;
            if (Diary.Any(i => i.Title == title)) return Diary;
            if (Feed.Any(i => i.Title == title)) return Feed;
            return Mine;
        }
    }
}
