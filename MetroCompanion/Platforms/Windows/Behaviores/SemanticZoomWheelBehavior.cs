using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using MetroCompanion.Controls;

namespace MetroCompanion.Behaviores
{
    /// <summary>
    /// WinRT 8.1 语义缩放：Ctrl+滚轮在放大/缩小视图间切换（与触摸捏合等价）。
    /// 挂载在 <see cref="SemanticZoom"/> 的原生包装元素上，滚轮事件自子内容冒泡。
    /// </summary>
    public partial class SemanticZoomWheelBehavior : PlatformBehavior<SemanticZoom, FrameworkElement>
    {
        private SemanticZoom? _zoom;

        // 高分辨率滚轮一格连发多个事件，去重保证一格只切换一次
        private DateTime _lastToggle = DateTime.MinValue;

        protected override void OnAttachedTo(SemanticZoom bindable, FrameworkElement nativeView)
        {
            base.OnAttachedTo(bindable, nativeView);
            _zoom = bindable;
            nativeView.AddHandler(UIElement.PointerWheelChangedEvent,
                new PointerEventHandler(OnPointerWheelChanged), true);
        }

        protected override void OnDetachedFrom(SemanticZoom bindable, FrameworkElement nativeView)
        {
            base.OnDetachedFrom(bindable, nativeView);
            nativeView.RemoveHandler(UIElement.PointerWheelChangedEvent,
                new PointerEventHandler(OnPointerWheelChanged));
            _zoom = null;
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (_zoom == null) return;

            // 仅 Ctrl+滚轮触发语义缩放（无修饰键时交给内容滚动）
            if (!e.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Control))
                return;

            var props = e.GetCurrentPoint((UIElement)sender).Properties;
            if (props.IsHorizontalMouseWheel)
                return;

            if (DateTime.Now - _lastToggle < TimeSpan.FromMilliseconds(300))
            {
                e.Handled = true;
                return;
            }

            _lastToggle = DateTime.Now;
            _zoom.ToggleZoom();
            _zoom.NotifyUserInteraction();
            e.Handled = true;
        }
    }
}
