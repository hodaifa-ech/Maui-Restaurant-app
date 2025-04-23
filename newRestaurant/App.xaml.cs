// App.xaml.cs
using newRestaurant.Views;

namespace newRestaurant // <-- Make sure this namespace matches your project name
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } // Add static property

        public App(LoginPage loginPage)
        {
            InitializeComponent(); // Initializes components defined in App.xaml
                                   // MainPage = new AppShell();
            MainPage = new NavigationPage(loginPage);


        }


    }
}