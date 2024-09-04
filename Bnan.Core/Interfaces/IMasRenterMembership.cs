using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IMasRenterMembership
    {
        List<List<string>> GetAllRenterMembershipsCount();

        int GetOneRenterMembershipCount(string id);
    }
}
