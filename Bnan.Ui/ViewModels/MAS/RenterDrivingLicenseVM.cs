using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterDrivingLicenseVM
    {
        public string CrMasSupRenterDrivingLicenseCode { get; set; } = null!;
        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupRenterDrivingLicenseNaqlCode { get; set; } = 0;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupRenterDrivingLicenseNaqlId { get; set; } = 0;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupRenterDrivingLicenseArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupRenterDrivingLicenseEnName { get; set; }
        public string? CrMasSupRenterDrivingLicenseStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterDrivingLicenseReasons { get; set; }
        public int? RentersHave_withType_Count { get; set; }

    }
}
