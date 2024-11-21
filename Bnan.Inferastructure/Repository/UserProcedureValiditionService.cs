using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class UserProcedureValiditionService : IUserProcedureValidition
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserProcedureValiditionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddProceduresValiditionsForEachUser(string userCode, string systemCode)
        {
            var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksSystemCode == systemCode);
            foreach (var item in subTasks)
            {
                if (item.CrMasSysSubTasksCode != "2207001" || item.CrMasSysSubTasksCode != "2207001" || item.CrMasSysSubTasksCode != "2207001")
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
    }
}
