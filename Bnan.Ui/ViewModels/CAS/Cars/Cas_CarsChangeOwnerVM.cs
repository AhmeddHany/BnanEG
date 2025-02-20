using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS.Cars
{
    public class Cas_CarsChangeOwnerVM
    {
        public List<CrCasCarInformation>? all_CarsData { get; set; }
        public List<CrCasBranchInformation>? all_BranchesData { get; set; }
        public List<CrCasOwner>? all_OwnersData { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(10, ErrorMessage = "requiredNoLengthFiled10")]
        public string? brnachCode { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? Reasons { get; set; }

    }
    public class CarDetails_Owner
    {
        public string? serialNo { get; set; }
        public string? customNo { get; set; }
        
        public string? carImage { get; set; }
        public string? carNameAr { get; set; }
        public string? carNameEn { get; set; }
        public string? ownerNameAr { get; set; }
        public string? ownerNameEn { get; set; }
        public string? ownerId { get; set; }
        public string? reasons { get; set; }
    }
    

}
