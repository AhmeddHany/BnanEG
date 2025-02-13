using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IPostBranch
    {
        Task<bool> AddPostBranchDefault(string LessorCode, CrCasBranchPost CrCasBranchPost, CrMasSupPostCity City);
        Task<bool> AddPostBranch(CrCasBranchPost CrCasBranchPost);
        Task<bool> UpdatePostBranch(CrCasBranchPost CrCasBranchPost);
        List<CrCasBranchPost> GetAllByLessor(string LessorCode);

    }
}
