using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSupContractCarCheckupDetail
    {
        public CrMasSupContractCarCheckupDetail()
        {
            CrCasRenterContractCarCheckups = new HashSet<CrCasRenterContractCarCheckup>();
        }

        public string CrMasSupContractCarCheckupDetailsCode { get; set; } = null!;
        public string CrMasSupContractCarCheckupDetailsNo { get; set; } = null!;
        public string? CrMasSupContractCarCheckupDetailsArName { get; set; }
        public string? CrMasSupContractCarCheckupDetailsEnName { get; set; }
        public string? CrMasSupContractCarCheckupDetailsStatus { get; set; }
        public string? CrMasSupContractCarCheckupDetailsReasons { get; set; }

        public virtual CrMasSupContractCarCheckup CrMasSupContractCarCheckupDetailsCodeNavigation { get; set; } = null!;
        public virtual ICollection<CrCasRenterContractCarCheckup> CrCasRenterContractCarCheckups { get; set; }
    }
}
