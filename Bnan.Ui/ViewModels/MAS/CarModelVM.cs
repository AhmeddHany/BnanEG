using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarModelVM
    {      

        public string CrMasSupCarModelCode { get; set; } = null!;

        public string? CrMasSupCarModelGroup = "31";
        public string? CrMasSupCarModelBrand { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarModelArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarModelEnName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(60, ErrorMessage = "requiredNoLengthFiled60")]
        public string? CrMasSupCarModelArConcatenateName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(60, ErrorMessage = "requiredNoLengthFiled60")]
        public string? CrMasSupCarModelConcatenateEnName { get; set; }
        public int? CrMasSupCarModelCounter { get; set; }
        public string? CrMasSupCarModelStatus { get; set; }
        public string? CrMasSupCarModelReasons { get; set; }

        public List<CrMasSupCarBrand> All_Brands = new List<CrMasSupCarBrand>();

        public virtual CrMasSupCarBrand? CrMasSupCarModelBrandNavigation { get; set; }
        public virtual CrMasSysGroup? CrMasSupCarModelGroupNavigation { get; set; }



    }
}
