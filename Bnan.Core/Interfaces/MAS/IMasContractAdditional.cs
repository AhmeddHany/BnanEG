using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasContractAdditional
    {
        Task<List<CrMasSupContractAdditional>> GetAllAsync();
        Task AddAsync(CrMasSupContractAdditional entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupContractAdditional entity);
        Task<bool> ExistsByDetails_Add_Async(CrMasSupContractAdditional entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code);
        Task<string> ExistsByCodeAsync(string Code_dataField);

    }
}
