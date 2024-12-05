using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasContractCarCheckup
    {
        Task<List<CrMasSupContractCarCheckup>> GetAllAsync();
        Task AddAsync(CrMasSupContractCarCheckup entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupContractCarCheckup entity);
        Task<bool> ExistsByDetails_Add_Async(CrMasSupContractCarCheckup entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
       
        //Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<string> ExistsByCodeAsync(string Code_dataField);

    }
}
