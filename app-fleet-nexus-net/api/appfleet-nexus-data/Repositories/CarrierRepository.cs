using Microsoft.EntityFrameworkCore;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Models;

namespace AppFleetNexus.Data.Repositories;

public class CarrierRepository : ICarrierRepository
{
    private readonly FleetNexusDbContext _context;

    public CarrierRepository(FleetNexusDbContext context)
    {
        _context = context;
    }

    public async Task<List<CarrierSummary>> GetTopCarriersByPowerUnitsAsync(int limit = 100)
    {
        return await _context.Carriers
            .Where(c => c.NbrPowerUnit.HasValue)
            .OrderByDescending(c => c.NbrPowerUnit)
            .Take(limit)
            .Select(c => new CarrierSummary(
                c.DotNumber,
                c.LegalName,
                c.PhyCity,
                c.PhyState,
                c.NbrPowerUnit))
            .ToListAsync();
    }

    public async Task<Carrier?> GetCarrierByDotNumberAsync(long dotNumber)
    {
        return await _context.Carriers
            .FirstOrDefaultAsync(c => c.DotNumber == dotNumber);
    }
}
