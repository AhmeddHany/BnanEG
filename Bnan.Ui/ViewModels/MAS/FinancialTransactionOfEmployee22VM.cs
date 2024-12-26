using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class FinancialTransactionOfEmployee22VM
    {
        public List<CrCasAccountReceipt>? crCasAccountReceipt { get; set; }
        public List<CrCasAccountSalesPoint>? crCasAccountSalesPoint { get; set; }
        public List<CrMasUserInformation>? crMasUserInformation { get; set; }
        public List<CrCasRenterContractBasic>? crCasRenterContractBasics { get; set; }

        public List<List<string>>? All_Counts = new List<List<string>>(); 
        public List<CrCasAccountReceipt>? FinancialTransactionOfEmployee_Filtered { get; set; }


    }
}
