using System;
using System.Collections.Generic;

namespace AppFleetNexus.Data.Models;

public class User
{
    public Guid Id { get; set; }           // Same as auth.users.id
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedDate { get; set; }

    // Audit
    public Guid? CreatedBy { get; set; }   // NULL on self-signup
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }

    // Navigation
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}
