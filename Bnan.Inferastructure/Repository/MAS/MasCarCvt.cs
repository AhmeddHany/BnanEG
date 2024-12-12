using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarCvt : IMasCarCvt
    {

        public IUnitOfWork _unitOfWork;

        public MasCarCvt(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarCvt>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarCvt.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarCvt entity)
        {
            await _unitOfWork.CrMasSupCarCvt.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarCvt entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarCvtCode != entity.CrMasSupCarCvtCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarCvtArName == entity.CrMasSupCarCvtArName ||
                    x.CrMasSupCarCvtEnName.ToLower().Equals(entity.CrMasSupCarCvtEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarCvt
                .FindAsync(x => x.CrMasSupCarCvtArName == arabicName && x.CrMasSupCarCvtCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarCvtEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarCvtCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationCvt == code && x.CrCasCarInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
