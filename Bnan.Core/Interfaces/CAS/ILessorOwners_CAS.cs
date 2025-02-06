using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.CAS
{
    public interface ILessorOwners_CAS
    {
        Task<List<CrCasOwner>> GetAllAsync();
        Task AddAsync(CrCasOwner entity);
        Task<bool> ExistsByDetailsAsync(CrCasOwner entity);
        Task<bool> ExistsByDetails_AddAsync(CrCasOwner entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> CheckIfCanEdit_It(string code);
        
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string company);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string company);
        //Task<bool> ExistsByEmailAsync(string email, string code);
        //Task<bool> ExistsByMobileAsync(string mobile, string code);
    }
}
