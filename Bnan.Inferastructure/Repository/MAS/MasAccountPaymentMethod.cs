using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasAccountPaymentMethod : IMasAccountPaymentMethod
    {

        public IUnitOfWork _unitOfWork;

        public MasAccountPaymentMethod(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupAccountPaymentMethod>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupAccountPaymentMethod.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupAccountPaymentMethod entity)
        {
            await _unitOfWork.CrMasSupAccountPaymentMethod.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupAccountPaymentMethod entity, bool isEdit)
        {
            if (!isEdit)
            {
                var pk = await ExistsByPKCodeAsync(entity.CrMasSupAccountPaymentMethodCode);
                var class1 = await CheckClassificationAsync(entity.CrMasSupAccountPaymentMethodClassification);
                if (pk || class1) return true;
            }


            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupAccountPaymentMethodCode != entity.CrMasSupAccountPaymentMethodCode && // Exclude the current entity being updated
                (
                    x.CrMasSupAccountPaymentMethodArName == entity.CrMasSupAccountPaymentMethodArName ||
                    x.CrMasSupAccountPaymentMethodEnName.ToLower().Equals(entity.CrMasSupAccountPaymentMethodEnName.ToLower())
                )
            );
        }

        public async Task<bool> ExistsByPKCodeAsync(string PKCode)
        {

            if (int.TryParse(PKCode, out var pk)==false || pk < 1 || pk > 99 ) return true;
            return await _unitOfWork.CrMasSupAccountPaymentMethod
                .FindAsync(x => x.CrMasSupAccountPaymentMethodCode == PKCode) != null;
        }
        public async Task<bool> CheckClassificationAsync(string classfication)
        {
            if (int.TryParse(classfication, out var class1) == false || class1 < 1 || class1 > 7) return true;
            return false;
        }
        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupAccountPaymentMethod
                .FindAsync(x => x.CrMasSupAccountPaymentMethodArName == arabicName && x.CrMasSupAccountPaymentMethodCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupAccountPaymentMethodEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupAccountPaymentMethodCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasAccountReceipt.CountAsync(x => x.CrCasAccountReceiptPaymentMethod == code);
            return rentersLicenceCount == 0;
        }
    }
}
