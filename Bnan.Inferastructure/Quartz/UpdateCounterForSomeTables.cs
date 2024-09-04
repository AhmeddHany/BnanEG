using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Quartz
{
    public class UpdateCounterForSomeTables : IJob
    {
        private readonly IUpdateCountOfTypeRenter _updateCountOfType;

        public UpdateCounterForSomeTables(IUpdateCountOfTypeRenter updateCountOfType)
        {
            _updateCountOfType = updateCountOfType;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var Renters = await _updateCountOfType.GetActiveRenters();
            var RentersPost = await _updateCountOfType.GetActivePostRenter();
            var CarColors = await _updateCountOfType.GetActiveCars();
            await _updateCountOfType.UpdateColorCarsCount(CarColors);
            await _updateCountOfType.UpdateNationalitiesCount(Renters);
            await _updateCountOfType.UpdateCountriesPostRenterCount(RentersPost);
            await _updateCountOfType.UpdateKeyCallingsCount(Renters);

        }
    }
}
