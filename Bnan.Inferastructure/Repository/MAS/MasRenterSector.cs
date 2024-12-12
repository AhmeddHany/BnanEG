using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterSector : IMasRenterSector
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterSector(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterSector>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterSector.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterSector entity)
        {
            await _unitOfWork.CrMasSupRenterSector.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterSector entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterSectorCode != entity.CrMasSupRenterSectorCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterSectorArName == entity.CrMasSupRenterSectorArName ||
                    x.CrMasSupRenterSectorEnName.ToLower().Equals(entity.CrMasSupRenterSectorEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterSector
                .FindAsync(x => x.CrMasSupRenterSectorArName == arabicName && x.CrMasSupRenterSectorCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterSectorEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterSectorCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationSector == code && x.CrMasRenterInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
