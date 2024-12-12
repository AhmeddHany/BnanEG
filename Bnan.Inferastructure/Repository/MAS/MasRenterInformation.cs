using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterInformation : IMasRenterInformation
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterInformation(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasRenterInformation>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasRenterInformation.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasRenterInformation entity)
        {
            await _unitOfWork.CrMasRenterInformation.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasRenterInformation entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasRenterInformationId != entity.CrMasRenterInformationId && // Exclude the current entity being updated
                (
                    x.CrMasRenterInformationArName == entity.CrMasRenterInformationArName ||
                    x.CrMasRenterInformationEnName.ToLower().Equals(entity.CrMasRenterInformationEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasRenterInformation
                .FindAsync(x => x.CrMasRenterInformationArName == arabicName && x.CrMasRenterInformationId != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasRenterInformationEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasRenterInformationId != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationSector == code);
            return rentersLicenceCount == 0;
        }
    }
}
