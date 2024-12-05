using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasContractCarCheckupDetails
    {
        Task<List<CrMasSupContractCarCheckupDetail>> GetAllAsync();
        Task AddAsync(CrMasSupContractCarCheckupDetail entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupContractCarCheckupDetail entity);
        Task<bool> ExistsByDetails_Add_Async(CrMasSupContractCarCheckupDetail entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string no);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string no);
        Task<string> ExistsByCodeAsync(string Code_dataField, string No);

    }
}
