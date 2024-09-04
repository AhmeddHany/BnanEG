using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CompanyDues_VM
    {
        public List<List<string>>? Contract_Count { get; set; }

        public int Amount_will_Pay = 0;
        public string New_TaxOwed_Tax_no = "";


        //////////////////

        public string? Company_ContractNo { get; set; }

        public List<CrMasLessorInformation>? CrMasLessorInformation { get; set; }
        public List<CrCasAccountContractCompanyOwed>? CrCasAccountContractCompanyOwed { get; set; }
        
        public List<string> Drop_Conracts_List = new List<string> {" "," "};


        //////////////////



        public List<CrCasAccountContractTaxOwed>? CrCasAccountContractTaxOwed { get; set; }
        public List<CrCasAccountContractTaxOwed>? CrCasAccountContractTaxOwed_Filtered { get; set; }
        public List<CrCasSysAdministrativeProcedure>? CrCasSysAdministrativeProcedure { get; set; }
        public CrCasSysAdministrativeProcedure? CrCasSysAdministrativeProcedure_Data { get; set; }
    }
}
