using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class Date_ReportFTPrenterVM
    {
        public DateTime? dates { get; set; }
    }
    public class listReportFTPrenterVM
    {

        public sumitionofClass_FTPrenter_VM summition = new sumitionofClass_FTPrenter_VM();
        public List<Renterinfo_FTP_VM> all_Rentersinfo = new List<Renterinfo_FTP_VM>();
        public List<TResult2> all_bonds = new List<TResult2>();
        public List<TResult2> all_exchanges = new List<TResult2>();
        public List<list_String_2> all_RentersIds_recipt = new List<list_String_2>();
        public list_String_4 ThisRenterData = new list_String_4();
        public List<list_String_4> all_UsersData = new List<list_String_4>();
        public List<Recipt_ForRenter_VM> all_Recipts = new List<Recipt_ForRenter_VM>();
        public List<list_String_4> all_lessors = new List<list_String_4>();



        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string UserId { get; set; }
    }
    public class sumitionofClass_FTPrenter_VM
    {
        public decimal? Creditor_Total { get; set; } = 0;
        public decimal? Debitor_Total { get; set; } = 0;

    }
    public class Renterinfo_FTP_VM
    {
        public string CrCasRenterLessorId { get; set; }
        public string CrCasRenterLessorCode { get; set; }
        public string? Renter_Ar { get; set; }
        public string? Renter_En { get; set; }
        //public string? CrCasRenterLessorSector { get; set; }
        //public int? CrCasRenterLessorCopyId { get; set; }
        //public string? CrCasRenterLessorIdtrype { get; set; }
        //public string? CrCasRenterLessorMembership { get; set; }
        public DateTime? CrCasRenterLessorDateFirstInteraction { get; set; }
        public DateTime? CrCasRenterLessorDateLastContractual { get; set; }
        //public int? CrCasRenterLessorContractCount { get; set; }
        //public int? CrCasRenterLessorContractExtension { get; set; }
        //public int? CrCasRenterLessorContractDays { get; set; }
        //public int? CrCasRenterLessorContractKm { get; set; }
        public decimal? CrCasRenterLessorContractTradedAmount { get; set; }
        public decimal? CrCasRenterLessorBalance { get; set; }
        public decimal? CrCasRenterLessorAvailableBalance { get; set; }
        public decimal? CrCasRenterLessorReservedBalance { get; set; }
        //public int? CrCasRenterLessorEvaluationNumber { get; set; }
        //public int? CrCasRenterLessorEvaluationTotal { get; set; }
        //public decimal? CrCasRenterLessorEvaluationValue { get; set; }
        //public string? CrCasRenterLessorStatisticsNationalities { get; set; }
        //public string? CrCasRenterLessorStatisticsGender { get; set; }
        //public string? CrCasRenterLessorStatisticsJobs { get; set; }
        //public string? CrCasRenterLessorStatisticsRegions { get; set; }
        //public string? CrCasRenterLessorStatisticsCity { get; set; }
        //public string? CrCasRenterLessorStatisticsAge { get; set; }
        //public string? CrCasRenterLessorStatisticsTraded { get; set; }
        //public string? CrCasRenterLessorDealingMechanism { get; set; }
        public string? CrCasRenterLessorStatus { get; set; }
        //public string? CrCasRenterLessorReasons { get; set; }
    }


    public class Recipt_ForRenter_VM
    {
        public string CrCasAccountReceiptNo { get; set; } = null!;
        //public string? CrCasAccountReceiptYear { get; set; }
        public string? CrCasAccountReceiptType { get; set; }
        public string? CrCasAccountReceiptLessorCode { get; set; }
        //public string? CrCasAccountReceiptBranchCode { get; set; }
        public DateTime? CrCasAccountReceiptDate { get; set; }
        //public string? CrCasAccountReceiptPaymentMethod { get; set; }
        public string? CrCasAccountReceiptReferenceType { get; set; }
        public string? CrCasAccountReceiptReferenceNo { get; set; }
        public decimal? CrCasAccountReceiptPayment { get; set; }
        public decimal? CrCasAccountReceiptReceipt { get; set; }
        public string? CrCasAccountReceiptSalesPoint { get; set; }

        //public string? CrCasAccountReceiptBank { get; set; }
        //public string? CrCasAccountReceiptAccount { get; set; }
        //public string? CrCasAccountReceiptCar { get; set; }
        //public decimal? CrCasAccountReceiptSalesPointPreviousBalance { get; set; }
        public string? CrCasAccountReceiptRenterId { get; set; }
        //public decimal? CrCasAccountReceiptRenterPreviousBalance { get; set; }
        public string? CrCasAccountReceiptUser { get; set; }
        public decimal? CrCasAccountReceiptUserPreviousBalance { get; set; }
        //public decimal? CrCasAccountReceiptBranchUserPreviousBalance { get; set; }
        public string? CrCasAccountReceiptIsPassing { get; set; }
        //public DateTime? CrCasAccountReceiptPassingDate { get; set; }
        public string? CrCasAccountReceiptPassingUser { get; set; }
        public string? CrCasAccountReceiptPassingReference { get; set; }
        public string? CrCasAccountReceiptPdfFile { get; set; }
        //public string? CrCasAccountReceiptReasons { get; set; }

        public string? PaymentMethod_Ar { get; set; }
        public string? PaymentMethod_En { get; set; }
        public string? Salespoint_Ar { get; set; }
        public string? Salespoint_En { get; set; }
        public string? ReferanceType_Ar { get; set; }
        public string? ReferanceType_En { get; set; }


    }

}
