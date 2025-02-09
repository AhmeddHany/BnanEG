using Bnan.Inferastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
 
    public class CAS_AccountSalesPointVM
    {
        public int countForSales { get; set; } = 0;

        [ Range(1,9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public string? CrCasAccountSalesPointCode { get; set; }
        [ MaxLength(4, ErrorMessage = "requiredFiled")]
        public string? CrCasAccountSalesPointLessor { get; set; } 
        [Required(ErrorMessage = "requiredFiled"), MaxLength(2, ErrorMessage = "requiredFiled")]
        public string? CrCasAccountSalesPointBank { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(3, ErrorMessage = "requiredFiled")] // branch
        public string? CrCasAccountSalesPointBrn { get; set; } // branch
        [Required(ErrorMessage = "requiredFiled"), MaxLength(8, ErrorMessage = "requiredNoLengthFiled8")]
        public string? CrCasAccountSalesPointAccountBank { get; set; } // Code of AccountBank another Table
        //[ MaxLength(2, ErrorMessage = "requiredFiled")]
        public string? CrCasAccountSalesPointSerial { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrCasAccountSalesPointNo { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrCasAccountSalesPointArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrCasAccountSalesPointEnName { get; set; }
        public decimal? CrCasAccountSalesPointTotalBalance { get; set; }
        public decimal? CrCasAccountSalesPointTotalReserved { get; set; }
        public decimal? CrCasAccountSalesPointTotalAvailable { get; set; }
        public string? CrCasAccountSalesPointBranchStatus { get; set; }
        public string? CrCasAccountSalesPointBankStatus { get; set; }
        public string? CrCasAccountSalesPointStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrCasAccountSalesPointReasons { get; set; }


        //public virtual CrCasAccountBank? CrCasAccountSalesPointAccountBankNavigation { get; set; }
        //public virtual CrMasSupAccountBank? CrCasAccountSalesPointBankNavigation { get; set; }
        //public virtual CrCasBranchInformation? CrCasAccountSalesPointNavigation { get; set; }
        //public virtual ICollection<CrCasAccountReceipt> CrCasAccountReceipts { get; set; }

        public List<CrCasAccountSalesPoint>? list_CrCasAccountSalesPoint = new List<CrCasAccountSalesPoint>();
        public List<CrMasSupAccountBank>? all_BanksName = new List<CrMasSupAccountBank>();
        public CrMasSupAccountBank? This_BankName = new CrMasSupAccountBank();
        public CrCasAccountBank? this_AccountData = new CrCasAccountBank();
        
        public List<TResult2> all_SalesPointsCount = new List<TResult2>();
        public List<cas_list_String_4> all_branchesNames = new List<cas_list_String_4>();
        public List<CrCasAccountBank> all_AccountsNames = new List<CrCasAccountBank>();
        public List<cas_list_String_4> all_BanksNames = new List<cas_list_String_4>();
        
    }
}
