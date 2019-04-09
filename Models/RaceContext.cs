using Microsoft.EntityFrameworkCore;

namespace Podwoozka.Models
{
    public class RaceContext : DbContext
    {
        public RaceContext(DbContextOptions<RaceContext> options)
            : base(options)
        {
        }

        public DbSet<RaceItem> RaceItems { get; set; }
    }
}