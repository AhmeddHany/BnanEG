using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    //public class Date_ReportFTPemployeeVM
    //{
    //    public DateTime? dates { get; set; }
    //}
    public class lessor_SMS_VM
    {
        public string? lessor_Code { get; set; }
        public string? sms_Api { get; set; }
        public string? sms_Name { get; set; }
        public string? sms_Status { get; set; }
    }
    public class Mas_CompanyMessages_VM
    {

        public List<CrMasLessorInformation> all_lessors = new List<CrMasLessorInformation>();
        public List<lessor_SMS_VM> all_status_sms = new List<lessor_SMS_VM>();
        //public List<Car_TypeVM> all_Cars_Type_distinct = new List<Car_TypeVM>();

        //public List<MASChartBranchDataVM> listMasChartdataVM = new List<MASChartBranchDataVM>();
        //public List<list_String_4> all_names = new List<list_String_4>();


        //public decimal? Cars_Count { get; set; } = 0;

        //public string start_Date { get; set; }
        //public string end_Date { get; set; }
        //public string UserId { get; set; }
    }


}
