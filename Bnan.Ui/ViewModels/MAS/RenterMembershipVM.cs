using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterMembershipVM
    {
        public string CrMasSupRenterMembershipCode { get; set; } = null!;

        public string? CrMasSupRenterMembershipGroupCode = "16";

        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupRenterMembershipArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupRenterMembershipEnName { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterMembershipAcceptPicture { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterMembershipRejectPicture { get; set; }
        public string? CrMasSupRenterMembershipStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterMembershipReasons { get; set; }

        public virtual CrMasSysGroup? CrMasSupRenterMembershipGroupCodeNavigation { get; set; }
    }
}
