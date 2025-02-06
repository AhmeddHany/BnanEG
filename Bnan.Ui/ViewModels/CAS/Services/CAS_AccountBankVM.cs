using Bnan.Inferastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
 
    public class CAS_AccountBankVM
    {
        public int countForSales { get; set; } = 0;

        [ Range(1,9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public string? CrCasAccountBankCode { get; set; }
        [ MaxLength(4, ErrorMessage = "requiredFiled")]
        public string? CrCasAccountBankLessor { get; set; } 
        [Required(ErrorMessage = "requiredFiled"), MaxLength(2, ErrorMessage = "requiredFiled")]
        public string? CrCasAccountBankNo { get; set; }
        //[ MaxLength(2, ErrorMessage = "requiredFiled")]
        public string? CrCasAccountBankSerail { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrCasAccountBankIban { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrCasAccountBankArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrCasAccountBankEnName { get; set; }
        public string? CrCasAccountBankStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrCasAccountBankReasons { get; set; }
        public bool? CrCasAccountBankCurrent { get; set; }
        public bool? CrCasAccountBankdad { get; set; }

        //public virtual CrMasLessorInformation? CrCasAccountBankLessorNavigation { get; set; }
        //public virtual CrMasSupAccountBank? CrCasAccountBankNoNavigation { get; set; }
        //public virtual ICollection<CrCasAccountReceipt> CrCasAccountReceipts { get; set; }
        //public virtual ICollection<CrCasAccountSalesPoint> CrCasAccountSalesPoints { get; set; }


        public List<CrCasAccountBank>? list_CrCasAccountBank = new List<CrCasAccountBank>();
        public List<CrMasSupAccountBank>? all_BanksName = new List<CrMasSupAccountBank>();
        public CrMasSupAccountBank? This_BankName = new CrMasSupAccountBank();
        public List<TResult2> all_SalesPointsCount = new List<TResult2>();

    }
}
