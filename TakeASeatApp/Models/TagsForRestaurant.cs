namespace TakeASeatApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TagsForRestaurant")]
    public partial class TagsForRestaurant
    {
        public int Id { get; set; }

        public int TagId { get; set; }

        [Required]
        [StringLength(64)]
        public string RestaurantId { get; set; }

        public virtual Restaurants Restaurants { get; set; }

        public virtual Tags Tags { get; set; }
    }
}
