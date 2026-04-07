using Microsoft.EntityFrameworkCore;

namespace MoneyTracker.Data.EntityFramework
{
    public class MoneyTrackerDbContext : DbContext
    {
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentCategory> PaymentCategories { get; set; }

        public DbSet<ForecastRecurrenceRule> ForecastRecurrenceRules { get; set; }
        public DbSet<ForecastExpense> ForecastExpenses { get; set; }
        public DbSet<ForecastIncome> ForecastIncomes { get; set; }


        public MoneyTrackerDbContext(DbContextOptions<MoneyTrackerDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ModifiedAt)
                    .IsRequired();

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                // Soft delete properties
                entity.Property(e => e.DeletedAt);
                entity.Property(e => e.DeletedBy)
                    .HasMaxLength(100);

                // Configure relationship with PaymentCategory
                entity.HasOne(e => e.PaymentCategory)
                    .WithMany(pc => pc.Payments)
                    .HasForeignKey(e => e.PaymentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

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

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ModifiedAt)
                    .IsRequired();

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.Code)
                    .IsUnique();
            });
        }
    }
}
