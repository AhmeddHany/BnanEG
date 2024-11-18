using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CarRegistrationVM
    {
        public string CrMasSupCarRegistrationCode { get; set; } = null!;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupCarRegistrationNaqlCode { get; set; } = 0;

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupCarRegistrationNaqlId { get; set; } = 0;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarRegistrationArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupCarRegistrationEnName { get; set; }
        public string? CrMasSupCarRegistrationStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupCarRegistrationReasons { get; set; }

        public List<CrMasSupCarRegistration> crMasSupCarRegistration = new List<CrMasSupCarRegistration>();

        //public List<CrCasCarInformation> cars_count = new List<CrCasCarInformation>();
        public List<TResult2>? cars_count = new List<TResult2>();
    }
}
