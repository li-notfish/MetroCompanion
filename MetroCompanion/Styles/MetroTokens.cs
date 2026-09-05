namespace MetroCompanion.Styles;

/// <summary>
/// Metro 设计语言令牌（Windows 8 / 8.1 时期规格）。
/// 默认值按设备类型自适应——与当年 WinRT 在不同设备家族使用不同默认值一致；
/// 控件属性与 MetroTheme.xaml 均可覆盖。
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
    /// <summary>Light 字重（大标题用）。</summary>
    public static string LightFontFamily => "Segoe UI Light";

    // ---- 字号（桌面 / 手机两套，对应 WinRT 8.1 与 WP8.1 的默认值）----
    /// <summary>Pivot 表头：桌面 24（PivotHeaderItemFontSize），手机 57.5。</summary>
    public static double PivotHeaderItemFontSize => IsPhone ? 57.5 : 24;
    /// <summary>Pivot 大标题：桌面 46（HubHeaderThemeFontSize），手机 64。</summary>
    public static double PivotTitleFontSize => IsPhone ? 64 : 46;
    /// <summary>Hub 节标题（HubSectionHeaderThemeFontSize）。</summary>
    public const double HubSectionHeaderFontSize = 26.667;
    /// <summary>Hub 大标题（HubHeaderThemeFontSize）。</summary>
    public const double HubHeaderFontSize = 46;
    /// <summary>Panorama 大标题（WP8 真机约 165px Light）。</summary>
    public const double PanoramaTitleFontSize = 165;
    /// <summary>未选中 Pivot 表头透明度（WinRT 视觉状态值）。</summary>
    public const double UnselectedHeaderOpacity = 0.5;

    // ---- 栅格 ----
    /// <summary>页面左右栅格：桌面 100 / 手机 24（WP8.1 页面栅格为 24）。</summary>
    public static double PageMargin => IsPhone ? 24 : 100;
}
