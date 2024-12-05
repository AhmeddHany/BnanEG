using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasContractCarCheckupDetails : IMasContractCarCheckupDetails
    {

        public IUnitOfWork _unitOfWork;

        public MasContractCarCheckupDetails(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupContractCarCheckupDetail>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupContractCarCheckupDetail.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupContractCarCheckupDetail entity)
        {
            await _unitOfWork.CrMasSupContractCarCheckupDetail.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupContractCarCheckupDetail entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupContractCarCheckupDetailsCode != entity.CrMasSupContractCarCheckupDetailsCode && // Exclude the current entity being updated
                (
                    x.CrMasSupContractCarCheckupDetailsArName == entity.CrMasSupContractCarCheckupDetailsArName ||
                    x.CrMasSupContractCarCheckupDetailsEnName.ToLower().Equals(entity.CrMasSupContractCarCheckupDetailsEnName.ToLower())
                   // || (x.CrMasSupContractCarCheckupDetailsNaqlCode == entity.CrMasSupContractCarCheckupDetailsNaqlCode && entity.CrMasSupContractCarCheckupDetailsNaqlCode != 0)
                )
            );
        }

        public async Task<bool> ExistsByDetails_Add_Async(CrMasSupContractCarCheckupDetail entity)
        {
            var allLicenses = await GetAllAsync();
            if (await ExistsByCodeAsync(entity.CrMasSupContractCarCheckupDetailsCode,entity.CrMasSupContractCarCheckupDetailsNo) != "0") return true;

            return allLicenses.Any(x =>
                x.CrMasSupContractCarCheckupDetailsCode == entity.CrMasSupContractCarCheckupDetailsCode && x.CrMasSupContractCarCheckupDetailsNo != entity.CrMasSupContractCarCheckupDetailsNo && // Exclude the current entity being updated
                (
                    x.CrMasSupContractCarCheckupDetailsArName == entity.CrMasSupContractCarCheckupDetailsArName ||
                    x.CrMasSupContractCarCheckupDetailsEnName.ToLower().Equals(entity.CrMasSupContractCarCheckupDetailsEnName.ToLower())
                )
            );
        }
        


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code,string no)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupContractCarCheckupDetail
                .FindAsync(x => x.CrMasSupContractCarCheckupDetailsArName == arabicName && x.CrMasSupContractCarCheckupDetailsCode == code.Trim() && x.CrMasSupContractCarCheckupDetailsNo != no.Trim()) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string no)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupContractCarCheckupDetailsEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupContractCarCheckupDetailsCode == code.Trim() && x.CrMasSupContractCarCheckupDetailsNo != no.Trim());
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code);
            return rentersLicenceCount == 0;
        }

        public async Task<string> ExistsByCodeAsync(string Code_dataField,string No)
        {

                if (Int64.TryParse(Code_dataField, out var code) == false || Int64.TryParse(No, out var nno) == false)
                {
                return "error_Codestart9";
                }
                else if (Code_dataField.ToString().Length != 2 || No.ToString().Length > 2)
                {
                return "error_Codestart9";
                }
                else if (Int64.TryParse(Code_dataField, out var id) && id != 0 && await _unitOfWork.CrMasSupContractCarCheckupDetail
                .FindAsync(x => x.CrMasSupContractCarCheckupDetailsCode.Trim() == id.ToString().Trim() && x.CrMasSupContractCarCheckupDetailsNo.Trim() == No.ToString().Trim()) != null)
                {
                return "Existing";
                }
            return "0";
        }
    }
}
