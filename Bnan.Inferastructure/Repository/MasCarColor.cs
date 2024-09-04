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
    public class MasCarColor : IMasCarColor
    {
        
        public IUnitOfWork _unitOfWork;

        public MasCarColor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<List<string>> GetAllCarColorsCount()
        {
            List<List<string>> Counts_ids = new List<List<string>>();
            IEnumerable<CrMasSupCarColor?> Categorys = _unitOfWork.CrMasSupCarColor.GetAll();
            if (Categorys != null)
            {
                foreach (var item in Categorys)
                {
                    List<string> Counts = new List<string>();
                    int x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == item.CrMasSupCarColorCode);
                    if (x != null)
                    {
                        Counts.Add(item.CrMasSupCarColorCode);
                        Counts.Add(x.ToString());
                        Counts_ids.Add(Counts);
                    }
                }
            }

            return (Counts_ids);
        }

        public int GetOneCarColorCount(string id)
        {
            int x = 0;
            x = _unitOfWork.CrCasCarInformation.Count(l => l.CrCasCarInformationCategory == id);

            return x;
        }
    }
 }
