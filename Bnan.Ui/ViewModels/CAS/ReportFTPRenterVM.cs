using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class Date_ReportFTPRenterVM
    {
        public DateTime? dates { get; set; }
    }
    public class listReportFTPRenterVM
    {
        
        public sumitionofClass_FTPRenter_VM summition = new sumitionofClass_FTPRenter_VM();
        public List<info_FTP_Renter_VM> all_Recipt_Info = new List<info_FTP_Renter_VM>();
        public List<TResult2> all_bonds = new List<TResult2>();
        public List<TResult2> all_exchanges = new List<TResult2>();
        public List<cas_list_String_2> all_userIds_recipt = new List<cas_list_String_2>();
        public List<UserInfo_FTR_Renter> all_UserData = new List<UserInfo_FTR_Renter>();
        public List<ReciptVM_Renter> all_Recipts = new List<ReciptVM_Renter>();
        public RenterInfo_FTR_Renter This_RenterData = new RenterInfo_FTR_Renter();
        public List<info_FTP_CasRenterLessor_Renter_VM> all_CasRenterLessor = new List<info_FTP_CasRenterLessor_Renter_VM>();

        


        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string RenterId { get; set; }
    }
    public class sumitionofClass_FTPRenter_VM
    {
        public decimal? Creditor_Total { get; set; } = 0;
        public decimal? Debitor_Total { get; set; } = 0;

    }
    

     public class info_FTP_CasRenterLessor_Renter_VM
    {
        public string? CrCasRenterLessorId { get; set; }

        public string? CrCasRenterLessorCode { get; set; }
        public string? Renter_Ar { get; set; }
        public string? Renter_En { get; set; }
        public decimal? TradedAmount { get; set; }
        public decimal? TotalBalance { get; set; }
        public decimal? AvailableBalance { get; set; }
        public decimal? ReservedBalance { get; set; }

    }

    public class info_FTP_Renter_VM
    {
        public string CrCasAccountReceiptNo { get; set; } = null!;

        public string? CrCasAccountReceiptLessorCode { get; set; }
        public string? CrCasAccountReceiptSalesPoint { get; set; }
        public string? CrCasAccountReceiptRenterId { get; set; }
        public string? SalesPoint_Ar { get; set; }
        public string? SalesPoint_En { get; set; }
        public string? Account_Ar { get; set; }
        public string? Account_En { get; set; }
        public string? bank_Ar { get; set; }
        public string? bank_En { get; set; }
        public string? branch_Ar { get; set; }
        public string? branch_En { get; set; }
        public decimal? TotalBalance { get; set; }
        public decimal? AvailableBalance { get; set; }
        public decimal? ReservedBalance { get; set; }
        
    }
    public class UserInfo_FTR_Renter
    {
        public string? CrMasUserInformationCode { get; set; }
        public string? CrMasUserInformationArName { get; set; }
        public string? CrMasUserInformationEnName { get; set; }
    }
    public class RenterInfo_FTR_Renter
    {
        public string? CrMasRenterInformationId { get; set; }
        public string? CrMasRenterInformationArName { get; set; }
        public string? CrMasRenterInformationEnName { get; set; }
    }
    public class ReciptVM_Renter
    {
        public string CrCasAccountReceiptNo { get; set; } = null!;

        public decimal? userPreviousBalance { get; set; }

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
        public string? branch_Ar { get; set; }
        public string? branch_En { get; set; }
        public string? ReferanceType_Ar { get; set; }
        public string? ReferanceType_En { get; set; }


    }

}
