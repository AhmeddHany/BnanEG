using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
    public class RenterContractEditVM
    {
        
        public CasRenterContractVM CasRenterContractVM { get; set; } = new CasRenterContractVM();
        public List<CrCasRenterContractBasic> CrCasRenterContractBasic { get; set; } = new List<CrCasRenterContractBasic>();
        public List<CrMasSysEvaluation> CrMasSysEvaluation { get; set; } = new List<CrMasSysEvaluation>();
        public List<CrCasAccountInvoice> CrCasAccountInvoice { get; set; } = new List<CrCasAccountInvoice>();


    }
}
