using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface ICommunications
    {
        Task<bool> AddCommunications(CrMasLessorCommunication model);
        Task<bool> UpdateCommunications(CrMasLessorCommunication model);
        Task<bool> AddCommunicationsDefault(string lessorCode);


    }
}
