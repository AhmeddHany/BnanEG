using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasContractAdditional : IMasContractAdditional
    {

        public IUnitOfWork _unitOfWork;

        public MasContractAdditional(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupContractAdditional>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupContractAdditional.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupContractAdditional entity)
        {
            await _unitOfWork.CrMasSupContractAdditional.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupContractAdditional entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupContractAdditionalCode != entity.CrMasSupContractAdditionalCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractAdditionalArName == entity.CrMasSupContractAdditionalArName ||
                    x.CrMasSupContractAdditionalEnName.ToLower().Equals(entity.CrMasSupContractAdditionalEnName.ToLower()) ||
                    (x.CrMasSupContractAdditionalNaqlCode == entity.CrMasSupContractAdditionalNaqlCode && entity.CrMasSupContractAdditionalNaqlCode != 0)
                )
            );
        }
        public async Task<bool> ExistsByDetails_Add_Async(CrMasSupContractAdditional entity)
        {
            var allLicenses = await GetAllAsync();
            if (await ExistsByCodeAsync(entity.CrMasSupContractAdditionalCode) != "0") return true;

            return allLicenses.Any(x =>
                x.CrMasSupContractAdditionalCode != entity.CrMasSupContractAdditionalCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractAdditionalArName == entity.CrMasSupContractAdditionalArName ||
                    x.CrMasSupContractAdditionalEnName.ToLower().Equals(entity.CrMasSupContractAdditionalEnName.ToLower()) ||
                    (x.CrMasSupContractAdditionalNaqlCode == entity.CrMasSupContractAdditionalNaqlCode && entity.CrMasSupContractAdditionalNaqlCode != 0)
                )
            );
        }
        

        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupContractAdditional
                .FindAsync(x => x.CrMasSupContractAdditionalArName == arabicName && x.CrMasSupContractAdditionalCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupContractAdditionalEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupContractAdditionalCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupContractAdditional
                .FindAsync(x => x.CrMasSupContractAdditionalNaqlCode == naqlCode && x.CrMasSupContractAdditionalCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code);
            return rentersLicenceCount == 0;
        }

        public async Task<string> ExistsByCodeAsync(string Code_dataField)
        {

                if (Int64.TryParse(Code_dataField, out var code) == false)
                {
                return "error_Codestart50";
                }
                else if (Code_dataField.ToString().Substring(0, 2) != "50")
                {
                return "error_Codestart50";
                }
                else if (Code_dataField.ToString().Length != 10)
                {
                return "error_Codestart50";
                }
                else if (Int64.TryParse(Code_dataField, out var id) && id != 0 && await _unitOfWork.CrMasSupContractAdditional
                .FindAsync(x => x.CrMasSupContractAdditionalCode.Trim() == id.ToString()) != null)
                {
                return "Existing";
                }
            return "0";
        }
    }
}
