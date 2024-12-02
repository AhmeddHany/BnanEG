using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasContractOptions : IMasContractOptions
    {

        public IUnitOfWork _unitOfWork;

        public MasContractOptions(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupContractOption>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupContractOption.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupContractOption entity)
        {
            await _unitOfWork.CrMasSupContractOption.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupContractOption entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupContractOptionsCode != entity.CrMasSupContractOptionsCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractOptionsArName == entity.CrMasSupContractOptionsArName ||
                    x.CrMasSupContractOptionsEnName.ToLower().Equals(entity.CrMasSupContractOptionsEnName.ToLower()) ||
                    (x.CrMasSupContractOptionsNaqlCode == entity.CrMasSupContractOptionsNaqlCode && entity.CrMasSupContractOptionsNaqlCode != 0)
                )
            );
        }

        public async Task<bool> ExistsByDetails_Add_Async(CrMasSupContractOption entity)
        {
            var allLicenses = await GetAllAsync();
            if (await ExistsByCodeAsync(entity.CrMasSupContractOptionsCode) != "0") return true;

            return allLicenses.Any(x =>
                x.CrMasSupContractOptionsCode != entity.CrMasSupContractOptionsCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractOptionsArName == entity.CrMasSupContractOptionsArName ||
                    x.CrMasSupContractOptionsEnName.ToLower().Equals(entity.CrMasSupContractOptionsEnName.ToLower()) ||
                    (x.CrMasSupContractOptionsNaqlCode == entity.CrMasSupContractOptionsNaqlCode && entity.CrMasSupContractOptionsNaqlCode != 0)
                )
            );
        }
        


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupContractOption
                .FindAsync(x => x.CrMasSupContractOptionsArName == arabicName && x.CrMasSupContractOptionsCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupContractOptionsEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupContractOptionsCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupContractOption
                .FindAsync(x => x.CrMasSupContractOptionsNaqlCode == naqlCode && x.CrMasSupContractOptionsCode != code) != null;
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
                return "error_Codestart51";
                }
                else if (Code_dataField.ToString().Substring(0, 2) != "51")
                {
                return "error_Codestart51";
                }
                else if (Code_dataField.ToString().Length != 10)
                {
                return "error_Codestart51";
                }
                else if (Int64.TryParse(Code_dataField, out var id) && id != 0 && await _unitOfWork.CrMasSupContractOption
                .FindAsync(x => x.CrMasSupContractOptionsCode.Trim() == id.ToString()) != null)
                {
                return "Existing";
                }
            return "0";
        }
    }
}
