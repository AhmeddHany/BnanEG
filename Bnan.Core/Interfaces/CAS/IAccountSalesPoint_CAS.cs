using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.CAS
{
    public interface IAccountSalesPoint_CAS
    {
        Task<List<CrCasAccountSalesPoint>> GetAllAsync();
        Task AddAsync(CrCasAccountSalesPoint entity);
        Task<bool> ExistsByDetailsAsync(CrCasAccountSalesPoint entity);
        Task<bool> ExistsByDetails_AddAsync(CrCasAccountSalesPoint entity);
        Task<bool> CheckIfCanDeleteIt(string code, string lessor);
        Task<bool> CheckIfCanEdit_It(string code, string lessor);
        
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string company);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string company);
        Task<bool> ExistsBySalesPointIbanAsync(string Iban, string code);
        //Task<bool> ExistsByEmailAsync(string email, string code);
        //Task<bool> ExistsByMobileAsync(string mobile, string code);
    }
}
