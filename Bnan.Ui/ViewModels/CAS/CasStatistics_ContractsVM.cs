using Bnan.Core.Models;
using Bnan.Ui.ViewModels.CAS;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    //public class Date_ReportFTPemployeeVM
    //{
    //    public DateTime? dates { get; set; }
    //}
    public class CAS_Contract_TypeVM
    {
        public string? Contract_Code { get; set; }
        public string? Type_Id { get; set; }
    }
    public class CasStatistics_ContractsVM
    {

        public List<CAS_Contract_TypeVM> all_Contracts_Type = new List<CAS_Contract_TypeVM>();
        public List<CAS_Contract_TypeVM> all_Contracts_Type_distinct = new List<CAS_Contract_TypeVM>();

        public List<CASChartBranchDataVM> listCasChartdataVM = new List<CASChartBranchDataVM>();
        public List<cas_list_String_4> all_names = new List<cas_list_String_4>();


        public decimal? Contracts_Count { get; set; } = 0;

        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string thisFunctionRunned { get; set; }
    }
   
   
}
