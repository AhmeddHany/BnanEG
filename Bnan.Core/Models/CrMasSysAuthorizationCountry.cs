using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSysAuthorizationCountry
    {
        public CrMasSysAuthorizationCountry()
        {
            CrCasRenterContractAuthrorizationCountries = new HashSet<CrCasRenterContractAuthrorizationCountry>();
        }

        public string CrMasSysAuthorizationCountriesCode { get; set; } = null!;
        public int? CrMasSysAuthorizationCountriesNaqlCode { get; set; }
        public int? CrMasSysAuthorizationCountriesNaqlId { get; set; }
        public string? CrMasSysAuthorizationCountriesArName { get; set; }
        public string? CrMasSysAuthorizationCountriesEnName { get; set; }
        public string? CrMasSysAuthorizationCountriesClassificationCode { get; set; }
        public int? CrMasSysAuthorizationCountriesCount { get; set; }
        public string? CrMasSysAuthorizationCountriesFlag { get; set; }
        public string? CrMasSysAuthorizationCountriesStatus { get; set; }
        public string? CrMasSysAuthorizationCountriesReasons { get; set; }

        public virtual ICollection<CrCasRenterContractAuthrorizationCountry> CrCasRenterContractAuthrorizationCountries { get; set; }
    }
}
