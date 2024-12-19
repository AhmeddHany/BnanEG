using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
    public class ReportActiveContractVM2
    {
        public List<CrCasRenterContractBasic>? crCasRenterContractBasic = new List<CrCasRenterContractBasic>();

        public List<CrCasRenterContractAlert>? crCasRenterContractAlert = new List<CrCasRenterContractAlert>();

        public List<(string, string?, string?)>? All_users = new List<(string, string?, string?)>();

        public List<(string?, string?, string?, string)>? All_Invoices = new List<(string?, string?, string?, string)>();


    }
}
