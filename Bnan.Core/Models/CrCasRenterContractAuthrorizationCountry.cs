using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasRenterContractAuthrorizationCountry
    {
        public string CrCasRenterContractAuthrorizationCountryNo { get; set; } = null!;
        public string CrCasRenterContractAuthrorizationCountryLessor { get; set; } = null!;
        public string CrCasRenterContractAuthrorizationCountryCode { get; set; } = null!;

        public virtual CrMasSysAuthorizationCountry CrCasRenterContractAuthrorizationCountryCodeNavigation { get; set; } = null!;
        public virtual CrMasLessorInformation CrCasRenterContractAuthrorizationCountryLessorNavigation { get; set; } = null!;
    }
}
