// AppShell.xaml.cs
using newRestaurant.Views;
using newRestaurant.Services.Interfaces; // For IAuthService

namespace newRestaurant
{
    public partial class AppShell : Shell
    {
        // Inject AuthService to handle logout if needed here
        private readonly IAuthService _authService;

        public AppShell(IAuthService authService) // Inject AuthService
        {
            _authService = authService;
            InitializeComponent();
            RegisterRoutes();

            // Example: Add Logout Flyout Item
            var logoutItem = new FlyoutItem() { Title = "Logout" };
            logoutItem.Items.Add(new ShellContent() { Route = "Logout", IsVisible = false }); // Dummy content
            Items.Add(logoutItem); // Add to the flyout
        }



        private void RegisterRoutes()
        {
            // Detail Pages
            Routing.RegisterRoute(nameof(CategoryDetailPage), typeof(CategoryDetailPage));
            Routing.RegisterRoute(nameof(PlatDetailPage), typeof(PlatDetailPage));
            Routing.RegisterRoute(nameof(ReservationDetailPage), typeof(ReservationDetailPage));
            Routing.RegisterRoute(nameof(TableDetailPage), typeof(TableDetailPage)); // <-- ADD


            // Main Pages (already defined in XAML ShellContent)
            Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
            Routing.RegisterRoute(nameof(PlatsPage), typeof(PlatsPage));
            Routing.RegisterRoute(nameof(ReservationsPage), typeof(ReservationsPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));
            Routing.RegisterRoute(nameof(TablesPage), typeof(TablesPage)); // <-- ADD

            // Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));

            // *** ADD Login and Register Routes ***
            // Use "//" for absolute routes if navigating back to them clears the stack
            Routing.RegisterRoute($"//{nameof(LoginPage)}", typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));

            // Register AppShell itself if needed for "//AppShell" navigation
            Routing.RegisterRoute($"//{nameof(AppShell)}", typeof(AppShell));
        }
    }
}