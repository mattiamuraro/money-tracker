using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using System.Security.Claims;

namespace MoneyTracker.Data.EntityFramework
{
    public class MoneyTrackerDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentCategory> PaymentCategories { get; set; }
        public DbSet<Income> Incomes { get; set; }

        public DbSet<ForecastExpense> ForecastExpenses { get; set; }
        public DbSet<ForecastIncome> ForecastIncomes { get; set; }
        public DbSet<ForecastRecurrenceRuleType> ForecastRecurrenceRuleTypes { get; set; }
        public DbSet<ForecastOccurrenceStatus> ForecastOccurrenceStatuses { get; set; }

        public DbSet<ForecastOccurrence> ForecastOccurrences { get; set; }

        public DbSet<User> Users { get; set; }

        public MoneyTrackerDbContext(DbContextOptions<MoneyTrackerDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUserEntity(modelBuilder.Entity<User>());
            ConfigurePaymentEntity(modelBuilder.Entity<Payment>());
            ConfigureIncomeEntity(modelBuilder.Entity<Income>());
            ConfigurePaymentCategoryEntity(modelBuilder.Entity<PaymentCategory>());
            ConfigureForecastExpenseEntity(modelBuilder.Entity<ForecastExpense>());
            ConfigureForecastIncomeEntity(modelBuilder.Entity<ForecastIncome>());
            ConfigureForecastRecurrenceRuleTypeEntity(modelBuilder.Entity<ForecastRecurrenceRuleType>());
            ConfigureForecastOccurrenceStatusEntity(modelBuilder.Entity<ForecastOccurrenceStatus>());
            ConfigureForecastOccurrenceEntity(modelBuilder.Entity<ForecastOccurrence>());
        }

        private static void ConfigureUserEntity(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .IsRequired();

            ConfigureAuditedEntity(entity);

            entity.HasIndex(e => e.Username)
                .IsUnique();
        }

        private static void ConfigurePaymentEntity(EntityTypeBuilder<Payment> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Date)
                .IsRequired();

            entity.Property(e => e.IsOneShot);

            entity.HasOne(e => e.PaymentCategory)
                .WithMany(pc => pc.Payments)
                .HasForeignKey(e => e.PaymentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            ConfigureAuditedEntity(entity);
            ConfigureSoftDeleteEntity(entity);

            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.PaymentCategoryId);
            entity.HasIndex(e => e.IdempotencyKey)
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
        }

        private static void ConfigureIncomeEntity(EntityTypeBuilder<Income> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Date)
                .IsRequired();

            ConfigureAuditedEntity(entity);
            ConfigureSoftDeleteEntity(entity);

            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.IdempotencyKey)
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
        }

        private static void ConfigurePaymentCategoryEntity(EntityTypeBuilder<PaymentCategory> entity)
        {
            ConfigureContextEntity(entity);
        }

        private static void ConfigureForecastExpenseEntity(EntityTypeBuilder<ForecastExpense> entity)
        {
            ConfigureForecastEntity(entity);
        }

        private static void ConfigureForecastIncomeEntity(EntityTypeBuilder<ForecastIncome> entity)
        {
            ConfigureForecastEntity(entity);
        }

        private static void ConfigureForecastRecurrenceRuleTypeEntity(EntityTypeBuilder<ForecastRecurrenceRuleType> entity)
        {
            ConfigureContextEntity(entity);
        }

        private static void ConfigureForecastOccurrenceStatusEntity(EntityTypeBuilder<ForecastOccurrenceStatus> entity)
        {
            ConfigureContextEntity(entity);
        }

        private static void ConfigureForecastOccurrenceEntity(EntityTypeBuilder<ForecastOccurrence> entity)
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.ExpectedDate)
                .IsRequired();

            entity.Property(e => e.ForecastOccurrenceStatusId)
                .IsRequired()
                .HasDefaultValue(ForecastOccurrenceStatus.PendingId);

            entity.HasOne(e => e.PaymentCategory)
                .WithMany()
                .HasForeignKey(e => e.PaymentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.ForecastOccurrenceStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            ConfigureAuditedEntity(entity);

            entity.HasIndex(e => e.ExpectedDate);
            entity.HasIndex(e => e.ForecastDefinitionId);
            entity.HasIndex(e => e.ForecastOccurrenceStatusId);
            entity.HasIndex(e => e.PaymentCategoryId);
        }

        private static void ConfigureAuditedEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
            where TEntity : BaseEntity
        {
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.CreatedById)
                .IsRequired()
                .HasDefaultValue(SystemUsers.SystemUserId);

            entity.Property(e => e.ModifiedAt)
                .IsRequired();

            entity.Property(e => e.ModifiedById)
                .IsRequired()
                .HasDefaultValue(SystemUsers.SystemUserId);

            ConfigureAuditRelations(entity);
        }

        private static void ConfigureContextEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
            where TEntity : BaseContextEntity
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(5);

            ConfigureAuditedEntity(entity);

            entity.HasIndex(e => e.Code)
                .IsUnique();
        }

        private static void ConfigureForecastEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
            where TEntity : BaseForecast
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.HasOne(e => e.ForecastRecurrenceRuleType)
                .WithMany()
                .HasForeignKey(e => e.ForecastRecurrenceRuleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            ConfigureAuditedEntity(entity);
        }

        private static void ConfigureSoftDeleteEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
            where TEntity : SoftDeleteEntity
        {
            entity.Property(e => e.DeletedAt);
            entity.Property(e => e.DeletedBy);

            entity.HasIndex(e => e.DeletedAt);
            entity.HasQueryFilter(e => !e.IsDeleted);
        }

        private static void ConfigureAuditRelations<TEntity>(EntityTypeBuilder<TEntity> entity)
            where TEntity : BaseEntity
        {
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedById)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges()
        {
            ApplyAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;
                var currentUser = GetCurrentUser();




                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedById = currentUser;
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedById = currentUser;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedById = currentUser;

                    // Prevent changes to CreatedAt and CreatedById
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedById).IsModified = false;
                }

                if (entry.Entity is SoftDeleteEntity softDeleteEntity
                    && softDeleteEntity.IsDeleted && !softDeleteEntity.DeletedBy.HasValue)
                {
                    softDeleteEntity.DeletedAt = now;
                    softDeleteEntity.DeletedBy = currentUser;
                }
            }
        }

        /// <summary>
        /// Gets the current user identifier. Override this method to implement your own logic
        /// for retrieving the current user (e.g., from HttpContext, claims, etc.)
        /// </summary>
        protected Guid GetCurrentUser()
        {
            var currentUserClaim = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(currentUserClaim, out var userId)
                ? userId
                : SystemUsers.SystemUserId;
        }
    }
}
