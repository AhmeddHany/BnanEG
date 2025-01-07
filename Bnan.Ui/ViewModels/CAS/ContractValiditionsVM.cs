using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class ContractValiditionsVM
    {
        public string CrMasUserContractValidityUserId { get; set; }
        public string? CrMasUserContractValidityAdmin { get; set; }
        public bool? CrMasUserContractValidityRegister { get; set; } = false;
        public bool? CrMasUserContractValidityChamber { get; set; } = false;
        public bool? CrMasUserContractValidityTransferPermission { get; set; } = false;
        public bool? CrMasUserContractValidityLicenceMunicipale { get; set; } = false;
        public bool? CrMasUserContractValidityCompanyAddress { get; set; } = false;
        public bool? CrMasUserContractValidityTrafficLicense { get; set; } = false;
        public bool? CrMasUserContractValidityInsurance { get; set; } = false;
        public bool? CrMasUserContractValidityOperatingCard { get; set; } = false;
        public bool? CrMasUserContractValidityChkecUp { get; set; } = false;
        public bool? CrMasUserContractValidityId { get; set; } = false;
        public bool? CrMasUserContractValidityDrivingLicense { get; set; } = false;
        public bool? CrMasUserContractValidityRenterAddress { get; set; } = false;
        public bool? CrMasUserContractValidityEmployer { get; set; } = false;
        public bool? CrMasUserContractValidityAge { get; set; } = false;
        public bool? CrMasUserContractValidityTires { get; set; } = false;
        public bool? CrMasUserContractValidityOil { get; set; } = false;
        public bool? CrMasUserContractValidityMaintenance { get; set; } = false;
        public bool? CrMasUserContractValidityFbrake { get; set; } = false;
        public bool? CrMasUserContractValidityBbrake { get; set; } = false;

        [Range(0, 100, ErrorMessage = "NumberBetween0To100")]
        public decimal CrMasUserContractValidityDiscountRate { get; set; } = 0;

        [Range(0, 500, ErrorMessage = "NumberBetween0To500")]
        public int CrMasUserContractValidityKm { get; set; } = 0;

        [Range(0, 9, ErrorMessage = "NumberBetween0To9")]
        public int CrMasUserContractValidityHour { get; set; } = 0;

        public bool? CrMasUserContractValidityLessContractValue { get; set; } = false;
        public bool? CrMasUserContractValidityCancel { get; set; } = false;
        public bool? CrMasUserContractValidityExtension { get; set; } = false;
        public bool? CrMasUserContractValidityEnd { get; set; } = false;
        public bool? CrMasUserContractValidityCreate { get; set; } = false;

        public virtual CrMasUserInformation CrMasUserContractValidityUser { get; set; } = null!;
        public virtual List<CrCasLessorMechanism>? CrCasLessorMechanism { get; set; }
        public virtual List<CrMasSysProcedure>? CrMasSysProcedure { get; set; }
    }
}
