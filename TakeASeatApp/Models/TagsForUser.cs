namespace TakeASeatApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TagsForUser")]
    public partial class TagsForUser
    {
        public int Id { get; set; }

        public int TagId { get; set; }

        [Required]
        [StringLength(64)]
        public string UserId { get; set; }

        public int Count { get; set; }

        public virtual AspNetUsers AspNetUsers { get; set; }

        public virtual Tags Tags { get; set; }
    }
}
