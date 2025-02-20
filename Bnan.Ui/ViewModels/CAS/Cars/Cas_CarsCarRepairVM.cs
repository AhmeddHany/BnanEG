using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS.Cars
{
    public class Cas_CarsCarRepairVM
    {
        public List<CrCasCarInformation>? all_CarsData { get; set; }
        public List<CrCasBranchInformation>? all_BranchesData { get; set; }
        public List<(string,string, string)>? all_StatusData { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(1, ErrorMessage = "requiredNoLengthFiled10")]
        public string? StatusCode { get; set; }
        public string? minDate { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(11, ErrorMessage = "requiredNoLengthFiled10")]
        public string? DateRepair { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? Reasons { get; set; }

    }
    public class CarDetails_CarRepair
    {
        public string? serialNo { get; set; }
        public string? customNo { get; set; }
        
        public string? carImage { get; set; }
        public string? carNameAr { get; set; }
        public string? carNameEn { get; set; }
        public string? statusNameAr { get; set; }
        public string? statusNameEn { get; set; }
        public string? statusId { get; set; }
        public string? reasons { get; set; }
        public string? minDate { get; set; }
        public string? maxDate { get; set; }
    }
    

}
