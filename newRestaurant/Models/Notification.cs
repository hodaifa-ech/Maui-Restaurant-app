using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Message { get; set; } // Added Message body

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        // Foreign Key for User
        public int UserId { get; set; }

        // Navigation Property (Many-to-One with User)
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
