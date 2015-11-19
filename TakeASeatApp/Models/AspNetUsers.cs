namespace TakeASeatApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AspNetUsers
    {
        public AspNetUsers()
        {
            Reservations = new HashSet<Reservations>();
            TagsForUser = new HashSet<TagsForUser>();
        }

        [StringLength(64)]
        public string Id { get; set; }

        [StringLength(128)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        [StringLength(128)]
        public string PasswordHash { get; set; }

        [StringLength(64)]
        public string SecurityStamp { get; set; }

        [StringLength(64)]
        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        [StringLength(128)]
        public string UserName { get; set; }

        public bool IsBlocked { get; set; }

        [StringLength(64)]
        public string FirstName { get; set; }

        [StringLength(64)]
        public string LastName { get; set; }

        public int NotificationCount { get; set; }

        public virtual ICollection<Reservations> Reservations { get; set; }

        public virtual ICollection<TagsForUser> TagsForUser { get; set; }
    }
}
