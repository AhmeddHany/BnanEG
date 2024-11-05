using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterDrivingLicenseVM
    {
        public string CrMasSupRenterDrivingLicenseCode { get; set; } = null!;
        [Required(ErrorMessage = "requiredFiled")]
        public int? CrMasSupRenterDrivingLicenseNaqlCode { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public int? CrMasSupRenterDrivingLicenseNaqlId { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupRenterDrivingLicenseArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupRenterDrivingLicenseEnName { get; set; }
        public string? CrMasSupRenterDrivingLicenseStatus { get; set; }
        [MaxLength(50, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterDrivingLicenseReasons { get; set; }

    }
}
