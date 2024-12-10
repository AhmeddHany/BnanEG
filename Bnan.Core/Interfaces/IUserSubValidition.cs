namespace Bnan.Core.Interfaces
{
    public interface IUserSubValidition
    {
        Task<bool> AddSubValiditionsForEachUser(string userCode, string systemCode);
        //Task<bool> AddSubValidaitionToUserWhenAddLessor(string userCode);
        Task<bool> AddSubValidaitionToUserCASFromMAS(string userCode);
    }
}
