using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasRenterContractShomoo
    {
        public string CrCasRenterContractShomoosContractNo { get; set; } = null!;
        public string? CrCasRenterContractShomoosLessor { get; set; }
        public bool? CrCasRenterContractShomoosType { get; set; }
        public string? CrCasRenterContractShomoosNo { get; set; }
        public DateTime? CrCasRenterContractShomoosStartDate { get; set; }
        public int? CrCasRenterContractShomoosDaysNo { get; set; }
        public decimal? CrCasRenterContractShomoosValue { get; set; }
        public DateTime? CrCasRenterContractShomoosEndDate { get; set; }
        public bool? CrCasRenterContractShomoosAction { get; set; }
        public string? CrCasRenterContractShomoosStatus { get; set; }

        public virtual CrMasLessorInformation? CrCasRenterContractShomoosLessorNavigation { get; set; }
    }
}
