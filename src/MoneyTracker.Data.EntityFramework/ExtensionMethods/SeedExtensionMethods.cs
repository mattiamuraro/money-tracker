using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using System.Data;

namespace MoneyTracker.Data.EntityFramework.ExtensionMethods
{
    public static class SeedExtensionMethods
    {
        public static async Task SeedDefaultDataAsync(this MoneyTrackerDbContext db, ILogger logger, AdminCredentialsOptions adminCredentials, CancellationToken cancellationToken)
        {
            await db.SeedSystemUserAsync(logger, cancellationToken);
            await db.SeedAdminUserAsync(logger, adminCredentials, cancellationToken);
            await db.SeedDefaultDataAsync(forecastRecurrenceRuleTypes, logger, cancellationToken);
            await db.SeedDefaultDataAsync(forecastOccurrenceStatuses, logger, cancellationToken);
        }

        private static async Task SeedSystemUserAsync(this MoneyTrackerDbContext db, ILogger logger, CancellationToken cancellationToken)
        {
            var systemUserExists = await db.Users
                .AnyAsync(u => u.Id == SystemUsers.SystemUserId, cancellationToken);

            if (systemUserExists)
                return;

            var systemUser = new User
            {
                Id = SystemUsers.SystemUserId,
                Username = SystemUsers.SystemUsername,
                PasswordHash = string.Empty,
            };

            db.Users.Add(systemUser);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded system user '{Username}' ({UserId}).", SystemUsers.SystemUsername, SystemUsers.SystemUserId);
        }

        private static async Task SeedAdminUserAsync(this MoneyTrackerDbContext db, ILogger logger, AdminCredentialsOptions adminCredentials, CancellationToken cancellationToken)
        {
            var username = adminCredentials.Username;
            var password = adminCredentials.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Admin user not seeded: Auth:Username or Auth:Password not configured.");
                return;
            }

            var adminExists = await db.Users
                .AnyAsync(u => u.Username == username, cancellationToken);

            if (adminExists)
                return;

            var hasher = new PasswordHasher<User>();
            var passwordHash = hasher.HashPassword(null!, password);
            var user = new User
            {
                Id = Guid.CreateVersion7(),
                Username = username,
                PasswordHash = passwordHash,
            };

            db.Users.Add(user);
            db.UserPasswordHistories.Add(new UserPasswordHistory
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded admin user '{Username}'.", username);
        }

        private static async Task SeedDefaultDataAsync<T>(this MoneyTrackerDbContext db, T[] entities, ILogger logger, CancellationToken cancellationToken) where T : BaseContextEntity
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
                new ForecastRecurrenceRuleType { Id = Guid.CreateVersion7(), Name = "One Time", Code = ForecastRecurrenceRuleType.OneTime, OrderIndex = 1 },
                new ForecastRecurrenceRuleType { Id = Guid.CreateVersion7(), Name = "Day", Code = ForecastRecurrenceRuleType.Day, OrderIndex = 2 },
                new ForecastRecurrenceRuleType { Id = Guid.CreateVersion7(), Name = "Week", Code = ForecastRecurrenceRuleType.Week, OrderIndex = 3 },
                new ForecastRecurrenceRuleType { Id = Guid.CreateVersion7(), Name = "Month", Code = ForecastRecurrenceRuleType.Month, OrderIndex = 4 },
                new ForecastRecurrenceRuleType { Id = Guid.CreateVersion7(), Name = "Year", Code = ForecastRecurrenceRuleType.Year, OrderIndex = 5 }
        };

        private static ForecastOccurrenceStatus[] forecastOccurrenceStatuses =
        {
                new ForecastOccurrenceStatus { Id = ForecastOccurrenceStatus.PendingId, Name = "Pending", Code = ForecastOccurrenceStatus.Pending, OrderIndex = 1 },
                new ForecastOccurrenceStatus { Id = ForecastOccurrenceStatus.ConfirmedId, Name = "Confirmed", Code = ForecastOccurrenceStatus.Confirmed, OrderIndex = 2 },
                new ForecastOccurrenceStatus { Id = ForecastOccurrenceStatus.SkippedId, Name = "Skipped", Code = ForecastOccurrenceStatus.Skipped, OrderIndex = 3 },
                new ForecastOccurrenceStatus { Id = ForecastOccurrenceStatus.CancelledId, Name = "Cancelled", Code = ForecastOccurrenceStatus.Cancelled, OrderIndex = 4 }
        };
    }
}