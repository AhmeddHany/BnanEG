using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Models;
using System.Diagnostics.Contracts;

namespace Bnan.Inferastructure.Repository.CAS
{
    public class AccountBank_CAS : IAccountBank_CAS
    {

        public IUnitOfWork _unitOfWork;

        public AccountBank_CAS(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrCasAccountBank>> GetAllAsync()
        {
            var result = await _unitOfWork.CrCasAccountBank.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrCasAccountBank entity)
        {
            await _unitOfWork.CrCasAccountBank.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrCasAccountBank entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrCasAccountBankCode != entity.CrCasAccountBankCode && ((x.CrCasAccountBankLessor == entity.CrCasAccountBankLessor && // Exclude the current entity being updated
                (
                    x.CrCasAccountBankCode == entity.CrCasAccountBankCode ||
                    x.CrCasAccountBankArName == entity.CrCasAccountBankArName ||
                    x.CrCasAccountBankEnName.ToLower().Equals(entity.CrCasAccountBankEnName.ToLower())
                // ||x.CrCasAccountBankEmail.ToLower().Equals(entity.CrCasAccountBankEmail.ToLower()) 
                // ||x.CrCasAccountBankMobile == entity.CrCasAccountBankMobile
                )) || x.CrCasAccountBankIban.ToLower().Equals(entity.CrCasAccountBankIban.ToLower()))
            );
        }

        public async Task<bool> ExistsByDetails_AddAsync(CrCasAccountBank entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                (x.CrCasAccountBankLessor == entity.CrCasAccountBankLessor && // Exclude the current entity being updated
                (
                    x.CrCasAccountBankCode == entity.CrCasAccountBankCode ||
                    x.CrCasAccountBankArName == entity.CrCasAccountBankArName ||
                    x.CrCasAccountBankEnName.ToLower().Equals(entity.CrCasAccountBankEnName.ToLower())
                // ||x.CrCasAccountBankEmail.ToLower().Equals(entity.CrCasAccountBankEmail.ToLower()) 
                // ||x.CrCasAccountBankMobile == entity.CrCasAccountBankMobile
                ))|| x.CrCasAccountBankIban.ToLower().Equals(entity.CrCasAccountBankIban.ToLower())
            );
        }

        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code,string company)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrCasAccountBank
                .FindAsync(x => x.CrCasAccountBankArName == arabicName && x.CrCasAccountBankCode != code && x.CrCasAccountBankLessor == company) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code,string company)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasAccountBankEnName.ToLower().Equals(englishName.ToLower()) && x.CrCasAccountBankCode != code && x.CrCasAccountBankLessor == company);
        }
        //public async Task<bool> ExistsByEmailAsync(string email, string code)
        //{
        //    if (string.IsNullOrEmpty(email)) return false;
        //    var allLicenses = await GetAllAsync();
        //    return allLicenses.Any(x => x.CrCasAccountBankEmail.ToLower().Equals(email.ToLower()) && x.CrCasAccountBankCode != code);
        //}
        public async Task<bool> ExistsByBankIbanAsync(string Iban, string code)
        {
            if (string.IsNullOrEmpty(Iban)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasAccountBankIban.ToLower().Equals(Iban.ToLower()) && x.CrCasAccountBankCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code, string lessor)
        {
            var countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == lessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountBank == code && x.CrCasAccountSalesPointTotalBalance >0);
            return countForSales == 0;
        }
        public async Task<bool> CheckIfCanEdit_It(string code, string lessor)
        {
            var countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == lessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountBank == code);
            return countForSales == 0;
        }
    }
}
