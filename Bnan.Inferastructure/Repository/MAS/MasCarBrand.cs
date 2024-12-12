using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarBrand : IMasCarBrand
    {

        public IUnitOfWork _unitOfWork;

        public MasCarBrand(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarBrand>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarBrand.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarBrand entity)
        {
            await _unitOfWork.CrMasSupCarBrand.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarBrand entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarBrandCode != entity.CrMasSupCarBrandCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarBrandArName == entity.CrMasSupCarBrandArName ||
                    x.CrMasSupCarBrandEnName.ToLower().Equals(entity.CrMasSupCarBrandEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarBrand
                .FindAsync(x => x.CrMasSupCarBrandArName == arabicName && x.CrMasSupCarBrandCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarBrandEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarBrandCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasSupCarModel.CountAsync(x => x.CrMasSupCarModelBrand == code && x.CrMasSupCarModelStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
