using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarCategoryVM
    {
        public string CrMasSupCarCategoryCode { get; set; } = null!;
        public string? CrMasSupCarCategoryGroup { get; set; } = "33";
        
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupCarCategoryArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupCarCategoryEnName { get; set; }
        public string? CrMasSupCarCategoryStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarCategoryReasons { get; set; }

        public virtual CrMasSysGroup? CrMasSupCarCategoryGroupNavigation { get; set; }

        public List<CrMasSupCarCategory> crMasSupCarCategory = new List<CrMasSupCarCategory>();

        public List<TResult2>? cars_count = new List<TResult2>();
    }
}
