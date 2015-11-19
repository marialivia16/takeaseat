namespace TakeASeatApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Restaurants
    {
        public Restaurants()
        {
            Managers = new HashSet<Managers>();
            Reservations = new HashSet<Reservations>();
            TagsForRestaurant = new HashSet<TagsForRestaurant>();
        }

        [StringLength(64)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }

        [StringLength(64)]
        public string PhoneNumber { get; set; }

        public string WebAddress { get; set; }

        [Required]
        [StringLength(8)]
        public string Verified { get; set; }

        public virtual ICollection<Managers> Managers { get; set; }

        public virtual ICollection<Reservations> Reservations { get; set; }

        public virtual ICollection<TagsForRestaurant> TagsForRestaurant { get; set; }
    }
}
