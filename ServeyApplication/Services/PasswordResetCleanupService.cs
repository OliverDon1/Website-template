using Microsoft.Extensions.Hosting;
using ServeyApplication.Data;
using Microsoft.EntityFrameworkCore;

public class PasswordResetCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public PasswordResetCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var expiredTokens = await db.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.Used)
                .ToListAsync(stoppingToken);

            if (expiredTokens.Any())
            {
                db.PasswordResetTokens.RemoveRange(expiredTokens);
                await db.SaveChangesAsync(stoppingToken);
            }
        }
    }
}