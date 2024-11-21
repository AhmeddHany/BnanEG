using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasAccountReference
    {
        Task<List<CrMasSupAccountReference>> GetAllAsync();
        Task AddAsync(CrMasSupAccountReference entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupAccountReference entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
