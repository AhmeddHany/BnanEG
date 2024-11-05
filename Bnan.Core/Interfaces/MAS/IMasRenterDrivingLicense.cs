using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterDrivingLicense
    {
        Task<List<CrMasSupRenterDrivingLicense>> GetAllAsync();
        Task AddAsync(CrMasSupRenterDrivingLicense entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterDrivingLicense entity);
        Task<bool> ExistsByArabicNameAsync(string arabicName);
        Task<bool> ExistsByEnglishNameAsync(string englishName);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode);
        Task<bool> ExistsByNaqlIdAsync(int naqlId);
    }
}
