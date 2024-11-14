using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterSector
    {
        Task<List<CrMasSupRenterSector>> GetAllAsync();
        Task AddAsync(CrMasSupRenterSector entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterSector entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
