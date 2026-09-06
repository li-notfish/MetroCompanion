# MetroCompanion

**Windows Phone 8 / Windows 8 时代 Metro 设计语言的 .NET MAUI 控件库。**

还原 Windows Phone 8/8.1 与 Windows 8/8.1（WinRT）的控件规格与交互细节——不是"差不多"的模仿：字号、字重、间距、位移公式均对照 SDK `generic.xaml` 与真机规格逐项校准。

[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green)](#license)

## 控件

### HubView — Hub / Panorama 二合一

可横滑的多节内容控件。默认还原 WinRT 8.1 **Hub** 规格（桌面 26.667px / 手机 19px Normal 节标题、字距 -10、桌面 100px 页面栅格）；开启 `IsPanoramaMode` 后切换为 WP8 真机 **Panorama** 表现：

- 首面板 165px Light 半透明大标题（仅首面板），随滚动淡出、回滚恢复；
- 后续面板为 66px Semelight 小表头、字距 -35（Silverlight `PanoramaItemHeaderFontSize`）；
- 每节可设 `WidthRequest` 变宽（真机 PanoramaItem 行为）；
- 全屏壁纸视差背景；
- WP8.1 Optional snap 吸附——靠近节边界才吸附，而非强制整页。

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

### PivotView — WP8.1 Pivot

整页横滑、始终吸附整页的 Pivot：

- 表头条带"尽量静止，仅当选中表头超出视口时移动刚好够的距离"（UWP/WP8.1 scroll-into-view 语义），选中/未选中表头随滚动交叉淡化（未选中透明度 0.4，对应 `PhonePivotUnselectedItemOpacity` / `PhoneControlDisabledColor`）；
- 表头规格取 WP8.1 原始值：手机 57px Semilight、字距 -25（`PivotHeaderItemCharacterSpacing`）、表头间隙 16（`PivotHeaderItemMargin`）、底部留白 6.5（`PivotHeaderItemPadding`）；
- 点击表头跳转；`SelectedIndex` / `SelectedItem` 双向绑定 + `SelectionChanged` 事件；
- **Windows**：鼠标滚轮整页翻页（高分辨率滚轮一格一页）、方向键翻页，惯性刚开始即按预测落点吸附整页（无"停稳再补动画"的顿感）；
- **移动端**：手势惯性直接落到整页；
- 全部表头规格可调：`TitleFontSize`、`HeaderFontSize`、`HeaderCharacterSpacing`、`HeaderSpacing`、`HeaderMargin`、`UnselectedHeaderOpacity`。

```xml
<mcm:PivotView Title="概览">
    <mcm:PivotItem Header="概览">
        <!-- 内容 -->
    </mcm:PivotItem>
    <mcm:PivotItem Header="内容" />
</mcm:PivotView>
```

### ApplicationBar — WP8 应用栏

72px 高的应用栏（PhoneChromeColor 底），还原 WP8 ApplicationBar 的结构与交互：

- 至多四个图标按钮（官方规格 48×48 图标 + 展开时才显示的 caption），宽度均分；
- "…" 按钮：点击旋转 90° 并向上展开菜单（菜单项 25.333px，即 19pt / `PhoneFontSizeMediumLarge`），激活任一命令后自动收起；
- `Mode`（Default / Minimized——仅显示"…"）、`Opacity`（WP8 语义，0 = 整栏隐藏不占位）、`IsOpen`（TwoWay）；
- 禁用态 40% 不透明（`PhoneDisabledColor`）；
- 在页面内容元素上挂 `MenuTrigger` 即得系统级呼出手势：**Windows 单击鼠标右键弹出/收回**（WinRT 桌面应用栏约定）、**移动端长按约 1 秒弹出**（WP8 按住行为）；点击页面其余区域收起由使用方处理（挂 `TapGestureRecognizer` 调 `Close()`）。

```xml
<!-- 放进页面底部 Grid 行 -->
<Grid RowDefinitions="*, Auto">
    <!-- 页面内容：mcm:ApplicationBar.MenuTrigger="{x:Reference Bar}" 提供右键/长按呼出 -->
    <mcm:ApplicationBar Grid.Row="1" x:Name="Bar">
        <mcm:AppBarButton Text="添加" Icon="appbar/add.png" Command="{Binding AddCommand}" />
        <mcm:AppBarButton Text="删除" Icon="appbar/delete.png" Command="{Binding DeleteCommand}" />
        <mcm:ApplicationBar.MenuItems>
            <mcm:AppBarMenuItem Text="设置" Command="{Binding SettingsCommand}" />
        </mcm:ApplicationBar.MenuItems>
    </mcm:ApplicationBar>
</Grid>
```

### SemanticZoom — WinRT 语义缩放

叠放放大/缩小两个视图的语义缩放容器，对标 WinRT 8.1 `SemanticZoom`：

- 捏合（移动端）、**Ctrl+滚轮**（Windows）、右下角缩放按钮三种切换方式；
- 切换为约 200ms 交叉淡化 + 轻微缩放（对应 SDK 模板的 FadeIn/FadeOut 视觉状态）；
- 缩放按钮按 Win8 模板规格：21×21、右下 Margin `0,0,7,24`，静止 3 秒自动淡出、交互时重新显示（`IsZoomButtonVisible` 可关闭）；
- `IsZoomedOutActive`（TwoWay）+ `ZoomIn()` / `ZoomOut()` / `ToggleZoom()`；放大视图内的定位（如点字母滚到分组）由内容实现。

```xml
<mcm:SemanticZoom>
    <mcm:SemanticZoom.ZoomedInView>
        <!-- 按字母分组的长列表 -->
    </mcm:SemanticZoom.ZoomedInView>
    <mcm:SemanticZoom.ZoomedOutView>
        <!-- 字母网格，点击后 ZoomIn() 并滚动到对应分组 -->
    </mcm:SemanticZoom.ZoomedOutView>
</mcm:SemanticZoom>
```

### 设计令牌

`MetroCompanion.Styles.MetroTokens` 提供桌面/手机自适应的字号、字重、颜色与栅格令牌（`PageMargin`、`PivotHeaderItemFontSize`、`PanoramaTitleFontSize`、`SemilightFontFamily` 等），附 ResourceDictionary 版本 `MetroTheme.xaml`，App 可整体合并后按需覆盖。数值来源：Windows Phone Kits 8.1 `generic.xaml`（WinRT 手机）、Windows Kits 8.0 `generic.xaml`（桌面 WinRT）、Windows Phone v8.1 `ThemeResources.xaml`（Silverlight）。

## 快速开始

```shell
dotnet add package MetroCompanion
```

包覆盖 `net10.0` / `net10.0-android` / `net10.0-ios` / `net10.0-maccatalyst` / `net10.0-windows10.0.19041` 五个目标框架。

| 平台 | 说明 |
|---|---|
| Windows | Segoe UI 字重（Semilight/Semibold/Light）精确生效；滚轮/键盘行为完整 |
| Android / iOS / MacCatalyst | 字体回退平台默认；滚动/吸附行为完整 |

## 运行 Demo

仓库自带示例（磁贴数据、Hub/Panorama 模式切换、Pivot 详情页、应用栏与语义缩放演示页）：

```shell
dotnet build MetroCompanionDemo/MetroCompanionDemo.csproj -f net10.0-windows10.0.19041.0
# 或部署到 Android 设备
dotnet build MetroCompanionDemo/MetroCompanionDemo.csproj -f net10.0-android
```

## 构建 NuGet 包

```shell
dotnet build MetroCompanion/MetroCompanion.csproj -c Release
dotnet pack MetroCompanion/MetroCompanion.csproj -c Release
# 产物：MetroCompanion/bin/Release/MetroCompanion.<版本>.nupkg + .snupkg
```

## License

[MIT](LICENSE)
