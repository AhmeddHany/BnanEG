using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterDrivingLicense : IMasRenterDrivingLicense
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterDrivingLicense(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterDrivingLicense>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterDrivingLicense entity)
        {
            await _unitOfWork.CrMasSupRenterDrivingLicense.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterDrivingLicense entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterDrivingLicenseCode != entity.CrMasSupRenterDrivingLicenseCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterDrivingLicenseArName == entity.CrMasSupRenterDrivingLicenseArName ||
                    x.CrMasSupRenterDrivingLicenseEnName.ToLower().Equals(entity.CrMasSupRenterDrivingLicenseEnName.ToLower()) ||
                    (x.CrMasSupRenterDrivingLicenseNaqlCode == entity.CrMasSupRenterDrivingLicenseNaqlCode && entity.CrMasSupRenterDrivingLicenseNaqlCode != 0) ||
                    (x.CrMasSupRenterDrivingLicenseNaqlId == entity.CrMasSupRenterDrivingLicenseNaqlId && entity.CrMasSupRenterDrivingLicenseNaqlId != 0)
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAsync(x => x.CrMasSupRenterDrivingLicenseArName == arabicName && x.CrMasSupRenterDrivingLicenseCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterDrivingLicenseEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterDrivingLicenseCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAsync(x => x.CrMasSupRenterDrivingLicenseNaqlCode == naqlCode && x.CrMasSupRenterDrivingLicenseCode != code) != null;
        }

        public async Task<bool> ExistsByNaqlIdAsync(int naqlId, string code)
        {
            if (naqlId == 0) return false;
            return await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAsync(x => x.CrMasSupRenterDrivingLicenseNaqlId == naqlId && x.CrMasSupRenterDrivingLicenseCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code && x.CrMasRenterInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
