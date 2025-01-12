using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class UserBranchValidity : IUserBranchValidity
    {
        private IUnitOfWork _unitOfWork;

        public UserBranchValidity(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddUserBranchValidity(string userCode, string LessorCode, string branchCode, string status)
        {
            var user = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationLessor == LessorCode && x.CrMasUserInformationCode == userCode);
            if (user != null)
            {
                CrMasUserBranchValidity crMasUserBranchValidity = new CrMasUserBranchValidity()
                {
                    CrMasUserBranchValidityId = userCode,
                    CrMasUserBranchValidityLessor = LessorCode,
                    CrMasUserBranchValidityBranch = branchCode,
                    CrMasUserBranchValidityBranchCashAvailable = 0,
                    CrMasUserBranchValidityBranchCashBalance = 0,
                    CrMasUserBranchValidityBranchCashReserved = 0,
                    CrMasUserBranchValidityBranchTransferAvailable = 0,
                    CrMasUserBranchValidityBranchTransferBalance = 0,
                    CrMasUserBranchValidityBranchTransferReserved = 0,
                    CrMasUserBranchValidityBranchSalesPointBalance = 0,
                    CrMasUserBranchValidityBranchSalesPointReserved = 0,
                    CrMasUserBranchValidityBranchSalesPointAvailable = 0,
                    CrMasUserBranchValidityBranchStatus = status
                };
                if (status == Status.Active) user.CrMasUserInformationDefaultBranch = branchCode;
                if (await _unitOfWork.CrMasUserBranchValidity.AddAsync(crMasUserBranchValidity) != null && _unitOfWork.CrMasUserInformation.Update(user) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserBranchValidity(string userCode, string LessorCode, string branchCode, string status)
        {
            var branchValidate = _unitOfWork.CrMasUserBranchValidity.Find(x => x.CrMasUserBranchValidityId == userCode && x.CrMasUserBranchValidityLessor == LessorCode && x.CrMasUserBranchValidityBranch == branchCode);
            var user = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationLessor == LessorCode && x.CrMasUserInformationCode == userCode);

            if (branchValidate == null)
            {
                await AddUserBranchValidity(userCode, LessorCode, branchCode, status);
                var NewbranchValidate = _unitOfWork.CrMasUserBranchValidity.Find(x => x.CrMasUserBranchValidityId == userCode && x.CrMasUserBranchValidityLessor == LessorCode && x.CrMasUserBranchValidityBranch == branchCode);
                NewbranchValidate.CrMasUserBranchValidityBranchStatus = status;
                if (status == Status.Active) user.CrMasUserInformationDefaultBranch = branchCode;
                if (_unitOfWork.CrMasUserInformation.Update(user) != null && _unitOfWork.CrMasUserBranchValidity.Update(NewbranchValidate) != null) return true;
            }
            else
            {
                branchValidate.CrMasUserBranchValidityBranchStatus = status;
                if (status == Status.Active) user.CrMasUserInformationDefaultBranch = branchCode;
                if (_unitOfWork.CrMasUserInformation.Update(user) != null && _unitOfWork.CrMasUserBranchValidity.Update(branchValidate) != null) return true;
            }
            return false;
        }
    }
}
