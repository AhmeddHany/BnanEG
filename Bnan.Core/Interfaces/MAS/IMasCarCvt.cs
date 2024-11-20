using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarCvt
    {
        Task<List<CrMasSupCarCvt>> GetAllAsync();
        Task AddAsync(CrMasSupCarCvt entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarCvt entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
