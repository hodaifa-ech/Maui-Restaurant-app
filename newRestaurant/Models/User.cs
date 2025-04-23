using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Models
{
    public class User
    {
        [Key] // Explicitly define Primary Key
        public int Id { get; set; } // Changed to int for simplicity with EF Core unless long is strictly needed

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; } // Store hashes, not plain text!

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        public UserRole Role { get; set; } = UserRole.Customer; // Added Role

        // Navigation Properties
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>(); // User can have multiple carts (e.g., active, history) or just one active
    }

    public enum UserRole
    {
        Customer,
        Staff,
        Admin
    }
}
