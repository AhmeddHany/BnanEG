using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
