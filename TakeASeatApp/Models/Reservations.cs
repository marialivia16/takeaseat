namespace TakeASeatApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Reservations
    {
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string UserId { get; set; }

        [Required]
        [StringLength(64)]
        public string RestaurantId { get; set; }

        public int Duration { get; set; }

        public int NumberOfGuests { get; set; }

        public DateTime DateAndTime { get; set; }

        public DateTime SentOn { get; set; }

        [Required]
        [StringLength(32)]
        public string Status { get; set; }

        [StringLength(64)]
        public string ManagerId { get; set; }

        public virtual AspNetUsers AspNetUsers { get; set; }

        public virtual Restaurants Restaurants { get; set; }
    }
}
