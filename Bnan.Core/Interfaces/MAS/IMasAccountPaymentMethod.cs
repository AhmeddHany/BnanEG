using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasAccountPaymentMethod
    {
        Task<List<CrMasSupAccountPaymentMethod>> GetAllAsync();
        Task AddAsync(CrMasSupAccountPaymentMethod entity);
        Task<bool> ExistsByDetailsAsync(CrMasSupAccountPaymentMethod entity,bool isEdit);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByPKCodeAsync(string PKCode);
        Task<bool> CheckClassificationAsync(string classfication);
        Task<bool> ExistsByArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByEnglishNameAsync(string englishName, string code);
    }
}
