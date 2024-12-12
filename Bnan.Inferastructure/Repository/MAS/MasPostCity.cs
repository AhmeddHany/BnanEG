using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasPostCity : IMasPostCity
    {

        public IUnitOfWork _unitOfWork;

        public MasPostCity(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupPostCity>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupPostCity.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupPostCity entity)
        {
            await _unitOfWork.CrMasSupPostCity.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupPostCity entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupPostCityCode != entity.CrMasSupPostCityCode && // Exclude the current entity being updated
                (
                    x.CrMasSupPostCityArName == entity.CrMasSupPostCityArName ||
                    x.CrMasSupPostCityEnName.ToLower().Equals(entity.CrMasSupPostCityEnName.ToLower()) ||
                    (x.CrMasSupPostCityLongitude == entity.CrMasSupPostCityLongitude && entity.CrMasSupPostCityLongitude != 0) ||
                    (x.CrMasSupPostCityLatitude == entity.CrMasSupPostCityLatitude && entity.CrMasSupPostCityLatitude != 0) ||
                    (x.CrMasSupPostCityLocation == entity.CrMasSupPostCityLocation && entity.CrMasSupPostCityLocation != "")
                )
            );
        }
        public async Task<bool> ExistsByDetailsEdit_onlyAsync(CrMasSupPostCity entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupPostCityCode != entity.CrMasSupPostCityCode && // Exclude the current entity being updated
                (
                    (x.CrMasSupPostCityLongitude == entity.CrMasSupPostCityLongitude && entity.CrMasSupPostCityLongitude != 0) ||
                    (x.CrMasSupPostCityLatitude == entity.CrMasSupPostCityLatitude && entity.CrMasSupPostCityLatitude != 0) ||
                    (x.CrMasSupPostCityLocation == entity.CrMasSupPostCityLocation && entity.CrMasSupPostCityLocation != "")
                )
            );
        }
        

        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupPostCity
                .FindAsync(x => x.CrMasSupPostCityArName == arabicName && x.CrMasSupPostCityCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupPostCityEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupPostCityCode != code);
        }

        public async Task<bool> ExistsByLongitudeAsync(decimal longitude, string code)
        {
            if (longitude == 0) return false;
            return await _unitOfWork.CrMasSupPostCity
                .FindAsync(x => x.CrMasSupPostCityLongitude == longitude && x.CrMasSupPostCityCode != code) != null;
        }

        public async Task<bool> ExistsByLatitudeAsync(decimal latitude, string code)
        {
            if (latitude == 0) return false;
            return await _unitOfWork.CrMasSupPostCity
                .FindAsync(x => x.CrMasSupPostCityLatitude == latitude && x.CrMasSupPostCityCode != code) != null;
        }



        public async Task<bool> ExistsByLocationAsync(string location, string code)
        {
            if (location == "") return false;
            return await _unitOfWork.CrMasSupPostCity
                .FindAsync(x => x.CrMasSupPostCityLocation == location && x.CrMasSupPostCityCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterPost.CountAsync(x => x.CrMasRenterPostCity == code && x.CrMasRenterPostStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
