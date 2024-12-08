using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasLessor
    {
        Task<bool> ExistsByDetailsAsync(CrMasLessorInformation entity);
        Task<bool> CheckIfCanDeleteIt(string code);
        Task<bool> ExistsByLongArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByLongEnglishNameAsync(string englishName, string code);
        Task<bool> ExistsByShortArabicNameAsync(string arabicName, string code);
        Task<bool> ExistsByShortEnglishNameAsync(string englishName, string code);
    }
}
