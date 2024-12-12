using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bnan.Ui.ViewModels.MAS
{
    public class IsValidLocation : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || !(value is string))
            {
                return false;
            }

            string input = (string)value;

            // Ensure input is numeric and has maximum length of 25
            if (!Regex.IsMatch(input, @"^\d{1,4}(\.\d{1,20})?$") || input.Length > 25)
            {
                return false;
            }

            return true;
        }
    }
    public class PostRegionsVM
    {
        public string CrMasSupPostRegionsCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string CrMasSupPostRegionsArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string CrMasSupPostRegionsEnName { get; set; }

        [ Range(0, 9999999999.99, ErrorMessage = "requiredNoLengthFiled10_decimal")]
        public decimal? CrMasSupPostRegionsLongitude { get; set; }
        [ Range(0, 9999999999.99, ErrorMessage = "requiredNoLengthFiled10_decimal")]
        public decimal? CrMasSupPostRegionsLatitude { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        public string? CrMasSupPostRegionsLocation { get; set; }
        public string? CrMasSupPostRegionsStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupPostRegionsReasons { get; set; }


        public List<CrMasSupPostRegion> PostRegions = new List<CrMasSupPostRegion>();

        public List<TResult2>? Region_count = new List<TResult2>();
        public List<TResult2>? cites_count = new List<TResult2>();

        
    }
}
// [IsValidLocation(ErrorMessage = "PostLongRequired")]     
//[IsValidLocation(ErrorMessage = "PostLatRequired")]
