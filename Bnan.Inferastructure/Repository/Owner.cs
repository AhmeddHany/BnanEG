using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Repository
{
    public class Owner : IOwner
    {
        private IUnitOfWork _unitOfWork;

        public Owner(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddOwner(string LessorCode)
        {
            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(LessorCode);
            var lessorOwner = new CrCasOwner
            {
                CrCasOwnersCode = lessor.CrMasLessorInformationGovernmentNo,
                CrCasOwnersLessorCode = lessor.CrMasLessorInformationCode,
                CrCasOwnersArName = lessor.CrMasLessorInformationArLongName,
                CrCasOwnersEnName = lessor.CrMasLessorInformationEnLongName,
                CrCasOwnersStatus = Status.Active
            };
            await _unitOfWork.CrCasOwners.AddAsync(lessorOwner);
            return true;
        }

        public async Task<bool> AddOwnerInCas(CrCasOwner model)
        {

            CrCasOwner crCasOwner = new CrCasOwner()
            {
                CrCasOwnersCode=model.CrCasOwnersCode,
                CrCasOwnersLessorCode = model.CrCasOwnersLessorCode,
                CrCasOwnersArName = model.CrCasOwnersArName,
                CrCasOwnersEnName=model.CrCasOwnersEnName,
                CrCasOwnersStatus=Status.Active,
                CrCasOwnersReasons= model.CrCasOwnersReasons
            };
            await _unitOfWork.CrCasOwners.AddAsync(crCasOwner);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateOwnerInCas(CrCasOwner model)
        {

            var crCasOwner = await _unitOfWork.CrCasOwners.FindAsync(x => x.CrCasOwnersCode == model.CrCasOwnersCode);
            crCasOwner.CrCasOwnersArName = model.CrCasOwnersArName;
            crCasOwner.CrCasOwnersEnName = model.CrCasOwnersEnName;
            crCasOwner.CrCasOwnersReasons = model.CrCasOwnersReasons;
            
            _unitOfWork.CrCasOwners.Update(crCasOwner);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
