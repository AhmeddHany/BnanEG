using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface ISMSConnect
    {
        Task<bool> AddDefault(string lessorCode);
        Task<bool> AddNew(CrCasLessorSmsConnect model);
        Task<bool> Update(CrCasLessorSmsConnect model);
    }
}
