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

            model.CrMasLessorTgaConnectContentType = "application/json";
            model.CrMasLessorTgaConnectAppId = "c49fda9f";
            model.CrMasLessorTgaConnectAppKey = "0a0ecdd133cbda8414c36b1d9f8f8f51";
            // Some Check and we delete it 
            if (!string.IsNullOrWhiteSpace(model.CrMasLessorTgaConnectAppId) &&
               !string.IsNullOrWhiteSpace(model.CrMasLessorTgaConnectAuthorization) &&
               !string.IsNullOrWhiteSpace(model.CrMasLessorTgaConnectAppKey) &&
               !string.IsNullOrWhiteSpace(model.CrMasLessorTgaConnectContentType))
            {
                model.CrMasLessorTgaConnectStatus = Status.Active;
            }
            else model.CrMasLessorTgaConnectStatus = Status.Renewed;

            var result = await _unitOfWork.CrCasLessorTgaConnect.AddAsync(model);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> Update(CrCasLessorTgaConnect model)
        {
            var TgaConnect = await _unitOfWork.CrCasLessorTgaConnect.FindAsync(x => x.CrMasLessorTgaConnectLessor == model.CrMasLessorTgaConnectLessor);
            if (TgaConnect == null) return false;
            //TgaConnect.CrMasLessorTgaConnectAppId = model.CrMasLessorTgaConnectAppId;
            TgaConnect.CrMasLessorTgaConnectAuthorization = model.CrMasLessorTgaConnectAuthorization;
            //TgaConnect.CrMasLessorTgaConnectAppKey = model.CrMasLessorTgaConnectAppKey;
            //TgaConnect.CrMasLessorTgaConnectContentType = model.CrMasLessorTgaConnectContentType;
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
