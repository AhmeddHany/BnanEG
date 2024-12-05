using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasContractCarCheckup : IMasContractCarCheckup
    {

        public IUnitOfWork _unitOfWork;

        public MasContractCarCheckup(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupContractCarCheckup>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupContractCarCheckup.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupContractCarCheckup entity)
        {
            await _unitOfWork.CrMasSupContractCarCheckup.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupContractCarCheckup entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupContractCarCheckupCode != entity.CrMasSupContractCarCheckupCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractCarCheckupArName == entity.CrMasSupContractCarCheckupArName ||
                    x.CrMasSupContractCarCheckupEnName.ToLower().Equals(entity.CrMasSupContractCarCheckupEnName.ToLower())
                   // || (x.CrMasSupContractCarCheckupNaqlCode == entity.CrMasSupContractCarCheckupNaqlCode && entity.CrMasSupContractCarCheckupNaqlCode != 0)
                )
            );
        }

        public async Task<bool> ExistsByDetails_Add_Async(CrMasSupContractCarCheckup entity)
        {
            var allLicenses = await GetAllAsync();
            if (await ExistsByCodeAsync(entity.CrMasSupContractCarCheckupCode) != "0") return true;

            return allLicenses.Any(x =>
                x.CrMasSupContractCarCheckupCode != entity.CrMasSupContractCarCheckupCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractCarCheckupArName == entity.CrMasSupContractCarCheckupArName ||
                    x.CrMasSupContractCarCheckupEnName.ToLower().Equals(entity.CrMasSupContractCarCheckupEnName.ToLower())
                   // || (x.CrMasSupContractCarCheckupNaqlCode == entity.CrMasSupContractCarCheckupNaqlCode && entity.CrMasSupContractCarCheckupNaqlCode != 0)
                )
            );
        }
        


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupContractCarCheckup
                .FindAsync(x => x.CrMasSupContractCarCheckupArName == arabicName && x.CrMasSupContractCarCheckupCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupContractCarCheckupEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupContractCarCheckupCode != code);
        }

        //public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        //{
        //    if (naqlCode == 0) return false;
        //    return await _unitOfWork.CrMasSupContractCarCheckup
        //        .FindAsync(x => x.CrMasSupContractCarCheckupNaqlCode == naqlCode && x.CrMasSupContractCarCheckupCode != code) != null;
        //}

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code);
            return rentersLicenceCount == 0;
        }

        public async Task<string> ExistsByCodeAsync(string Code_dataField)
        {

                if (Int64.TryParse(Code_dataField, out var code) == false)
                {
                return "error_Codestart2";
                }
                else if (Code_dataField.ToString().Length != 2)
                {
                return "error_Codestart2";
                }
                else if (Int64.TryParse(Code_dataField, out var id) && id != 0 && await _unitOfWork.CrMasSupContractCarCheckup
                .FindAsync(x => x.CrMasSupContractCarCheckupCode.Trim() == id.ToString()) != null)
                {
                return "Existing";
                }
            return "0";
        }
    }
}
