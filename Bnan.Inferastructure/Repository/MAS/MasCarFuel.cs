using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasCarFuel : IMasCarFuel
    {

        public IUnitOfWork _unitOfWork;

        public MasCarFuel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSupCarFuel>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSupCarFuel.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSupCarFuel entity)
        {
            await _unitOfWork.CrMasSupCarFuel.AddAsync(entity);
        }

        public async Task<bool> ExistsByDetailsAsync(CrMasSupCarFuel entity)
        {
            var allLicenses = await GetAllAsync();

            return allLicenses.Any(x =>
                x.CrMasSupCarFuelCode != entity.CrMasSupCarFuelCode && // Exclude the current entity being updated
                (
                    x.CrMasSupCarFuelArName == entity.CrMasSupCarFuelArName ||
                    x.CrMasSupCarFuelEnName.ToLower().Equals(entity.CrMasSupCarFuelEnName.ToLower()) ||
                    (x.CrMasSupCarFuelNaqlCode == entity.CrMasSupCarFuelNaqlCode && entity.CrMasSupCarFuelNaqlCode != 0) ||
                    (x.CrMasSupCarFuelNaqlId == entity.CrMasSupCarFuelNaqlId && entity.CrMasSupCarFuelNaqlId != 0)
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasSupCarFuel
                .FindAsync(x => x.CrMasSupCarFuelArName == arabicName && x.CrMasSupCarFuelCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            var allLicenses = await GetAllAsync();
            return allLicenses.Any(x => x.CrMasSupCarFuelEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasSupCarFuelCode != code);
        }

        public async Task<bool> ExistsByNaqlCodeAsync(int naqlCode, string code)
        {
            if (naqlCode == 0) return false;
            return await _unitOfWork.CrMasSupCarFuel
                .FindAsync(x => x.CrMasSupCarFuelNaqlCode == naqlCode && x.CrMasSupCarFuelCode != code) != null;
        }

        public async Task<bool> ExistsByNaqlIdAsync(int naqlId, string code)
        {
            if (naqlId == 0) return false;
            return await _unitOfWork.CrMasSupCarFuel
                .FindAsync(x => x.CrMasSupCarFuelNaqlId == naqlId && x.CrMasSupCarFuelCode != code) != null;
        }

        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationFuel == code && x.CrCasCarInformationStatus != Status.Deleted);
            return rentersLicenceCount == 0;
        }
    }
}
