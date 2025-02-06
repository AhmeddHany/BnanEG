using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.CAS
{
    public interface IRenterDriver_CAS
    {
        Task<List<CrCasRenterPrivateDriverInformation>> GetAllAsync();
        Task AddAsync(CrCasRenterPrivateDriverInformation entity);
        Task<bool> ExistsByDetailsAsync(CrCasRenterPrivateDriverInformation entity);
        Task<bool> ExistsByDetails_AddAsync(CrCasRenterPrivateDriverInformation entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> CheckIfCanEditStatus_It(string code, string lessor);

        Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string company);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string company);
        Task<bool> ExistsByIDAsync(string code, string company);
        //Task<bool> ExistsByEmailAsync(string email, string code);
        //Task<bool> ExistsByMobileAsync(string mobile, string code);
    }
}
