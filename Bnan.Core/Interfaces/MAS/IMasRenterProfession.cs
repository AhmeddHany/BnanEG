using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterProfession
    {
        Task<List<CrMasSupRenterProfession>> GetAllAsync();
        Task AddAsync(CrMasSupRenterProfession entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterProfession entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
