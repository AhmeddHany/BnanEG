using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    //public class Date_ReportFTPemployeeVM
    //{
    //    public DateTime? dates { get; set; }
    //}
    public class MAS_Renter_TypeVM
    {
        public string? Renter_Code { get; set; }
        public string? Type_Id { get; set; }
    }
    public class MasStatistics_RentersVM
    {

        public List<MAS_Renter_TypeVM> all_Renters_Type = new List<MAS_Renter_TypeVM>();
        public List<MAS_Renter_TypeVM> all_Renters_Type_distinct = new List<MAS_Renter_TypeVM>();

        public List<MASChartBranchDataVM> listMasChartdataVM = new List<MASChartBranchDataVM>();
        public List<list_String_4> all_names = new List<list_String_4>();


        public decimal? Renters_Count { get; set; } = 0;

        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string UserId { get; set; }
    }
   
   
}
