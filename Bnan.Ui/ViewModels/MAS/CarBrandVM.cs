using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;


namespace Bnan.Ui.ViewModels.MAS
{
    public class CarBrandVM
    {
        public string CrMasSupCarBrandCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string CrMasSupCarBrandArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarBrandEnName { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarBrandImage { get; set; }
        public string? CrMasSupCarBrandStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarBrandReasons { get; set; }

        public List<CrMasSupCarBrand> crMasSupCarBrand = new List<CrMasSupCarBrand>();

        public List<TResult2>? cars_count = new List<TResult2>();

        public List<TResult2>? models_count = new List<TResult2>();
    }
}
