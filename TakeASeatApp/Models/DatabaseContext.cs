namespace TakeASeatApp.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Managers> Managers { get; set; }
        public virtual DbSet<Reservations> Reservations { get; set; }
        public virtual DbSet<Restaurants> Restaurants { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<TagsForRestaurant> TagsForRestaurant { get; set; }
        public virtual DbSet<TagsForUser> TagsForUser { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetUsers>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<AspNetUsers>()
                .Property(e => e.PasswordHash)
                .IsUnicode(false);

            modelBuilder.Entity<AspNetUsers>()
                .Property(e => e.SecurityStamp)
                .IsUnicode(false);

            modelBuilder.Entity<AspNetUsers>()
                .Property(e => e.PhoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.Reservations)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.TagsForUser)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Restaurants>()
                .Property(e => e.Longitude)
                .HasPrecision(11, 8);

            modelBuilder.Entity<Restaurants>()
                .Property(e => e.Latitude)
                .HasPrecision(10, 8);

            modelBuilder.Entity<Restaurants>()
                .Property(e => e.PhoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<Restaurants>()
                .HasMany(e => e.Managers)
                .WithRequired(e => e.Restaurants)
                .HasForeignKey(e => e.RestaurantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Restaurants>()
                .HasMany(e => e.Reservations)
                .WithRequired(e => e.Restaurants)
                .HasForeignKey(e => e.RestaurantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Restaurants>()
                .HasMany(e => e.TagsForRestaurant)
                .WithRequired(e => e.Restaurants)
                .HasForeignKey(e => e.RestaurantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tags>()
                .HasMany(e => e.TagsForRestaurant)
                .WithRequired(e => e.Tags)
                .HasForeignKey(e => e.TagId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tags>()
                .HasMany(e => e.TagsForUser)
                .WithRequired(e => e.Tags)
                .HasForeignKey(e => e.TagId)
                .WillCascadeOnDelete(false);
        }
    }
}
