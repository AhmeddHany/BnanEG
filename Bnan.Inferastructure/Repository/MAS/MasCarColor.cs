using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarColor : IMasCarColor
    {

        public IUnitOfWork _unitOfWork;

        public MasCarColor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarColor>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarColor.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarColor entity)
        {
            await _unitOfWork.CrMasSupCarColor.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarColor entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarColorCode != entity.CrMasSupCarColorCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarColorArName == entity.CrMasSupCarColorArName ||
                    x.CrMasSupCarColorEnName.ToLower().Equals(entity.CrMasSupCarColorEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarColor
                .FindAsync(x => x.CrMasSupCarColorArName == arabicName && x.CrMasSupCarColorCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarColorEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarColorCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationMainColor == code && x.CrCasCarInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
