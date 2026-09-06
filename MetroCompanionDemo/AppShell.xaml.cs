namespace MetroCompanionDemo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(PivotDemoPage), typeof(PivotDemoPage));
            Routing.RegisterRoute(nameof(AppBarDemoPage), typeof(AppBarDemoPage));
            Routing.RegisterRoute(nameof(SemanticZoomDemoPage), typeof(SemanticZoomDemoPage));
        }
    }
}
