using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterGenderVM
    {
        public string CrMasSupRenterGenderCode { get; set; } = null!;

        public string? CrMasSupRenterGenderGroupCode = "11";

        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupRenterGenderArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupRenterGenderEnName { get; set; }

        public string? CrMasSupRenterGenderStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterGenderReasons { get; set; }
        public virtual CrMasSysGroup? CrMasSupRenterGenderGroupCodeNavigation { get; set; }
    }
}
