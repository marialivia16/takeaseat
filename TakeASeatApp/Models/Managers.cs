namespace TakeASeatApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Managers
    {
        [Key]
        [StringLength(64)]
        public string UserId { get; set; }

        [Required]
        [StringLength(64)]
        public string RestaurantId { get; set; }

        public virtual Restaurants Restaurants { get; set; }
    }
}
