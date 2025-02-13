﻿using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasAccountBank
    {
        public CrCasAccountBank()
        {
            CrCasAccountReceipts = new HashSet<CrCasAccountReceipt>();
            CrCasAccountSalesPoints = new HashSet<CrCasAccountSalesPoint>();
        }

        public string CrCasAccountBankCode { get; set; } = null!;
        public string? CrCasAccountBankLessor { get; set; }
        public string? CrCasAccountBankNo { get; set; }
        public string? CrCasAccountBankSerail { get; set; }
        public string? CrCasAccountBankIban { get; set; }
        public string? CrCasAccountBankArName { get; set; }
        public string? CrCasAccountBankEnName { get; set; }
        public bool? CrCasAccountBankCurrent { get; set; }
        public bool? CrCasAccountBankSdad { get; set; }
        public string? CrCasAccountBankStatus { get; set; }
        public string? CrCasAccountBankReasons { get; set; }

        public virtual CrMasLessorInformation? CrCasAccountBankLessorNavigation { get; set; }
        public virtual CrMasSupAccountBank? CrCasAccountBankNoNavigation { get; set; }
        public virtual ICollection<CrCasAccountReceipt> CrCasAccountReceipts { get; set; }
        public virtual ICollection<CrCasAccountSalesPoint> CrCasAccountSalesPoints { get; set; }
    }
}
