namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasBase
    {
        Task<bool> CheckValidation(string userCode, string subTask, string operation);
    }
}
