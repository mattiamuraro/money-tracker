using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using System.Data;

namespace MoneyTracker.Data.EntityFramework.ExtensionMethods
{
    public static class SeedExtensionMethods
    {
        public static async Task SeedDefaultDataAsync(this MoneyTrackerDbContext db, string actor, ILogger logger, CancellationToken cancellationToken)
        {
            await db.SeedDefaultDataAsync(forecastRecurrenceRuleTypes, actor, logger, cancellationToken);
        }

        public static async Task SeedDefaultDataAsync(this MoneyTrackerDbContext db, string actor, ILogger logger, IConfiguration configuration, CancellationToken cancellationToken)
        {
            await db.SeedDefaultDataAsync(forecastRecurrenceRuleTypes, actor, logger, cancellationToken);
            await db.SeedAdminUserAsync(logger, configuration, cancellationToken);
        }

        private static async Task SeedAdminUserAsync(this MoneyTrackerDbContext db, ILogger logger, IConfiguration configuration, CancellationToken cancellationToken)
        {
            if (await db.Users.AnyAsync(cancellationToken))
                return;

            var username = configuration["Auth:Username"];
            var password = configuration["Auth:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Admin user not seeded: Auth:Username or Auth:Password not configured.");
                return;
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = hasher.HashPassword(null!, password),
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded admin user '{Username}'.", username);
        }

        private static async Task SeedDefaultDataAsync<T>(this MoneyTrackerDbContext db, T[] entities, string actor, ILogger logger, CancellationToken cancellationToken) where T : BaseContextEntity
        {
            logger.LogInformation("Seeding default data...");

            var entity = db.Model.FindEntityType(typeof(T));
            var entityTableName = entity?.GetTableName();

            if (string.IsNullOrWhiteSpace(entityTableName) || !await TableExistsAsync(db, entityTableName, cancellationToken))
            {
                logger.LogWarning($"Skipping {entityTableName} rule type seeding: table not found.");
                return;
            }

            var existingCodes = await db.Set<T>()
                .Select(x => x.Code)
                .ToListAsync(cancellationToken);

            var missingDefaults = entities
                .Where(x => !existingCodes.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missingDefaults.Count == 0)
            {
                logger.LogInformation("Default data already present.");
                return;
            }

            db.Set<T>().AddRange(missingDefaults);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Seeded {missingDefaults.Count} {entityTableName}.");
        }

        private static async Task<bool> TableExistsAsync(MoneyTrackerDbContext db, string tableName, CancellationToken cancellationToken)
        {
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync(cancellationToken);

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName) THEN 1 ELSE 0 END";

                var tableNameParameter = command.CreateParameter();
                tableNameParameter.ParameterName = "@tableName";
                tableNameParameter.Value = tableName;
                command.Parameters.Add(tableNameParameter);

                var result = await command.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt32(result) == 1;
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        private static ForecastRecurrenceRuleType[] forecastRecurrenceRuleTypes =
        {
                new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "One Time", Code = ForecastRecurrenceRuleType.OneTime, OrderIndex = 1 },
                new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Day", Code = ForecastRecurrenceRuleType.Day, OrderIndex = 2 },
                new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Week", Code = ForecastRecurrenceRuleType.Week, OrderIndex = 3 },
                new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Month", Code = ForecastRecurrenceRuleType.Month, OrderIndex = 4 },
                new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Year", Code = ForecastRecurrenceRuleType.Year, OrderIndex = 5 }
        };
    }
}