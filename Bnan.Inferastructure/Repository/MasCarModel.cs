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
    public class MasCarModel : IMasCarModel
    {
        
        public IUnitOfWork _unitOfWork;

        public MasCarModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<List<string>> GetAllCarModelsCount()
        {
            List<List<string>> Counts_ids = new List<List<string>>();
            IEnumerable<CrMasSupCarModel?> Categorys = _unitOfWork.CrMasSupCarModel.GetAll();
            if (Categorys != null)
            {
                foreach (var item in Categorys)
                {
                    List<string> Counts = new List<string>();
                    int x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == item.CrMasSupCarModelCode);
                    if (x != null)
                    {
                        Counts.Add(item.CrMasSupCarModelCode);
                        Counts.Add(x.ToString());
                        Counts_ids.Add(Counts);
                    }
                }
            }

            return (Counts_ids);
        }

        public int GetOneCarModelCount(string id)
        {
            int x = 0;
            x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == id);

            return x;
        }
    }
 }
