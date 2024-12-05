using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterIdtypeVM
    {
        public string CrMasSupRenterIdtypeCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupRenterIdtypeArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupRenterIdtypeEnName { get; set; }
        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public Int64? CrMasSupRenterIdtypeNaqlCode { get; set; }
        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public Int64? CrMasSupRenterIdtypeNaqlId { get; set; }
        public string? CrMasSupRenterIdtypeStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterIdtypeReasons { get; set; }
        public int? RentersHave_withType_Count { get; set; }

    }
}
