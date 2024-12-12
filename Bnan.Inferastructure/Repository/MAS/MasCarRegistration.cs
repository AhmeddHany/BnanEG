using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarRegistration : IMasCarRegistration
    {

        public IUnitOfWork _unitOfWork;

        public MasCarRegistration(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarRegistration>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarRegistration.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarRegistration entity)
        {
            await _unitOfWork.CrMasSupCarRegistration.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarRegistration entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarRegistrationCode != entity.CrMasSupCarRegistrationCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarRegistrationArName == entity.CrMasSupCarRegistrationArName ||
                    x.CrMasSupCarRegistrationEnName.ToLower().Equals(entity.CrMasSupCarRegistrationEnName.ToLower()) ||
                    (x.CrMasSupCarRegistrationNaqlCode == entity.CrMasSupCarRegistrationNaqlCode && entity.CrMasSupCarRegistrationNaqlCode != 0) ||
                    (x.CrMasSupCarRegistrationNaqlId == entity.CrMasSupCarRegistrationNaqlId && entity.CrMasSupCarRegistrationNaqlId != 0)
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarRegistration
                .FindAsync(x => x.CrMasSupCarRegistrationArName == arabicName && x.CrMasSupCarRegistrationCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarRegistrationEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarRegistrationCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupCarRegistration
                .FindAsync(x => x.CrMasSupCarRegistrationNaqlCode == naqlCode && x.CrMasSupCarRegistrationCode != code) != null;
        }

        public async Task<bool> ExistsByNaqlIdAsync(int naqlId, string code)
        {
            if (naqlId == 0) return false;
            return await _unitOfWork.CrMasSupCarRegistration
                .FindAsync(x => x.CrMasSupCarRegistrationNaqlId == naqlId && x.CrMasSupCarRegistrationCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationRegistration == code && x.CrCasCarInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
