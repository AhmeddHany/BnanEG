using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class CarDistribution : ICarDistribution
    {
        public IUnitOfWork _unitOfWork;

        public CarDistribution(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddCarDisribtion(CrMasSupCarDistribution crMasSupCarDistribution)
        {

            var model = await _unitOfWork.CrMasSupCarModel.GetByIdAsync(crMasSupCarDistribution.CrMasSupCarDistributionModel);
            var Category = await _unitOfWork.CrMasSupCarCategory.GetByIdAsync(crMasSupCarDistribution.CrMasSupCarDistributionCategory);
            var CarDistributionArConcat = $"{model.CrMasSupCarModelArConcatenateName}-{Category.CrMasSupCarCategoryArName}-{crMasSupCarDistribution.CrMasSupCarDistributionYear}";
            var CarDistributionEnConcat = $"{model.CrMasSupCarModelConcatenateEnName}-{Category.CrMasSupCarCategoryEnName}-{crMasSupCarDistribution.CrMasSupCarDistributionYear}";

            CrMasSupCarDistribution NewcrMasSupCarDistribution = new()
            {
                CrMasSupCarDistributionCode = crMasSupCarDistribution.CrMasSupCarDistributionCode,
                CrMasSupCarDistributionBrand = model.CrMasSupCarModelBrand,
                CrMasSupCarDistributionModel = crMasSupCarDistribution.CrMasSupCarDistributionModel,
                CrMasSupCarDistributionCategory = crMasSupCarDistribution.CrMasSupCarDistributionCategory,
                CrMasSupCarDistributionYear = crMasSupCarDistribution.CrMasSupCarDistributionYear,
                CrMasSupCarDistributionDoor = crMasSupCarDistribution.CrMasSupCarDistributionDoor,
                CrMasSupCarDistributionBagBags = crMasSupCarDistribution.CrMasSupCarDistributionBagBags,
                CrMasSupCarDistributionSmallBags = crMasSupCarDistribution.CrMasSupCarDistributionSmallBags,
                CrMasSupCarDistributionPassengers = crMasSupCarDistribution.CrMasSupCarDistributionPassengers,
                CrMasSupCarDistributionCount = 0,
                CrMasSupCarDistributionConcatenateArName = CarDistributionArConcat,
                CrMasSupCarDistributionConcatenateEnName = CarDistributionEnConcat,
                CrMasSupCarDistributionImage = crMasSupCarDistribution.CrMasSupCarDistributionImage,
                CrMasSupCarDistributionReasons = crMasSupCarDistribution.CrMasSupCarDistributionReasons,
                CrMasSupCarDistributionStatus = "A"

            };
            if (await _unitOfWork.CrMasSupCarDistribution.AddAsync(NewcrMasSupCarDistribution) != null) return true;
            return false;
        }

        public async Task<bool> UpdateCarDisribtion(CrMasSupCarDistribution crMasSupCarDistribution)
        {
            var carDis = await _unitOfWork.CrMasSupCarDistribution.FindAsync(x => x.CrMasSupCarDistributionCode == crMasSupCarDistribution.CrMasSupCarDistributionCode);
            if (carDis == null) return false;

            carDis.CrMasSupCarDistributionDoor = crMasSupCarDistribution.CrMasSupCarDistributionDoor;
            carDis.CrMasSupCarDistributionBagBags = crMasSupCarDistribution.CrMasSupCarDistributionBagBags;
            carDis.CrMasSupCarDistributionSmallBags = crMasSupCarDistribution.CrMasSupCarDistributionSmallBags;
            carDis.CrMasSupCarDistributionPassengers = crMasSupCarDistribution.CrMasSupCarDistributionPassengers;
            carDis.CrMasSupCarDistributionImage = crMasSupCarDistribution.CrMasSupCarDistributionImage;
            carDis.CrMasSupCarDistributionReasons = crMasSupCarDistribution.CrMasSupCarDistributionReasons;
            if (_unitOfWork.CrMasSupCarDistribution.Update(carDis) != null) return true;
            return false;
        }
    }
}
