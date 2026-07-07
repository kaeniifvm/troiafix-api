using Microsoft.EntityFrameworkCore;
using TroiaFix.API.Models;

namespace TroiaFix.API.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<License> Licenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=troiafix.db");
        }
    }
}