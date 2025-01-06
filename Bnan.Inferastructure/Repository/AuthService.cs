using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Bnan.Core.Repository
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<CrMasUserInformation> _userManager;
        private readonly SignInManager<CrMasUserInformation> _signInManager;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _access;
        private readonly IServiceProvider _ser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserMainValidtion _UserMainValidtion;
        private readonly IUserSubValidition _UserSubValidition;
        private readonly IUserProcedureValidition _UserProcedureValidition;

        public AuthService(UserManager<CrMasUserInformation> userManager,
            SignInManager<CrMasUserInformation> signInManager, IUserService userService, IHttpContextAccessor access, IServiceProvider ser, IUnitOfWork unitOfWork, IUserMainValidtion userMainValidtion, IUserSubValidition userSubValidition, IUserProcedureValidition userProcedureValidition)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _access = access;
            _ser = ser;
            _unitOfWork = unitOfWork;
            _UserMainValidtion = userMainValidtion;
            _UserSubValidition = userSubValidition;
            _UserProcedureValidition = userProcedureValidition;
        }

        public async Task<bool> AddRoleAsync(CrMasUserInformation user, string role)
        {
            var Role = await _userManager.AddToRoleAsync(user, role);
            return Role.Succeeded;
        }
        public async Task<bool> RemoveRoleAsync(CrMasUserInformation user, string role)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<string> AddUserCompanyForCas(CrMasUserInformation model)
        {
            var user = new CrMasUserInformation
            {
                CrMasUserInformationCode = model.CrMasUserInformationCode,
                CrMasUserInformationPassWord = model.CrMasUserInformationCode,
                CrMasUserInformationLessor = model.CrMasUserInformationLessor,
                CrMasUserInformationRemindMe = "رقم الموظف",
                CrMasUserInformationDefaultBranch = "100",
                CrMasUserInformationDefaultLanguage = "AR",
                CrMasUserInformationAuthorizationAdmin = true,
                CrMasUserInformationAuthorizationBnan = false,
                CrMasUserInformationAuthorizationBranch = false,
                CrMasUserInformationAuthorizationFoolwUp = false,
                CrMasUserInformationAuthorizationOwner = false,
                CrMasUserInformationArName = model.CrMasUserInformationArName,
                CrMasUserInformationEnName = model.CrMasUserInformationEnName,
                CrMasUserInformationTasksArName = "المدير الفني للنظام",
                CrMasUserInformationTasksEnName = "Technical Director of the System",
                CrMasUserInformationAvailableBalance = 0,
                CrMasUserInformationReservedBalance = 0,
                CrMasUserInformationTotalBalance = 0,
                CrMasUserInformationCreditLimit = 0,
                CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo,
                CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey,
                Email = model.CrMasUserInformationEmail,
                CrMasUserInformationExitTimer = 5,
                CrMasUserInformationChangePassWordLastDate = null,
                CrMasUserInformationEntryLastDate = null,
                CrMasUserInformationExitLastDate = null,
                CrMasUserInformationPicture = "~/images/common/user.jpg",
                CrMasUserInformationSignature = "~/images/common/DefualtUserSignature.png",
                CrMasUserInformationOperationStatus = false,
                CrMasUserInformationStatus = "A",
                UserName = model.CrMasUserInformationCode,
                Id = model.CrMasUserInformationCode,
                CrMasUserInformationEmail = model.CrMasUserInformationEmail,
                CrMasUserInformationReasons = model.CrMasUserInformationReasons,

            };
            var result = await _userManager.CreateAsync(user, user.CrMasUserInformationPassWord);
            if (result.Succeeded)
            {
                var addRole = await AddRoleAsync(user, "CAS");
                var AddMainValidaition = await _UserMainValidtion.AddMainValidaitionToUserCASFromMAS(user.CrMasUserInformationCode);
                var AddSubValidaition = await _UserSubValidition.AddSubValidaitionToUserCASFromMAS(user.CrMasUserInformationCode);
                var AddProceduresValiditions = await _UserProcedureValidition.AddProceduresValiditionsToUserCASFromMAS(user.CrMasUserInformationCode);
                if (addRole && AddMainValidaition && AddSubValidaition && AddProceduresValiditions)
                {
                    return user.CrMasUserInformationCode;
                }
            }
            return string.Empty;
        }

        public async Task<bool> CheckPassword(string username, string password)
        {
            var user = await _userService.GetUserByUserNameAsync(username);
            return (await _userManager.CheckPasswordAsync(user, password));
        }

        public async Task<SignInResult> LoginAsync(string username, string password)
        {
            var user = await _userService.GetUserByUserNameAsync(username);
            var result = await _signInManager.PasswordSignInAsync(user, password, false, true);
            return result;
        }

        public async Task<bool> RegisterAsync(CrMasUserInformation model)
        {
            var user = new CrMasUserInformation
            {
                UserName = model.CrMasUserInformationCode,
                CrMasUserInformationCode = model.CrMasUserInformationCode,
                Id = model.CrMasUserInformationCode,
                CrMasUserInformationEmail = model.CrMasUserInformationEmail,
                CrMasUserInformationPassWord = model.CrMasUserInformationCode,
                CrMasUserInformationArName = model.CrMasUserInformationArName,
                CrMasUserInformationEnName = model.CrMasUserInformationEnName,
                CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey,
                CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo,
                CrMasUserInformationDefaultLanguage = "AR",
                CrMasUserInformationLessor = "0000",
                CrMasUserInformationDefaultBranch = "000",
                CrMasUserInformationStatus = "A",
                CrMasUserInformationOperationStatus = false,
                CrMasUserInformationExitTimer = 5,
                CrMasUserInformationRemindMe = model.CrMasUserInformationCode,
                CrMasUserInformationAvailableBalance = 0,
                CrMasUserInformationReservedBalance = 0,
                CrMasUserInformationTotalBalance = 0,
                CrMasUserInformationCreditLimit = 0,
                CrMasUserInformationAuthorizationBnan = true,
                CrMasUserInformationAuthorizationAdmin = false,
                CrMasUserInformationAuthorizationBranch = false,
                CrMasUserInformationAuthorizationFoolwUp = false,
                CrMasUserInformationAuthorizationOwner = false,
                Email = model.CrMasUserInformationEmail,
                CrMasUserInformationTasksArName = model.CrMasUserInformationTasksArName,
                CrMasUserInformationTasksEnName = model.CrMasUserInformationTasksEnName,
                CrMasUserInformationSignature = model.CrMasUserInformationSignature,
                CrMasUserInformationPicture = model.CrMasUserInformationPicture

            };
            var result = await _userManager.CreateAsync(user, user.CrMasUserInformationPassWord);
            if (result.Succeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<CrMasUserInformation> RegisterForCasAsync(CrMasUserInformation model)
        {
            var user = new CrMasUserInformation
            {
                UserName = model.CrMasUserInformationCode,
                CrMasUserInformationCode = model.CrMasUserInformationCode,
                CrMasUserInformationId = model.CrMasUserInformationId,
                Id = model.CrMasUserInformationCode,
                CrMasUserInformationEmail = model.CrMasUserInformationEmail,
                CrMasUserInformationPassWord = model.CrMasUserInformationCode,
                CrMasUserInformationArName = model.CrMasUserInformationArName,
                CrMasUserInformationEnName = model.CrMasUserInformationEnName,
                CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey,
                CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo,
                CrMasUserInformationLessor = model.CrMasUserInformationLessor,
                CrMasUserInformationDefaultLanguage = "AR",
                CrMasUserInformationDefaultBranch = "000",
                CrMasUserInformationStatus = "A",
                CrMasUserInformationOperationStatus = false,
                CrMasUserInformationExitTimer = 5,
                CrMasUserInformationRemindMe = model.CrMasUserInformationCode,
                CrMasUserInformationAvailableBalance = 0,
                CrMasUserInformationReservedBalance = 0,
                CrMasUserInformationTotalBalance = 0,
                CrMasUserInformationCreditLimit = model.CrMasUserInformationCreditLimit,
                CrMasUserInformationCreditDaysLimit = model.CrMasUserInformationCreditDaysLimit,
                CrMasUserInformationAuthorizationBnan = false,
                CrMasUserInformationAuthorizationAdmin = model.CrMasUserInformationAuthorizationAdmin,
                CrMasUserInformationAuthorizationBranch = model.CrMasUserInformationAuthorizationBranch,
                CrMasUserInformationAuthorizationFoolwUp = false,
                CrMasUserInformationAuthorizationOwner = model.CrMasUserInformationAuthorizationOwner,
                Email = model.CrMasUserInformationEmail,
                CrMasUserInformationTasksArName = model.CrMasUserInformationTasksArName,
                CrMasUserInformationTasksEnName = model.CrMasUserInformationTasksEnName,
                CrMasUserInformationReasons = model.CrMasUserInformationReasons

            };
            var result = await _userManager.CreateAsync(user, user.CrMasUserInformationPassWord);
            if (result.Succeeded) return user;
            else return null;
        }

        public async Task SignOut()
        {
            await _signInManager.SignOutAsync();
        }

        public Task UserLogins(string username)
        {
            throw new NotImplementedException();
        }

        public async Task<CrMasUserInformation> UpdateForCasAsync(CrMasUserInformation model)
        {
            var user = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == model.CrMasUserInformationCode);
            if (user != null)
            {

                user.CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo;
                user.CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey;
                user.CrMasUserInformationTasksArName = model.CrMasUserInformationTasksArName;
                user.CrMasUserInformationTasksEnName = model.CrMasUserInformationTasksEnName;
                user.CrMasUserInformationCreditDaysLimit = model.CrMasUserInformationCreditDaysLimit;
                user.CrMasUserInformationCreditLimit = model.CrMasUserInformationCreditLimit;
                user.CrMasUserInformationEmail = model.CrMasUserInformationEmail;
                user.CrMasUserInformationAuthorizationAdmin = model.CrMasUserInformationAuthorizationAdmin;
                user.CrMasUserInformationAuthorizationBranch = model.CrMasUserInformationAuthorizationBranch;
                user.CrMasUserInformationAuthorizationOwner = model.CrMasUserInformationAuthorizationOwner;
                user.CrMasUserInformationReasons = model.CrMasUserInformationReasons;
                if (_unitOfWork.CrMasUserInformation.Update(user) != null) return user;
            }
            return new CrMasUserInformation();
        }
    }
}