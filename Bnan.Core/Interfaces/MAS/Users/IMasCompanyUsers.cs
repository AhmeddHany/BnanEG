using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS.Users
{
    public interface IMasCompanyUsers
    {
        Task<List<CrMasUserInformation>> GetAllAsync();
        Task<bool> UpdateUser(CrMasUserInformation model);
        Task<bool> ExistsByDetailsAsync(CrMasUserInformation entity);
        //Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByUserCodeAsync(string userCode);
        Task<bool> ExistsByUserIdAsync(string userId, string userCode);
    }
}
