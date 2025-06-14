using Microsoft.EntityFrameworkCore;
using PosyanduAPI.Models;

namespace PosyanduAPI.Data
{
    public class PosyanduContext : DbContext
    {
        public PosyanduContext(DbContextOptions<PosyanduContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.NIK)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.UserType)
                .HasMaxLength(10)
                .HasConversion<string>();
        }
    }
}