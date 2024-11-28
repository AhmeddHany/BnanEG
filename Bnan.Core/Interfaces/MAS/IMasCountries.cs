using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasCountries
    {
        Task<List<CrMasSysCallingKey>> GetAllAsync();
        Task AddAsync(CrMasSysCallingKey entity);
        Task<bool> ExistsByDetailsAsync(CrMasSysCallingKey entity);
        Task<bool> ExistsByDetailsEdit_onlyAsync(CrMasSysCallingKey entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<bool> ExistsByNaqlIdAsync(int naqlId, string code);
    }
}
