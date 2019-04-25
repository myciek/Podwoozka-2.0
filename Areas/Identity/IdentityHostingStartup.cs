using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Podwoozka.Areas.Identity.Data;

[assembly: HostingStartup(typeof(Podwoozka.Areas.Identity.IdentityHostingStartup))]
namespace Podwoozka.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<PodwoozkaIdentityDbContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("PodwoozkaIdentityDbContextConnection")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<PodwoozkaIdentityDbContext>();
            });
        }
    }
}