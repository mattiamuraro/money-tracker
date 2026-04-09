using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MoneyTracker.Data.EntityFramework.ExtensionMethods
{
    public static class SeedExtensionMethods
    {
        public static async Task SeedDefaultDataAsync(this MoneyTrackerDbContext db, string actor, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("Seeding default data...");

            var recurrenceRuleTypeEntity = db.Model.FindEntityType(typeof(ForecastRecurrenceRuleType));
            var recurrenceRuleTypeTable = recurrenceRuleTypeEntity?.GetTableName();

            if (string.IsNullOrWhiteSpace(recurrenceRuleTypeTable) || !await TableExistsAsync(db, recurrenceRuleTypeTable, cancellationToken))
            {
                logger.LogWarning("Skipping forecast recurrence rule type seeding: table not found.");
                return;
            }

            var existingCodes = await db.Set<ForecastRecurrenceRuleType>()
                .Select(x => x.Code)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            var defaults = new[]
            {
            new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "One Time", Code = ForecastRecurrenceRuleType.OneTime, CreatedAt = now, CreatedBy = actor, ModifiedAt = now, ModifiedBy = actor, OrderIndex = 1 },
            new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Day", Code = ForecastRecurrenceRuleType.Day, CreatedAt = now, CreatedBy = actor, ModifiedAt = now, ModifiedBy = actor, OrderIndex = 2 },
            new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Week", Code = ForecastRecurrenceRuleType.Week, CreatedAt = now, CreatedBy = actor, ModifiedAt = now, ModifiedBy = actor, OrderIndex = 3 },
            new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Month", Code = ForecastRecurrenceRuleType.Month, CreatedAt = now, CreatedBy = actor, ModifiedAt = now, ModifiedBy = actor, OrderIndex = 4 },
            new ForecastRecurrenceRuleType { Id = Guid.NewGuid(), Name = "Year", Code = ForecastRecurrenceRuleType.Year, CreatedAt = now, CreatedBy = actor, ModifiedAt = now, ModifiedBy = actor, OrderIndex = 5 }
        };

            var missingDefaults = defaults
                .Where(x => !existingCodes.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missingDefaults.Count == 0)
            {
                logger.LogInformation("Default data already present.");
                return;
            }

            db.Set<ForecastRecurrenceRuleType>().AddRange(missingDefaults);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded {Count} forecast recurrence rule types.", missingDefaults.Count);
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
    }
}
