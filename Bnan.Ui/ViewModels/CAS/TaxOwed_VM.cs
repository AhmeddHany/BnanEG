using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
    public class TaxOwed_VM
    {
        public List<List<string>>? Contract_Count { get; set; }

        public decimal Add_AllInvoices_Value_new = 0.0m;
        public decimal Add_Percentage_OfTax = 0.0m;
        public decimal Total_TaxValue = 0.0m;
        public int invoices_Count = 0;

        public string? New_TaxOwed_Tax_no { get; set; }
        public string? Quarter { get; set; }
        public int? Invoices_Count = 0;
        public string? pay_User { get; set; }
        public string? Amount_will_Pay { get; set; }
        public string? All_Tax_eg { get; set; }
        public List<CrCasAccountContractTaxOwed>? CrCasAccountContractTaxOwed { get; set; }
        public List<CrCasAccountContractTaxOwed>? CrCasAccountContractTaxOwed_Filtered { get; set; }
        public List<CrCasSysAdministrativeProcedure>? CrCasSysAdministrativeProcedure { get; set; }
        public CrCasSysAdministrativeProcedure? CrCasSysAdministrativeProcedure_Data { get; set; }

        public List<CrCasAccountInvoice>? CrCasAccountInvoice = new List<CrCasAccountInvoice>();

    }
}
