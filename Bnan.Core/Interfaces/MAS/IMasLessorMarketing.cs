using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasLessorMarketing
    {
        Task<List<CrMasSupContractSource>> GetAllAsync();
        Task AddAsync(CrMasSupContractSource entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupContractSource entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByEmailAsync(string email, string code);
        Task<bool> ExistsByMobileAsync(string mobile, string code);
    }
}
