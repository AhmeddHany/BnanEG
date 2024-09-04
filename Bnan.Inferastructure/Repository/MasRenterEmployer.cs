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
    public class MasRenterEmployer : IMasRenterEmployer
    {
        
        public IUnitOfWork _unitOfWork;

        public MasRenterEmployer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<List<string>> GetAllRenterEmployersCount()
        {
            List<List<string>> Counts_ids = new List<List<string>>();
            IEnumerable<CrMasSupRenterEmployer?> Categorys = _unitOfWork.CrMasSupRenterEmployer.GetAll();
            if (Categorys != null)
            {
                foreach (var item in Categorys)
                {
                    List<string> Counts = new List<string>();
                    int x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == item.CrMasSupRenterEmployerCode);
                    if (x != null)
                    {
                        Counts.Add(item.CrMasSupRenterEmployerCode);
                        Counts.Add(x.ToString());
                        Counts_ids.Add(Counts);
                    }
                }
            }

            return (Counts_ids);
        }

        public int GetOneRenterEmployerCount(string id)
        {
            int x = 0;
            x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == id);

            return x;
        }
    }
 }
