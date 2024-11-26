using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRate : IMasRate
    {

        public IUnitOfWork _unitOfWork;

        public MasRate(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSysEvaluation>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSysEvaluation.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSysEvaluation entity)
        {
            await _unitOfWork.CrMasSysEvaluation.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSysEvaluation entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSysEvaluationsCode != entity.CrMasSysEvaluationsCode && // Exclude the current entity being updated
                (
                    x.CrMasSysEvaluationsArDescription == entity.CrMasSysEvaluationsArDescription ||
                    x.CrMasSysEvaluationsEnDescription.ToLower().Equals(entity.CrMasSysEvaluationsEnDescription.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSysEvaluation
                .FindAsync(x => x.CrMasSysEvaluationsArDescription == arabicName && x.CrMasSysEvaluationsCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSysEvaluationsEnDescription.ToLower().Equals(englishName.ToLower()) && x.CrMasSysEvaluationsCode != code);
        }


        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasRenterLessor.CountAsync(x => x.CrCasRenterLessorMembership == code);
            return rentersLicenceCount == 0;
        }
    }
}
