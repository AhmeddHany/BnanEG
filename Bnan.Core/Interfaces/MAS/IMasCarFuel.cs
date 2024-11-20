using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCarFuel
    {
        Task<List<CrMasSupCarFuel>> GetAllAsync();
        Task AddAsync(CrMasSupCarFuel entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupCarFuel entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<bool> ExistsByNaqlIdAsync(int naqlId, string code);
    }
}
