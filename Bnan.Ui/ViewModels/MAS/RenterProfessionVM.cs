using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterProfessionVM
    {
        public string CRMasSupRenterProfessionsCode { get; set; } = null!;
        
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CRMasSupRenterProfessionsArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        
        public string? CRMasSupRenterProfessionsGroupCode = "14";
        public string? CRMasSupRenterProfessionsEnName { get; set; }
        public string? CRMasSupRenterProfessionsStatus { get; set; }
        public string? CRMasSupRenterProfessionsReasons { get; set; }

    }
}
