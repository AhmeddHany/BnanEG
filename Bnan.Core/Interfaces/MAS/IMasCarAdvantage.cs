using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarAdvantage
    {
        Task<List<CrMasSupCarAdvantage>> GetAllAsync();
        Task AddAsync(CrMasSupCarAdvantage entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarAdvantage entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
