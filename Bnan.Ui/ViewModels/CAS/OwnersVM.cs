using Bnan.Core.Models;
using Bnan.Ui.ViewModels.MAS;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class OwnersVM
    {
        [RegularExpression(@"^7\d{9}$", ErrorMessage = "requiredNoLengthFiled10")]
        [Required(ErrorMessage = "requiredFiled")]
        public string CrCasOwnersCode { get; set; } = null!;
        public string CrCasOwnersLessorCode { get; set; } = null!;
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasOwnersArName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasOwnersEnName { get; set; }
        public string? CrCasOwnersStatus { get; set; }
        public string? CrCasOwnersReasons { get; set; }

        public string? CrCasOwnersCountryKey { get; set; }
        public string? CrCasOwnersMobile { get; set; }
        public string? CrCasOwnersConnectStatus { get; set; }
        public string? CrCasOwnersEmail { get; set; }

    }
}
