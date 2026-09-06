using MetroCompanion.Controls;

namespace MetroCompanion.Behaviores
{
    // 鼠标右键弹出/收回应用栏菜单（WinRT 桌面应用栏约定）；非 Windows TFM 的编译占位
    public partial class AppBarRightClickBehavior
    {
        public ApplicationBar? Bar { get; set; }
    }
}
