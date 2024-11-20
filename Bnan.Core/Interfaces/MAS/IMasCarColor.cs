using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarColor
    {
        Task<List<CrMasSupCarColor>> GetAllAsync();
        Task AddAsync(CrMasSupCarColor entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarColor entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
