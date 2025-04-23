using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key for User
        public int UserId { get; set; }

        // Navigation Property (Many-to-One with User)
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public CartStatus Status { get; set; } = CartStatus.Active; // e.g., Active, Ordered, Abandoned

        // Navigation Property for Many-to-Many with Plat (via CartPlat)
        public virtual ICollection<CartPlat> CartPlats { get; set; } = new List<CartPlat>();

        // Calculated property (not stored in DB) - useful for ViewModel
        [NotMapped]
        public decimal TotalPrice => CartPlats?.Sum(cp => cp.Quantity * cp.Plat?.Price ?? 0) ?? 0;
    }

    public enum CartStatus
    {
        Active,
        Ordered, // After payment simulation
        Abandoned
    }
}
