using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasLessor : IMasLessor
    {
        public IUnitOfWork _unitOfWork;

        public MasLessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasLessorInformation>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasLessorInformation.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }
        public async Task<bool> ExistsByDetailsAsync(CrMasLessorInformation entity)
        {
            var allLessors = await GetAllAsync();

            return allLessors.Any(x =>
                x.CrMasLessorInformationCode != entity.CrMasLessorInformationCode && // Exclude the current entity being updated
                (
                    x.CrMasLessorInformationArLongName == entity.CrMasLessorInformationArLongName ||
                    x.CrMasLessorInformationEnLongName.ToLower().Equals(entity.CrMasLessorInformationEnLongName.ToLower()) ||
                    x.CrMasLessorInformationArShortName == entity.CrMasLessorInformationArShortName ||
                    x.CrMasLessorInformationEnShortName.ToLower().Equals(entity.CrMasLessorInformationEnShortName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByLongArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            var allLessors = await GetAllAsync();

            return allLessors.Any(x => x.CrMasLessorInformationArLongName == arabicName && x.CrMasLessorInformationCode != code) != null;
        }

        public async Task<bool> ExistsByLongEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLessors = await GetAllAsync();
            return allLessors.Any(x => x.CrMasLessorInformationEnLongName.ToLower().Equals(englishName.ToLower()) && x.CrMasLessorInformationCode != code);
        }


        public async Task<bool> ExistsByShortArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            var allLessors = await GetAllAsync();
            return allLessors.Any(x => x.CrMasLessorInformationArShortName == arabicName && x.CrMasLessorInformationCode != code) != null;
        }

        public async Task<bool> ExistsByShortEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLessors = await GetAllAsync();
            return allLessors.Any(x => x.CrMasLessorInformationEnShortName.ToLower().Equals(englishName.ToLower()) && x.CrMasLessorInformationCode != code);
        }
        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            //var rentersLicenceCount = await _unitOfWork.CrMasLessorInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code);
            //return rentersLicenceCount == 0;
            return true;
        }

    }
}
