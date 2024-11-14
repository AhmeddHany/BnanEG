using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterIdtype : IMasRenterIdtype
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterIdtype(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterIdtype>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterIdtype.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterIdtype entity)
        {
            await _unitOfWork.CrMasSupRenterIdtype.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterIdtype entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterIdtypeCode != entity.CrMasSupRenterIdtypeCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterIdtypeArName == entity.CrMasSupRenterIdtypeArName ||
                    x.CrMasSupRenterIdtypeEnName.ToLower().Equals(entity.CrMasSupRenterIdtypeEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterIdtype
                .FindAsync(x => x.CrMasSupRenterIdtypeArName == arabicName && x.CrMasSupRenterIdtypeCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterIdtypeEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterIdtypeCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationIdtype == code);
            return rentersLicenceCount == 0;
        }
    }
}
