using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasLessorMarketing : IMasLessorMarketing
    {

        public IUnitOfWork _unitOfWork;

        public MasLessorMarketing(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupContractSource>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupContractSource.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupContractSource entity)
        {
            await _unitOfWork.CrMasSupContractSource.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupContractSource entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupContractSourceCode != entity.CrMasSupContractSourceCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractSourceArName == entity.CrMasSupContractSourceArName ||
                    x.CrMasSupContractSourceEnName.ToLower().Equals(entity.CrMasSupContractSourceEnName.ToLower()) ||
                    //x.CrMasSupContractSourceEmail.ToLower().Equals(entity.CrMasSupContractSourceEmail.ToLower()) ||
                    x.CrMasSupContractSourceMobile.Equals(entity.CrMasSupContractSourceMobile) 
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupContractSource
                .FindAsync(x => x.CrMasSupContractSourceArName == arabicName && x.CrMasSupContractSourceCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupContractSourceEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupContractSourceCode != code);
        }
        public async Task<bool> ExistsByEmailAsync(string email, string code)
        {
            if (string.IsNullOrEmpty(email)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupContractSourceEmail.ToLower().Equals(email.ToLower()) && x.CrMasSupContractSourceCode != code);
        }
        public async Task<bool> ExistsByMobileAsync(string mobile, string code)
        {
            if (string.IsNullOrEmpty(mobile)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupContractSourceMobile.Equals(mobile) && x.CrMasSupContractSourceCode != code);
        }
        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasAccountBank.CountAsync(x => x.CrCasAccountBankNo == code && x.CrCasAccountBankStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
