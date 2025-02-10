using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS.Users
{
    public class CompanyUserVM
    {
        [Required(ErrorMessage = "requiredFiled")]
        [StringLength(7, ErrorMessage = "requiredFiledMaxLength7")]
        public string CrMasUserInformationCode { get; set; } = null!;
        public string? CrMasUserInformationId { get; set; }
        public string? CrMasUserInformationPassWord { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationLessor { get; set; }
        public bool? CrMasUserInformationAuthorizationBnan { get; set; }
        public bool? CrMasUserInformationAuthorizationAdmin { get; set; }
        public bool? CrMasUserInformationAuthorizationBranch { get; set; }
        public bool? CrMasUserInformationAuthorizationOwner { get; set; }
        public bool? CrMasUserInformationAuthorizationFoolwUp { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationArName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationEnName { get; set; }
        public decimal? CrMasUserInformationTotalBalance { get; set; }
        public decimal? CrMasUserInformationReservedBalance { get; set; }
        public decimal? CrMasUserInformationAvailableBalance { get; set; }
        public decimal? CrMasUserInformationCreditLimit { get; set; }
        public string? CrMasUserInformationTasksArName { get; set; }
        public string? CrMasUserInformationTasksEnName { get; set; }
        public string? CrMasUserInformationRemindMe { get; set; }
        public string? CrMasUserInformationDefaultBranch { get; set; }
        public string? CrMasUserInformationDefaultLanguage { get; set; }
        public string? CrMasUserInformationCallingKey { get; set; }
        [RegularExpression(@"^\d{1,15}$", ErrorMessage = "MobilePatternError")]
        public string? CrMasUserInformationMobileNo { get; set; }
        public string? CrMasUserInformationEmail { get; set; }
        public DateTime? CrMasUserInformationChangePassWordLastDate { get; set; }
        public DateTime? CrMasUserInformationEntryLastDate { get; set; }
        public TimeSpan? CrMasUserInformationEntryLastTime { get; set; }
        public DateTime? CrMasUserInformationExitLastDate { get; set; }
        public TimeSpan? CrMasUserInformationExitLastTime { get; set; }
        public DateTime? CrMasUserInformationLastActionDate { get; set; }
        public int? CrMasUserInformationExitTimer { get; set; }
        public string? CrMasUserInformationPicture { get; set; }
        public string? CrMasUserInformationSignature { get; set; }
        public bool? CrMasUserInformationOperationStatus { get; set; }
        public string? CrMasUserInformationStatus { get; set; }
        public string? CrMasUserInformationReasons { get; set; }
        public string? LessorArName { get; set; }
        public string? LessorEnName { get; set; }
    }
}
