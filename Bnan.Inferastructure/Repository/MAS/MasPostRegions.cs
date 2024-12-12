using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasPostRegions : IMasPostRegions
    {

        public IUnitOfWork _unitOfWork;

        public MasPostRegions(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupPostRegion>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupPostRegion.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupPostRegion entity)
        {
            await _unitOfWork.CrMasSupPostRegion.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupPostRegion entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupPostRegionsCode != entity.CrMasSupPostRegionsCode && // Exclude the current entity being updated
                (
                    x.CrMasSupPostRegionsArName == entity.CrMasSupPostRegionsArName ||
                    x.CrMasSupPostRegionsEnName.ToLower().Equals(entity.CrMasSupPostRegionsEnName.ToLower()) ||
                    (x.CrMasSupPostRegionsLongitude == entity.CrMasSupPostRegionsLongitude && entity.CrMasSupPostRegionsLongitude != 0) ||
                    (x.CrMasSupPostRegionsLatitude == entity.CrMasSupPostRegionsLatitude && entity.CrMasSupPostRegionsLatitude != 0) ||
                    (x.CrMasSupPostRegionsLocation == entity.CrMasSupPostRegionsLocation && entity.CrMasSupPostRegionsLocation != "")
                )
            );
        }
        public async Task<bool> ExistsByDetailsEdit_onlyAsync(CrMasSupPostRegion entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupPostRegionsCode != entity.CrMasSupPostRegionsCode && // Exclude the current entity being updated
                (
                    (x.CrMasSupPostRegionsLongitude == entity.CrMasSupPostRegionsLongitude && entity.CrMasSupPostRegionsLongitude != 0) ||
                    (x.CrMasSupPostRegionsLatitude == entity.CrMasSupPostRegionsLatitude && entity.CrMasSupPostRegionsLatitude != 0) ||
                    (x.CrMasSupPostRegionsLocation == entity.CrMasSupPostRegionsLocation && entity.CrMasSupPostRegionsLocation != "")
                )
            );
        }
        

        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupPostRegion
                .FindAsync(x => x.CrMasSupPostRegionsArName == arabicName && x.CrMasSupPostRegionsCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupPostRegionsEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupPostRegionsCode != code);
        }

        public async Task<bool> ExistsByLongitudeAsync(decimal longitude, string code)
        {
            if (longitude == 0) return false;
            return await _unitOfWork.CrMasSupPostRegion
                .FindAsync(x => x.CrMasSupPostRegionsLongitude == longitude && x.CrMasSupPostRegionsCode != code) != null;
        }

        public async Task<bool> ExistsByLatitudeAsync(decimal latitude, string code)
        {
            if (latitude == 0) return false;
            return await _unitOfWork.CrMasSupPostRegion
                .FindAsync(x => x.CrMasSupPostRegionsLatitude == latitude && x.CrMasSupPostRegionsCode != code) != null;
        }



        public async Task<bool> ExistsByLocationAsync(string location, string code)
        {
            if (location == "") return false;
            return await _unitOfWork.CrMasSupPostRegion
                .FindAsync(x => x.CrMasSupPostRegionsLocation == location && x.CrMasSupPostRegionsCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasSupPostCity.CountAsync(x => x.CrMasSupPostCityRegionsCode == code && x.CrMasSupPostCityStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
