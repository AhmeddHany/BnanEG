using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Quartz
{
    public class RefreshLoginJob : IJob
    {
        private readonly IUpdateStatusForUser _updateStatusForUser;

        public RefreshLoginJob(IUpdateStatusForUser updateStatusForUser)
        {
            _updateStatusForUser = updateStatusForUser;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _updateStatusForUser.RefreshLogin();
        }
    }
}
