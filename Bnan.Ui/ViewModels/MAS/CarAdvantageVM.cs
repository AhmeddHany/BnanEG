using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarAdvantageVM
    {
        public string CrMasSupCarAdvantagesCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarAdvantagesArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarAdvantagesEnName { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarAdvantagesImage { get; set; }
        public string? CrMasSupCarAdvantagesStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarAdvantagesReasons { get; set; }
        public int? RentersHave_withType_Count { get; set; }
       
        public List<CrMasSupCarAdvantage> crMasSupCarAdvantage = new List<CrMasSupCarAdvantage>();

        public List<TResult2>? advantages_count = new List<TResult2>();
    }
}
