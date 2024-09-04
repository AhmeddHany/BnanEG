using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSysCountry
    {
        public string CrMasSysCountryCode { get; set; } = null!;
        public int? CrMasSysCountryNaqlCode { get; set; }
        public int? CrMasSysCountryNaqlId { get; set; }
        public string? CrMasSysCountryArName { get; set; }
        public string? CrMasSysCountryEnName { get; set; }
        public string? CrMasSysCountryClassificationCode { get; set; }
        public int? CrMasSysCountryCount { get; set; }
        public string? CrMasSysCountryFlag { get; set; }
        public string? CrMasSysCountryStatus { get; set; }
        public string? CrMasSysCountryReasons { get; set; }
    }
}
