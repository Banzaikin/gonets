using CommonBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommonBackend;

public static class DbInitializer
{
    public static async Task ApplyMigrationsAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var applied = await db.Database.GetAppliedMigrationsAsync();
        var pending = await db.Database.GetPendingMigrationsAsync();

        if (pending.Any())
        {
            logger.LogInformation("Есть неприменённые миграции. Применяем...");
            await db.Database.MigrateAsync();
        }
        else
        {
            logger.LogInformation("Все миграции уже применены.");
        }
    }
}
