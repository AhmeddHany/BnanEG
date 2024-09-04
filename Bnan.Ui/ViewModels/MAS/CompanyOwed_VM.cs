using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CompanyOwed_VM
    {
        public List<List<string>>? Contract_Count { get; set; }

        public int Amount_will_Pay = 0;
        public string? New_CompanyOwed_Tax_no { get; set; }

        public string? Company_ContractNo { get; set; }

        public List<CrMasLessorInformation>? CrMasLessorInformation { get; set; }

        public List<string> Drop_Conracts_List = new List<string> { " ", " " };

        public List<CrCasAccountContractCompanyOwed>? CrCasAccountContractCompanyOwed { get; set; }
        public List<CrCasAccountContractCompanyOwed>? CrCasAccountContractCompanyOwed_Filtered { get; set; }
        public List<CrCasSysAdministrativeProcedure>? CrCasSysAdministrativeProcedure { get; set; }
        public CrCasSysAdministrativeProcedure? CrCasSysAdministrativeProcedure_Data { get; set; }
    }
}
