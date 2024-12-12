using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCountries : IMasCountries
    {

        public IUnitOfWork _unitOfWork;

        public MasCountries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSysCallingKey>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSysCallingKeys.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSysCallingKey entity)
        {
            await _unitOfWork.CrMasSysCallingKeys.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSysCallingKey entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSysCallingKeysCode != entity.CrMasSysCallingKeysCode && // Exclude the current entity being updated
                (
                    x.CrMasSysCallingKeysArName == entity.CrMasSysCallingKeysArName ||
                    x.CrMasSysCallingKeysEnName.ToLower().Equals(entity.CrMasSysCallingKeysEnName.ToLower()) ||
                    (x.CrMasSysCallingKeysNaqlCode == entity.CrMasSysCallingKeysNaqlCode && entity.CrMasSysCallingKeysNaqlCode != 0) ||
                    (x.CrMasSysCallingKeysNaqlId == entity.CrMasSysCallingKeysNaqlId && entity.CrMasSysCallingKeysNaqlId != 0)
                )
            );
        }
        public async Task<bool> ExistsByDetailsEdit_onlyAsync(CrMasSysCallingKey entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSysCallingKeysCode != entity.CrMasSysCallingKeysCode && // Exclude the current entity being updated
                (
                    (x.CrMasSysCallingKeysNaqlCode == entity.CrMasSysCallingKeysNaqlCode && entity.CrMasSysCallingKeysNaqlCode != 0) ||
                    (x.CrMasSysCallingKeysNaqlId == entity.CrMasSysCallingKeysNaqlId && entity.CrMasSysCallingKeysNaqlId != 0)
                )
            );
        }
        

        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSysCallingKeys
                .FindAsync(x => x.CrMasSysCallingKeysArName == arabicName && x.CrMasSysCallingKeysCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSysCallingKeysEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSysCallingKeysCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSysCallingKeys
                .FindAsync(x => x.CrMasSysCallingKeysNaqlCode == naqlCode && x.CrMasSysCallingKeysCode != code) != null;
        }

        public async Task<bool> ExistsByNaqlIdAsync(int naqlId, string code)
        {
            if (naqlId == 0) return false;
            return await _unitOfWork.CrMasSysCallingKeys
                .FindAsync(x => x.CrMasSysCallingKeysNaqlId == naqlId && x.CrMasSysCallingKeysCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationCountreyKey == code && x.CrMasRenterInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
