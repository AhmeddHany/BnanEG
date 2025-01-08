using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class Communications : ICommunications
    {
        private IUnitOfWork _unitOfWork;

        public Communications(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddCommunications(CrMasLessorCommunication model)
        {
            var communication = await _unitOfWork.CrMasLessorCommunication.FindAsync(x => x.CrMasLessorCommunicationsLessorCode == model.CrMasLessorCommunicationsLessorCode);
            if (communication == null)
            {
                var result = await _unitOfWork.CrMasLessorCommunication.AddAsync(model);
                if (result != null) return true;
            }
            return false;
        }

        public async Task<bool> AddCommunicationsDefault(string lessorCode)
        {
            CrMasLessorCommunication crMasLessorCommunication = new CrMasLessorCommunication()
            {
                CrMasLessorCommunicationsLessorCode = lessorCode,
                CrMasLessorCommunicationsDefaultLanguage = "AR",
                CrMasLessorCommunicationsShomoosStatus = Status.Renewed,
                CrMasLessorCommunicationsSmsStatus = Status.Renewed,
                CrMasLessorCommunicationsTgaStatus = Status.Renewed,
            };
            var result = await _unitOfWork.CrMasLessorCommunication.AddAsync(crMasLessorCommunication);
            if (result != null) return true;
            return false;
        }

        public async Task<bool> UpdateCommunications(CrMasLessorCommunication model)
        {
            var communication = await _unitOfWork.CrMasLessorCommunication.FindAsync(x => x.CrMasLessorCommunicationsLessorCode == model.CrMasLessorCommunicationsLessorCode);
            if (communication != null)
            {
                communication.CrMasLessorCommunicationsTgaAppId = model.CrMasLessorCommunicationsTgaAppId;
                communication.CrMasLessorCommunicationsTgaAppKey = model.CrMasLessorCommunicationsTgaAppKey;
                communication.CrMasLessorCommunicationsTgaAuthorization = model.CrMasLessorCommunicationsTgaAuthorization;
                communication.CrMasLessorCommunicationsTgaContentType = model.CrMasLessorCommunicationsTgaContentType;
                communication.CrMasLessorCommunicationsShomoosAuthorization = model.CrMasLessorCommunicationsShomoosAuthorization;
                communication.CrMasLessorCommunicationsSmsApi = model.CrMasLessorCommunicationsSmsApi;
                communication.CrMasLessorCommunicationsSmsName = model.CrMasLessorCommunicationsSmsName;
                var result = _unitOfWork.CrMasLessorCommunication.Update(communication);
                if (result != null) return true;
            }
            return false;
        }
    }
}
