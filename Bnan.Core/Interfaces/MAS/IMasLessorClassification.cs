using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasLessorClassification
    {
        Task<List<CrCasLessorClassification>> GetAllAsync();
        Task AddAsync(CrCasLessorClassification entity);
        Task<bool> ExistsByDetailsAsync(CrCasLessorClassification entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
