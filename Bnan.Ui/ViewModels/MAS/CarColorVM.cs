using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarColorVM
    {
        public string CrMasSupCarColorCode { get; set; } = null!;
        //public string? CrMasSupCarColorGroup { get; set; }
        
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupCarColorArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupCarColorEnName { get; set; }
        public string? CrMasSupCarColorStatus { get; set; }
        public string? CrMasSupCarColorReasons { get; set; }

        public virtual CrMasSysGroup? CrMasSupCarColorGroupNavigation { get; set; }
    }
}
