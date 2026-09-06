# MetroCompanion

Windows Phone 8 / Windows 8 时代 Metro 设计语言的 .NET MAUI 控件库。

## 控件

### HubView（Hub / Panorama）

可横滑的多节内容控件，默认还原 WinRT 8.1 Hub 规格；开启 `IsPanoramaMode` 后切换为 WP8 真机 Panorama 表现：首面板超大 Light 半透明标题（滚动淡出）、后续面板 Semilight 小表头、支持 `WidthRequest` 变宽面板、背景视差。

```xml
xmlns:mcm="clr-namespace:MetroCompanion.Controls;assembly=MetroCompanion"

<mcm:HubView
    PanoramaTitle="旅程"
    IsPanoramaMode="True"
    BackgroundSource="bg.jpg">
    <mcm:HubSection Title="旅程">
        <!-- 内容 -->
    </mcm:HubSection>
    <mcm:HubSection Title="动态" WidthRequest="320" />
</mcm:HubView>
```

| 属性 | 说明 |
|---|---|
| `BackgroundSource` | 全屏壁纸（Panorama 视差背景） |
| `IsParallaxEnabled` | 背景视差开关（默认开） |
| `IsPanoramaMode` | WP8 Panorama 风格开关 |
| `PanoramaTitle` | Panorama 模式下首面板的大标题 |
| `SnapThreshold` | 靠近节边界才吸附的阈值（WP8.1 Optional snap 行为） |

### PivotView（WP8.1 Pivot）

整页横滑、始终吸附整页；表头条带"尽量静止、仅当选中表头超出视口时移动刚好够的距离"，选中/未选中表头交叉淡化，点击表头跳转；Windows 桌面支持鼠标滚轮整页翻页与方向键，移动端惯性直接落到整页。

```xml
<mcm:PivotView Title="标题">
    <mcm:PivotItem Header="概览">
        <!-- 内容 -->
    </mcm:PivotItem>
    <mcm:PivotItem Header="内容" />
</mcm:PivotView>
```

可选属性：`TitleFontSize`、`HeaderFontSize`、`HeaderSpacing`、`HeaderMargin`、`UnselectedHeaderOpacity`、`SelectedIndex`/`SelectedItem`（双向绑定），`SelectionChanged` 事件。

## 设计令牌

`MetroCompanion.Styles.MetroTokens` 提供桌面/手机自适应的字号、字重、颜色与栅格令牌（如 `PageMargin`、`PivotHeaderItemFontSize`、`PanoramaTitleFontSize`），可被 App 级资源覆盖；`MetroTheme.xaml` 为对应的 ResourceDictionary 版本。

## 平台说明

- 字体按 Segoe UI 家族（Semilight/Semibold/Light）声明，Windows 上精确生效，其他平台回退平台默认字体；
- 滚轮/键盘翻页行为仅编译进 Windows 目标。

## License

MIT
