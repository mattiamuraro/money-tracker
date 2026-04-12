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
        public DbSet<User> Users { get; set; }

        public MoneyTrackerDbContext(DbContextOptions<MoneyTrackerDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.CreatedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);
                entity.Property(e => e.ModifiedAt).IsRequired();
                entity.Property(e => e.ModifiedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);
                entity.HasIndex(e => e.Username).IsUnique();
                ConfigureAuditRelations(entity);
            });

            // Configure Payment entity with soft delete support
            modelBuilder.Entity<Payment>(entity =>
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

                // Soft delete properties
                entity.Property(e => e.DeletedAt);
                entity.Property(e => e.DeletedBy);

                // Configure relationship with PaymentCategory
                entity.HasOne(e => e.PaymentCategory)
                    .WithMany(pc => pc.Payments)
                    .HasForeignKey(e => e.PaymentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                ConfigureAuditRelations(entity);

                // Indexes
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.PaymentCategoryId);
                entity.HasIndex(e => e.DeletedAt);
                entity.HasIndex(e => e.IdempotencyKey)
                    .IsUnique()
                    .HasFilter("[IdempotencyKey] IS NOT NULL");

                // Global query filter for soft deletes
                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            modelBuilder.Entity<Income>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Date)
                    .IsRequired();

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

                // Soft delete properties
                entity.Property(e => e.DeletedAt);
                entity.Property(e => e.DeletedBy);

                ConfigureAuditRelations(entity);

                // Indexes
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.DeletedAt);
                entity.HasIndex(e => e.IdempotencyKey)
                    .IsUnique()
                    .HasFilter("[IdempotencyKey] IS NOT NULL");

                // Global query filter for soft deletes
                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            // Configure PaymentCategory entity
            modelBuilder.Entity<PaymentCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(5);

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

                entity.HasIndex(e => e.Code)
                    .IsUnique();
            });

            modelBuilder.Entity<ForecastExpense>(entity =>
            {
                entity.Property(e => e.CreatedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);
                entity.Property(e => e.ModifiedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);

                entity.HasOne(e => e.ForecastRecurrenceRuleType)
                    .WithMany()
                    .HasForeignKey(e => e.ForecastRecurrenceRuleTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                ConfigureAuditRelations(entity);
            });

            modelBuilder.Entity<ForecastIncome>(entity =>
            {
                entity.Property(e => e.CreatedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);
                entity.Property(e => e.ModifiedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);

                entity.HasOne(e => e.ForecastRecurrenceRuleType)
                    .WithMany()
                    .HasForeignKey(e => e.ForecastRecurrenceRuleTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                ConfigureAuditRelations(entity);
            });

            modelBuilder.Entity<ForecastRecurrenceRuleType>(entity =>
            {
                entity.Property(e => e.CreatedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);
                entity.Property(e => e.ModifiedById)
                    .IsRequired()
                    .HasDefaultValue(SystemUsers.SystemUserId);

                ConfigureAuditRelations(entity);
            });
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
