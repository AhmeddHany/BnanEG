using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bnan.Ui.ViewModels.MAS
{
    public class PostCityVM
    {
        public string CrMasSupPostCityCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string CrMasSupPostCityArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string CrMasSupPostCityEnName { get; set; }

        [ Range(0, 9999999999.99, ErrorMessage = "requiredNoLengthFiled10_decimal")]
        public decimal? CrMasSupPostCityLongitude { get; set; }
        [ Range(0, 9999999999.99, ErrorMessage = "requiredNoLengthFiled10_decimal")]
        public decimal? CrMasSupPostCityLatitude { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        public string? CrMasSupPostCityLocation { get; set; }
        public string? CrMasSupPostCityStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupPostCityReasons { get; set; }


        public string? CrMasSupPostCityGroupCode { get; set; } = "17";
        [Required(ErrorMessage = "requiredFiled"), MaxLength(2, ErrorMessage = "requiredFiled")]
        public string? CrMasSupPostCityRegionsCode { get; set; }

        public string? CrMasSupPostCityConcatenateArName { get; set; }
        public string? CrMasSupPostCityConcatenateEnName { get; set; }

        public int? CrMasSupPostCityCounter { get; set; }
        public string? CrMasSupPostCityRegionsStatus { get; set; }



        public List<CrMasSupPostCity> PostCity = new List<CrMasSupPostCity>();

        public List<CrMasSupPostRegion> Regions = new List<CrMasSupPostRegion>();

        public List<TResult2>? City_count = new List<TResult2>();

    }
}
// [IsValidLocation(ErrorMessage = "PostLongRequired")]     
//[IsValidLocation(ErrorMessage = "PostLatRequired")]
