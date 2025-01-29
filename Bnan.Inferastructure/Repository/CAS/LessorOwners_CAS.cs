using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class LessorOwners_CAS : ILessorOwners_CAS
    {

        public IUnitOfWork _unitOfWork;

        public LessorOwners_CAS(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrCasOwner>> GetAllAsync()
        {
            var result = await _unitOfWork.CrCasOwner.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrCasOwner entity)
        {
            await _unitOfWork.CrCasOwner.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrCasOwner entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrCasOwnersCode != entity.CrCasOwnersCode && // Exclude the current entity being updated
                (
                    x.CrCasOwnersArName == entity.CrCasOwnersArName ||
                    x.CrCasOwnersEnName.ToLower().Equals(entity.CrCasOwnersEnName.ToLower()) ||
                    //x.CrCasOwnersEmail.ToLower().Equals(entity.CrCasOwnersEmail.ToLower()) ||
                    x.CrCasOwnersMobile == entity.CrCasOwnersMobile
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrCasOwner
                .FindAsync(x => x.CrCasOwnersArName == arabicName && x.CrCasOwnersCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasOwnersEnName.ToLower().Equals(englishName.ToLower()) && x.CrCasOwnersCode != code);
        }
        //public async Task<bool> ExistsByEmailAsync(string email, string code)
        //{
        //    if (string.IsNullOrEmpty(email)) return false;
        //    var allLicenses = await GetAllAsync();
        //    return allLicenses.Any(x => x.CrCasOwnersEmail.ToLower().Equals(email.ToLower()) && x.CrCasOwnersCode != code);
        //}
        public async Task<bool> ExistsByMobileAsync(string mobile, string code)
        {
            if (string.IsNullOrEmpty(mobile)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrCasOwnersMobile== mobile && x.CrCasOwnersCode != code);
        }
        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationOwner == code && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationOwnerStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
