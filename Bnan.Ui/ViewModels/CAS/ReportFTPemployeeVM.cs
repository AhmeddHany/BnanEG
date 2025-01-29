using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class Date_ReportFTPemployeeVM
    {
        public DateTime? dates { get; set; }
    }
    public class listReportFTPemployeeVM
    {
        
        public sumitionofClass_FTPemployee_VM summition = new sumitionofClass_FTPemployee_VM();
        public List<userinfo_FTP_VM> all_usersinfo = new List<userinfo_FTP_VM>();
        public List<TResult2> all_bonds = new List<TResult2>();
        public List<TResult2> all_exchanges = new List<TResult2>();
        public List<cas_list_String_2> all_userIds_recipt = new List<cas_list_String_2>();
        public CrMasUserInformation ThisUserData = new CrMasUserInformation();
        public List<ReciptVM> all_Recipts = new List<ReciptVM>();
        public List<cas_list_String_4> all_lessors = new List<cas_list_String_4>();



        public string start_Date { get; set; }
        public string end_Date { get; set; }
        public string UserId { get; set; }
    }
    public class sumitionofClass_FTPemployee_VM
    {
        public decimal? Creditor_Total { get; set; } = 0;
        public decimal? Debitor_Total { get; set; } = 0;

    }
    public class userinfo_FTP_VM
    {
        public string CrMasUserInformationCode { get; set; } = null!;
      
        public string? CrMasUserInformationLessor { get; set; }
        //public string? CrMasUserInformationId { get; set; }
     
        public string? CrMasUserInformationArName { get; set; }
        public string? CrMasUserInformationEnName { get; set; }
        //public string? CrMasUserInformationTasksArName { get; set; }
        //public string? CrMasUserInformationTasksEnName { get; set; }
        public decimal? CrMasUserInformationReservedBalance { get; set; }
        public decimal? CrMasUserInformationTotalBalance { get; set; }
        public decimal? CrMasUserInformationAvailableBalance { get; set; }
        public decimal? CrMasUserInformationCreditLimit { get; set; }
        //public string? CrMasUserInformationCallingKey { get; set; }
        //public string? CrMasUserInformationMobileNo { get; set; }
        //public string? CrMasUserInformationEmail { get; set; }
        //public DateTime? CrMasUserInformationChangePassWordLastDate { get; set; }
       
        //public int? CrMasUserInformationCreditDaysLimit { get; set; }
        //public DateTime? CrMasUserInformationEntryLastDate { get; set; }
        //public TimeSpan? CrMasUserInformationEntryLastTime { get; set; }
        //public DateTime? CrMasUserInformationExitLastDate { get; set; }
        //public TimeSpan? CrMasUserInformationExitLastTime { get; set; }
        public DateTime? CrMasUserInformationLastActionDate { get; set; }
        public string? CrMasUserInformationPicture { get; set; }
        //public string? CrMasUserInformationSignature { get; set; }
        public bool? CrMasUserInformationOperationStatus { get; set; }
        public string? CrMasUserInformationStatus { get; set; }
    }


    public class ReciptVM
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
