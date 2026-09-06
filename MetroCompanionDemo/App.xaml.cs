using Microsoft.Extensions.DependencyInjection;

namespace MetroCompanionDemo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Demo 页面均为纯黑 Metro 底，固定暗色主题使 ApplicationBar/SemanticZoom
            // 跟随 PhoneChromeColor 暗色值（WP8 真机暗色系统观感）
            UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}