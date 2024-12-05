using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasQuestionsAnswer : IMasQuestionsAnswer
    {

        public IUnitOfWork _unitOfWork;

        public MasQuestionsAnswer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasSysQuestionsAnswer>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasSysQuestionsAnswer.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }

        public async Task AddAsync(CrMasSysQuestionsAnswer entity)
        {
            await _unitOfWork.CrMasSysQuestionsAnswer.AddAsync(entity);
        }


        public async Task<bool> CheckIfCanDeleteIt(string code)
        {
            var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code);
            return rentersLicenceCount == 0;
        }
    }
}
