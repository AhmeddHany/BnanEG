using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSupCarOil
    {
        public CrMasSupCarOil()
        {
            CrCasCarInformations = new HashSet<CrCasCarInformation>();
        }

        public string CrMasSupCarOilCode { get; set; } = null!;
        public string? CrMasSupCarOilArName { get; set; }
        public string? CrMasSupCarOilEnName { get; set; }
        public int? CrMasSupCarOilNaqlCode { get; set; }
        public int? CrMasSupCarOilNaqlId { get; set; }
        public string? CrMasSupCarOilImage { get; set; }
        public string? CrMasSupCarOilStatus { get; set; }
        public string? CrMasSupCarOilReasons { get; set; }

        public virtual ICollection<CrCasCarInformation> CrCasCarInformations { get; set; }
    }
}
