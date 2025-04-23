// AppShell.xaml.cs
using Microsoft.Maui.Controls;
using newRestaurant.Views;
using newRestaurant.Services.Interfaces;
using newRestaurant.Models;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics; // For Debug.WriteLine
// using System.Windows.Input; // Needed for ICommand if using that approach

// Define the placeholder page needed for the LogoutAction route
public class LogoutPagePlaceholder : ContentPage { }

namespace newRestaurant
{
    public partial class AppShell : Shell
    {
        private readonly IAuthService _authService;

        public AppShell(IAuthService authService)
        {
            InitializeComponent(); // Initialize first!

            _authService = authService;

            RegisterRoutes(); // Then register routes
            SetupFlyoutItems(); // Then setup dynamic items

            // Listen for changes in the logged-in user
            _authService.PropertyChanged += AuthService_PropertyChanged;

            // *** Call the method to set initial visibility ***
            UpdateFlyoutVisibility();
        }

        private void SetupFlyoutItems()
        {
            // Check if an item targeting the "LogoutAction" route already exists
            bool logoutExists = Items.SelectMany(section => section.Items)
                                     .Any(content => content.Route == "LogoutAction");

            if (!logoutExists)
            {
                var logoutItem = new FlyoutItem() { Title = "Logout" };
                logoutItem.Items.Add(new ShellContent()
                {
                    Title = "Logout",
                    Route = "LogoutAction",
                    ContentTemplate = new DataTemplate(() => new LogoutPagePlaceholder()),
                    IsVisible = false // Content itself isn't shown, just used for routing
                });
                Items.Add(logoutItem);
                Debug.WriteLine("Dynamic Logout FlyoutItem added.");
            }
            else
            {
                Debug.WriteLine("Logout FlyoutItem/Route already exists.");
            }
        }

        private void AuthService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAuthService.CurrentUser) || e.PropertyName == nameof(IAuthService.IsLoggedIn))
            {
                // Run UI updates on the main thread
                MainThread.BeginInvokeOnMainThread(UpdateFlyoutVisibility);
            }
        }

        // ***** THIS IS THE METHOD TO ADD *****
        private void UpdateFlyoutVisibility()
        {
            bool isLoggedIn = _authService.IsLoggedIn;
            var currentUserRole = _authService.CurrentUser?.Role;

            Debug.WriteLine($"Updating Flyout Visibility: LoggedIn={isLoggedIn}, Role={currentUserRole}");

            // --- Find items reliably (Using Title is often simplest) ---
            var managementItem = Items.FirstOrDefault(item => item.Title == "Management");
            var ordersItem = Items.FirstOrDefault(item => item.Title == "Orders and Reservations");
            var reportsItem = Items.FirstOrDefault(item => item.Title == "Reports"); // Find the new reports item
            var logoutItem = Items.FirstOrDefault(item => item.Title == "Logout"); // Find the dynamic item

            // --- Apply Visibility Logic ---
            if (managementItem != null)
            {
                // Show Management only to Staff/Admin
                bool shouldBeVisible = isLoggedIn && (currentUserRole == UserRole.Staff || currentUserRole == UserRole.Admin);
                managementItem.IsEnabled = shouldBeVisible; // Enable/disable based on role
                managementItem.IsVisible = shouldBeVisible; // Show/hide based on role
                Debug.WriteLine($"Management Item Visible: {managementItem.IsVisible}, Enabled: {managementItem.IsEnabled}");
            }
            else { Debug.WriteLine("Management Item not found by Title."); }

            if (ordersItem != null)
            {
                // Show Orders/Reservations to all logged-in users
                bool shouldBeVisible = isLoggedIn;
                ordersItem.IsEnabled = shouldBeVisible;
                ordersItem.IsVisible = shouldBeVisible;
                Debug.WriteLine($"Orders Item Visible: {ordersItem.IsVisible}, Enabled: {ordersItem.IsEnabled}");
            }
            else { Debug.WriteLine("Orders Item not found by Title."); }

            if (reportsItem != null)
            {
                // Show Reports only to Staff/Admin
                bool shouldBeVisible = isLoggedIn && (currentUserRole == UserRole.Staff || currentUserRole == UserRole.Admin);
                reportsItem.IsEnabled = shouldBeVisible;
                reportsItem.IsVisible = shouldBeVisible;
                Debug.WriteLine($"Reports Item Visible: {reportsItem.IsVisible}, Enabled: {reportsItem.IsEnabled}");
            }
            else { Debug.WriteLine("Reports Item not found by Title."); }


            if (logoutItem != null)
            {
                // Show Logout only when logged in
                bool shouldBeVisible = isLoggedIn;
                logoutItem.IsEnabled = shouldBeVisible; // Make it clickable only when logged in
                logoutItem.IsVisible = shouldBeVisible; // Show/hide based on login status
                Debug.WriteLine($"Logout Item Visible: {logoutItem.IsVisible}, Enabled: {logoutItem.IsEnabled}");
            }
            else { Debug.WriteLine("Logout Item not found by Title."); }

            // Optional: Force layout update if experiencing visual glitches
            // this.InvalidateMeasure();
        }
        // ***************************************


        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            // Intercept navigation to the dummy "LogoutAction" route
            if (args.Target.Location.OriginalString.Equals("LogoutAction", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("LogoutAction navigation intercepted.");
                args.Cancel(); // Cancel the actual navigation
                if (args.Source != ShellNavigationSource.Pop) // Prevent potential loops
                {
                    // Optional: Ask for confirmation
                    // bool confirm = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");
                    // if(confirm) await _authService.LogoutAsync();
                    await _authService.LogoutAsync(); // Perform logout
                }
            }
        }

        

        private void RegisterRoutes()
        {
            // Detail Pages
            Routing.RegisterRoute(nameof(CategoryDetailPage), typeof(CategoryDetailPage));
            Routing.RegisterRoute(nameof(PlatDetailPage), typeof(PlatDetailPage));
            Routing.RegisterRoute(nameof(ReservationDetailPage), typeof(ReservationDetailPage));
            Routing.RegisterRoute(nameof(TableDetailPage), typeof(TableDetailPage));

            // Main Pages (ensure these match routes in XAML ShellContent)
            Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
            Routing.RegisterRoute(nameof(PlatsPage), typeof(PlatsPage));
            Routing.RegisterRoute(nameof(ReservationsPage), typeof(ReservationsPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));
            Routing.RegisterRoute(nameof(TablesPage), typeof(TablesPage));
            Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage)); // Make sure StatisticsPage is registered
            Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
            // Auth Pages
            Routing.RegisterRoute($"//{nameof(LoginPage)}", typeof(LoginPage)); // Absolute route
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage)); // Relative route
            Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
            // AppShell itself (for absolute navigation back to the shell's root)
            Routing.RegisterRoute($"//{nameof(AppShell)}", typeof(AppShell));

            // Register the dummy logout route handler
            Routing.RegisterRoute("LogoutAction", typeof(LogoutPagePlaceholder));
            Debug.WriteLine("Routes Registered, including LogoutAction.");
        }
    }
}