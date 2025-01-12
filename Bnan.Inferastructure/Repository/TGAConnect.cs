using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class TGAConnect : ITGAConnect
    {
        private IUnitOfWork _unitOfWork;

        public TGAConnect(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddDefault(string lessorCode)
        {
            CrCasLessorTgaConnect crCasLessorTgaConnect = new CrCasLessorTgaConnect();
            crCasLessorTgaConnect.CrMasLessorTgaConnectLessor = lessorCode;
            crCasLessorTgaConnect.CrMasLessorTgaConnectStatus = Status.Renewed;
            var result = await _unitOfWork.CrCasLessorTgaConnect.AddAsync(crCasLessorTgaConnect);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> AddNew(CrCasLessorTgaConnect model)
        {
            var TgaConnect = await _unitOfWork.CrCasLessorTgaConnect.FindAsync(x => x.CrMasLessorTgaConnectLessor == model.CrMasLessorTgaConnectLessor);
            if (TgaConnect != null) return false;

            CrCasLessorTgaConnect crCasLessorTgaConnect = new CrCasLessorTgaConnect();
            crCasLessorTgaConnect.CrMasLessorTgaConnectLessor = model.CrMasLessorTgaConnectLessor;
            crCasLessorTgaConnect.CrMasLessorTgaConnectAppId = model.CrMasLessorTgaConnectAppId;
            crCasLessorTgaConnect.CrMasLessorTgaConnectAuthorization = model.CrMasLessorTgaConnectAuthorization;
            crCasLessorTgaConnect.CrMasLessorTgaConnectAppKey = model.CrMasLessorTgaConnectAppKey;
            crCasLessorTgaConnect.CrMasLessorTgaConnectContentType = model.CrMasLessorTgaConnectContentType;
            // Some Check and we delete it 
            if (!string.IsNullOrWhiteSpace(crCasLessorTgaConnect.CrMasLessorTgaConnectAppId) &&
               !string.IsNullOrWhiteSpace(crCasLessorTgaConnect.CrMasLessorTgaConnectAuthorization) &&
               !string.IsNullOrWhiteSpace(crCasLessorTgaConnect.CrMasLessorTgaConnectAppKey) &&
               !string.IsNullOrWhiteSpace(crCasLessorTgaConnect.CrMasLessorTgaConnectContentType))
            {
                crCasLessorTgaConnect.CrMasLessorTgaConnectStatus = Status.Active;
            }
            else crCasLessorTgaConnect.CrMasLessorTgaConnectStatus = Status.Renewed;

            var result = _unitOfWork.CrCasLessorTgaConnect.Update(crCasLessorTgaConnect);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> Update(CrCasLessorTgaConnect model)
        {
            var TgaConnect = await _unitOfWork.CrCasLessorTgaConnect.FindAsync(x => x.CrMasLessorTgaConnectLessor == model.CrMasLessorTgaConnectLessor);
            if (TgaConnect == null) return false;
            TgaConnect.CrMasLessorTgaConnectAppId = model.CrMasLessorTgaConnectAppId;
            TgaConnect.CrMasLessorTgaConnectAuthorization = model.CrMasLessorTgaConnectAuthorization;
            TgaConnect.CrMasLessorTgaConnectAppKey = model.CrMasLessorTgaConnectAppKey;
            TgaConnect.CrMasLessorTgaConnectContentType = model.CrMasLessorTgaConnectContentType;
            // Some Check and we delete it 
            if (!string.IsNullOrWhiteSpace(TgaConnect.CrMasLessorTgaConnectAppId) &&
               !string.IsNullOrWhiteSpace(TgaConnect.CrMasLessorTgaConnectAuthorization) &&
               !string.IsNullOrWhiteSpace(TgaConnect.CrMasLessorTgaConnectAppKey) &&
               !string.IsNullOrWhiteSpace(TgaConnect.CrMasLessorTgaConnectContentType))
            {
                TgaConnect.CrMasLessorTgaConnectStatus = Status.Active;
            }
            else TgaConnect.CrMasLessorTgaConnectStatus = Status.Renewed;


            var result = _unitOfWork.CrCasLessorTgaConnect.Update(TgaConnect);
            if (result != null) return true;
            return false;
        }
    }
}
