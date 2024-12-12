using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasAccountReference : IMasAccountReference
    {

        public IUnitOfWork _unitOfWork;

        public MasAccountReference(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupAccountReference>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupAccountReference.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupAccountReference entity)
        {
            await _unitOfWork.CrMasSupAccountReference.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupAccountReference entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupAccountReceiptReferenceCode != entity.CrMasSupAccountReceiptReferenceCode && // Exclude the current entity being updated
                (
                    x.CrMasSupAccountReceiptReferenceArName == entity.CrMasSupAccountReceiptReferenceArName ||
                    x.CrMasSupAccountReceiptReferenceEnName.ToLower().Equals(entity.CrMasSupAccountReceiptReferenceEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupAccountReference
                .FindAsync(x => x.CrMasSupAccountReceiptReferenceArName == arabicName && x.CrMasSupAccountReceiptReferenceCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupAccountReceiptReferenceEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupAccountReceiptReferenceCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasAccountReceipt.CountAsync(x => x.CrCasAccountReceiptReferenceType == code);
            return rentersLicenceCount == 0;
        }
    }
}
