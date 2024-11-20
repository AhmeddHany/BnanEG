using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarModel
    {
        Task<List<CrMasSupCarModel>> GetAllAsync();
        Task AddAsync(CrMasSupCarModel entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarModel entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code,string brand);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code,string brand);

    }
}
