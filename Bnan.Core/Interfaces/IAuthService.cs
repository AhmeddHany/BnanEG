using Bnan.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Bnan.Core.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(CrMasUserInformation model);
        Task<CrMasUserInformation> RegisterForCasAsync(CrMasUserInformation model);
        Task<CrMasUserInformation> UpdateForCasAsync(CrMasUserInformation model);
        //Task<bool> AddUserDefault(string LessorCode);
        Task<string> AddUserCompanyForCas(CrMasUserInformation model);
        Task<bool> AddRoleAsync(CrMasUserInformation user, string Role);
        Task<bool> RemoveRoleAsync(CrMasUserInformation user, string role);
        Task<SignInResult> LoginAsync(string username, string password);
        Task<bool> CheckPassword(string username, string password);
        Task UserLogins(string username);
        Task SignOut();

    }
}