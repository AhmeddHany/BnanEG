using Bnan.Core.CustomAttribute;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.BS.CreateContract
{
    public class ContractVM
    {
        //[Required(ErrorMessage = "requiredFiled")]
        //public string? RenterId { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        //public string? RenterIdType { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        ////[CustomBirthDateValidation(15, ErrorMessage = "requiredAgeAtLeast15")]
        //public DateTime? RenterBirthDate { get; set; }
        public RenterInfoVM? RenterInfo { get; set; }
        public RenterInfoVM? DriverInfo { get; set; }
        public RenterInfoVM? AddDriverInfo { get; set; }

        //[Required(ErrorMessage = "requiredFiled")]
        //[RegularExpression(@"^(?!7)\d{1,20}$", ErrorMessage = "NotStartWith7")]
        //public string? DriverId { get; set; }
        //[Required(ErrorMessage = "requiredFiled")]
        //[RegularExpression(@"^(?!7)\d{1,20}$", ErrorMessage = "NotStartWith7")]
        //public string? AdditionalDriverId { get; set; }
        public string? PrivateDriverId { get; set; }
        public string? RenterReasons { get; set; }
        public string? DriverReasons { get; set; }
        public string? AddDriverReasons { get; set; }
        public string? PaymentReasons { get; set; }
        public string? SerialNo { get; set; }
        public string? PriceNo { get; set; }
        public string? CurrentMeter { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? DaysNo { get; set; }
        public string? OutFeesTmm { get; set; }
        public string? FeesTmmValue { get; set; }
        public string? UserDiscount { get; set; }
        public string? UserAddHours { get; set; }
        public string? UserAddKm { get; set; }
        public string? AmountPayed { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? PaymentMethod { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? SalesPoint { get; set; }
        public string? AccountNo { get; set; }
        public string? OptionTotal { get; set; }
        public string? AdditionalTotal { get; set; }
        public string? ContractValueBeforeDiscount { get; set; }
        public string? DiscountValue { get; set; }
        public string? ContractValueAfterDiscount { get; set; }
        public string? TaxValue { get; set; }
        public string? TotalContractAmount { get; set; }
        public string? AdvantagesTotalValue { get; set; }
        public string? AccountReceiptNo { get; set; }
        public string? InitialInvoiceNo { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? BranchReceivingCode { get; set; }
    }
    public class CarCheckupDetailsVM
    {
        public string? Reason { get; set; }
        public string? ReasonCheck { get; set; }
        public bool CheckUp { get; set; }

    }
}
