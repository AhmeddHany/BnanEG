using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarColorVM
    {
        public string CrMasSupCarColorCode { get; set; } = null!;
        //public string? CrMasSupCarColorGroup { get; set; }
        
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupCarColorArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupCarColorEnName { get; set; }
        public string? CrMasSupCarColorStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarColorReasons { get; set; }

        public int? CrMasSupCarColorCounter { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarColorImage { get; set; }


        public virtual CrMasSysGroup? CrMasSupCarColorGroupNavigation { get; set; }

        public List<CrMasSupCarColor> crMasSupCarColor = new List<CrMasSupCarColor>();

        public List<TResult2>? cars_count = new List<TResult2>();
    }
}
