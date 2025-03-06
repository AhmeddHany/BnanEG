using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class CarInformation : ICarInformation
    {

        public IUnitOfWork _unitOfWork;

        public CarInformation(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddCarInformation(CrCasCarInformation model)
        {
            var lessor = await _unitOfWork.CrMasLessorInformation.FindAsync(x => x.CrMasLessorInformationCode == model.CrCasCarInformationLessor);
            var branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationLessor == model.CrCasCarInformationLessor && x.CrCasBranchInformationCode == "100", new[] { "CrCasBranchPost" });
            var owner = await _unitOfWork.CrCasOwners.FindAsync(x => x.CrCasOwnersCode == lessor.CrMasLessorInformationGovernmentNo && x.CrCasOwnersLessorCode == lessor.CrMasLessorInformationCode);
            var benficty = await _unitOfWork.CrCasBeneficiary.FindAsync(x => x.CrCasBeneficiaryCode == lessor.CrMasLessorInformationGovernmentNo && x.CrCasBeneficiaryLessorCode == lessor.CrMasLessorInformationCode);
            var distribution = await _unitOfWork.CrMasSupCarDistribution.FindAsync(x => x.CrMasSupCarDistributionCode == model.CrCasCarInformationDistribution);
            var color = await _unitOfWork.CrMasSupCarColor.FindAsync(x => x.CrMasSupCarColorCode == model.CrCasCarInformationMainColor);
            if (string.IsNullOrEmpty(model.CrCasCarInformationSecondaryColor)) model.CrCasCarInformationSecondaryColor = null;
            if (string.IsNullOrEmpty(model.CrCasCarInformationSeatColor)) model.CrCasCarInformationSeatColor = null;
            if (string.IsNullOrEmpty(model.CrCasCarInformationFloorColor)) model.CrCasCarInformationFloorColor = null;
            var carPirce = await _unitOfWork.CrCasPriceCarBasic.FindAsync(x => x.CrCasPriceCarBasicDistributionCode == distribution.CrMasSupCarDistributionCode);
            if (carPirce != null)
            {
                model.CrCasCarInformationPriceNo = carPirce.CrCasPriceCarBasicNo;
                model.CrCasCarInformationPriceStatus = true;
            }
            else
            {
                model.CrCasCarInformationPriceNo = null;
                model.CrCasCarInformationPriceStatus = false;
            }

            CrCasCarInformation casCarInformation = new CrCasCarInformation()
            {
                CrCasCarInformationSerailNo = model.CrCasCarInformationSerailNo,
                CrCasCarInformationConcatenateArName = $"{distribution.CrMasSupCarDistributionConcatenateArName}-{model.CrCasCarInformationPlateArNo}-{color.CrMasSupCarColorArName}",
                CrCasCarInformationConcatenateEnName = $"{distribution.CrMasSupCarDistributionConcatenateEnName}-{model.CrCasCarInformationPlateEnNo}-{color.CrMasSupCarColorEnName}",
                CrCasCarDocumentsMaintenances = model.CrCasCarDocumentsMaintenances,
                CrCasCarAdvantages = model.CrCasCarAdvantages,
                CrCasCarInformationCategory = distribution.CrMasSupCarDistributionCategory,
                CrCasCarInformationBrand = distribution.CrMasSupCarDistributionBrand,
                CrCasCarInformationModel = distribution.CrMasSupCarDistributionModel,
                CrCasCarInformationDistribution = model.CrCasCarInformationDistribution,
                CrCasCarInformationYear = distribution.CrMasSupCarDistributionYear,
                CrCasCarInformationCity = branch.CrCasBranchPost?.CrCasBranchPostCity,
                CrCasCarInformationRegion = branch.CrCasBranchPost?.CrCasBranchPostRegions,
                CrCasCarInformationCurrentMeter = model.CrCasCarInformationCurrentMeter,
                CrCasCarInformationCvt = model.CrCasCarInformationCvt,
                CrCasCarInformationFuel = model.CrCasCarInformationFuel,
                CrCasCarInformationFloorColor = model.CrCasCarInformationFloorColor,
                CrCasCarInformationMainColor = model.CrCasCarInformationMainColor,
                CrCasCarInformationSeatColor = model.CrCasCarInformationSeatColor,
                CrCasCarInformationSecondaryColor = model.CrCasCarInformationSecondaryColor,
                CrCasCarInformationJoinedFleetDate = model.CrCasCarInformationJoinedFleetDate,
                CrCasCarInformationLessor = model.CrCasCarInformationLessor,
                CrCasCarInformationStructureNo = model.CrCasCarInformationStructureNo,
                CrCasCarInformationOwner = owner.CrCasOwnersCode,
                CrCasCarInformationBeneficiary = benficty.CrCasBeneficiaryCode,
                CrCasCarInformationBranch = "100",
                CrCasCarInformationLocation = "100",
                CrCasCarInformationPlateArNo = model.CrCasCarInformationPlateArNo,
                CrCasCarInformationPlateEnNo = model.CrCasCarInformationPlateEnNo,
                CrCasCarInformationRegistration = model.CrCasCarInformationRegistration,
                CrCasCarInformationReasons = model.CrCasCarInformationReasons,
                CrCasCarInformationPriceNo = model.CrCasCarInformationPriceNo,
                CrCasCarInformationDocumentationStatus = false,
                CrCasCarInformationMaintenanceStatus = true,
                CrCasCarInformationPriceStatus = model.CrCasCarInformationPriceStatus,
                CrCasCarInformationBranchStatus = "A",
                CrCasCarInformationStatus = "A",
                CrCasCarInformationOwnerStatus = "A",
                CrCasCarInformationForSaleStatus = "A",
                CrCasCarInformationOfferValueSale = 0,
                CrCasCarInformationConractDaysNo = 0,
                CrCasCarInformationConractCount = 0,
            };
            await _unitOfWork.CrCasCarInformation.AddAsync(casCarInformation);
            return true;
        }
        public async Task<bool> AddAdvantagesToCar(string serialNumber, string advantageCode, string lessor, string distributionCode, string status)
        {
            var distribution = await _unitOfWork.CrMasSupCarDistribution.FindAsync(x => x.CrMasSupCarDistributionCode == distributionCode);
            var advantage = await _unitOfWork.CrMasSupCarAdvantage.FindAsync(x => x.CrMasSupCarAdvantagesCode == advantageCode);
            if (advantage != null)
            {
                CrCasCarAdvantage crCasCarAdvantage = new CrCasCarAdvantage()
                {
                    CrCasCarAdvantagesSerialNo = serialNumber,
                    CrCasCarAdvantagesLessor = lessor,
                    CrCasCarAdvantagesCode = advantage.CrMasSupCarAdvantagesCode,
                    CrCasCarAdvantagesBrand = distribution.CrMasSupCarDistributionBrand,
                    CrCasCarAdvantagesModel = distribution.CrMasSupCarDistributionModel,
                    CrCasCarAdvantagesCarYear = distribution.CrMasSupCarDistributionYear,
                    CrCasCarAdvantagesCategory = distribution.CrMasSupCarDistributionCategory,
                    CrCasCarAdvantagesStatus = status
                };
                await _unitOfWork.CrCasCarAdvantage.AddAsync(crCasCarAdvantage);
                return true;
            }
            return false;

        }

        public async Task<bool> UpdateCarInformation(CrCasCarInformation crCasCarInformation)
        {
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == crCasCarInformation.CrCasCarInformationSerailNo);
            if (car != null)
            {
                car.CrCasCarInformationRegistration = crCasCarInformation.CrCasCarInformationRegistration;
                car.CrCasCarInformationCvt = crCasCarInformation.CrCasCarInformationCvt;
                car.CrCasCarInformationFuel = crCasCarInformation.CrCasCarInformationFuel;
                car.CrCasCarInformationFloorColor = crCasCarInformation.CrCasCarInformationFloorColor;
                car.CrCasCarInformationSeatColor = crCasCarInformation.CrCasCarInformationSeatColor;
                car.CrCasCarInformationSecondaryColor = crCasCarInformation.CrCasCarInformationSecondaryColor;
                car.CrCasCarInformationReasons = crCasCarInformation.CrCasCarInformationReasons;
                _unitOfWork.CrCasCarInformation.Update(car);
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateAdvantagesToCar(string serialNumber, string advantageCode, string lessor, string status)
        {
            var advantageCar = await _unitOfWork.CrCasCarAdvantage.FindAsync(x => x.CrCasCarAdvantagesSerialNo == serialNumber && x.CrCasCarAdvantagesCode == advantageCode && x.CrCasCarAdvantagesLessor == lessor);
            if (advantageCar != null)
            {
                advantageCar.CrCasCarAdvantagesStatus = status;
                _unitOfWork.CrCasCarAdvantage.Update(advantageCar);
                return true;
            }
            return false;
        }

        public async Task<string> UpdateCarToSale(CrCasCarInformation crCasCarInformation)
        {
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == crCasCarInformation.CrCasCarInformationSerailNo &&
                                                                           x.CrCasCarInformationLessor == crCasCarInformation.CrCasCarInformationLessor);
            if (car != null)
            {
                string status;
                if (crCasCarInformation.CrCasCarInformationForSaleStatus?.ToLower() == "true") status = Status.RendAndForSale;
                else status = Status.ForSale;

                car.CrCasCarInformationOfferedSaleDate = crCasCarInformation.CrCasCarInformationOfferedSaleDate;
                car.CrCasCarInformationOfferValueSale = crCasCarInformation.CrCasCarInformationOfferValueSale;
                car.CrCasCarInformationReasons = crCasCarInformation.CrCasCarInformationReasons;
                car.CrCasCarInformationForSaleStatus = status;
                if (_unitOfWork.CrCasCarInformation.Update(car) != null) return status;
            }
            return string.Empty;
        }

        public async Task<bool> SoldCar(CrCasCarInformation crCasCarInformation)
        {
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == crCasCarInformation.CrCasCarInformationSerailNo &&
                                                                           x.CrCasCarInformationLessor == crCasCarInformation.CrCasCarInformationLessor);
            if (car != null)
            {

                car.CrCasCarInformationOfferedSaleDate = crCasCarInformation.CrCasCarInformationOfferedSaleDate;
                car.CrCasCarInformationOfferValueSale = crCasCarInformation.CrCasCarInformationOfferValueSale;
                car.CrCasCarInformationReasons = crCasCarInformation.CrCasCarInformationReasons;
                car.CrCasCarInformationForSaleStatus = crCasCarInformation.CrCasCarInformationForSaleStatus;
                car.CrCasCarInformationStatus = crCasCarInformation.CrCasCarInformationStatus;
                if (_unitOfWork.CrCasCarInformation.Update(car) != null) return true;
            }
            return false;
        }

        public async Task<bool> CancelOffer(string serialNo, string lessorCode)
        {
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == serialNo &&
                                                                          x.CrCasCarInformationLessor == lessorCode);
            if (car != null)
            {
                car.CrCasCarInformationOfferedSaleDate = null;
                car.CrCasCarInformationOfferValueSale = 0;
                car.CrCasCarInformationReasons = "";
                car.CrCasCarInformationForSaleStatus = Status.Active;
                if (_unitOfWork.CrCasCarInformation.Update(car) != null) return true;
            }
            return false;
        }
    }
}
