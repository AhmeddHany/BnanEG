using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.CAS
{
    public class RenterDriver_CAS : IRenterDriver_CAS
    {

        public IUnitOfWork _unitOfWork;

        public RenterDriver_CAS(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrCasRenterPrivateDriverInformation>> GetAllAsync()
        {
            var result = await _unitOfWork.CrCasRenterPrivateDriverInformation.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrCasRenterPrivateDriverInformation entity)
        {
            await _unitOfWork.CrCasRenterPrivateDriverInformation.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrCasRenterPrivateDriverInformation entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrCasRenterPrivateDriverInformationId != entity.CrCasRenterPrivateDriverInformationId && x.CrCasRenterPrivateDriverInformationLessor == entity.CrCasRenterPrivateDriverInformationLessor && // Exclude the current entity being updated
                (
                    x.CrCasRenterPrivateDriverInformationId == entity.CrCasRenterPrivateDriverInformationId ||
                    x.CrCasRenterPrivateDriverInformationArName == entity.CrCasRenterPrivateDriverInformationArName ||
                    x.CrCasRenterPrivateDriverInformationEnName.ToLower().Equals(entity.CrCasRenterPrivateDriverInformationEnName.ToLower()) 
                    // ||x.CrCasRenterPrivateDriverInformationEmail.ToLower().Equals(entity.CrCasRenterPrivateDriverInformationEmail.ToLower()) 
                    // ||x.CrCasRenterPrivateDriverInformationMobile == entity.CrCasRenterPrivateDriverInformationMobile
                )
            );
        }

        public async Task<bool> ExistsByDetails_AddAsync(CrCasRenterPrivateDriverInformation entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrCasRenterPrivateDriverInformationLessor == entity.CrCasRenterPrivateDriverInformationLessor && // Exclude the current entity being updated
                (
                    x.CrCasRenterPrivateDriverInformationId == entity.CrCasRenterPrivateDriverInformationId ||
                    x.CrCasRenterPrivateDriverInformationArName == entity.CrCasRenterPrivateDriverInformationArName ||
                    x.CrCasRenterPrivateDriverInformationEnName.ToLower().Equals(entity.CrCasRenterPrivateDriverInformationEnName.ToLower())
                // ||x.CrCasRenterPrivateDriverInformationEmail.ToLower().Equals(entity.CrCasRenterPrivateDriverInformationEmail.ToLower()) 
                // ||x.CrCasRenterPrivateDriverInformationMobile == entity.CrCasRenterPrivateDriverInformationMobile
                )
            );
        }

        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code,string company)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrCasRenterPrivateDriverInformation
                .FindAsync(x => x.CrCasRenterPrivateDriverInformationArName == arabicName && x.CrCasRenterPrivateDriverInformationId != code && x.CrCasRenterPrivateDriverInformationLessor == company) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code,string company)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasRenterPrivateDriverInformationEnName.ToLower().Equals(englishName.ToLower()) && x.CrCasRenterPrivateDriverInformationId != code && x.CrCasRenterPrivateDriverInformationLessor == company);
        }
        public async Task<bool> ExistsByIDAsync(string code, string company)
        {
            if (string.IsNullOrEmpty(code)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasRenterPrivateDriverInformationId.Equals(code.Trim()) && x.CrCasRenterPrivateDriverInformationLessor == company);
        }

        //public async Task<bool> ExistsByEmailAsync(string email, string code)
        //{
        //    if (string.IsNullOrEmpty(email)) return false;
        //    var allLicenses = await GetAllAsync();
        //    return allLicenses.Any(x => x.CrCasRenterPrivateDriverInformationEmail.ToLower().Equals(email.ToLower()) && x.CrCasRenterPrivateDriverInformationId != code);
        //}
        public async Task<bool> ExistsByMobileAsync(string mobile, string code)
        {
            if (string.IsNullOrEmpty(mobile)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasRenterPrivateDriverInformationMobile== mobile && x.CrCasRenterPrivateDriverInformationId != code);
        }
        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            //var rentersLicenceCount = await _unitOfWork.CrCasRenterLessor.CountAsync(x => x. == code && x.CrCasCarInformationStatus != Status.Deleted);
            //return rentersLicenceCount == 0;
            return true;
        }
        public async Task<bool> CheckIfCanEditStatus_It(string code,string lessor)
        {
            var Count2 = await _unitOfWork.CrCasRenterPrivateDriverInformation.CountAsync(x => x.CrCasRenterPrivateDriverInformationId.Trim() == code.Trim() && x.CrCasRenterPrivateDriverInformationLessor== lessor && x.CrCasRenterPrivateDriverInformationStatus==Status.Rented);
            return Count2 == 0;
        }
    }
}
