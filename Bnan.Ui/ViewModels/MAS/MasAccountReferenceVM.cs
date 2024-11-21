using Bnan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class MasAccountReferenceVM
    {
        public string CrMasSupAccountReceiptReferenceCode { get; set; } = null!;
        [Required(ErrorMessage = "requiredFiled"), MaxLength(40, ErrorMessage = "requiredNoLengthFiled40")]

        public string? CrMasSupAccountReceiptReferenceArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(40, ErrorMessage = "requiredNoLengthFiled40")]

        public string? CrMasSupAccountReceiptReferenceEnName { get; set; }
        public string? CrMasSupAccountPaymentMethodStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupAccountPaymentMethodReasons { get; set; }

        public List<CrMasSupAccountReference> crMasSupAccountReference = new List<CrMasSupAccountReference>();

        public List<TResult2>? AccountReferences_count = new List<TResult2>();
    }
}
