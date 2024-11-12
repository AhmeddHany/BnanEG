using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterDrivingLicense
    {
        Task<List<CrMasSupRenterDrivingLicense>> GetAllAsync();
        Task AddAsync(CrMasSupRenterDrivingLicense entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterDrivingLicense entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<bool> ExistsByNaqlIdAsync(int naqlId, string code);
    }
}
