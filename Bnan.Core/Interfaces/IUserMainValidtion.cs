namespace Bnan.Core.Interfaces
{
    public interface IUserMainValidtion
    {
        Task<bool> AddMainValiditionsForEachUser(string userCode, string systemCode);

        //Task<bool> AddMainValidaitionToUserWhenAddLessor(string userCode);
        Task<bool> AddMainValidaitionToUserCASFromMAS(string userCode);
    }
}
