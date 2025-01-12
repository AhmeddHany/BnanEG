using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasUser
    {
        Task<List<CrMasUserInformation>> GetAllAsync();
        Task<bool> UpdateUser(CrMasUserInformation model);
        Task<bool> ExistsByDetailsAsync(CrMasUserInformation entity);
        //Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string lessorCode);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string lessorCode);
        Task<bool> ExistsByUserCodeAsync(string userCode);
        Task<bool> ExistsByUserIdAsync(string userId, string userCode);
    }
}
