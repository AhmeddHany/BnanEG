using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasRenterContractEvaluation
    {
        public string CrCasRenterContractEvaluationContract { get; set; } = null!;
        public string CrCasRenterContractEvaluationType { get; set; } = null!;
        public string? CrCasRenterContractEvaluationLessor { get; set; }
        public string? CrCasRenterContractEvaluationRenter { get; set; }
        public string? CrCasRenterContractEvaluationUser { get; set; }
        public string? CrCasRenterContractEvaluationCode { get; set; }
        public int? CrCasRenterContractEvaluationValue { get; set; }
        public DateTime? CrCasRenterContractEvaluationDate { get; set; }
        public string? CrCasRenterContractEvaluationReasons { get; set; }

        public virtual CrMasSysEvaluation? CrCasRenterContractEvaluationCodeNavigation { get; set; }
        public virtual CrCasRenterLessor? CrCasRenterContractEvaluationNavigation { get; set; }
        public virtual CrMasUserInformation? CrCasRenterContractEvaluationUserNavigation { get; set; }
    }
}
