using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    //public class Date_ReportFTPemployeeVM
    //{
    //    public DateTime? dates { get; set; }
    //}
    public class Contract_TypeVM
    {
        public string? Contract_Code { get; set; }
        public string? Type_Id { get; set; }
    }
    public class MasStatistics_ContractsVM
    {

        public List<Contract_TypeVM> all_Contracts_Type = new List<Contract_TypeVM>();
        public List<Contract_TypeVM> all_Contracts_Type_distinct = new List<Contract_TypeVM>();

        public List<MASChartBranchDataVM> listMasChartdataVM = new List<MASChartBranchDataVM>();
        public List<list_String_4> all_names = new List<list_String_4>();


        public decimal? Contracts_Count { get; set; } = 0;

        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string UserId { get; set; }
    }
   
   
}
