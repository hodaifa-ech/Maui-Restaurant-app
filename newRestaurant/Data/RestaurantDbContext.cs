// Data/RestaurantDbContext.cs
using Microsoft.EntityFrameworkCore;
using newRestaurant.Models; // Ensure correct namespace
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace newRestaurant.Data // Ensure correct namespace
{
    public class RestaurantDbContext : DbContext
    {
        // DbSets remain the same
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Plat> Plats { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartPlat> CartPlats { get; set; }

        // Constructor used by Dependency Injection and DesignTime Factory
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }

        // REMOVED: OnConfiguring override - Configuration is now external

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Model Configurations Remain the Same ---

            // Configure the composite primary key for the CartPlat junction table
            modelBuilder.Entity<CartPlat>()
                .HasKey(cp => new { cp.CartId, cp.PlatId });

            // Configure the many-to-many relationship between Cart and Plat using CartPlat
            modelBuilder.Entity<CartPlat>()
                .HasOne(cp => cp.Cart)
                .WithMany(c => c.CartPlats)
                .HasForeignKey(cp => cp.CartId);

            modelBuilder.Entity<CartPlat>()
                .HasOne(cp => cp.Plat)
                .WithMany(p => p.CartPlats)
                .HasForeignKey(cp => cp.PlatId);

            // Configure relationships explicitly (optional if conventions are followed, but good practice)
            modelBuilder.Entity<Plat>()
               .HasOne(p => p.Category)
               .WithMany(c => c.Plats)
               .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId);

            modelBuilder.Entity<Notification>()
               .HasOne(n => n.User)
               .WithMany(u => u.Notifications)
               .HasForeignKey(n => n.UserId);

            modelBuilder.Entity<Cart>()
               .HasOne(c => c.User)
               .WithMany(u => u.Carts)
               .HasForeignKey(c => c.UserId);

            // Seeding data can remain if desired
            // modelBuilder.Entity<Category>().HasData(new Category { Id = 1, Name = "Appetizers" });
            // modelBuilder.Entity<Table>().HasData(new Table { Id = 1, TableNumber = "T1", Capacity = 4 });
        }
    }
}