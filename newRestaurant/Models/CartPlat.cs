using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newRestaurant.Models
{
    public partial class CartPlat : ObservableObject
    {
        // Composite Key defined in DbContext
        public int CartId { get; set; }
        
        public int PlatId { get; set; }

        [ObservableProperty] // <-- Add this attribute
        private int _quantity = 1; // Backing field needed for source generator

        // Navigation Properties
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("PlatId")]
        public virtual Plat Plat { get; set; }

        [NotMapped]
        public decimal TotalLinePrice => (Plat?.Price ?? 0) * Quantity;
    }
}
