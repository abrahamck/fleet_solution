using System;

namespace AppFleetNexus.Data.Models;

/// <summary>
/// Base class for all tenant-scoped entities.
/// Provides tenant isolation, audit columns, and soft delete.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    // Audit
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
}
