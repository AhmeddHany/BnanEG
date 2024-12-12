using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface ICarDistribution
    {
        Task<bool> AddCarDisribtion(CrMasSupCarDistribution crMasSupCarDistribution);
        Task<bool> UpdateCarDisribtion(CrMasSupCarDistribution crMasSupCarDistribution);
    }
}
