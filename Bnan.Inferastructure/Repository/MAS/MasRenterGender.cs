using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterGender : IMasRenterGender
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterGender(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterGender>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterGender.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterGender entity)
        {
            await _unitOfWork.CrMasSupRenterGender.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterGender entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterGenderCode != entity.CrMasSupRenterGenderCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterGenderArName == entity.CrMasSupRenterGenderArName ||
                    x.CrMasSupRenterGenderEnName.ToLower().Equals(entity.CrMasSupRenterGenderEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterGender
                .FindAsync(x => x.CrMasSupRenterGenderArName == arabicName && x.CrMasSupRenterGenderCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterGenderEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterGenderCode != code);
        }


        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasRenterLessor.CountAsync(x => x.CrCasRenterLessorStatisticsGender == code && x.CrCasRenterLessorStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
