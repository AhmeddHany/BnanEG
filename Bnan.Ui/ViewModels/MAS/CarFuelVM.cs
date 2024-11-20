using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarFuelVM
    {
        public string CrMasSupCarFuelCode { get; set; } = null!;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupCarFuelNaqlCode { get; set; } = 0;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupCarFuelNaqlId { get; set; } = 0;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(40, ErrorMessage = "requiredNoLengthFiled40")]
        public string CrMasSupCarFuelArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(40, ErrorMessage = "requiredNoLengthFiled40")]
        public string CrMasSupCarFuelEnName { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarFuelImage { get; set; }
        public string? CrMasSupCarFuelStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]

        public string? CrMasSupCarFuelReasons { get; set; }


        public List<CrMasSupCarFuel> crMasSupCarFuel = new List<CrMasSupCarFuel>();

        //public List<CrCasCarInformation> cars_count = new List<CrCasCarInformation>();
        public List<TResult2>? cars_count = new List<TResult2>();

    }
}
