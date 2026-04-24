using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data.Base;

/// <summary>
/// Base entity with soft delete support
/// </summary>
public abstract class SoftDeleteEntity : BaseEntity
{
    /// <summary>
    /// Timestamp when entity was soft deleted (null if not deleted)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User who deleted the entity
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Indicates if the entity is deleted
    /// </summary>
    public bool IsDeleted { get; set; }
}
