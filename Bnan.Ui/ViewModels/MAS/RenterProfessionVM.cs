using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterProfessionVM
    {
        public string CrMasSupRenterProfessionsCode { get; set; } = null!;
        
        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupRenterProfessionsArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupRenterProfessionsEnName { get; set; }
        public string? CrMasSupRenterProfessionsStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterProfessionsReasons { get; set; }

        public string? CrMasSupRenterProfessionsGroupCode = "14";

        public int? RentersHave_withType_Count { get; set; }

    }
}
