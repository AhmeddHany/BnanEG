using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarModel : IMasCarModel
    {

        public IUnitOfWork _unitOfWork;

        public MasCarModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarModel>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarModel.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarModel entity)
        {
            await _unitOfWork.CrMasSupCarModel.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarModel entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarModelCode != entity.CrMasSupCarModelCode && // Exclude the current entity being updated
                x.CrMasSupCarModelBrand == entity.CrMasSupCarModelBrand &&
                (
                    x.CrMasSupCarModelArName == entity.CrMasSupCarModelArName ||
                    x.CrMasSupCarModelEnName.ToLower().Equals(entity.CrMasSupCarModelEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string brandCode)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarModel
                .FindAsync(x => x.CrMasSupCarModelBrand == brandCode && x.CrMasSupCarModelArName == arabicName &&  x.CrMasSupCarModelCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string brandCode)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarModelBrand == brandCode && x.CrMasSupCarModelEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarModelCode != code);
        }
        

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationModel == code && x.CrCasCarInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
