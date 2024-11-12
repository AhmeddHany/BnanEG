namespace Bnan.Core.Interfaces.Base
{
    public interface IBaseRepo
    {
        Task<bool> CheckValidation(string userCode, string subTask, string operation);
    }
}
