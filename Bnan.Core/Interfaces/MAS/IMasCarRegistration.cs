using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarRegistration
    {
        Task<List<CrMasSupCarRegistration>> GetAllAsync();
        Task AddAsync(CrMasSupCarRegistration entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarRegistration entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<bool> ExistsByNaqlIdAsync(int naqlId, string code);
    }
}
