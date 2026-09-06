namespace MetroCompanion.Styles;

/// <summary>
/// Metro 设计语言令牌（Windows 8 / 8.1 时期规格）。
/// 默认值按设备类型自适应——与当年 WinRT 在不同设备家族使用不同默认值一致；
/// 控件属性与 MetroTheme.xaml 均可覆盖。
/// 数值来源：Windows Phone Kits 8.1 generic.xaml（WinRT 手机）、Windows Kits 8.0
/// generic.xaml（桌面 WinRT）、Windows Phone v8.1 ThemeResources.xaml（Silverlight）。
/// </summary>
public static class MetroTokens
{
    public static bool IsPhone => DeviceInfo.Current.Idiom == DeviceIdiom.Phone;

    // ---- 颜色 ----
    /// <summary>Windows 8 默认强调蓝。</summary>
    public static readonly Color AccentColor = Color.FromArgb("#0078D7");
    public static readonly Color ForegroundColor = Colors.White;
    /// <summary>次级文本：白 70%。</summary>
    public static readonly Color SubtleForegroundColor = Color.FromArgb("#B3FFFFFF");

    // ---- 字体 ----
    /// <summary>常规字体。手机上的 Segoe WP 在非 Windows 平台会回退为平台默认。</summary>
    public static string FontFamily => "Segoe UI";
    /// <summary>Semibold 字重（Windows 下以独立字体家族名生效）。</summary>
    public static string SemiboldFontFamily => "Segoe UI Semibold";
    /// <summary>Semilight 字重（Pivot 表头规格 PivotHeaderItemFontFamily = Segoe WP SemiLight）。</summary>
    public static string SemilightFontFamily => "Segoe UI Semilight";
    /// <summary>Light 字重（大标题用）。</summary>
    public static string LightFontFamily => "Segoe UI Light";

    // ---- 字号（桌面 / 手机两套，对应 WinRT 8.1 与 WP8.1 的默认值）----
    /// <summary>Pivot 表头：桌面 24（PivotHeaderItemFontSize），手机 57（WP8.1 generic.xaml）。</summary>
    public static double PivotHeaderItemFontSize => IsPhone ? 57 : 24;
    /// <summary>Pivot 表头字距（PivotHeaderItemCharacterSpacing = -25，单位 1/1000 em）。</summary>
    public const double PivotHeaderItemCharacterSpacing = -25;
    /// <summary>
    /// Pivot 表头间隙：WinRT PivotHeaderItemMargin 为左 16（相邻间隙 16），
    /// 桌面沿用早期规格 24（左右各 12）。
    /// </summary>
    public static double PivotHeaderItemSpacing => IsPhone ? 16 : 24;
    /// <summary>Pivot 表头底部留白（PivotHeaderItemPadding 底 6.5）。</summary>
    public const double PivotHeaderItemPaddingBottom = 6.5;
    /// <summary>Pivot 大标题：桌面 46（HubHeaderThemeFontSize），手机 64。</summary>
    public static double PivotTitleFontSize => IsPhone ? 64 : 46;
    /// <summary>未选中 Pivot 表头透明度：Silverlight PhonePivotUnselectedItemOpacity = 0.4，
    /// 与 WinRT PhoneControlDisabledColor #66FFFFFF 一致。</summary>
    public const double UnselectedHeaderOpacity = 0.4;

    /// <summary>Hub 大标题：桌面 46（WinRT 8.0/8.1 桌面 HubHeaderFontSize），手机 78（WP8.1 generic.xaml）。</summary>
    public static double HubHeaderFontSize => IsPhone ? 78 : 46;
    /// <summary>Hub 大标题字距（HubHeaderCharacterSpacing = -22，单位 1/1000 em）。</summary>
    public const double HubHeaderCharacterSpacing = -22;
    /// <summary>Hub 节标题：桌面 26.667，手机 19（WP8.1 HubSectionHeaderFontSize）。</summary>
    public static double HubSectionHeaderFontSize => IsPhone ? 19 : 26.667;
    /// <summary>Hub 节标题字距（HubSectionHeaderCharacterSpacing = -10，单位 1/1000 em）。</summary>
    public const double HubSectionHeaderCharacterSpacing = -10;
    /// <summary>Hub 大标题边距（HubHeaderMarginThickness = 15,1,0,0）。</summary>
    public static Thickness HubHeaderMargin => new(15, 1, 0, 0);
    /// <summary>Hub 节标题边距（HubSectionHeaderMarginThickness = -1,5,0,31.5）。</summary>
    public static Thickness HubSectionHeaderMargin => new(-1, 5, 0, 31.5);
    /// <summary>Panorama 大标题：手机 165px（WP8 真机规格）；桌面视口宽得多，收敛到 100px。</summary>
    public static double PanoramaTitleFontSize => IsPhone ? 165 : 100;
    /// <summary>PanoramaItem 小表头（Silverlight PanoramaItemHeaderFontSize = 66，50pt Semilight）。</summary>
    public const double PanoramaItemHeaderFontSize = 66;
    /// <summary>PanoramaItem 表头字距（PanoramaItemCharacterSpacing = -35，单位 1/1000 em）。</summary>
    public const double PanoramaItemHeaderCharacterSpacing = -35;

