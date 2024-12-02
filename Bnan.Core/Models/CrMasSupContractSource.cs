using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSupContractSource
    {
        public CrMasSupContractSource()
        {
            CrCasRenterContractBasics = new HashSet<CrCasRenterContractBasic>();
        }

        public string CrMasSupContractSourceCode { get; set; } = null!;
        public string? CrMasSupContractSourceArName { get; set; }
        public string? CrMasSupContractSourceEnName { get; set; }
        public string? CrMasSupContractSourceMobile { get; set; }
        public string? CrMasSupContractSourceEmail { get; set; }
        public string? CrMasSupContractSourceStatus { get; set; }
        public string? CrMasSupContractSourceReasons { get; set; }

        public virtual ICollection<CrCasRenterContractBasic> CrCasRenterContractBasics { get; set; }
    }
}
