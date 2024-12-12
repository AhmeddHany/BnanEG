using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterNationalityVM
    {
        public string CrMasSupRenterNationalitiesCode { get; set; } = null!;
        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupRenterNationalitiesNaqlCode { get; set; } = 0;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupRenterNationalitiesNaqlId { get; set; } = 0;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupRenterNationalitiesArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupRenterNationalitiesEnName { get; set; }
        public string? CrMasSupRenterNationalitiesStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterNationalitiesReasons { get; set; }
        public int? RentersHave_withType_Count { get; set; }

        public string? CrMasSupRenterNationalitiesGroupCode { get; set; } = "10";
        [Required(ErrorMessage = "requiredFiled"), Range(1, 9, ErrorMessage = "requiredFiled")]
        public string? CrMasSupRenterNationalitiesNaqlGcc { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterNationalitiesFlag { get; set; }
        public int? CrMasSupRenterNationalitiesCounter { get; set; }

        public List<CrMasSupRenterNationality> List_Nationality = new List<CrMasSupRenterNationality>();

        public List<TResult2>? Nationality_count_1 = new List<TResult2>();

        public List<TResult2>? Nationality_count_2 = new List<TResult2>();

        public List<CrMasSupCountryClassification>? crMasSupCountryClassificationSS = new List<CrMasSupCountryClassification>();

        public virtual CrMasSysGroup? CrMasSupRenterNationalitiesGroupCodeNavigation { get; set; }
    }
}
