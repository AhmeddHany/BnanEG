using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Repository
{
    public class BranchInformation : IBranchInformation
    {
        private IUnitOfWork _unitOfWork;

        public BranchInformation(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddBranchInformation(CrCasBranchInformation CrCasBranchInformation)
        {
            var BranchInformation = new CrCasBranchInformation
            {
                CrCasBranchInformationLessor = CrCasBranchInformation.CrCasBranchInformationLessor,
                CrCasBranchInformationCode = CrCasBranchInformation.CrCasBranchInformationCode,
                CrCasBranchInformationTgaCode = CrCasBranchInformation.CrCasBranchInformationTgaCode,
                CrCasBranchInformationGovernmentNo = CrCasBranchInformation.CrCasBranchInformationGovernmentNo,
                CrCasBranchInformationTaxNo = CrCasBranchInformation.CrCasBranchInformationTaxNo,
                CrCasBranchInformationArName = CrCasBranchInformation.CrCasBranchInformationArName,
                CrCasBranchInformationArShortName = CrCasBranchInformation.CrCasBranchInformationArShortName,
                CrCasBranchInformationEnName = CrCasBranchInformation.CrCasBranchInformationEnName,
                CrCasBranchInformationEnShortName = CrCasBranchInformation.CrCasBranchInformationEnShortName,
                CrCasBranchInformationDirectorArName = CrCasBranchInformation.CrCasBranchInformationDirectorArName,
                CrCasBranchInformationDirectorEnName = CrCasBranchInformation.CrCasBranchInformationDirectorEnName,
                CrCasBranchInformationMobile = CrCasBranchInformation.CrCasBranchInformationMobile,
                CrMasBranchInformationMobileKey = CrCasBranchInformation.CrMasBranchInformationMobileKey,
                CrCasBranchInformationTelephone = CrCasBranchInformation.CrCasBranchInformationTelephone,
                CrMasBranchInformationTeleKey = CrCasBranchInformation.CrMasBranchInformationMobileKey,
                CrCasBranchInformationDirectorSignature = CrCasBranchInformation.CrCasBranchInformationDirectorSignature,
                CrCasBranchInformationReasons = CrCasBranchInformation.CrCasBranchInformationReasons,
                CrCasBranchInformationAvailableBalance = 0,
                CrCasBranchInformationReservedBalance = 0,
                CrCasBranchInformationTotalBalance = 0,
                CrCasBranchInformationStatus = "A"

            };
            await _unitOfWork.CrCasBranchInformation.AddAsync(BranchInformation);
            return true;
        }

        public async Task<bool> AddBranchInformationDefault(string LesssorCode)
        {
            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(LesssorCode);
            var BranchInformation = new CrCasBranchInformation
            {
                CrCasBranchInformationLessor = lessor.CrMasLessorInformationCode,
                CrCasBranchInformationCode = "100",
                CrCasBranchInformationGovernmentNo = lessor.CrMasLessorInformationGovernmentNo,
                CrCasBranchInformationTaxNo = lessor.CrMasLessorInformationTaxNo,
                CrCasBranchInformationArName = "الإدارة العامة - الفرع الرئيسي",
                CrCasBranchInformationArShortName = "الرئيسي",
                CrCasBranchInformationEnName = "General Administration - Head office",
                CrCasBranchInformationEnShortName = "Head office",
                CrCasBranchInformationDirectorArName = lessor.CrMasLessorInformationDirectorArName,
                CrCasBranchInformationDirectorEnName = lessor.CrMasLessorInformationDirectorEnName,
                CrMasBranchInformationMobileKey = lessor.CrMasLessorInformationCommunicationMobileKey,
                CrCasBranchInformationMobile = lessor.CrMasLessorInformationCommunicationMobile,
                CrMasBranchInformationTeleKey = lessor.CrMasLessorInformationCallFreeKey,
                CrCasBranchInformationTelephone = lessor.CrMasLessorInformationCallFree,
                CrCasBranchInformationDirectorSignature = "~/images/common/DefualtUserSignature",
                CrCasBranchInformationAvailableBalance = 0,
                CrCasBranchInformationReservedBalance = 0,
                CrCasBranchInformationTotalBalance = 0,
                CrCasBranchInformationStatus = "A"

            };
            await _unitOfWork.CrCasBranchInformation.AddAsync(BranchInformation);
            return true;
        }
        public async Task<List<CrCasBranchInformation>> GetAllAsyncByLessor(string lessorCode)
        {
            var result = await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(x => x.CrCasBranchInformationLessor == lessorCode);
            return result;
        }
        public async Task<bool> ExistsByDetailsAsync(CrCasBranchInformation entity)
        {
            //var allBranches = await GetAllAsyncByLessor(entity.CrCasBranchInformationLessor);
            var allBranches = await _unitOfWork.CrCasBranchInformation.GetAllAsyncAsNoTrackingAsync();
            bool isTgaCodeExists = allBranches.Any(x => x.CrCasBranchInformationTgaCode == entity.CrCasBranchInformationTgaCode &&
                                                        x.CrCasBranchInformationCode != entity.CrCasBranchInformationCode);
            var lessorBranches = allBranches.Where(x => x.CrCasBranchInformationLessor == entity.CrCasBranchInformationLessor).ToList();
            bool isNameExists = lessorBranches.Any(x => x.CrCasBranchInformationCode != entity.CrCasBranchInformationCode &&
                                                                 (
                                                                     x.CrCasBranchInformationArName == entity.CrCasBranchInformationArName ||
                                                                     x.CrCasBranchInformationEnName.ToLower() == entity.CrCasBranchInformationEnName.ToLower() ||
                                                                     x.CrCasBranchInformationArShortName == entity.CrCasBranchInformationArShortName ||
                                                                     x.CrCasBranchInformationEnShortName.ToLower() == entity.CrCasBranchInformationEnShortName.ToLower()
                                                                 ));
            return isTgaCodeExists || isNameExists;
        }


        public async Task<bool> ExistsByLongArabicNameAsync(string arabicName, string lessorCode, string branchCode)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            var allBranches = await GetAllAsyncByLessor(lessorCode);

            return allBranches.Any(x => x.CrCasBranchInformationArName == arabicName && x.CrCasBranchInformationCode != branchCode) != null;
        }

        public async Task<bool> ExistsByLongEnglishNameAsync(string englishName, string lessorCode, string branchCode)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allBranches = await GetAllAsyncByLessor(lessorCode);
            return allBranches.Any(x => x.CrCasBranchInformationEnName.ToLower().Equals(englishName.ToLower()) && x.CrCasBranchInformationCode != branchCode);
        }


        public async Task<bool> ExistsByShortArabicNameAsync(string arabicName, string lessorCode, string branchCode)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            var allBranches = await GetAllAsyncByLessor(lessorCode);
            return allBranches.Any(x => x.CrCasBranchInformationArShortName == arabicName && x.CrCasBranchInformationCode != branchCode) != null;
        }

        public async Task<bool> ExistsByShortEnglishNameAsync(string englishName, string lessorCode, string branchCode)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allBranches = await GetAllAsyncByLessor(lessorCode);
            return allBranches.Any(x => x.CrCasBranchInformationEnShortName.ToLower().Equals(englishName.ToLower()) && x.CrCasBranchInformationCode != branchCode);
        }

        public async Task<bool> ExistsByTGACodeAsync(int tgaNo, string branchCode)
        {
            var allBranches = await _unitOfWork.CrCasBranchInformation.GetAllAsyncAsNoTrackingAsync();

            return allBranches.Any(x => x.CrCasBranchInformationTgaCode == tgaNo
                                        && x.CrCasBranchInformationCode != branchCode);
        }

        public async Task<bool> UpdateBranchInformation(CrCasBranchInformation CrCasBranchInformation)
        {
            var branchInfo= await _unitOfWork.CrCasBranchInformation.FindAsync(x=>x.CrCasBranchInformationLessor== CrCasBranchInformation.CrCasBranchInformationLessor&&
                                                                                    x.CrCasBranchInformationCode== CrCasBranchInformation.CrCasBranchInformationCode);
            if (branchInfo == null) return false;

            branchInfo.CrCasBranchInformationTgaCode = CrCasBranchInformation.CrCasBranchInformationTgaCode;
            branchInfo.CrMasBranchInformationTeleKey = CrCasBranchInformation.CrMasBranchInformationTeleKey;
            branchInfo.CrCasBranchInformationTelephone = CrCasBranchInformation.CrCasBranchInformationTelephone;
            branchInfo.CrMasBranchInformationMobileKey = CrCasBranchInformation.CrMasBranchInformationMobileKey;
            branchInfo.CrCasBranchInformationMobile = CrCasBranchInformation.CrCasBranchInformationMobile;
            branchInfo.CrCasBranchInformationDirectorArName = CrCasBranchInformation.CrCasBranchInformationDirectorArName;
            branchInfo.CrCasBranchInformationDirectorEnName = CrCasBranchInformation.CrCasBranchInformationDirectorEnName;
            branchInfo.CrCasBranchInformationDirectorSignature = CrCasBranchInformation.CrCasBranchInformationDirectorSignature;
            branchInfo.CrCasBranchInformationReasons = CrCasBranchInformation.CrCasBranchInformationReasons;
            if (branchInfo.CrCasBranchInformationAvailableBalance == null) branchInfo.CrCasBranchInformationAvailableBalance = 0;
            if (branchInfo.CrCasBranchInformationTotalBalance == null) branchInfo.CrCasBranchInformationTotalBalance = 0;
            if (branchInfo.CrCasBranchInformationReservedBalance == null) branchInfo.CrCasBranchInformationReservedBalance = 0;
            _unitOfWork.CrCasBranchInformation.Update(branchInfo);
            return true;
        }

        public async Task<bool> CheckIfCanDeleteIt(string lessorCode,string branchCode)
        {
            var carsActive = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationLessor == lessorCode &&x.CrCasCarInformationBranch==branchCode&&
                                                                                   x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold);
            var salesPointsHaveBalance = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointStatus != Status.Deleted &&x.CrCasAccountSalesPointTotalAvailable > 0);
            return carsActive == 0 && salesPointsHaveBalance == 0;
        }
    }
}
