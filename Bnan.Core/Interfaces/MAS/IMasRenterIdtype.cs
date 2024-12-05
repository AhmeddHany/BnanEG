using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterIdtype
    {
        Task<List<CrMasSupRenterIdtype>> GetAllAsync();
        Task AddAsync(CrMasSupRenterIdtype entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterIdtype entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<bool> ExistsByNaqlIdAsync(int naqlId, string code);
    }
}
