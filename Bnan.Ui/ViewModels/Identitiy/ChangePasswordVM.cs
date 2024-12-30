using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.Identitiy

{
    public class ChangePasswordVM
    {

        [Required(ErrorMessage = "requiredFiled")]
        [MaxLength(10, ErrorMessage = "requiredFiledMaxLength10Passowrd")]
        public string? CurrentPassword { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        [MaxLength(10, ErrorMessage = "requiredFiledMaxLength10Passowrd")]
        public string? NewPassword { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        [MaxLength(10, ErrorMessage = "requiredFiledMaxLength10Passowrd")]
        [Compare("NewPassword", ErrorMessage = "NewAndConfirmPassInCorrect")]
        public string? ConfirmPassword { get; set; }

    }
}
