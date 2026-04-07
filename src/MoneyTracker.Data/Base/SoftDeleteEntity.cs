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
    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Indicates if the entity is deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Soft delete the entity
    /// </summary>
    public void Delete(string deletedBy)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        IsDeleted = true;
    }

    /// <summary>
    /// Restore a soft-deleted entity
    /// </summary>
    public void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
    }
}
