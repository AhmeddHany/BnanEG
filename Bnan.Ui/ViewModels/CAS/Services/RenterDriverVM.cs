using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bnan.Ui.ViewModels.CAS.Services
{
    public class SelectListItem2 
    {
        public string? Value { get; set; }
        public string? text_Ar { get; set; }
        public string? text_En { get; set; }

    }
    public class RenterDriverVM
    {
        public string? ToDay { get; set; }
        public string? before7Y { get; set; }
        public string? after10Y { get; set; }
        public string? before100Y { get; set; }


        [Required(ErrorMessage = "requiredFiled"), MaxLength(10, ErrorMessage = "requiredNoLengthFiled1_2"), Range(1000000001, 2999999999, ErrorMessage = "requiredNoLengthFiled1_2")]
        public string? CrCasRenterPrivateDriverInformationId { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(4, ErrorMessage = "requiredFiled"), Range(4001, 4999, ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationLessor { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationIdtrype { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(110, ErrorMessage = "requiredNoLengthFiled110")]
        public string? CrCasRenterPrivateDriverInformationArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(110, ErrorMessage = "requiredNoLengthFiled110")]
        public string? CrCasRenterPrivateDriverInformationEnName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrCasRenterPrivateDriverInformationBirthDate { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrCasRenterPrivateDriverInformationIssueIdDate { get; set; }

        //[Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrCasRenterPrivateDriverInformationExpiryIdDate { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationLicenseNo { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationLicenseType { get; set; }

        [Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrCasRenterPrivateDriverInformationLicenseDate { get; set; }

        [Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrCasRenterPrivateDriverInformationLicenseExpiry { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationNationality { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationGender { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationKeyMobile { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrCasRenterPrivateDriverInformationMobile { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrCasRenterPrivateDriverInformationEmail { get; set; } = null;
        public DateTime? CrCasRenterPrivateDriverInformationLastContract { get; set; }
        public int? CrCasRenterPrivateDriverInformationContractCount { get; set; }
        public int? CrCasRenterPrivateDriverInformationDaysCount { get; set; }
        public int? CrCasRenterPrivateDriverInformationTraveledDistance { get; set; }
        public decimal? CrCasRenterPrivateDriverInformationEvaluationTotal { get; set; }
        public decimal? CrCasRenterPrivateDriverInformationEvaluationValue { get; set; }
        public string? CrCasRenterPrivateDriverInformationSignature { get; set; }
        public string? CrCasRenterPrivateDriverInformationIdImage { get; set; }
        public string? CrCasRenterPrivateDriverInformationLicenseImage { get; set; }
        public string? CrCasRenterPrivateDriverInformationStatus { get; set; }
        public string? CrCasRenterPrivateDriverInformationReasons { get; set; }


        public List<SelectListItem2>? all_Nationalities = new List<SelectListItem2>();
        public List<SelectListItem2>? all_IdType = new List<SelectListItem2>();
        public List<SelectListItem2>? all_LicenseType = new List<SelectListItem2>();
        public List<SelectListItem2>? all_callingKey = new List<SelectListItem2>();
        public List<SelectListItem2>? all_Genders = new List<SelectListItem2>();
        
    }
}
