using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterNationality
    {
        Task<List<CrMasSupRenterNationality>> GetAllAsync();
        Task AddAsync(CrMasSupRenterNationality entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterNationality entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<bool> ExistsByNaqlIdAsync(int naqlId, string code);
    }
}
