using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSupRenterNationalitiesNaql
    {
        public string CrMasSupRenterNationalitiesNCode { get; set; } = null!;
        public string? CrMasSupRenterNationalitiesNGroupCode { get; set; }
        public int? CrMasSupRenterNationalitiesNNaqlId { get; set; }
        public int? CrMasSupRenterNationalitiesNNaqlCode { get; set; }
        public string? CrMasSupRenterNationalitiesNArName { get; set; }
        public string? CrMasSupRenterNationalitiesNEnName { get; set; }
        public bool? CrMasSupRenterNationalitiesNNaqlGcc { get; set; }
        public string? CrMasSupRenterNationalitiesNFlag { get; set; }
        public int? CrMasSupRenterNationalitiesNCounter { get; set; }
        public string? CrMasSupRenterNationalitiesNStatus { get; set; }
        public string? CrMasSupRenterNationalitiesNReasons { get; set; }

        public virtual CrMasSysGroup? CrMasSupRenterNationalitiesNGroupCodeNavigation { get; set; }
    }
}
