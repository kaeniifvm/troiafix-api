using Microsoft.EntityFrameworkCore;
using TroiaFix.API.Models;

namespace TroiaFix.API.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<License> Licenses { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}