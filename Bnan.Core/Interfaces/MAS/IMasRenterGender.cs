using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterGender
    {
        Task<List<CrMasSupRenterGender>> GetAllAsync();
        Task AddAsync(CrMasSupRenterGender entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterGender entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
