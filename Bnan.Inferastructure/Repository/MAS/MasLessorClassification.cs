using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasLessorClassification : IMasLessorClassification
    {

        public IUnitOfWork _unitOfWork;

        public MasLessorClassification(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrCasLessorClassification>> GetAllAsync()
        {
            var result = await _unitOfWork.CrCasLessorClassification.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrCasLessorClassification entity)
        {
            await _unitOfWork.CrCasLessorClassification.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrCasLessorClassification entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrCasLessorClassificationCode != entity.CrCasLessorClassificationCode && // Exclude the current entity being updated
                (
                    x.CrCasLessorClassificationArName == entity.CrCasLessorClassificationArName ||
                    x.CrCasLessorClassificationEnName.ToLower().Equals(entity.CrCasLessorClassificationEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrCasLessorClassification
                .FindAsync(x => x.CrCasLessorClassificationArName == arabicName && x.CrCasLessorClassificationCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasLessorClassificationEnName.ToLower().Equals(englishName.ToLower()) && x.CrCasLessorClassificationCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasLessorInformation.CountAsync(x => x.CrMasLessorInformationClassification == code && x.CrMasLessorInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
