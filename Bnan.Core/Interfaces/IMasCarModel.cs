using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IMasCarModel
    {
        List<List<string>> GetAllCarModelsCount();

        int GetOneCarModelCount(string id);
    }
}
