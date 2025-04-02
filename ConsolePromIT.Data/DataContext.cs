using ConsolePromIT.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolePromIT.Data
{
    public class DataContext : DbContext
    {
        public DbSet<WordStatistic> WordStatistics { get; set; } = null!;
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            Database.EnsureCreated(); 
        }
    }
}
