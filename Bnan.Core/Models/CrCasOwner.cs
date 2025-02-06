using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasOwner
    {
        public CrCasOwner()
        {
            CrCasCarInformations = new HashSet<CrCasCarInformation>();
            CrCasRenterContractBasics = new HashSet<CrCasRenterContractBasic>();
        }

        public string CrCasOwnersCode { get; set; } = null!;
        public string CrCasOwnersLessorCode { get; set; } = null!;
        public string? CrCasOwnersCountryKey { get; set; }
        public string? CrCasOwnersMobile { get; set; }
        public string? CrCasOwnersConnectStatus { get; set; }
        public string? CrCasOwnersEmail { get; set; }
        public string? CrCasOwnersArName { get; set; }
        public string? CrCasOwnersEnName { get; set; }
        public string? CrCasOwnersStatus { get; set; }
        public string? CrCasOwnersReasons { get; set; }
        

        public virtual CrMasLessorInformation CrCasOwnersLessorCodeNavigation { get; set; } = null!;
        public virtual ICollection<CrCasCarInformation> CrCasCarInformations { get; set; }
        public virtual ICollection<CrCasRenterContractBasic> CrCasRenterContractBasics { get; set; }
    }
}
