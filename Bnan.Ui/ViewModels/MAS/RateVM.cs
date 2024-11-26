using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RateVM
    {
        public string CrMasSysEvaluationsCode { get; set; }
        public string? CrMasSysEvaluationsClassification { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSysEvaluationsArDescription { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSysEvaluationsEnDescription { get; set; }
        [Range(0, 100, ErrorMessage = "requiredFiled"), Required(ErrorMessage = "requiredFiled")]
        public int? CrMasSysServiceEvaluationsValue { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSysEvaluationsImage { get; set; }
        public string? CrMasSysEvaluationsStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSysEvaluationsReasons { get; set; }

    }
    public class RateVVM
    {
        public List<RateVM> renter_Rates { get; set; } = new List<RateVM>();
        public List<RateVM> lessor_Rates { get; set; } = new List<RateVM>();
    }
}
