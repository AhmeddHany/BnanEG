using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterDrivingLicenseVM
    {
        public string CRMasSupRenterDrivingLicenseCode { get; set; } = null!;
        
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CRMasSupRenterDrivingLicenseArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CRMasSupRenterDrivingLicenseEnName { get; set; }
        public string? CRMasSupRenterDrivingLicenseStatus { get; set; }
        public string? CRMasSupRenterDrivingLicenseReasons { get; set; }

    }
}
