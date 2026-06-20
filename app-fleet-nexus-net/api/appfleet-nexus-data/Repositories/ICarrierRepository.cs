using appfleet_nexus_data.Models;

namespace appfleet_nexus_data.Repositories;

public interface ICarrierRepository
{
    Task<List<CarrierSummary>> GetTopCarriersByPowerUnitsAsync(int limit = 100);
    Task<Carrier?> GetCarrierByDotNumberAsync(long dotNumber);
}
