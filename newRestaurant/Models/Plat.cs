using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Models
{
    public class Plat // Renamed from Dish if needed, sticking to original request 'Plat'
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000.00)] // Example price range
        public decimal Price { get; set; } // Use decimal for currency

        // Foreign Key for Category
        public int CategoryId { get; set; }

        // Navigation Property (Many-to-One with Category)
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        // Navigation Property for Many-to-Many with Cart (via CartPlat)
        public virtual ICollection<CartPlat> CartPlats { get; set; } = new List<CartPlat>();
    }
}
