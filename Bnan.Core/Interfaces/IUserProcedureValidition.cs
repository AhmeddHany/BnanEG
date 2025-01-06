namespace Bnan.Core.Interfaces
{
    public interface IUserProcedureValidition
    {
        Task<bool> AddProceduresValiditionsForEachUser(string userCode, string systemCode);
        Task<bool> AddProceduresValiditionsToUserCASFromMAS(string userCode);

    }
}
