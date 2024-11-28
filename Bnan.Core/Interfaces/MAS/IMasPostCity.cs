using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasPostCity
    {
        Task<List<CrMasSupPostCity>> GetAllAsync();
        Task AddAsync(CrMasSupPostCity entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupPostCity entity);
        Task<bool> ExistsByDetailsEdit_onlyAsync(CrMasSupPostCity entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByLocationAsync(string location, string code);
        Task<bool> ExistsByLongitudeAsync(decimal longitude, string code);
        Task<bool> ExistsByLatitudeAsync(decimal latitude, string code);
    }
}
