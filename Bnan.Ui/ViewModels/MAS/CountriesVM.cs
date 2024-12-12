using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CountriesVM
    {
        public string CrMasSysCallingKeysCode { get; set; } = null!;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSysCallingKeysNaqlCode { get; set; } = 0;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSysCallingKeysNaqlId { get; set; } = 0;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string CrMasSysCallingKeysArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string CrMasSysCallingKeysEnName { get; set; }

        public string? CrMasSysCallingKeysStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSysCallingKeysReasons { get; set; }


        [Required(ErrorMessage = "requiredFiled"), Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10"), MaxLength(10, ErrorMessage = "requiredNoLengthFiled10")]
        public string? CrMasSysCallingKeysNo { get; set; }
        [Required(ErrorMessage = "requiredFiled"), Range(1, 9, ErrorMessage = "requiredFiled"), MaxLength(1, ErrorMessage = "requiredFiled")]
        public string? CrMasSysCallingKeysClassificationCode { get; set; }
        public long? CrMasSysCallingKeysCount { get; set; }
        public string? CrMasSysCallingKeysFlag { get; set; }


        public List<CrMasSysCallingKey> Countries = new List<CrMasSysCallingKey>();

        public List<TResult2>? Country_count = new List<TResult2>();

        public List<CrMasSupCountryClassification>? crMasSupCountryClassificationSS = new List<CrMasSupCountryClassification>();


    }
}
