using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterMembership : IMasRenterMembership
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterMembership(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterMembership>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterMembership.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterMembership entity)
        {
            await _unitOfWork.CrMasSupRenterMembership.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterMembership entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterMembershipCode != entity.CrMasSupRenterMembershipCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterMembershipArName == entity.CrMasSupRenterMembershipArName ||
                    x.CrMasSupRenterMembershipEnName.ToLower().Equals(entity.CrMasSupRenterMembershipEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterMembership
                .FindAsync(x => x.CrMasSupRenterMembershipArName == arabicName && x.CrMasSupRenterMembershipCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterMembershipEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterMembershipCode != code);
        }


        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasRenterLessor.CountAsync(x => x.CrCasRenterLessorMembership == code && x.CrCasRenterLessorStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
