using AppFleetNexus.Data.Models;

namespace AppFleetNexus.Data.Repositories;

public interface ICarrierRepository
{
    Task<List<CarrierSummary>> GetTopCarriersByPowerUnitsAsync(int limit = 100);
    Task<Carrier?> GetCarrierByDotNumberAsync(long dotNumber);
}
