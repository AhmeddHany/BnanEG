using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasContractOptions
    {
        Task<List<CrMasSupContractOption>> GetAllAsync();
        Task AddAsync(CrMasSupContractOption entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupContractOption entity);
        Task<bool> ExistsByDetails_Add_Async(CrMasSupContractOption entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<string> ExistsByCodeAsync(string Code_dataField);

    }
}
