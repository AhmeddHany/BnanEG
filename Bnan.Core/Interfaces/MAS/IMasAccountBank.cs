using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasAccountBank
    {
        Task<List<CrMasSupAccountBank>> GetAllAsync();
        Task AddAsync(CrMasSupAccountBank entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupAccountBank entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
