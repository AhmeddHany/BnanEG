using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class LessorClassificationVM
    {      
        public string CrCasLessorClassificationCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrCasLessorClassificationArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrCasLessorClassificationEnName { get; set; }
        public string? CrMasLessorClassificationStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasLessorClassificationReasons { get; set; }

        //    "CrMasLessorInformations"
    }
}
