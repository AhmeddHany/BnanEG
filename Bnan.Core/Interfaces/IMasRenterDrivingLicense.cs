using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IMasRenterDrivingLicense
    {
        List<List<string>> GetAllRenterDrivingLicensesCount();

        int GetOneRenterDrivingLicenseCount(string id);
    }
}
