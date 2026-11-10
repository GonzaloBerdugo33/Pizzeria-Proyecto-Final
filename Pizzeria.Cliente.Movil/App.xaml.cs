namespace Pizzeria.Cliente.Movil
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Pizzeria.Cliente.Movil" };
        }
    }
}
