using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS.Employees
{
    public class EmployeesWithAuthrizationVM
    {
        [Required(ErrorMessage = "requiredFiled")]
        public string CrMasUserInformationCode { get; set; } = null!;
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationId { get; set; }
        public string? CrMasUserInformationLessor { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationArName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationEnName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public decimal? CrMasUserInformationCreditLimit { get; set; }
        public int? CrMasUserInformationCreditDaysLimit { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationTasksArName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasUserInformationTasksEnName { get; set; }
        public string? CrMasUserInformationCallingKey { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "MobilePatternError")]
        public string? CrMasUserInformationMobileNo { get; set; }
        public string? CrMasUserInformationEmail { get; set; }
        public string? CrMasUserInformationStatus { get; set; }
        public bool CrMasUserInformationAuthorizationAdmin { get; set; }
        public bool CrMasUserInformationAuthorizationBranch { get; set; }
        public bool CrMasUserInformationAuthorizationOwner { get; set; }
        public string? CrMasUserInformationReasons { get; set; }

        public List<AuthrizationBranchesVM>? BranchesAuthrization { get; set; }
    }
}
