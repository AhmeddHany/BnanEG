using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IMasRenterEmployer
    {
        List<List<string>> GetAllRenterEmployersCount();

        int GetOneRenterEmployerCount(string id);
    }
}
