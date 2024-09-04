using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrMasSysCallingKey
    {
        public string CrMasSysCallingKeysCode { get; set; } = null!;
        public int? CrMasSysCallingKeysNaqlCode { get; set; }
        public int? CrMasSysCallingKeysNaqlId { get; set; }
        public string? CrMasSysCallingKeysArName { get; set; }
        public string? CrMasSysCallingKeysEnName { get; set; }
        public string? CrMasSysCallingKeysNo { get; set; }
        public string? CrMasSysCallingKeysClassificationCode { get; set; }
        public long? CrMasSysCallingKeysCount { get; set; }
        public string? CrMasSysCallingKeysFlag { get; set; }
        public string? CrMasSysCallingKeysStatus { get; set; }
        public string? CrMasSysCallingKeysReasons { get; set; }
    }
}
