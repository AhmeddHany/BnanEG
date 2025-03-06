using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSupContractCloseMechanism
    {
        public CrMasSupContractCloseMechanism()
        {
            CrCasRenterContractBasics = new HashSet<CrCasRenterContractBasic>();
        }

        public string CrMasSupContractCloseMechanismCode { get; set; } = null!;
        public string? CrMasSupContractCloseMechanismType { get; set; }
        public decimal? CrMasSupContractCloseMechanismValue { get; set; }
        public int? CrMasSupContractCloseMechanismNaqlMain { get; set; }
        public int? CrMasSupContractCloseMechanismNaqlSub { get; set; }
        public string? CrMasSupContractCloseMechanismArName { get; set; }
        public string? CrMasSupContractCloseMechanismEnName { get; set; }
        public string? CrMasSupContractCloseMechanismStatus { get; set; }
        public string? CrMasSupContractCloseMechanismReasons { get; set; }

        public virtual ICollection<CrCasRenterContractBasic> CrCasRenterContractBasics { get; set; }
    }
}
