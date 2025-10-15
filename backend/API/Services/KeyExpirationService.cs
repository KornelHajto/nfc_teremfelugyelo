using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace API.Services
{
    public class KeyExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public KeyExpirationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.UtcNow;

                    var expiredKeys = await db.Keys
                        .Where(k => k.Expiration != null && k.Expiration <= now)
                        .ToListAsync(stoppingToken);

                    if (expiredKeys.Count > 0)
                    {
                        db.Keys.RemoveRange(expiredKeys);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
