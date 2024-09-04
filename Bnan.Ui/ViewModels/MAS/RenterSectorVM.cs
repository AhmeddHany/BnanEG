using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterSectorVM
    {      
        public string CrMasSupRenterSectorCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupRenterSectorArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupRenterSectorEnName { get; set; }
        public string? CrMasSupRenterSectorStatus { get; set; }
        public string? CrMasSupRenterSectorReasons { get; set; }


    }
}
