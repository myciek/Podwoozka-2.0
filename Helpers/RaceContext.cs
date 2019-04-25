using Microsoft.EntityFrameworkCore;
using System;
using Podwoozka.Entities;
namespace Podwoozka.Helpers
{
    public class RaceContext : DbContext
    {
        public RaceContext(DbContextOptions<RaceContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RaceItem>()
            .Property(e => e.Participants)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            // Customize the ASP.NET Core Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Core Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public DbSet<RaceItem> RaceItems { get; set; }

    }
}