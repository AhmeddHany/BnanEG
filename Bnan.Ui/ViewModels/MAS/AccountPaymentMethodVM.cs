using Bnan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class AccountPaymentMethodVM
    {
        [Required(ErrorMessage = "requiredFiled"),MaxLength(2, ErrorMessage = "requiredFiled"), Range(1, 99, ErrorMessage = "requiredFiled")]
        public string? CrMasSupAccountPaymentMethodCode { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(1, ErrorMessage = "requiredFiled"), Range(1, 7, ErrorMessage = "requiredFiled")]
        public string? CrMasSupAccountPaymentMethodClassification { get; set; }

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupAccountPaymentMethodNaqlCode { get; set; } = 0;
        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupAccountPaymentMethodNaqlId { get; set; } = 0;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupAccountPaymentMethodArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupAccountPaymentMethodEnName { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupAccountPaymentMethodAcceptImage { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupAccountPaymentMethodRejectImage { get; set; }
        public string? CrMasSupAccountPaymentMethodStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupAccountPaymentMethodReasons { get; set; }

        public List<CrMasSupAccountPaymentMethod> crMasSupAccountPaymentMethod = new List<CrMasSupAccountPaymentMethod>();

        public List<CrMasSupCountryClassification>? crMasSupCountryClassificationSS = new List<CrMasSupCountryClassification>();

        //public List<TResult2>? Banks_count = new List<TResult2>();  new []{ "CrCasAccountReceipts" }
    }
}
