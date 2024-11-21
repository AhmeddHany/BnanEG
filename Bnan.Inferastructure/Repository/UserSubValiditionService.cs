using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class UserSubValiditionService : IUserSubValidition
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _UserService;

        public UserSubValiditionService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<bool> AddSubValidaitionToUserWhenAddLessor(string userCode)
        {
            var SubTasks = _unitOfWork.CrMasSysSubTask.FindAll(l => l.CrMasSysSubTasksStatus == "A" && l.CrMasSysSubTasksSystemCode == "2", new[] { "CrMasSysSubTasksMainCodeNavigation" });
            var user = await _UserService.GetUserByUserNameAsync(userCode);


            foreach (var item in SubTasks)
            {
                CrMasUserSubValidation CrMasUserSubValidation = new CrMasUserSubValidation();

                CrMasUserSubValidation = new CrMasUserSubValidation
                {
                    CrMasUserSubValidationUser = user.CrMasUserInformationCode,
                    CrMasUserSubValidationSubTasks = item.CrMasSysSubTasksCode,
                    CrMasUserSubValidationSystem = item.CrMasSysSubTasksSystemCode,
                    CrMasUserSubValidationMain = item.CrMasSysSubTasksMainCodeNavigation.CrMasSysMainTasksCode,
                    CrMasUserSubValidationAuthorization = true
                };
                await _unitOfWork.CrMasUserSubValidations.AddAsync(CrMasUserSubValidation);
            }
            return true;
        }

        public async Task<bool> AddSubValiditionsForEachUser(string userCode, string systemCode)
        {
            var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksSystemCode == systemCode && x.CrMasSysSubTasksStatus == Status.Active);
            foreach (var item in subTasks)
            {
                if (item.CrMasSysSubTasksCode != "2207001" || item.CrMasSysSubTasksCode != "2207001" || item.CrMasSysSubTasksCode != "2207001")
                {
                    CrMasUserSubValidation crMasUserSubValidation = new CrMasUserSubValidation();
                    crMasUserSubValidation.CrMasUserSubValidationUser = userCode;
                    crMasUserSubValidation.CrMasUserSubValidationSystem = systemCode;
                    crMasUserSubValidation.CrMasUserSubValidationMain = item.CrMasSysSubTasksMainCode;
                    crMasUserSubValidation.CrMasUserSubValidationSubTasks = item.CrMasSysSubTasksCode;
                    crMasUserSubValidation.CrMasUserSubValidationAuthorization = false;
                    if (await _unitOfWork.CrMasUserSubValidations.AddAsync(crMasUserSubValidation) == null) return false;
                }

            }
            return true;
        }
    }
}
