using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class Date_ReportCarContract_CAS_VM
    {
        public DateTime? dates { get; set; }
    }
    public class listReportCarContract_CAS_VM
    {
        public List<ReportCarContract_CAS_VM> all_contractBasic = new List<ReportCarContract_CAS_VM>();
        public List<CarContract_CAS_VM> all_CarContracts = new List<CarContract_CAS_VM>();
        public List<CrMasLessorInformation> all_lessors = new List<CrMasLessorInformation>();
        public List<CrMasRenterInformation> all_Renters = new List<CrMasRenterInformation>();
        public List<CrCasAccountInvoice> all_Invoices = new List<CrCasAccountInvoice>();
        public sumitionofClass_up_VM summition = new sumitionofClass_up_VM();
        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string CrCasRenterContractBasicCarSerailNo { get; set; }
    }
    public class sumitionofClass_up_VM
    {
        public int? Days_Count { get; set; } = 0;
        public decimal? km_Count { get; set; } = 0;
        public int? Contracts_Count { get; set; } = 0;
        public decimal? contract_Values_Total { get; set; } = 0;
    }

    public class CarContract_CAS_VM
    {
        public string CrCasCarInformationSerailNo { get; set; } = null!;
        public string? CrCasCarInformationLessor { get; set; }
        //public string? CrCasCarInformationBranch { get; set; }
        //public string? CrCasCarInformationLocation { get; set; }
        //public string? CrCasCarInformationRegion { get; set; }
        //public string? CrCasCarInformationCity { get; set; }
        //public string? CrCasCarInformationOwner { get; set; }
        //public string? CrCasCarInformationBeneficiary { get; set; }
        //public string? CrCasCarInformationRegistration { get; set; }
        //public string? CrCasCarInformationBrand { get; set; }
        //public string? CrCasCarInformationModel { get; set; }
        //public string? CrCasCarInformationCategory { get; set; }
        //public string? CrCasCarInformationDistribution { get; set; }
        //public string? CrCasCarInformationYear { get; set; }
        //public string? CrCasCarInformationCustomsNo { get; set; }
        //public string? CrCasCarInformationStructureNo { get; set; }
        //public string? CrCasCarInformationMainColor { get; set; }
        //public string? CrCasCarInformationSecondaryColor { get; set; }
        //public string? CrCasCarInformationSeatColor { get; set; }
        //public string? CrCasCarInformationFloorColor { get; set; }
        //public string? CrCasCarInformationFuel { get; set; }
        //public decimal? CrCasCarInformationFuelValue { get; set; }
        //public string? CrCasCarInformationOil { get; set; }
        //public string? CrCasCarInformationCvt { get; set; }
        //public string? CrCasCarInformationPlateArNo { get; set; }
        //public string? CrCasCarInformationPlateEnNo { get; set; }
        public string? CrCasCarInformationConcatenateArName { get; set; }
        public string? CrCasCarInformationConcatenateEnName { get; set; }
        //public DateTime? CrCasCarInformationJoinedFleetDate { get; set; }
        public int? CrCasCarInformationCurrentMeter { get; set; }
        public DateTime? CrCasCarInformationLastContractDate { get; set; }
        public int? CrCasCarInformationConractCount { get; set; }
        public int? CrCasCarInformationConractDaysNo { get; set; }
        //public DateTime? CrCasCarInformationOfferedSaleDate { get; set; }
        //public decimal? CrCasCarInformationOfferValueSale { get; set; }
        //public string? CrCasCarInformationLastPictures { get; set; }
        //public DateTime? CrCasCarInformationSoldDate { get; set; }
        //public string? CrCasCarInformationPriceNo { get; set; }
        //public bool? CrCasCarInformationDocumentationStatus { get; set; }
        public bool? CrCasCarInformationMaintenanceStatus { get; set; }
        //public bool? CrCasCarInformationPriceStatus { get; set; }
        //public string? CrCasCarInformationBranchStatus { get; set; }
        //public string? CrCasCarInformationOwnerStatus { get; set; }
        public string? CrCasCarInformationForSaleStatus { get; set; }
        public string? CrCasCarInformationStatus { get; set; }
        //public string? CrCasCarInformationReasons { get; set; }
    }
    public class ReportCarContract_CAS_VM
    {

        public string CrMasRenterInformationId { get; set; } = null!;
       
        [Required(ErrorMessage = "requiredFiled"), MaxLength(110, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasRenterInformationArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(110, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasRenterInformationEnName { get; set; }
       
        public string? CrMasRenterInformationStatus { get; set; }

        public string CrCasRenterContractBasicNo { get; set; } = null!;
        public int CrCasRenterContractBasicCopy { get; set; }

        public string? CrCasRenterContractBasicLessor { get; set; }
        public DateTime? CrCasRenterContractBasicIssuedDate { get; set; }
        public DateTime? CrCasRenterContractBasicExpectedStartDate { get; set; }
        public DateTime? CrCasRenterContractBasicExpectedEndDate { get; set; }
        public int? CrCasRenterContractBasicExpectedRentalDays { get; set; }
        public string? CrCasRenterContractBasicRenterId { get; set; }
        public string? CrCasRenterContractBasicDriverId { get; set; }

        public string? CrCasRenterContractBasicCarSerailNo { get; set; }
        public int? CrCasRenterContractBasicCurrentReadingMeter { get; set; }
        public decimal? CrCasRenterContractBasicExpectedValueBeforDiscount { get; set; }
        public decimal? CrCasRenterContractBasicExpectedDiscountValue { get; set; }
        public decimal? CrCasRenterContractBasicExpectedValueAfterDiscount { get; set; }
        public decimal? CrCasRenterContractBasicExpectedTaxValue { get; set; }
        public decimal? CrCasRenterContractBasicExpectedTotal { get; set; }
        public decimal? CrCasRenterContractBasicPreviousBalance { get; set; }
        public decimal? CrCasRenterContractBasicAmountRequired { get; set; }
        public decimal? CrCasRenterContractBasicAmountPaidAdvance { get; set; }
        public DateTime? CrCasRenterContractBasicActualCloseDateTime { get; set; }
        public string? CrCasRenterContractBasicDelayMechanism { get; set; }
        public int? CrCasRenterContractBasicActualDays { get; set; }
        public int? CrCasRenterContractBasicActualCurrentReadingMeter { get; set; }
        public decimal? CrCasRenterContractBasicActualRentValue { get; set; }
   
        public decimal? CrCasRenterContractBasicActualTotal { get; set; }

        public decimal? CrCasRenterContractBasicActualAmountRequired { get; set; }
        public decimal? CrCasRenterContractBasicAmountPaid { get; set; }
        public string? CrCasRenterContractBasicPdfFile { get; set; }
        public string? CrCasRenterContractBasicPdfTga { get; set; }
        public string? CrCasRenterContractBasicUserInsert { get; set; }
        public string? CrCasRenterContractBasicStatus { get; set; }


        ////////////////
        public string? RenterArName { get; set; }
        public string? RenterEnName { get; set; }
        public string? CarArName { get; set; }
        public string? CarEnName { get; set; }
        public string? LessorArName { get; set; }
        public string? LessorEnName { get; set; }
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

    }
}
