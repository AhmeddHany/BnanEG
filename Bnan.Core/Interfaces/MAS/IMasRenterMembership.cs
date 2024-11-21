using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasRenterMembership
    {
        Task<List<CrMasSupRenterMembership>> GetAllAsync();
        Task AddAsync(CrMasSupRenterMembership entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupRenterMembership entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);

    }
}
