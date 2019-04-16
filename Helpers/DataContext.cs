using Microsoft.EntityFrameworkCore;
using Podwoozka.Entities;

namespace Podwoozka.Helpers
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}