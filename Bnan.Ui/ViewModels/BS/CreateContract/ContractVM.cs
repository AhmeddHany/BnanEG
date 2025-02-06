using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.BS.CreateContract
{
    public class ContractVM
    {
        public RenterInfoVM? RenterInfo { get; set; }
        public RenterInfoVM? DriverInfo { get; set; }
        public RenterInfoVM? AddDriverInfo { get; set; }
        //public CarsInfoFromTajeerVM? CarsInfoFromTajeer { get; set; }
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
        public string? AuthorizationNumber { get; set; }
        public string? AdditionalHourValue { get; set; }
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
        [Required(ErrorMessage = "requiredFiled")]
        public string? PolicyCode { get; set; }
        public string? SourceCode { get; set; }
        public string? TokenNo { get; set; }
        public long? TGAContractNo { get; set; }
        public string? ContractTypeCode { get; set; }
        public bool? SaveOrConclusionContract { get; set; }
    }
    public class CarCheckupDetailsVM
    {
        public string? Reason { get; set; }
        public string? ReasonCheckCode { get; set; }
        public bool CheckUp { get; set; }

    }
}
