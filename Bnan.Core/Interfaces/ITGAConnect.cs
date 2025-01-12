using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface ITGAConnect
    {
        Task<bool> AddDefault(string lessorCode);
        Task<bool> AddNew(CrCasLessorTgaConnect model);
        Task<bool> Update(CrCasLessorTgaConnect model);
    }
}
