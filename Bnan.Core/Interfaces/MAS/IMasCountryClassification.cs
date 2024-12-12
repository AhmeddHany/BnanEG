using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCountryClassification
    {
        Task<List<CrMasSupCountryClassification>> GetAllAsync();
        Task AddAsync(CrMasSupCountryClassification entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCountryClassification entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
