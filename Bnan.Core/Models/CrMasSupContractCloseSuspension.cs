using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSupContractCloseSuspension
    {
        public CrMasSupContractCloseSuspension()
        {
            CrCasRenterContractBasics = new HashSet<CrCasRenterContractBasic>();
        }

        public string CrMasSupContractCloseSuspensionCode { get; set; } = null!;
        public string? CrMasSupContractCloseSuspensionType { get; set; }
        public int? CrMasSupContractCloseSuspensionNaqlMain { get; set; }
        public int? CrMasSupContractCloseSuspensionNaqlSub { get; set; }
        public string? CrMasSupContractCloseSuspensionArName { get; set; }
        public string? CrMasSupContractCloseSuspensionEnName { get; set; }
        public string? CrMasSupContractCloseSuspensionStatus { get; set; }
        public string? CrMasSupContractCloseSuspensionReasons { get; set; }

        public virtual ICollection<CrCasRenterContractBasic> CrCasRenterContractBasics { get; set; }
    }
}
