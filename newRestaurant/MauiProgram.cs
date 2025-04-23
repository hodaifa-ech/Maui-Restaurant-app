using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using newRestaurant.Data;
using newRestaurant.Services;
using newRestaurant.Services.Interfaces;
using newRestaurant.ViewModels;
using newRestaurant.Views;
using CommunityToolkit.Maui;

namespace newRestaurant
{
    public static class MauiProgram
    {
        public static IServiceProvider Services { get; private set; } // Add static property

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();
            // --- Database Context Registration ---
            const string server = "localhost"; // YOUR IP
            const string port = "3306";
            const string database = "amine"; // YOUR DB NAME
            const string user = "root"; // YOUR USER
            const string password = ""; // YOUR PASSWORD
            string connectionString = $"Server={server};Port={port};Database={database};Uid={user};Pwd={password};";
            Console.WriteLine($"[Runtime] Configuring DbContext for MySQL Database: {database} on Server: {server}");
            builder.Services.AddDbContext<RestaurantDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
#if DEBUG
            // options.LogTo(Console.WriteLine, LogLevel.Information).EnableSensitiveDataLogging();
#endif
            });
            // --- Register Services ---
            builder.Services.AddSingleton<INavigationService, MauiNavigationService>(); // Keep Navigation Service as is
            // Register NEW model-specific services (Scoped is good practice with Scoped DbContext)
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IPlatService, PlatService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<ITableService, TableService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IStatisticsService, StatisticsService>();
            // REMOVED: builder.Services.AddScoped<IDataService, MySqlDataService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddScoped<IStatisticsService, StatisticsService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            // --- Register ViewModels ---
            // (Ensure these inject the NEW specific service interfaces)
            builder.Services.AddTransient<CategoriesViewModel>();
            builder.Services.AddTransient<CategoryDetailViewModel>();
            builder.Services.AddTransient<PlatsViewModel>();
            builder.Services.AddTransient<PlatDetailViewModel>();
            builder.Services.AddTransient<ReservationsViewModel>();
            builder.Services.AddTransient<ReservationDetailViewModel>();
            builder.Services.AddSingleton<CartViewModel>();
            builder.Services.AddTransient<StatisticsViewModel>();
            //builder.Services.AddTransient<LoginViewModel>(); // Register if using
            //builder.Services.AddTransient<StatisticsViewModel>(); // Register if using
            builder.Services.AddTransient<TablesViewModel>(); // <-- ADD
            builder.Services.AddTransient<TableDetailViewModel>(); // <-- ADD
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<NotificationsViewModel>();
            // --- Register Views ---
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddTransient<CategoryDetailPage>();
            builder.Services.AddTransient<PlatsPage>();
            builder.Services.AddTransient<PlatDetailPage>();
            builder.Services.AddTransient<ReservationsPage>();
            builder.Services.AddTransient<ReservationDetailPage>();
            builder.Services.AddTransient<CartPage>();
            builder.Services.AddTransient<StatisticsPage>();
            builder.Services.AddTransient<NotificationsPage>();
            //builder.Services.AddTransient<LoginPage>(); // Register if using
            //builder.Services.AddTransient<StatisticsPage>(); // Register if using
            builder.Services.AddTransient<TablesPage>(); // <-- ADD
            builder.Services.AddTransient<TableDetailPage>(); // <-- ADD
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddSingleton<AppShell>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();
            Services = app.Services; // Assign the provider after building
            // --- Apply Migrations (remains the same) ---
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
                    Console.WriteLine("[Runtime] Testing database connection...");
                    var canConnect = dbContext.Database.CanConnect();
                    Console.WriteLine($"[Runtime] Can connect: {canConnect}");
                    if (!canConnect)
                        throw new Exception("Cannot connect to the database.");
                    Console.WriteLine("[Runtime] Applying migrations...");
                    dbContext.Database.Migrate();
                    Console.WriteLine("[Runtime] Database migrations checked/applied successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Runtime] An error occurred during DB setup: {ex.ToString()}");
            }

            return app;
        }
    }
}