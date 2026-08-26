using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
// 将 Windows 原生的控件命名空间取个别名，比如 WUC
using WUC = Microsoft.UI.Xaml.Controls;
// 或者是直接给具体的类取别名
using WinScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;

namespace MetroCompanion.Behaviores
{
    // 这里显式使用 MAUI 的 ScrollView 和 Windows 的 ScrollViewer (WinScrollViewer)
    public partial class HubHorizontalScrollBehavior : PlatformBehavior<Microsoft.Maui.Controls.ScrollView, WUC.ScrollViewer>
    {
        // 定义一个字段来持有引用（可选，但方便在事件中使用）
        private Microsoft.Maui.Controls.ScrollView _mauiScrollView;

        protected override void OnAttachedTo(Microsoft.Maui.Controls.ScrollView bindable, WUC.ScrollViewer nativeView)
        {
            base.OnAttachedTo(bindable, nativeView);
            _mauiScrollView = bindable;

            // 1. 挂载原生事件
            nativeView.AddHandler(UIElement.PointerWheelChangedEvent,
                new PointerEventHandler(OnPointerWheelChanged), true);

            // 2. 顺手开启 Windows 的左键拖拽支持
            nativeView.HorizontalScrollMode = WUC.ScrollMode.Enabled;
        }

        protected override void OnDetachedFrom(Microsoft.Maui.Controls.ScrollView bindable, WUC.ScrollViewer nativeView)
        {
            base.OnDetachedFrom(bindable, nativeView);

            nativeView.RemoveHandler(UIElement.PointerWheelChangedEvent,
                new PointerEventHandler(OnPointerWheelChanged));

            _mauiScrollView = null;
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var nativeScroll = sender as WUC.ScrollViewer;

            // 使用字段引用的 MAUI 控件
            if (nativeScroll == null || _mauiScrollView == null) return;

            var props = e.GetCurrentPoint(nativeScroll).Properties;

            // 逻辑：如果是垂直滚轮（!IsHorizontalMouseWheel）
            if (!props.IsHorizontalMouseWheel)
            {
                double delta = props.MouseWheelDelta;

                // 计算位移
                double targetX = _mauiScrollView.ScrollX - delta;

                // 边界检查
                double maxScroll = _mauiScrollView.ContentSize.Width - _mauiScrollView.Width;
                targetX = Math.Clamp(targetX, 0, maxScroll);

                // 驱动 MAUI ScrollView 滚动
                _mauiScrollView.ScrollToAsync(targetX, 0, false);

                // 标记已处理，防止触发系统的垂直滚动
                e.Handled = true;
            }
        }
    }
}
