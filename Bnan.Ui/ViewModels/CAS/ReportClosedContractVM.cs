using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class Date_ReportClosedContract_CasVM
    {
        public DateTime? dates { get; set; }
    }
    public class listReportClosedContract_CasVM
    {
        public List<ReportClosedContract_CasVM> all_contractBasic = new List<ReportClosedContract_CasVM>();
        public List<CrMasLessorInformation> all_lessors = new List<CrMasLessorInformation>();
        public List<CrMasRenterInformation> all_RentersMas = new List<CrMasRenterInformation>();
        public List<cas_list_String_4> all_Invoices = new List<cas_list_String_4>();
        
        public string? start_Date { get; set; }
        public string? end_Date { get; set; }
        public string? Sum_ContractsValue { get; set; }
        public string? Sum_ExpensesValue { get; set; }
        public string? Sum_CompensationsValue { get; set; }

    }

    public class ReportClosedContract_CasVM
    {
        public string? branchAr { get; set; }
        public string? branchEn { get; set; }
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
        public int? CrCasRenterContractBasicActualExtraHours { get; set; }
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
        public decimal? CrCasRenterContractBasicExpensesValue { get; set; }
        public decimal? CrCasRenterContractBasicCompensationValue { get; set; }

        public decimal? CrCasRenterContractBasicActualAmountRequired { get; set; }
        public decimal? CrCasRenterContractBasicAmountPaid { get; set; }
        public string? CrCasRenterContractBasicPdfFile { get; set; }
        public string? CrCasRenterContractBasicPdfTga { get; set; }
        public string? CrCasRenterContractBasicUserInsert { get; set; }
        public string? CrCasRenterContractBasicStatus { get; set; }
        public string? Reasons { get; set; }


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
