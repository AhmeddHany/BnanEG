using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS.Renters
{
    //public class Date_ReportFTPemployeeVM
    //{
    //    public DateTime? dates { get; set; }
    //}
    public class Renter_SMS_VM
    {
        public string? Renter_Code { get; set; }
        public string? sms_Api { get; set; }
        public string? sms_Name { get; set; }
        public string? sms_Status { get; set; }
    }
    public class Cas_RenterMessages_VM
    {

        public List<CrCasRenterLessor>? all_Renters = new List<CrCasRenterLessor>();
        public List<Renter_SMS_VM>? all_status_sms = new List<Renter_SMS_VM>();
        public List<CrMasSysEvaluation>? Rates = new List<CrMasSysEvaluation>();
        public CrMasLessorInformation? thisCompanyData { get; set; }

        //public List<Car_TypeVM> all_Cars_Type_distinct = new List<Car_TypeVM>();

        //public List<MASChartBranchDataVM> listMasChartdataVM = new List<MASChartBranchDataVM>();
        //public List<list_String_4> all_names = new List<list_String_4>();


        //public decimal? Cars_Count { get; set; } = 0;

        //public string start_Date { get; set; }
        //public string end_Date { get; set; }
        //public string UserId { get; set; }
    }


}
