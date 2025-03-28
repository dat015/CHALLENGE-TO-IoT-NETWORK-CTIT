using Microsoft.EntityFrameworkCore;
using Web3IoT.Models;

namespace Web3IoT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Crop> Crops { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Fertilizer> Fertilizers { get; set; }
        public DbSet<Pesticide> Pesticides { get; set; }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);

        //     // Configure relationships and constraints here
        //     modelBuilder.Entity<Farm>()
        //         .HasOne(f => f.User)
        //         .WithMany(u => u.Farms)
        //         .HasForeignKey(f => f.UserId)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     modelBuilder.Entity<Crop>()
        //         .HasOne(c => c.Farm)
        //         .WithMany(f => f.Crops)
        //         .HasForeignKey(c => c.FarmId)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     modelBuilder.Entity<Sensor>()
        //         .HasOne(s => s.Farm)
        //         .WithMany(f => f.Sensors)
        //         .HasForeignKey(s => s.FarmId)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     modelBuilder.Entity<Device>()
        //         .HasOne(d => d.Farm)
        //         .WithMany(f => f.Devices)
        //         .HasForeignKey(d => d.FarmId)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     modelBuilder.Entity<Fertilizer>()
        //         .HasOne(f => f.Farm)
        //         .WithMany(f => f.Fertilizers)
        //         .HasForeignKey(f => f.FarmId)
        //         .OnDelete(DeleteBehavior.Cascade);

        //     modelBuilder.Entity<Pesticide>()
        //         .HasOne(p => p.Farm)
        //         .WithMany(f => f.Pesticides)
        //         .HasForeignKey(p => p.FarmId)
        //         .OnDelete(DeleteBehavior.Cascade);
        // }
    }
} 