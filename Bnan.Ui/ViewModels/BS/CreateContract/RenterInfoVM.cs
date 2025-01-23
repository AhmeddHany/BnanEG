using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.BS.CreateContract

{
    public class RenterInfoVM
    {
        [Required(ErrorMessage = "requiredFiled")]
        [RegularExpression("^[1-9][0-9]*$", ErrorMessage = "requiredFiled")]
        public string CrMasRenterInformationId { get; set; } = null!;
        public int? CrMasRenterInformationCopyId { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationIdtype { get; set; }
        public string? CrMasRenterInformationSector { get; set; }
        public string? CrMasRenterInformationTaxNo { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationArName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationEnName { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime? CrMasRenterInformationBirthDate { get; set; }
        public DateTime? CrMasRenterInformationIssueIdDate { get; set; }
        public DateTime? CrMasRenterInformationExpiryIdDate { get; set; }
        public string? CrMasRenterInformationIssuePlace { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationDrivingLicenseNo { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationDrivingLicenseType { get; set; }
        public DateTime? CrMasRenterInformationDrivingLicenseDate { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrMasRenterInformationExpiryDrivingLicenseDate { get; set; }
        public string? CrMasRenterInformationCommunicationLanguage { get; set; }
        public string? CrMasRenterInformationProfession { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationNationality { get; set; }
        public string? CrMasRenterInformationGender { get; set; }
        public string? CrMasRenterInformationEmployer { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationCity { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationCountreyKey { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CrMasRenterInformationMobile { get; set; }
        [EmailAddress(ErrorMessage = "requiredFiledEmail")]
        public string? CrMasRenterInformationEmail { get; set; }
        public string? CrMasRenterInformationBank { get; set; }
        public string? CrMasRenterInformationIban { get; set; }
        public DateTime? CrMasRenterInformationUpDatePersonalData { get; set; }
        public DateTime? CrMasRenterInformationUpDateWorkplaceData { get; set; }
        public DateTime? CrMasRenterInformationUpDateLicenseData { get; set; }
        public string? CrMasRenterInformationSignature { get; set; }
        public string? OldSignature { get; set; }
        public string? CrMasRenterInformationRenterIdImage { get; set; }
        public string? CrMasRenterInformationRenterLicenseImage { get; set; }
        public string? CrMasRenterInformationStatus { get; set; }
        public string? CrMasRenterInformationReasons { get; set; }
        // Extension 
        public string? BirthDate { get; set; }
        public string? ExpiryLicenseDate { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? CityText { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? NationalityText { get; set; }
        public string? CrMasRenterInformationEmployerName { get; set; }
        public string? CrMasRenterInformationArAddress { get; set; }
        public string? CrMasRenterInformationEnAddress { get; set; }

        [Required(ErrorMessage = "requiredFiled")]
        public string? DayDate { get; set; }

        [Required(ErrorMessage = "requiredFiled")]
        public string? MonthDate { get; set; }

        [Required(ErrorMessage = "requiredFiled")]
        public string? YearDate { get; set; }

    }
}
