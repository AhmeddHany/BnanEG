using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarBrand
    {
        Task<List<CrMasSupCarBrand>> GetAllAsync();
        Task AddAsync(CrMasSupCarBrand entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarBrand entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
