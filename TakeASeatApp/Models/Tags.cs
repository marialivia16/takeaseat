namespace TakeASeatApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Tags
    {
        public Tags()
        {
            TagsForRestaurant = new HashSet<TagsForRestaurant>();
            TagsForUser = new HashSet<TagsForUser>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Count { get; set; }

        public virtual ICollection<TagsForRestaurant> TagsForRestaurant { get; set; }

        public virtual ICollection<TagsForUser> TagsForUser { get; set; }
    }
}
