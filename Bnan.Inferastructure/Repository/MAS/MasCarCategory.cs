using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarCategory : IMasCarCategory
    {

        public IUnitOfWork _unitOfWork;

        public MasCarCategory(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarCategory>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarCategory.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarCategory entity)
        {
            await _unitOfWork.CrMasSupCarCategory.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarCategory entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarCategoryCode != entity.CrMasSupCarCategoryCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarCategoryArName.Replace(" ","") == entity.CrMasSupCarCategoryArName.Replace(" ","") ||
                    x.CrMasSupCarCategoryEnName.ToLower().Replace(" ","").Equals(entity.CrMasSupCarCategoryEnName.ToLower().Replace(" ",""))
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarCategory
                .FindAsync(x => x.CrMasSupCarCategoryArName.Replace(" ","") == arabicName.Replace(" ","") && x.CrMasSupCarCategoryCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarCategoryEnName.ToLower().Replace(" ","").Equals(englishName.ToLower().Replace(" ","")) && x.CrMasSupCarCategoryCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationCategory == code && x.CrCasCarInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
