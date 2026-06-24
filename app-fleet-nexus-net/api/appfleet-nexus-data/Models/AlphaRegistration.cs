using System;
using System.Collections.Generic;

namespace AppFleetNexus.Data.Models;

public class AlphaRegistration
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    
    // Maps to PostgreSQL text[] array natively
    public List<string> Roadblocks { get; set; } = new();
    
    public string? CustomRoadblock { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
