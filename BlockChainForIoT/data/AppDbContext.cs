using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockChainForIoT.blockchain;
using Microsoft.EntityFrameworkCore;

namespace BlockChainForIoT.data
{
    public class AppDbContext : DbContext
    {
        // public DbSet<model.Farm> Farms { get; set; }
        // public DbSet<model.Batch> Batches { get; set; }
        // public DbSet<model.Sensor> Sensors { get; set; }    
        // public DbSet<model.TransactionSensor> TransactionSensors { get; set; }
        // public DbSet<model.TransactionAction> TransactionActions { get; set; }
        // public DbSet<model.TransactionOrigin> TransactionOrigins { get; set; }
        public DbSet<model.Crop> Crops { get; set; }
        public DbSet<model.Sensor> Sensors { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    }
}