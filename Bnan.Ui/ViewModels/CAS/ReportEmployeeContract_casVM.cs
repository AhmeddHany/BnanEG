using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class Date_ReportEmployeeContract_CAS_VM
    {
        public DateTime? dates { get; set; }
    }
    public class listReportEmployeeContract_CAS_VM
    {
        public List<ReportEmployeeContract_CAS_VM> all_contractBasic = new List<ReportEmployeeContract_CAS_VM>();
        public List<EmployeeContract_CAS_VM> all_EmployeeContracts = new List<EmployeeContract_CAS_VM>();
        public List<UserInfo_EmployeeContract_CAS_VM> all_UserData = new List<UserInfo_EmployeeContract_CAS_VM>();     
        public List<CrMasRenterInformation> all_Renters = new List<CrMasRenterInformation>();
        public List<CrCasAccountInvoice> all_Invoices = new List<CrCasAccountInvoice>();
        public sumitionofClass_Employee_up_VM summition = new sumitionofClass_Employee_up_VM();
        public List<UserInfo_EmployeeContract_CAS_VM> ThisUserData = new List<UserInfo_EmployeeContract_CAS_VM>();

        
        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string UserInsert { get; set; }
    }
    public class sumitionofClass_Employee_up_VM
    {
        public int? Days_Count { get; set; } = 0;
        public decimal? km_Count { get; set; } = 0;
        public int? Contracts_Count { get; set; } = 0;
        public decimal? contract_Values_Total { get; set; } = 0;
    }

    public class EmployeeContract_CAS_VM
    {
        public string CrCasRenterContractStatisticsNo { get; set; } = null!;
        public string? CrCasRenterContractStatisticsUserOpen { get; set; }
        public string? CrCasRenterContractStatisticsUserClose { get; set; }
        public DateTime? CrCasRenterContractStatisticsDate { get; set; }
        public int? CrCasRenterContractStatisicsDays { get; set; }
        public decimal? CrCasRenterContractStatisticsExpensesValue { get; set; }
        public decimal? CrCasRenterContractStatisticsCompensationValue { get; set; }
        public decimal? CrCasRenterContractStatisticsRentValue { get; set; }

    }
    public class UserInfo_EmployeeContract_CAS_VM
    {
        public string CrMasUserInformationCode { get; set; } = null!;
        public string? CrMasUserInformationArName { get; set; }
        public string? CrMasUserInformationEnName { get; set; }
        public bool? CrMasUserInformationOperationStatus { get; set; }
        public string? CrMasUserInformationStatus { get; set; }
        public string? CrMasUserInformationPicture { get; set; }
        public List<EmployeeContract_CAS_VM>? list_Contracts { get; set; } = new List<EmployeeContract_CAS_VM>();
        public string? LastDate { get; set; } = " ";
        public string? RentValue { get; set; } = "0.00";
        public int? open { get; set; } = 0;
        public int? close { get; set; } = 0;
        public int? DaysCount { get; set; } = 0;

    }
    public class ReportEmployeeContract_CAS_VM
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
