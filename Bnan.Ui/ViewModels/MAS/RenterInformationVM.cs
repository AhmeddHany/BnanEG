using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterInformationVM
    {

        public string CrMasRenterInformationId { get; set; } = null!;
        public int? CrMasRenterInformationCopyId { get; set; }
        public string? CrMasRenterInformationIdtype { get; set; }
        public string? CrMasRenterInformationSector { get; set; }
        public string? CrMasRenterInformationTaxNo { get; set; }
       
        [Required(ErrorMessage = "requiredFiled"), MaxLength(110, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasRenterInformationArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(110, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasRenterInformationEnName { get; set; }
        public DateTime? CrMasRenterInformationBirthDate { get; set; }
        public DateTime? CrMasRenterInformationIssueIdDate { get; set; }
        public DateTime? CrMasRenterInformationExpiryIdDate { get; set; }
        public string? CrMasRenterInformationIssuePlace { get; set; }
        public string? CrMasRenterInformationDrivingLicenseNo { get; set; }
        public string? CrMasRenterInformationDrivingLicenseType { get; set; }
        public DateTime? CrMasRenterInformationDrivingLicenseDate { get; set; }
        public DateTime? CrMasRenterInformationExpiryDrivingLicenseDate { get; set; }
        public string? CrMasRenterInformationCommunicationLanguage { get; set; }
        public string? CrMasRenterInformationProfession { get; set; }
        public string? CrMasRenterInformationNationality { get; set; }
        public string? CrMasRenterInformationGender { get; set; }
        public string? CrMasRenterInformationEmployer { get; set; }
        public string? CrMasRenterInformationCountreyKey { get; set; }
        public string? CrMasRenterInformationMobile { get; set; }
        public string? CrMasRenterInformationEmail { get; set; }
        public string? CrMasRenterInformationBank { get; set; }
        public string? CrMasRenterInformationIban { get; set; }
        public DateTime? CrMasRenterInformationUpDatePersonalData { get; set; }
        public DateTime? CrMasRenterInformationUpDateWorkplaceData { get; set; }
        public DateTime? CrMasRenterInformationUpDateLicenseData { get; set; }
        public string? CrMasRenterInformationSignature { get; set; }
        public string? CrMasRenterInformationRenterIdImage { get; set; }
        public string? CrMasRenterInformationRenterLicenseImage { get; set; }
        public string? CrMasRenterInformationStatus { get; set; }
        public string? CrMasRenterInformationReasons { get; set; }

        ////////////////
        public string? IDtypeArName { get; set; }
        public string? IDtypeEnName { get; set; }
        public string? NationalityArName { get; set; }
        public string? NationalityEnName { get; set; }
        public string? GenderArName { get; set; }
        public string? GenderEnName { get; set; }
        public string? ProffessionArName { get; set; }
        public string? ProffessionEnName { get; set; }
        public string? WorkPlaceArName { get; set; }
        public string? WorkPlaceEnName { get; set; }
        public string? DrivingLicesnseArName { get; set; }
        public string? DrivingLicesnseEnName { get; set; }
        public string? BankArName { get; set; }
        public string? BankEnName { get; set; }
        public string? IbanArName { get; set; }
        public string? IbanEnName { get; set; }
        public string? addressArName { get; set; }
        public string? addressEnName { get; set; }
        /// <summary>
        /// ////////////
        /// </summary>

        public List<CrMasRenterInformation> all_Renters = new List<CrMasRenterInformation>();
        public List<CrMasRenterPost> all_post = new List<CrMasRenterPost>();
        public List<CrMasSupRenterNationality> all_Nationalities = new List<CrMasSupRenterNationality>();
        public List<CrMasSupRenterProfession> all_Professions = new List<CrMasSupRenterProfession>();
    }
}
