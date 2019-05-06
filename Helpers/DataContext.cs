using Microsoft.EntityFrameworkCore;
using Podwoozka.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Podwoozka.Helpers
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}