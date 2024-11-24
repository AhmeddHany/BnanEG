using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.Base
{
    public class BaseRepo : IBaseRepo
    {
        public IUnitOfWork _unitOfWork;

        public BaseRepo(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }


        public async Task<bool> CheckValidation(string userCode, string subTask, string status)
        {
            if (status == Status.ViewInformation)
            {
                var validation = _unitOfWork.CrMasUserSubValidations
                    .Find(x => x.CrMasUserSubValidationUser == userCode && x.CrMasUserSubValidationSubTasks == subTask)
                    ?.CrMasUserSubValidationAuthorization;
                return validation ?? false;
            }
            else if (status == Status.Insert)
            {
                var validation = _unitOfWork.CrMasUserProceduresValidations
                    .Find(x => x.CrMasUserProceduresValidationCode == userCode && x.CrMasUserProceduresValidationSubTasks == subTask)
                    ?.CrMasUserProceduresValidationInsertAuthorization;
                return validation ?? false;
            }
            else if (status == Status.Update)
            {
                var validation = _unitOfWork.CrMasUserProceduresValidations
                    .Find(x => x.CrMasUserProceduresValidationCode == userCode && x.CrMasUserProceduresValidationSubTasks == subTask)
                    ?.CrMasUserProceduresValidationUpDateAuthorization;
                return validation ?? false;
            }
            else if (status == Status.Deleted)
            {
                var validation = _unitOfWork.CrMasUserProceduresValidations
                    .Find(x => x.CrMasUserProceduresValidationCode == userCode && x.CrMasUserProceduresValidationSubTasks == subTask)
                    ?.CrMasUserProceduresValidationDeleteAuthorization;
                return validation ?? false;
            }
            else if (status == Status.UnDeleted)
            {
                var validation = _unitOfWork.CrMasUserProceduresValidations
                    .Find(x => x.CrMasUserProceduresValidationCode == userCode && x.CrMasUserProceduresValidationSubTasks == subTask)
                    ?.CrMasUserProceduresValidationUnDeleteAuthorization;
                return validation ?? false;
            }
            else if (status == Status.Hold)
            {
                var validation = _unitOfWork.CrMasUserProceduresValidations
                    .Find(x => x.CrMasUserProceduresValidationCode == userCode && x.CrMasUserProceduresValidationSubTasks == subTask)
                    ?.CrMasUserProceduresValidationHoldAuthorization;
                return validation ?? false;
            }
            else if (status == Status.UnHold)
            {
                var validation = _unitOfWork.CrMasUserProceduresValidations
                    .Find(x => x.CrMasUserProceduresValidationCode == userCode && x.CrMasUserProceduresValidationSubTasks == subTask)
                    ?.CrMasUserProceduresValidationUnHoldAuthorization;
                return validation ?? false;
            }

            return false;
        }

        public async Task<CrMasUserProceduresValidation?> GetAll_Mas_Validation_For_All(string userCode, string subTask)
        {
            var validation = await _unitOfWork.CrMasUserProceduresValidations.FindAllAsync(x => x.CrMasUserProceduresValidationCode == userCode && x.CrMasUserProceduresValidationSubTasks == subTask);

            return validation.FirstOrDefault();
        }

    }
}
