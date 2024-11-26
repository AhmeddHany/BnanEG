using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CurrencyVM
    {
        public string CrMasSysConvertNoToTextCode { get; set; }
        public string? CrMasSysConvertNoToTextType { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSysConvertNoToTextArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSysConvertNoToTextEnName { get; set; }
        public string? CrMasSysConvertNoToTextStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSysConvertNoToTextReasons { get; set; }
    }
    public class CurrencyVVM
    {
        public List<CurrencyVM> Cuurency_full { get; set; } = new List<CurrencyVM>();
        public List<CurrencyVM> Cuurency_ratio { get; set; } = new List<CurrencyVM>();
    }
}
