using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;


namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasBase : IMasBase
    {
        public IUnitOfWork _unitOfWork;

        public MasBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        public async Task<bool> CheckValidation(string userCode, string subTask, string operation)
        {
            var validation = await _unitOfWork.CrMasUserProceduresValidations
                                              .FindAllAsync(x => x.CrMasUserProceduresValidationCode == userCode &&
                                                                 x.CrMasUserProceduresValidationSubTasks == subTask);
            var validationEntry = validation?.FirstOrDefault();
            if (validationEntry == null) return false;

            var operationMap = new Dictionary<string, bool?>
            {
                { Status.Hold, validationEntry.CrMasUserProceduresValidationHoldAuthorization },
                { Status.UnHold, validationEntry.CrMasUserProceduresValidationUnHoldAuthorization },
                { Status.Deleted, validationEntry.CrMasUserProceduresValidationDeleteAuthorization },
                { Status.UnDeleted, validationEntry.CrMasUserProceduresValidationUnDeleteAuthorization },
                { Status.UpdateStatus, validationEntry.CrMasUserProceduresValidationUpDateAuthorization },
                { Status.Insert, validationEntry.CrMasUserProceduresValidationInsertAuthorization }
            };

            return operationMap.TryGetValue(operation, out var isAuthorized) && isAuthorized == true;
        }

    }
}
