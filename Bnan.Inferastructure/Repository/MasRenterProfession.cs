using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using System;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Repository
{
    public class MasRenterProfession : IMasRenterProfession
    {
        
        public IUnitOfWork _unitOfWork;

        public MasRenterProfession(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<List<string>> GetAllRenterProfessionsCount()
        {
            List<List<string>> Counts_ids = new List<List<string>>();
            IEnumerable<CrMasSupRenterProfession?> Categorys = _unitOfWork.CrMasSupRenterProfession.GetAll();
            if (Categorys != null)
            {
                foreach (var item in Categorys)
                {
                    List<string> Counts = new List<string>();
                    int x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == item.CrMasSupRenterProfessionsCode);
                    if (x != null)
                    {
                        Counts.Add(item.CrMasSupRenterProfessionsCode);
                        Counts.Add(x.ToString());
                        Counts_ids.Add(Counts);
                    }
                }
            }

            return (Counts_ids);
        }

        public int GetOneRenterProfessionCount(string id)
        {
            int x = 0;
            x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == id);

            return x;
        }
    }
 }
