using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class ShomoosConnect : IShomoosConnect
    {
        private IUnitOfWork _unitOfWork;

        public ShomoosConnect(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddDefault(string lessorCode)
        {
            CrCasLessorShomoosConnect crCasLessorShomoosConnect = new CrCasLessorShomoosConnect();
            crCasLessorShomoosConnect.CrMasLessorShomoosConnectLessor = lessorCode;
            crCasLessorShomoosConnect.CrMasLessorShomoosConnectStatus = Status.Renewed;
            var result = await _unitOfWork.CrCasLessorShomoosConnect.AddAsync(crCasLessorShomoosConnect);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> AddNew(CrCasLessorShomoosConnect model)
        {
            var ShomoosConnect = await _unitOfWork.CrCasLessorShomoosConnect.FindAsync(x => x.CrMasLessorShomoosConnectLessor == model.CrMasLessorShomoosConnectLessor);
            if (ShomoosConnect != null) return false;


            // Some Check and we delete it 
            if (!string.IsNullOrWhiteSpace(model.CrMasLessorShomoosConnectAppId) &&
               !string.IsNullOrWhiteSpace(model.CrMasLessorShomoosConnectAuthorization) &&
               !string.IsNullOrWhiteSpace(model.CrMasLessorShomoosConnectAppKey) &&
               !string.IsNullOrWhiteSpace(model.CrMasLessorShomoosConnectContentType))
            {
                model.CrMasLessorShomoosConnectStatus = Status.Active;
            }
            else model.CrMasLessorShomoosConnectStatus = Status.Renewed;

            var result = await _unitOfWork.CrCasLessorShomoosConnect.AddAsync(model);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> Update(CrCasLessorShomoosConnect model)
        {
            var ShomoosConnect = await _unitOfWork.CrCasLessorShomoosConnect.FindAsync(x => x.CrMasLessorShomoosConnectLessor == model.CrMasLessorShomoosConnectLessor);
            if (ShomoosConnect == null) return false;
            ShomoosConnect.CrMasLessorShomoosConnectAppId = model.CrMasLessorShomoosConnectAppId;
            ShomoosConnect.CrMasLessorShomoosConnectAuthorization = model.CrMasLessorShomoosConnectAuthorization;
            ShomoosConnect.CrMasLessorShomoosConnectAppKey = model.CrMasLessorShomoosConnectAppKey;
            ShomoosConnect.CrMasLessorShomoosConnectContentType = model.CrMasLessorShomoosConnectContentType;
            // Some Check and we delete it 
            if (!string.IsNullOrWhiteSpace(ShomoosConnect.CrMasLessorShomoosConnectAppId) &&
               !string.IsNullOrWhiteSpace(ShomoosConnect.CrMasLessorShomoosConnectAuthorization) &&
               !string.IsNullOrWhiteSpace(ShomoosConnect.CrMasLessorShomoosConnectAppKey) &&
               !string.IsNullOrWhiteSpace(ShomoosConnect.CrMasLessorShomoosConnectContentType))
            {
                ShomoosConnect.CrMasLessorShomoosConnectStatus = Status.Active;
            }
            else ShomoosConnect.CrMasLessorShomoosConnectStatus = Status.Renewed;


            var result = _unitOfWork.CrCasLessorShomoosConnect.Update(ShomoosConnect);
            if (result != null) return true;
            return false;
        }
    }
}
