using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class UserProcedureValiditionService : IUserProcedureValidition
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _UserService;

        public UserProcedureValiditionService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<bool> AddProceduresValiditionsForEachUser(string userCode, string systemCode)
        {
            var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksSystemCode == systemCode);
            foreach (var item in subTasks)
            {
                if (item.CrMasSysSubTasksCode != "2207001" || item.CrMasSysSubTasksCode != "2207002" || item.CrMasSysSubTasksCode != "2207003" || item.CrMasSysSubTasksCode != "2208004")
                {
                    CrMasUserProceduresValidation crMasUserProceduresValidation = new CrMasUserProceduresValidation();
                    crMasUserProceduresValidation.CrMasUserProceduresValidationCode = userCode;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationSystem = systemCode;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationMainTask = item.CrMasSysSubTasksMainCode;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationSubTasks = item.CrMasSysSubTasksCode;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationDeleteAuthorization = false;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationUnDeleteAuthorization = false;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationUpDateAuthorization = false;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationInsertAuthorization = false;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationHoldAuthorization = false;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationUnHoldAuthorization = false;
                    if (await _unitOfWork.CrMasUserProceduresValidations.AddAsync(crMasUserProceduresValidation) == null) return false;
                }
            }
            return true;
        }

        public async Task<bool> AddProceduresValiditionsToUserCASFromMAS(string userCode)
        {
            var subTasks = _unitOfWork.CrMasSysSubTasks.FindAll(l => l.CrMasSysSubTasksStatus == Status.Active && l.CrMasSysSubTasksSystemCode == "2" &&
                                                                                       (l.CrMasSysSubTasksMainCode == "207" || l.CrMasSysSubTasksMainCode == "208"));
            var user = await _UserService.GetUserByUserNameAsync(userCode);

            foreach (var item in subTasks)
            {
                if (item.CrMasSysSubTasksCode == "2207001" || item.CrMasSysSubTasksCode == "2207002" || item.CrMasSysSubTasksCode == "2207003" || item.CrMasSysSubTasksCode == "2208004")
                {
                    CrMasUserProceduresValidation crMasUserProceduresValidation = new CrMasUserProceduresValidation();
                    crMasUserProceduresValidation.CrMasUserProceduresValidationCode = userCode;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationSystem = "2";
                    crMasUserProceduresValidation.CrMasUserProceduresValidationMainTask = item.CrMasSysSubTasksMainCode;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationSubTasks = item.CrMasSysSubTasksCode;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationDeleteAuthorization = true;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationUnDeleteAuthorization = true;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationUpDateAuthorization = true;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationInsertAuthorization = true;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationHoldAuthorization = true;
                    crMasUserProceduresValidation.CrMasUserProceduresValidationUnHoldAuthorization = true;
                    if (await _unitOfWork.CrMasUserProceduresValidations.AddAsync(crMasUserProceduresValidation) == null) return false;
                }
            }
            return true;
        }
    }
}
