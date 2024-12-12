using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarAdvantage : IMasCarAdvantage
    {

        public IUnitOfWork _unitOfWork;

        public MasCarAdvantage(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarAdvantage>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarAdvantage.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarAdvantage entity)
        {
            await _unitOfWork.CrMasSupCarAdvantage.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarAdvantage entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarAdvantagesCode != entity.CrMasSupCarAdvantagesCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarAdvantagesArName == entity.CrMasSupCarAdvantagesArName ||
                    x.CrMasSupCarAdvantagesEnName.ToLower().Equals(entity.CrMasSupCarAdvantagesEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarAdvantage
                .FindAsync(x => x.CrMasSupCarAdvantagesArName == arabicName && x.CrMasSupCarAdvantagesCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarAdvantagesEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarAdvantagesCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasRenterContractAdvantage.CountAsync(x => x.CrCasRenterContractAdvantagesCode == code );
            return rentersLicenceCount == 0;
        }
    }
}
