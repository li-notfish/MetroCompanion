using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Diagnostics;
// 将 Windows 原生的控件命名空间取个别名，比如 WUC
using WUC = Microsoft.UI.Xaml.Controls;
// 或者是直接给具体的类取别名
using WinScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;

namespace MetroCompanion.Behaviores
{
    // 这里显式使用 MAUI 的 ScrollView 和 Windows 的 ScrollViewer (WinScrollViewer)
    public partial class HubHorizontalScrollBehavior : PlatformBehavior<Microsoft.Maui.Controls.ScrollView, WUC.ScrollViewer>
    {
        private Microsoft.Maui.Controls.ScrollView _mauiScrollView;
        private WinScrollViewer _nativeScroll;

        // StepByPage 定义在根目录的分部类存根中，两个 TFM 共用

        // 高分辨率滚轮一格会连发多个事件，去重保证一格只翻一页（WinRT 8.1 Pivot 行为）
        private DateTime _lastWheelPageSwitch = DateTime.MinValue;

        // 自定义短促翻页动画的代际计数：新输入会使其失效
        private int _animationGen;

        protected override void OnAttachedTo(Microsoft.Maui.Controls.ScrollView bindable, WUC.ScrollViewer nativeView)
        {
            base.OnAttachedTo(bindable, nativeView);
            _mauiScrollView = bindable;
            _nativeScroll = nativeView;

            // 1. 挂载原生事件
            nativeView.AddHandler(UIElement.PointerWheelChangedEvent,
                new PointerEventHandler(OnPointerWheelChanged), true);

            // 2. Pivot 模式下支持方向键翻页（WinRT 8.1 桌面 Pivot 行为）
            if (StepByPage)
            {
                nativeView.IsTabStop = true;
                nativeView.KeyDown += OnKeyDown;

                // 惯性刚开始即按预测落点整页吸附（WP8.1 每次手势直接落到整页），
                // 避免"惯性停稳→等 300ms→再补一段矫正动画"的顿感
                nativeView.ViewChanging += OnNativeViewChanging;
            }

            // 3. 顺手开启 Windows 的左键拖拽支持
            nativeView.HorizontalScrollMode = WUC.ScrollMode.Enabled;
        }

        protected override void OnDetachedFrom(Microsoft.Maui.Controls.ScrollView bindable, WUC.ScrollViewer nativeView)
        {
            base.OnDetachedFrom(bindable, nativeView);

            // 打断进行中的翻页动画
            _animationGen++;

            nativeView.RemoveHandler(UIElement.PointerWheelChangedEvent,
                new PointerEventHandler(OnPointerWheelChanged));
            nativeView.KeyDown -= OnKeyDown;
            nativeView.ViewChanging -= OnNativeViewChanging;

            _mauiScrollView = null;
            _nativeScroll = null;
        }

        private void OnNativeViewChanging(object sender, WUC.ScrollViewerViewChangingEventArgs e)
        {
            // 只处理惯性阶段：拖拽中 NextView == FinalView，惯性中 FinalView 是
            // WinUI 的预测落点。落点不在整页上时立即用短促动画接管（设置偏移
            // 会取消惯性），直接滑到最近的整页
            var nativeScroll = sender as WinScrollViewer;
            if (nativeScroll == null) return;

            double page = nativeScroll.ViewportWidth;
            if (page <= 0) return;

            double next = e.NextView.HorizontalOffset;
            double final = e.FinalView.HorizontalOffset;
            if (Math.Abs(final - next) <= 0.5) return;

            double maxScroll = nativeScroll.ExtentWidth - page;
            double targetX = Math.Clamp(Math.Round(final / page) * page, 0, maxScroll);

            if (Math.Abs(targetX - final) > 0.5)
                AnimatePageTo(targetX);
        }

        private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (_nativeScroll == null) return;

            int direction = e.Key switch
            {
                Windows.System.VirtualKey.Left => -1,
                Windows.System.VirtualKey.Right => 1,
                _ => 0
            };
            if (direction == 0) return;

            double pageWidth = _nativeScroll.ViewportWidth;
            if (pageWidth <= 0) return;

            double maxScroll = _nativeScroll.ExtentWidth - pageWidth;
            int currentIndex = (int)Math.Round(_nativeScroll.HorizontalOffset / pageWidth);
            double targetX = Math.Clamp((currentIndex + direction) * pageWidth, 0, maxScroll);

            AnimatePageTo(targetX);
            e.Handled = true;
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var nativeScroll = sender as WinScrollViewer ?? _nativeScroll;
            if (nativeScroll == null || _mauiScrollView == null) return;

            var props = e.GetCurrentPoint(nativeScroll).Properties;

            // 逻辑：如果是垂直滚轮（!IsHorizontalMouseWheel）
            if (!props.IsHorizontalMouseWheel)
            {
                double delta = props.MouseWheelDelta;
                double pageWidth = _nativeScroll.ViewportWidth;
                if (pageWidth <= 0) return;
                double maxScroll = _nativeScroll.ExtentWidth - pageWidth;

                if (StepByPage)
                {
                    // Pivot 模式：基于当前所在页整页步进（滚轮向上 = 上一页）
                    if (DateTime.Now - _lastWheelPageSwitch < TimeSpan.FromMilliseconds(300))
                    {
                        // 高分辨率滚一格连发的事件吞掉，但保持 Handled 防止垂直滚动
                        e.Handled = true;
                        return;
                    }

                    _lastWheelPageSwitch = DateTime.Now;
                    int currentIndex = (int)Math.Round(_nativeScroll.HorizontalOffset / pageWidth);
                    int direction = delta > 0 ? -1 : 1;
                    double targetX = Math.Clamp((currentIndex + direction) * pageWidth, 0, maxScroll);

                    AnimatePageTo(targetX);
                    e.Handled = true;
                    return;
                }

                // Hub 模式：随滚轮连续平移
                double target = Math.Clamp(_nativeScroll.HorizontalOffset - delta, 0, maxScroll);
                _nativeScroll.ScrollToHorizontalOffset(target);

                // 标记已处理，防止触发系统的垂直滚动
                e.Handled = true;
            }
        }

        /// <summary>
        /// 120ms CubicOut 短促翻页：逐帧直接驱动原生 ScrollViewer。
        /// 不走 MAUI ScrollToAsync——它的瞬时模式在 WinUI 忽略 ChangeView 时
        /// 返回的 Task 永不完成，await 会卡死动画；ScrollX 回写也滞后于实际位置。
        /// </summary>
        private async void AnimatePageTo(double targetX)
        {
            var nativeScroll = _nativeScroll;
            if (nativeScroll == null) return;

            int gen = ++_animationGen;
            double startX = nativeScroll.HorizontalOffset;
            const int durationMs = 120;
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < durationMs)
            {
                if (gen != _animationGen) return;

                double progress = Math.Min(sw.ElapsedMilliseconds / (double)durationMs, 1.0);
                double eased = 1.0 - Math.Pow(1.0 - progress, 3);
                nativeScroll.ScrollToHorizontalOffset(startX + (targetX - startX) * eased);

                await Task.Delay(16);
            }

            if (gen == _animationGen)
                nativeScroll.ScrollToHorizontalOffset(targetX);
        }
    }
}
