using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarOil : IMasCarOil
    {

        public IUnitOfWork _unitOfWork;

        public MasCarOil(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarOil>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarOil.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarOil entity)
        {
            await _unitOfWork.CrMasSupCarOil.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarOil entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarOilCode != entity.CrMasSupCarOilCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarOilArName == entity.CrMasSupCarOilArName ||
                    x.CrMasSupCarOilEnName.ToLower().Equals(entity.CrMasSupCarOilEnName.ToLower()) ||
                    (x.CrMasSupCarOilNaqlCode == entity.CrMasSupCarOilNaqlCode && entity.CrMasSupCarOilNaqlCode != 0) ||
                    (x.CrMasSupCarOilNaqlId == entity.CrMasSupCarOilNaqlId && entity.CrMasSupCarOilNaqlId != 0)
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarOil
                .FindAsync(x => x.CrMasSupCarOilArName == arabicName && x.CrMasSupCarOilCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarOilEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarOilCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupCarOil
                .FindAsync(x => x.CrMasSupCarOilNaqlCode == naqlCode && x.CrMasSupCarOilCode != code) != null;
        }

        public async Task<bool> ExistsByNaqlIdAsync(int naqlId, string code)
        {
            if (naqlId == 0) return false;
            return await _unitOfWork.CrMasSupCarOil
                .FindAsync(x => x.CrMasSupCarOilNaqlId == naqlId && x.CrMasSupCarOilCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationOil == code && x.CrCasCarInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
