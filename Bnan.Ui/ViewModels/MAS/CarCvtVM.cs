using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarCvtVM
    {
        public string CrMasSupCarCvtCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarCvtArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarCvtEnName { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarCvtImage { get; set; }
        public string? CrMasSupCarCvtStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarCvtReasons { get; set; }


        public List<CrMasSupCarCvt> crMasSupCarCvt = new List<CrMasSupCarCvt>();

        public List<TResult2>? cars_count = new List<TResult2>();
    }
}
