using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface IShomoosConnect
    {
        Task<bool> AddDefault(string lessorCode);
        Task<bool> AddNew(CrCasLessorShomoosConnect model);
        Task<bool> Update(CrCasLessorShomoosConnect model);
    }
}
