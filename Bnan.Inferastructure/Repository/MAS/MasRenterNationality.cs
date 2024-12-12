using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterNationality : IMasRenterNationality
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterNationality(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterNationality>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterNationality.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterNationality entity)
        {
            await _unitOfWork.CrMasSupRenterNationality.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterNationality entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterNationalitiesCode != entity.CrMasSupRenterNationalitiesCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterNationalitiesArName == entity.CrMasSupRenterNationalitiesArName ||
                    x.CrMasSupRenterNationalitiesEnName.ToLower().Equals(entity.CrMasSupRenterNationalitiesEnName.ToLower()) ||
                    (x.CrMasSupRenterNationalitiesNaqlCode == entity.CrMasSupRenterNationalitiesNaqlCode && entity.CrMasSupRenterNationalitiesNaqlCode != 0) ||
                    (x.CrMasSupRenterNationalitiesNaqlId == entity.CrMasSupRenterNationalitiesNaqlId && entity.CrMasSupRenterNationalitiesNaqlId != 0)
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterNationality
                .FindAsync(x => x.CrMasSupRenterNationalitiesArName == arabicName && x.CrMasSupRenterNationalitiesCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterNationalitiesEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterNationalitiesCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupRenterNationality
                .FindAsync(x => x.CrMasSupRenterNationalitiesNaqlCode == naqlCode && x.CrMasSupRenterNationalitiesCode != code) != null;
        }

        public async Task<bool> ExistsByNaqlIdAsync(int naqlId, string code)
        {
            if (naqlId == 0) return false;
            return await _unitOfWork.CrMasSupRenterNationality
                .FindAsync(x => x.CrMasSupRenterNationalitiesNaqlId == naqlId && x.CrMasSupRenterNationalitiesCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationNationality == code && x.CrMasRenterInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
