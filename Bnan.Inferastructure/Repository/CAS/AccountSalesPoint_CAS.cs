using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Models;
using System.Diagnostics.Contracts;

namespace Bnan.Inferastructure.Repository.CAS
{
    public class AccountSalesPoint_CAS : IAccountSalesPoint_CAS
    {

        public IUnitOfWork _unitOfWork;

        public AccountSalesPoint_CAS(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrCasAccountSalesPoint>> GetAllAsync()
        {
            var result = await _unitOfWork.CrCasAccountSalesPoint.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrCasAccountSalesPoint entity)
        {
            await _unitOfWork.CrCasAccountSalesPoint.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrCasAccountSalesPoint entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrCasAccountSalesPointCode != entity.CrCasAccountSalesPointCode && ((x.CrCasAccountSalesPointLessor == entity.CrCasAccountSalesPointLessor && // Exclude the current entity being updated
                (
                    x.CrCasAccountSalesPointCode == entity.CrCasAccountSalesPointCode ||
                    x.CrCasAccountSalesPointArName == entity.CrCasAccountSalesPointArName ||
                    x.CrCasAccountSalesPointEnName.ToLower().Equals(entity.CrCasAccountSalesPointEnName.ToLower())
                // ||x.CrCasAccountSalesPointEmail.ToLower().Equals(entity.CrCasAccountSalesPointEmail.ToLower()) 
                // ||x.CrCasAccountSalesPointMobile == entity.CrCasAccountSalesPointMobile
                )) || x.CrCasAccountSalesPointNo.ToLower().Equals(entity.CrCasAccountSalesPointNo.ToLower()))
            );
        }

        public async Task<bool> ExistsByDetails_AddAsync(CrCasAccountSalesPoint entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                (x.CrCasAccountSalesPointLessor == entity.CrCasAccountSalesPointLessor && // Exclude the current entity being updated
                (
                    x.CrCasAccountSalesPointCode == entity.CrCasAccountSalesPointCode ||
                    x.CrCasAccountSalesPointArName == entity.CrCasAccountSalesPointArName ||
                    x.CrCasAccountSalesPointEnName.ToLower().Equals(entity.CrCasAccountSalesPointEnName.ToLower())
                // ||x.CrCasAccountSalesPointEmail.ToLower().Equals(entity.CrCasAccountSalesPointEmail.ToLower()) 
                // ||x.CrCasAccountSalesPointMobile == entity.CrCasAccountSalesPointMobile
                ))|| x.CrCasAccountSalesPointNo.ToLower().Equals(entity.CrCasAccountSalesPointNo.ToLower())
            );
        }

        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code,string company)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrCasAccountSalesPoint
                .FindAsync(x => x.CrCasAccountSalesPointArName == arabicName && x.CrCasAccountSalesPointCode != code && x.CrCasAccountSalesPointLessor == company) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code,string company)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasAccountSalesPointEnName.ToLower().Equals(englishName.ToLower()) && x.CrCasAccountSalesPointCode != code && x.CrCasAccountSalesPointLessor == company);
        }
        //public async Task<bool> ExistsByEmailAsync(string email, string code)
        //{
        //    if (string.IsNullOrEmpty(email)) return false;
        //    var allLicenses = await GetAllAsync();
        //    return allLicenses.Any(x => x.CrCasAccountSalesPointEmail.ToLower().Equals(email.ToLower()) && x.CrCasAccountSalesPointCode != code);
        //}
        public async Task<bool> ExistsBySalesPointIbanAsync(string Iban, string code)
        {
            if (string.IsNullOrEmpty(Iban)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasAccountSalesPointNo.ToLower().Equals(Iban.ToLower()) && x.CrCasAccountSalesPointCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code, string lessor)
        {
            //var countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == lessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountSalesPoint == code);
            //return countForSales == 0;
            return false;
        }
        public async Task<bool> CheckIfCanEdit_It(string code, string lessor)
        {
            //var countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == lessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountSalesPoint == code);
            //return countForSales == 0;
            return false;

        }
    }
}
