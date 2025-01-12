using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class SMSConnect : ISMSConnect
    {
        private IUnitOfWork _unitOfWork;

        public SMSConnect(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddDefault(string lessorCode)
        {
            CrCasLessorSmsConnect crCasLessorSMSConnect = new CrCasLessorSmsConnect();
            crCasLessorSMSConnect.CrMasLessorSmsConnectLessor = lessorCode;
            crCasLessorSMSConnect.CrMasLessorSmsConnectStatus = Status.Renewed;
            var result = await _unitOfWork.CrCasLessorSmsConnect.AddAsync(crCasLessorSMSConnect);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> AddNew(CrCasLessorSmsConnect model)
        {
            var SmsConnect = await _unitOfWork.CrCasLessorSmsConnect.FindAsync(x => x.CrMasLessorSmsConnectLessor == model.CrMasLessorSmsConnectLessor);
            if (SmsConnect != null) return false;


            // Some Check and we delete it 
            if (!string.IsNullOrWhiteSpace(model.CrMasLessorSmsConnectName) &&
               !string.IsNullOrWhiteSpace(model.CrMasLessorSmsConnectAuthorization))
            {
                model.CrMasLessorSmsConnectStatus = Status.Active;
            }
            else model.CrMasLessorSmsConnectStatus = Status.Renewed;

            var result = await _unitOfWork.CrCasLessorSmsConnect.AddAsync(model);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> Update(CrCasLessorSmsConnect model)
        {
            var SmsConnect = await _unitOfWork.CrCasLessorSmsConnect.FindAsync(x => x.CrMasLessorSmsConnectLessor == model.CrMasLessorSmsConnectLessor);
            if (SmsConnect == null) return false;
            SmsConnect.CrMasLessorSmsConnectName = model.CrMasLessorSmsConnectName;
            SmsConnect.CrMasLessorSmsConnectAuthorization = model.CrMasLessorSmsConnectAuthorization;
            // Some Check and we delete it 
            if (!string.IsNullOrWhiteSpace(SmsConnect.CrMasLessorSmsConnectName) &&
               !string.IsNullOrWhiteSpace(SmsConnect.CrMasLessorSmsConnectAuthorization))
            {
                SmsConnect.CrMasLessorSmsConnectStatus = Status.Active;
            }
            else SmsConnect.CrMasLessorSmsConnectStatus = Status.Renewed;


            var result = _unitOfWork.CrCasLessorSmsConnect.Update(SmsConnect);
            if (result != null) return true;
            return false;
        }
    }
}
