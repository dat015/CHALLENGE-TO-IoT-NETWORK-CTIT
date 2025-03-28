using Microsoft.EntityFrameworkCore;
using BlockchainMVC.Models;

namespace BlockchainMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Crop> Crops { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<Origin> Origins { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Trace> Traces { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Fertilizer> Fertilizers { get; set; }
        public DbSet<Pesticide> Pesticides { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình unique cho Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cấu hình unique cho mã cảm biến
            modelBuilder.Entity<Sensor>()
                .HasIndex(s => s.SensorCode)
                .IsUnique();

            modelBuilder.Entity<Sensor>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Sensor>()
                .HasIndex(s => s.Type)
                .IsUnique();

            // Seed initial sensors
            var fixedDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Sensor>().HasData(
                new Sensor { Id = 1, Name = "Cảm biến nhiệt độ 1", SensorCode = "TEMP001" },
                new Sensor { Id = 2, Name = "Cảm biến độ ẩm 1", SensorCode = "HUM001" },
                new Sensor { Id = 1, Name = "Cảm biến nhiệt độ 1", sensorCode = "TEMP001" },
                new Sensor { Id = 2, Name = "Cảm biến độ ẩm 1", sensorCode = "HUM001" },
                new Sensor { Id = 3, Name = "Cảm biến ánh sáng 1", sensorCode = "LIGHT001" }
            );

            // Seed initial crops
            modelBuilder.Entity<Crop>().HasData(
                new Crop 
                { 
                    Id = 1, 
                    Name = "Lúa 1", 
                    sensorId = 1, 
                    DatePlanted = fixedDateTime,
                    DateHarvested = null
                },
                new Crop 
                { 
                    Id = 2, 
                    Name = "Ngô 1", 
                    sensorId = 2, 
                    DatePlanted = fixedDateTime,
                    DateHarvested = null
                },
                new Crop 
                { 
                    Id = 3, 
                    Name = "Khoai tây 1", 
                    sensorId = 3, 
                    DatePlanted = fixedDateTime,
                    DateHarvested = null
                }
            );
        }
    }
} 