using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasQuestionsAnswer
    {
        Task<List<CrMasSysQuestionsAnswer>> GetAllAsync();
        Task AddAsync(CrMasSysQuestionsAnswer entity);
        //Task<bool> ExistsByDetailsAsync(CrMasSysQuestionsAnswer entity);
        //Task<bool> ExistsByDetails_Add_Async(CrMasSysQuestionsAnswer entity);
        //Task<string> ExistsByCodeAsync(string Code_dataField, string No);
    }
}