    // ---- ApplicationBar（WP8 Silverlight 应用栏）----
    /// <summary>应用栏高度（WP8 设计规格 72px）。</summary>
    public const double AppBarHeight = 72;
    /// <summary>图标按钮区域（官方图标 PNG 即 48×48，透明边距内含约 26×26 glyph）。</summary>
    public const double AppBarIconSize = 48;
    /// <summary>图标按钮 caption 字号（WP8 规格 12pt ≈ 16px，展开时显示）。</summary>
    public const double AppBarCaptionFontSize = 16;
    /// <summary>菜单项字号（PhoneFontSizeMediumLarge = 25.333，19pt）。</summary>
    public const double AppBarMenuItemFontSize = 25.333;
    /// <summary>菜单项左右缩进（WPToolkit ContextMenu MenuItem 规格 25）。</summary>
    public const double AppBarMenuItemIndent = 25;
    /// <summary>应用栏背景：PhoneChromeColor（暗 #FF1F1F1F / 亮 #FFDDDDDD）。</summary>
    public static Color AppBarBackground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#FFDDDDDD")
        : Color.FromArgb("#FF1F1F1F");
    /// <summary>应用栏前景（AppBarItemForegroundThemeBrush = PhoneForegroundColor）。</summary>
    public static Color AppBarForeground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#DE000000")
        : Colors.White;
    /// <summary>禁用态前景（PhoneDisabledColor：暗 #66FFFFFF / 亮 #66000000）。</summary>
    public static Color AppBarItemDisabledForeground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#66000000")
        : Color.FromArgb("#66FFFFFF");
    /// <summary>指针悬停底色（AppBarItemPointerOverBackgroundThemeBrush = PhoneBaseLowColor）。</summary>
    public static Color AppBarItemPointerOverBackground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#40000000")
        : Color.FromArgb("#73FFFFFF");

    // ---- SemanticZoom（WinRT SemanticZoom）----
    /// <summary>缩放按钮字号：桌面 14.667（8.0 generic.xaml），手机 11.73（WP8.1 generic.xaml）。</summary>
    public static double SemanticZoomButtonFontSize => IsPhone ? 11.73 : 14.667;
    /// <summary>缩放按钮尺寸（桌面模板 Root Border 21×21，1px 边框）。</summary>
    public const double SemanticZoomButtonSize = 21;
    /// <summary>缩放按钮右下角边距（桌面模板 Margin 0,0,7,24）。</summary>
    public static Thickness SemanticZoomButtonMargin => new(0, 0, 7, 24);
    /// <summary>缩放按钮静止后自动淡出的等待时间（Win8 ZoomOutButtonStates：3s）。</summary>
    public static readonly TimeSpan SemanticZoomButtonHideDelay = TimeSpan.FromSeconds(3);
    /// <summary>视图切换过渡时长（FadeIn/FadeOutThemeAnimation 节奏，毫秒）。</summary>
    public const uint SemanticZoomTransitionMs = 200;
    /// <summary>缩放按钮配色（手机 SemanticZoomButton* 资源；桌面亮色为 #59D5D5D5/#99000000）。</summary>
    public static Color SemanticZoomButtonBackground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#59D5D5D5")
        : Color.FromArgb("#CCFFFFFF");
    public static Color SemanticZoomButtonForeground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#99000000")
        : Color.FromArgb("#73FFFFFF");
    public static Color SemanticZoomButtonPressedBackground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#40000000")
        : Color.FromArgb("#73FFFFFF");
    public static Color SemanticZoomButtonPressedForeground => Application.Current?.RequestedTheme == AppTheme.Light
        ? Color.FromArgb("#DE000000")
        : Colors.White;

    // ---- 栅格 ----
    /// <summary>页面左右栅格：桌面 100 / 手机 24（WP8.1 页面栅格为 24）。</summary>
    public static double PageMargin => IsPhone ? 24 : 100;

    /// <summary>
    /// WinRT CharacterSpacing（单位 1/1000 em）→ MAUI CharacterSpacing（单位 pt）。
    /// Windows 上 MAUI 以 pt×62.4 直传 WinUI，反算即得；其他平台按字号换算成 pt
    /// （1pt = 4/3 px @96dpi）。
    /// </summary>
    public static double ToCharacterSpacing(double fontSizePx, double winrtPerMil) =>
#if WINDOWS
        winrtPerMil / 62.4;
#else
        fontSizePx * winrtPerMil / 1000.0 * 0.75;
#endif
}
