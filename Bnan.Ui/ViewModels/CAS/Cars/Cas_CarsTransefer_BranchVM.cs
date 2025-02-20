using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS.Cars
{
    public class Cas_CarsTransefer_BranchVM
    {
        public List<CrCasCarInformation>? all_CarsData { get; set; }
        public List<CrCasBranchInformation>? all_BranchesData { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(10, ErrorMessage = "requiredNoLengthFiled10")]
        public string? brnachCode { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? Reasons { get; set; }

    }
    public class CarDetails_Transefer
    {
        public string? serialNo { get; set; }
        public string? customNo { get; set; }
        
        public string? carImage { get; set; }
        public string? carNameAr { get; set; }
        public string? carNameEn { get; set; }
        public string? branchNameAr { get; set; }
        public string? branchNameEn { get; set; }
        public string? branchId { get; set; }
        public string? reasons { get; set; }
    }
    

}
