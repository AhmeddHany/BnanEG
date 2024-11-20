using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarCategory
    {
        Task<List<CrMasSupCarCategory>> GetAllAsync();
        Task AddAsync(CrMasSupCarCategory entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarCategory entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
