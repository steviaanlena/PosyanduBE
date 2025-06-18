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
            // Configure User entity for PostgreSQL
            modelBuilder.Entity<User>(entity =>
            {
                // Use lowercase table name (PostgreSQL convention)
                entity.ToTable("users");

                // Configure primary key with PostgreSQL identity
                entity.Property(e => e.Id)
                    .UseIdentityColumn();

                // Configure NIK with unique index
                entity.HasIndex(u => u.NIK)
                    .IsUnique()
                    .HasDatabaseName("IX_users_nik"); // Explicit index name

                // Configure NIK column
                entity.Property(u => u.NIK)
                    .IsRequired()
                    .HasMaxLength(450);

                // Configure UserType
                entity.Property(u => u.UserType)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasConversion<string>();

                // Configure other string properties with proper lengths
                entity.Property(u => u.Nama)
                    .HasMaxLength(200)
                    .HasDefaultValue("");

                entity.Property(u => u.Alamat)
                    .HasMaxLength(500)
                    .HasDefaultValue("");

                entity.Property(u => u.NoTelp)
                    .HasMaxLength(20)
                    .HasDefaultValue("");

                entity.Property(u => u.TTL)
                    .HasMaxLength(200)
                    .HasDefaultValue("");

                // Configure timestamp with timezone
                entity.Property(u => u.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure binary data for password hash and salt
                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.PasswordSalt)
                    .IsRequired();
            });
        }
    }
}