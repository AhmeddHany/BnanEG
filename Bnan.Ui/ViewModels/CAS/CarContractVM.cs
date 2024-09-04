using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class CarContractVM
    {
        public List<CrCasRenterContractBasic>? crCasRenterContractBasics { get; set; }
        public List<CrCasCarInformation>? crCasCarInformation { get; set; }
        public List<CrMasUserInformation>? crMasUserInformation { get; set; }

        public List<List<string>>? contracts_Counts = new List<List<string>>();
        public List<CrCasRenterContractBasic>? ContractBasic_Filtered { get; set; }

        public List<CrCasRenterContractStatistic> crCasRenterContractStatistic = new List<CrCasRenterContractStatistic>();

        public List<CrCasAccountInvoice>? CrCasAccountInvoice_308 = new List<CrCasAccountInvoice>();

        public List<CrCasAccountInvoice>? CrCasAccountInvoice_309 = new List<CrCasAccountInvoice>();

        public List<CrCasCarAdvantage>? AdvantagesAll = new List<CrCasCarAdvantage>();



    }
}
