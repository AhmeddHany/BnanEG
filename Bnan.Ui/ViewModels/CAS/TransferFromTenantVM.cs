using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
    public class TransferFromTenantVM
    {


        public IEnumerable<CrCasRenterLessor> renterLessor { get; set; }


        public List<CrMasSysEvaluation> crMasSysEvaluation = new List<CrMasSysEvaluation>();

    }
}
