using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasLessorPolicy
    {
        public int CrCasLessorPolicyCode { get; set; }
        public string CrCasLessorPolicyLessor { get; set; } = null!;
        public string? CrCasLessorPolicyShortName { get; set; }
        public string? CrCasLessorPolicyFuel { get; set; }
        public string? CrCasLessorPolicyEarlyReturn { get; set; }
        public string? CrCasLessorPolicyExtension { get; set; }
        public int? CrCasLessorPolicyExtendDays { get; set; }
        public string? CrCasLessorPolicyAccidents { get; set; }
        public string? CrCasLessorPolicyFault { get; set; }
        public bool? CrCasLessorPolicyDefault { get; set; }
        public DateTime? CrCasLessorPolicyDateTime { get; set; }
        public string? CrCasLessorPolicyStatus { get; set; }
        public string? CrCasLessorPolicyReasons { get; set; }

        public virtual CrMasLessorInformation CrCasLessorPolicyLessorNavigation { get; set; } = null!;
    }
}
