namespace SAA
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LandingPage());

            //MainPage = new NavigationPage(new MainPage());
            Task.Run(async () =>
            {
                if (MainPage is NavigationPage navPage && navPage.CurrentPage is RegisterPage registerPage)
                {
                    await registerPage.TrySyncUnsyncedUsers();
                }
            });
        }
    }
}
