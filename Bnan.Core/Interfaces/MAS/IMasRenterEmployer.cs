using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterEmployer
    {
        Task<List<CrMasSupRenterEmployer>> GetAllAsync();
        Task AddAsync(CrMasSupRenterEmployer entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterEmployer entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
