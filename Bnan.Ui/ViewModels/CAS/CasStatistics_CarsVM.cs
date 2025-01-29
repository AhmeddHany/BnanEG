using Bnan.Core.Models;
using Bnan.Ui.ViewModels.CAS;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    //public class Date_ReportFTPemployeeVM
    //{
    //    public DateTime? dates { get; set; }
    //}
    public class CAS_Car_TypeVM
    {
        public string? Car_Code { get; set; }
        public string? Type_Id { get; set; }
    }

    public class CAS_Car_Type_ForStatus_VM
    {
        public string? Car_Code { get; set; }
        public string? Type_Id { get; set; }
        public bool? CrCasCarInformationDocumentationStatus { get; set; }
        public bool? CrCasCarInformationMaintenanceStatus { get; set; }
        public bool? CrCasCarInformationPriceStatus { get; set; }
        public string? CrCasCarInformationBranchStatus { get; set; }
        public string? CrCasCarInformationOwnerStatus { get; set; }
        public string? CrCasCarInformationForSaleStatus { get; set; }
        public string? CrCasCarInformationStatus { get; set; }
    }

    public class CasStatistics_CarsVM
    {

        public List<CAS_Car_TypeVM> all_Cars_Type = new List<CAS_Car_TypeVM>();
        public List<CAS_Car_TypeVM> all_Cars_Type_distinct = new List<CAS_Car_TypeVM>();

        public List<CASChartBranchDataVM> listCasChartdataVM = new List<CASChartBranchDataVM>();
        public List<cas_list_String_4> all_names = new List<cas_list_String_4>();


        public decimal? count_Contracts { get; set; } = 0;
        public decimal? Cars_Count { get; set; } = 0;
        public decimal? Renters_count { get; set; } = 0;

        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string UserId { get; set; }
    }
   
   
}
