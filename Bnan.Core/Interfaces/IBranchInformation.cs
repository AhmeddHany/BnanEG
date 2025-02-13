using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IBranchInformation
    {
        Task<List<CrCasBranchInformation>> GetAllAsyncByLessor(string lessorCode);
        Task<bool> AddBranchInformationDefault(string LesssorCode);
        Task<bool> AddBranchInformation(CrCasBranchInformation CrCasBranchInformation);
        Task<bool> UpdateBranchInformation(CrCasBranchInformation CrCasBranchInformation);
        Task<bool> ExistsByDetailsAsync(CrCasBranchInformation entity);
        Task<bool> CheckIfCanDeleteIt(string lessorCode, string branchCode);
        Task<bool> ExistsByLongArabicNameAsync(string arabicName,   string lessorCode ,string branchCode);
        Task<bool> ExistsByTGACodeAsync(int tgaNo ,string branchCode);
        Task<bool> ExistsByLongEnglishNameAsync(string englishName, string lessorCode ,string branchCode);
        Task<bool> ExistsByShortArabicNameAsync(string arabicName,  string lessorCode ,string branchCode);
        Task<bool> ExistsByShortEnglishNameAsync(string englishName,string lessorCode ,string branchCode);
    }
}
