using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    //public class Date_ReportFTPemployeeVM
    //{
    //    public DateTime? dates { get; set; }
    //}
    public class MAS_Contract_TypeVM
    {
        public string? Contract_Code { get; set; }
        public string? Type_Id { get; set; }
    }
    public class MasStatistics_ContractsVM
    {

        public List<MAS_Contract_TypeVM> all_Contracts_Type = new List<MAS_Contract_TypeVM>();
        public List<MAS_Contract_TypeVM> all_Contracts_Type_distinct = new List<MAS_Contract_TypeVM>();

        public List<MASChartBranchDataVM> listMasChartdataVM = new List<MASChartBranchDataVM>();
        public List<list_String_4> all_names = new List<list_String_4>();


        public decimal? Contracts_Count { get; set; } = 0;

        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string thisFunctionRunned { get; set; }
    }
   
   
}
