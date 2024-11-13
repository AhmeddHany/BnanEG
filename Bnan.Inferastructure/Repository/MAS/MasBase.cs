using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;


namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasBase : IMasBase
    {
        public IUnitOfWork _unitOfWork;

        public MasBase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

    }
}
