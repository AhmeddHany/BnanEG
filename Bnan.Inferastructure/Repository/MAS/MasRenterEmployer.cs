using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterEmployer : IMasRenterEmployer
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterEmployer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterEmployer>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterEmployer.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterEmployer entity)
        {
            await _unitOfWork.CrMasSupRenterEmployer.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterEmployer entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterEmployerCode != entity.CrMasSupRenterEmployerCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterEmployerArName == entity.CrMasSupRenterEmployerArName ||
                    x.CrMasSupRenterEmployerEnName.ToLower().Equals(entity.CrMasSupRenterEmployerEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterEmployer
                .FindAsync(x => x.CrMasSupRenterEmployerArName == arabicName && x.CrMasSupRenterEmployerCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterEmployerEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterEmployerCode != code);
        }


        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationEmployer == code && x.CrMasRenterInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
