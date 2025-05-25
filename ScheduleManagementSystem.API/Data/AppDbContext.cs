using Microsoft.EntityFrameworkCore;
using ScheduleManagementSystem.API.Models;

namespace ScheduleManagementSystem.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<AuthMethod> AuthMethods { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure enum to use string values
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<AuthMethod>()
                .Property(u => u.Provider)
                .HasConversion<string>();

            modelBuilder.Entity<Event>()
                .Property(e => e.Type)
                .HasConversion<string>();

            // Configure relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.Groups)
                .WithMany(g => g.Users)
                .UsingEntity(j =>
                {
                    j.ToTable("GroupUsers");
                    j.Property<int>("UserId");
                    j.Property<int>("GroupId");
                });

            modelBuilder.Entity<AuthMethod>()
                .HasOne(am => am.User)
                .WithMany(u => u.AuthMethods)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Group)
                .WithMany(g => g.Events)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);


            // Configure uniqueness
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AuthMethod>()
                .HasIndex(a => new { a.UserId, a.Provider })
                .IsUnique()
                .HasDatabaseName("IX_UserProvider");

            //modelBuilder.Entity<Event>()
            //    .Property(e => e.Date)
            //    .HasConversion(
            //        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            //        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            //    );
        }
    }
}