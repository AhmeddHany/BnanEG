using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.CAS
{
    public interface IAccountBank_CAS
    {
        Task<List<CrCasAccountBank>> GetAllAsync();
        Task AddAsync(CrCasAccountBank entity);
        Task<bool> ExistsByDetailsAsync(CrCasAccountBank entity);
        Task<bool> ExistsByDetails_AddAsync(CrCasAccountBank entity);
        Task<bool> CheckIfCanDeleteIt(string code, string lessor);
        Task<bool> CheckIfCanEdit_It(string code, string lessor);
        
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string company);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string company);
        Task<bool> ExistsByBankIbanAsync(string Iban, string code);
        //Task<bool> ExistsByEmailAsync(string email, string code);
        //Task<bool> ExistsByMobileAsync(string mobile, string code);
    }
}
