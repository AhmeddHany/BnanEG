using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasAccountBank : IMasAccountBank
    {

        public IUnitOfWork _unitOfWork;

        public MasAccountBank(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupAccountBank>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupAccountBanks.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupAccountBank entity)
        {
            await _unitOfWork.CrMasSupAccountBanks.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupAccountBank entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupAccountBankCode != entity.CrMasSupAccountBankCode && // Exclude the current entity being updated
                (
                    x.CrMasSupAccountBankArName == entity.CrMasSupAccountBankArName ||
                    x.CrMasSupAccountBankEnName.ToLower().Equals(entity.CrMasSupAccountBankEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupAccountBanks
                .FindAsync(x => x.CrMasSupAccountBankArName == arabicName && x.CrMasSupAccountBankCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupAccountBankEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupAccountBankCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasAccountBank.CountAsync(x => x.CrCasAccountBankNo == code && x.CrCasAccountBankStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
