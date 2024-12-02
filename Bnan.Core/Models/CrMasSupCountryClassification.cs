using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSupCountryClassification
    {
        public string CrMasLessorCountryClassificationCode { get; set; } = null!;
        public string? CrMasLessorCountryClassificationArName { get; set; }
        public string? CrMasLessorCountryClassificationEnName { get; set; }
        public string? CrMasLessorCountryClassificationStatus { get; set; }
        public string? CrMasLessorCountryClassificationReasons { get; set; }
    }
}
