namespace Bnan.Core.Models
{
    public partial class CrCasRenterContractCarCheckup
    {
        public string CrCasRenterContractCarCheckupNo { get; set; } = null!;
        public string CrCasRenterContractCarCheckupCode { get; set; } = null!;
        public string CrCasRenterContractCarCheckupType { get; set; } = null!;
        public string? CrCasRenterContractCarCheckupCheck { get; set; }
        public bool? CrCasRenterContractCarCheckupStatus { get; set; }
        public string? CrCasRenterContractCarCheckupReasons { get; set; }

        public virtual CrMasSupContractCarCheckupDetail? CrCasRenterContractCarCheckupC { get; set; }
    }
}
