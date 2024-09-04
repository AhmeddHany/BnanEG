using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Quartz
{
    public class UpdateContractJob : IJob
    {

        private readonly IUpdateStatusForContracts _updateStatusForContracts;

        public UpdateContractJob(IUpdateStatusForContracts updateStatusForContracts)
        {
            _updateStatusForContracts = updateStatusForContracts;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _updateStatusForContracts.UpdateDatabase();
        }

    }
}
