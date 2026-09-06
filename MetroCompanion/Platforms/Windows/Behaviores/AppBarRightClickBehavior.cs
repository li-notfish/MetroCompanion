using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using MetroCompanion.Controls;

namespace MetroCompanion.Behaviores
{
    /// <summary>
    /// WinRT 桌面应用栏约定：在页面内容上单击鼠标右键，弹出或收回应用栏菜单
    /// （与 Win8.1 底部应用栏的显示/消失手势一致）。
    /// </summary>
    public partial class AppBarRightClickBehavior : PlatformBehavior<VisualElement, FrameworkElement>
    {
        protected override void OnAttachedTo(VisualElement bindable, FrameworkElement nativeView)
        {
            base.OnAttachedTo(bindable, nativeView);
            nativeView.RightTapped += OnRightTapped;
        }

        protected override void OnDetachedFrom(VisualElement bindable, FrameworkElement nativeView)
        {
            base.OnDetachedFrom(bindable, nativeView);
            nativeView.RightTapped -= OnRightTapped;
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Bar?.Toggle();
            e.Handled = true;
        }
    }
}
