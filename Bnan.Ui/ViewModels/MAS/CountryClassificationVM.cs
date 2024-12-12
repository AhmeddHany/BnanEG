using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CountryClassificationVM
    {      
        public string CrMasLessorCountryClassificationCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasLessorCountryClassificationArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasLessorCountryClassificationEnName { get; set; }
        public string? CrMasLessorCountryClassificationStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasLessorCountryClassificationReasons { get; set; }

        public List<CrMasSupCountryClassification> Countries = new List<CrMasSupCountryClassification>();

        public List<TResult2>? Country_count = new List<TResult2>();


    }
}
