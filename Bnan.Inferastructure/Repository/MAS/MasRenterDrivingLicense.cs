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
                x.CrMasSupRenterDrivingLicenseArName == entity.CrMasSupRenterDrivingLicenseArName ||
                x.CrMasSupRenterDrivingLicenseEnName == entity.CrMasSupRenterDrivingLicenseEnName ||
                x.CrMasSupRenterDrivingLicenseNaqlCode == entity.CrMasSupRenterDrivingLicenseNaqlCode ||
                x.CrMasSupRenterDrivingLicenseNaqlId == entity.CrMasSupRenterDrivingLicenseNaqlId);
        }
        public async Task<bool> ExistsByArabicNameAsync(string arabicName)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAsync(x => x.CrMasSupRenterDrivingLicenseArName == arabicName) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            return await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAsync(x => x.CrMasSupRenterDrivingLicenseEnName == englishName) != null;
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAsync(x => x.CrMasSupRenterDrivingLicenseNaqlCode == naqlCode) != null;
        }

        public async Task<bool> ExistsByNaqlIdAsync(int naqlId)
        {
            if (naqlId == 0) return false;
            return await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAsync(x => x.CrMasSupRenterDrivingLicenseNaqlId == naqlId) != null;
        }
    }
}
