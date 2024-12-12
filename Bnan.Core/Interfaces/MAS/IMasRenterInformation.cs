using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterInformation
    {
        Task<List<CrMasRenterInformation>> GetAllAsync();
        Task AddAsync(CrMasRenterInformation entity);
        Task<bool> ExistsByDetailsAsync(CrMasRenterInformation entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
