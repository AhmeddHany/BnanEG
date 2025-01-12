using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class UserContractValididation : IUserContractValididation
    {
        private IUnitOfWork _unitOfWork;
        public UserContractValididation(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddContractValiditionsForEachUserInCas(string userCode, string lastRecordProcedure)
        {
            CrMasUserContractValidity crMasUserContractValidity = new CrMasUserContractValidity()
            {
                CrMasUserContractValidityUserId = userCode,
                CrMasUserContractValidityAdmin = lastRecordProcedure,
                CrMasUserContractValidityAge = false,
                CrMasUserContractValidityBbrake = false,
                CrMasUserContractValidityCancel = false,
                CrMasUserContractValidityChamber = false,
                CrMasUserContractValidityChkecUp = false,
                CrMasUserContractValidityCompanyAddress = false,
                CrMasUserContractValidityDiscountRate = 0,
                CrMasUserContractValidityDrivingLicense = false,
                CrMasUserContractValidityEmployer = false,
                CrMasUserContractValidityEnd = false,
                CrMasUserContractValidityExtension = false,
                CrMasUserContractValidityFbrake = false,
                CrMasUserContractValidityHour = 0,
                CrMasUserContractValidityInsurance = false,
                CrMasUserContractValidityId = false,
                CrMasUserContractValidityKm = 0,
                CrMasUserContractValidityLessContractValue = false,
                CrMasUserContractValidityLicenceMunicipale = false,
                CrMasUserContractValidityMaintenance = false,
                CrMasUserContractValidityOil = false,
                CrMasUserContractValidityOperatingCard = false,
                CrMasUserContractValidityRegister = false,
                CrMasUserContractValidityRenterAddress = false,
                CrMasUserContractValidityTires = false,
                CrMasUserContractValidityTrafficLicense = false,
                CrMasUserContractValidityTransferPermission = false,
                CrMasUserContractValidityCreate = false,
            };
            if (await _unitOfWork.CrMasUserContractValidity.AddAsync(crMasUserContractValidity) != null) return true;
            return false;
        }

        public async Task<bool> EditContractValiditionsForEmployee(CrMasUserContractValidity model)
        {
            var contractValidition = await _unitOfWork.CrMasUserContractValidity.FindAsync(x => x.CrMasUserContractValidityUserId == model.CrMasUserContractValidityUserId);

            contractValidition.CrMasUserContractValidityUserId = model.CrMasUserContractValidityUserId;
            contractValidition.CrMasUserContractValidityAdmin = model.CrMasUserContractValidityAdmin;
            contractValidition.CrMasUserContractValidityAge = model.CrMasUserContractValidityAge;
            contractValidition.CrMasUserContractValidityBbrake = model.CrMasUserContractValidityBbrake;
            contractValidition.CrMasUserContractValidityCancel = model.CrMasUserContractValidityCancel;
            contractValidition.CrMasUserContractValidityChamber = model.CrMasUserContractValidityChamber;
            contractValidition.CrMasUserContractValidityChkecUp = model.CrMasUserContractValidityChkecUp;
            contractValidition.CrMasUserContractValidityCompanyAddress = model.CrMasUserContractValidityCompanyAddress;
            contractValidition.CrMasUserContractValidityDiscountRate = model.CrMasUserContractValidityDiscountRate;
            contractValidition.CrMasUserContractValidityDrivingLicense = model.CrMasUserContractValidityDrivingLicense;
            contractValidition.CrMasUserContractValidityEmployer = model.CrMasUserContractValidityEmployer;
            contractValidition.CrMasUserContractValidityEnd = model.CrMasUserContractValidityEnd;
            contractValidition.CrMasUserContractValidityExtension = model.CrMasUserContractValidityExtension;
            contractValidition.CrMasUserContractValidityFbrake = model.CrMasUserContractValidityFbrake;
            contractValidition.CrMasUserContractValidityHour = model.CrMasUserContractValidityHour;
            contractValidition.CrMasUserContractValidityInsurance = model.CrMasUserContractValidityInsurance;
            contractValidition.CrMasUserContractValidityTrafficLicense = model.CrMasUserContractValidityTrafficLicense;
            contractValidition.CrMasUserContractValidityId = model.CrMasUserContractValidityId;
            contractValidition.CrMasUserContractValidityKm = model.CrMasUserContractValidityKm;
            contractValidition.CrMasUserContractValidityLessContractValue = model.CrMasUserContractValidityLessContractValue;
            contractValidition.CrMasUserContractValidityLicenceMunicipale = model.CrMasUserContractValidityLicenceMunicipale;
            contractValidition.CrMasUserContractValidityMaintenance = model.CrMasUserContractValidityMaintenance;
            contractValidition.CrMasUserContractValidityOil = model.CrMasUserContractValidityOil;
            contractValidition.CrMasUserContractValidityOperatingCard = model.CrMasUserContractValidityOperatingCard;
            contractValidition.CrMasUserContractValidityRegister = model.CrMasUserContractValidityRegister;
            contractValidition.CrMasUserContractValidityRenterAddress = model.CrMasUserContractValidityRenterAddress;
            contractValidition.CrMasUserContractValidityTires = model.CrMasUserContractValidityTires;
            contractValidition.CrMasUserContractValidityTransferPermission = model.CrMasUserContractValidityTransferPermission;
            contractValidition.CrMasUserContractValidityCreate = model.CrMasUserContractValidityCreate;

            if (_unitOfWork.CrMasUserContractValidity.Update(contractValidition) != null) return true;
            return false;

        }
    }
}
