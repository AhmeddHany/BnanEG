using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.BS
{
    public class CarInfomationVM
    {
        public string CrCasCarInformationSerailNo { get; set; } = null!;
        public string? CrCasCarInformationConcatenateArName { get; set; }
        public string? CrCasCarInformationConcatenateEnName { get; set; }
        public int? CrCasCarInformationCurrentMeter { get; set; }
        public string? RegisterationAr { get; set; }
        public string? RegisterationEn { get; set; }
        public string? FuelAr { get; set; }
        public string? FuelEn { get; set; }
        public string? CVTAr { get; set; }
        public string? CVTEn { get; set; }
        public string? PeriodicInspectionNo { get; set; }
        public string? RunningCardNo { get; set; }
        public string? RunningCardEndDate { get; set; }
        public string? InsurancePolicyNo { get; set; }
        public string? InsurancePolicyEndDate { get; set; }


    }
}
