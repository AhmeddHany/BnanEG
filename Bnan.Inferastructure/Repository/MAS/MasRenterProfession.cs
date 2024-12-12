using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasRenterProfession : IMasRenterProfession
    {

        public IUnitOfWork _unitOfWork;

        public MasRenterProfession(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupRenterProfession>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupRenterProfession.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupRenterProfession entity)
        {
            await _unitOfWork.CrMasSupRenterProfession.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupRenterProfession entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupRenterProfessionsCode != entity.CrMasSupRenterProfessionsCode && // Exclude the current entity being updated
                (
                    x.CrMasSupRenterProfessionsArName == entity.CrMasSupRenterProfessionsArName ||
                    x.CrMasSupRenterProfessionsEnName.ToLower().Equals(entity.CrMasSupRenterProfessionsEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupRenterProfession
                .FindAsync(x => x.CrMasSupRenterProfessionsArName == arabicName && x.CrMasSupRenterProfessionsCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupRenterProfessionsEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupRenterProfessionsCode != code);
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationProfession == code && x.CrMasRenterInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
