// Data/RestaurantDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
// REMOVED: using System.IO; // Not needed for server DB
// REMOVED: using SQLitePCL;

namespace newRestaurant.Data // Ensure correct namespace
{
    // Factory for EF Core Tools (Add-Migration, Update-Database) using MySQL
    public class RestaurantDbContextFactory : IDesignTimeDbContextFactory<RestaurantDbContext>
    {
        public RestaurantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();

            // --- MySQL Configuration ---
            // IMPORTANT: Use the SAME connection details as MauiProgram.cs
            const string server = "localhost"; // YOUR IP
            const string port = "3306";
            const string database = "amine";      // YOUR DB NAME
            const string user = "root";          // YOUR USER
            const string password = "";   // YOUR PASSWORD

            string connectionString = $"Server={server};Port={port};Database={database};Uid={user};Pwd={password};";

            Console.WriteLine($"[DesignTimeFactory] Using Connection: Server={server};Port={port};Database={database};Uid={user};Pwd=***"); // Mask password in log

            // Use Pomelo MySQL Provider
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            // Optional: Add logging for EF Core tools if needed for debugging
            // optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

            // **Crucially, return the context using the configured options:**
            return new RestaurantDbContext(optionsBuilder.Options);
        }
    }
}