using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.UpdateDataBaseJobs
{
    public interface IUpdateCountOfTypeRenter
    {
        Task<List<CrMasRenterInformation>> GetActiveRenters();
        Task<List<CrCasCarInformation>> GetActiveCars();
        Task<List<CrMasRenterPost>> GetActivePostRenter();
        Task UpdateCountriesPostRenterCount(List<CrMasRenterPost> renters);
        Task UpdateNationalitiesCount(List<CrMasRenterInformation> renters);
        Task UpdateColorCarsCount(List<CrCasCarInformation> renters);
        Task UpdateKeyCallingsCount(List<CrMasRenterInformation> renters);
        Task UpdateDistributionCarCount();

    }
}
