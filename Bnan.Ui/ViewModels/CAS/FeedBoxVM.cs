using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class FeedBoxVM
    {
        public CrMasUserInformation? UserInformation { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? FeedValue { get; set; }
        public string? Reasons { get; set; }

    }
}
