namespace MetroCompanionDemo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(PivotDemoPage), typeof(PivotDemoPage));
        }
    }
}
