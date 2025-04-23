using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TableNumber { get; set; } // More descriptive than just Id

        public int Capacity { get; set; } = 4; // Example property

        // Navigation Property (One-to-Many with Reservation)
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
