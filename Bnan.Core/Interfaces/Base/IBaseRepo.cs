using Bnan.Core.Models;

namespace Bnan.Core.Interfaces.Base
{
    public interface IBaseRepo
    {
        Task<bool> CheckValidation(string userCode, string subTask, string status);
        Task<CrMasUserProceduresValidation?> GetAll_Mas_Validation_For_All(string userCode, string subTask);
    }
}
