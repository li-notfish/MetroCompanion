# MetroCompanion

**Windows Phone 8 / Windows 8 时代 Metro 设计语言的 .NET MAUI 控件库。**

还原 Windows Phone 8/8.1 与 Windows 8/8.1（WinRT）的控件规格与交互细节——不是"差不多"的模仿：字号、字重、间距、位移公式均对照 SDK `generic.xaml` 与真机规格逐项校准。

[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green)](#license)

## 控件

### HubView — Hub / Panorama 二合一

可横滑的多节内容控件。默认还原 WinRT 8.1 **Hub** 规格（26.667px Normal 节标题、桌面 100px 页面栅格）；开启 `IsPanoramaMode` 后切换为 WP8 真机 **Panorama** 表现：

- 首面板 165px Light 半透明大标题（仅首面板），随滚动淡出、回滚恢复；
- 后续面板为 ~45px Semilight 小表头（对应真机 `PanoramaItem`）；
- 每节可设 `WidthRequest` 变宽（真机 PanoramaItem 行为）；
- 全屏壁纸视差背景；
- WP8.1 Optional snap 吸附——靠近节边界才吸附，而非强制整页。

```xml
xmlns:mcm="clr-namespace:MetroCompanion.Controls;assembly=MetroCompanion"

<mcm:HubView PanoramaTitle="旅程" IsPanoramaMode="True">
    <mcm:HubSection Title="旅程">
        <!-- 内容 -->
    </mcm:HubSection>
    <mcm:HubSection Title="动态" WidthRequest="320" />
</mcm:HubView>
```

### PivotView — WP8.1 Pivot

整页横滑、始终吸附整页的 Pivot：

- 表头条带"尽量静止，仅当选中表头超出视口时移动刚好够的距离"（UWP/WP8.1 scroll-into-view 语义），选中/未选中表头随滚动交叉淡化；
- 点击表头跳转；`SelectedIndex` / `SelectedItem` 双向绑定 + `SelectionChanged` 事件；
- **Windows**：鼠标滚轮整页翻页（高分辨率滚轮一格一页）、方向键翻页，惯性刚开始即按预测落点吸附整页（无"停稳再补动画"的顿感）；
- **移动端**：手势惯性直接落到整页；
- 全部表头规格可调：`TitleFontSize`、`HeaderFontSize`、`HeaderSpacing`、`HeaderMargin`、`UnselectedHeaderOpacity`。

```xml
<mcm:PivotView Title="概览">
    <mcm:PivotItem Header="概览">
        <!-- 内容 -->
    </mcm:PivotItem>
    <mcm:PivotItem Header="内容" />
</mcm:PivotView>
```

### 设计令牌

`MetroCompanion.Styles.MetroTokens` 提供桌面/手机自适应的字号、字重、颜色与栅格令牌（`PageMargin`、`PivotHeaderItemFontSize`、`PanoramaTitleFontSize`、`SemilightFontFamily` 等），附 ResourceDictionary 版本 `MetroTheme.xaml`，App 可整体合并后按需覆盖。

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

仓库自带示例（磁贴数据、Hub/Panorama 模式切换、Pivot 详情页）：

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
