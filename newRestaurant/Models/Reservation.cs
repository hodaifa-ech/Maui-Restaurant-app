using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime TimeStart { get; set; }

        [Required]
        public DateTime TimeEnd { get; set; }

        // Foreign Key for User
        public int UserId { get; set; }

        // Navigation Property (Many-to-One with User)
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Foreign Key for Table
        public int TableId { get; set; }

        // Navigation Property (Many-to-One with Table)
        [ForeignKey("TableId")]
        public virtual Table Table { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending; // Added Status
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }
}
