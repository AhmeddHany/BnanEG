using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRate
    {
        Task<List<CrMasSysEvaluation>> GetAllAsync();
        Task AddAsync(CrMasSysEvaluation entity);
        Task<bool> ExistsByDetailsAsync(CrMasSysEvaluation entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
