using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCountryClassification : IMasCountryClassification
    {

        public IUnitOfWork _unitOfWork;

        public MasCountryClassification(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCountryClassification>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCountryClassification.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCountryClassification entity)
        {
            await _unitOfWork.CrMasSupCountryClassification.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCountryClassification entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasLessorCountryClassificationCode != entity.CrMasLessorCountryClassificationCode && // Exclude the current entity being updated
                (
                    x.CrMasLessorCountryClassificationArName == entity.CrMasLessorCountryClassificationArName ||
                    x.CrMasLessorCountryClassificationEnName.ToLower().Equals(entity.CrMasLessorCountryClassificationEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCountryClassification
                .FindAsync(x => x.CrMasLessorCountryClassificationArName == arabicName && x.CrMasLessorCountryClassificationCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasLessorCountryClassificationEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasLessorCountryClassificationCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasSysCallingKeys.CountAsync(x => x.CrMasSysCallingKeysClassificationCode == code && x.CrMasSysCallingKeysStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
