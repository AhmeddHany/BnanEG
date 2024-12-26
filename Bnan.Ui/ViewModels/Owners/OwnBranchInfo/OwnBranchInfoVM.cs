namespace Bnan.Ui.ViewModels.Owners.OwnBranchInfo
{
    public class OwnBranchInfoVM
    {
        public string? CrCasBranchInformationCode { get; set; }
        public decimal CrCasBranchInformationReservedBalance { get; set; }
        public OwnCrCasBranchPostVM? CrCasBranchPost { get; set; }
        public List<OwnCrCasRenterContractBasicVM>? CrCasRenterContractBasics { get; set; }
        public List<OwnCrCasCarInformationVM>? CrCasCarInformations { get; set; }
        public List<OwnCrCasRenterContractAlertVM>? CrCasRenterContractAlerts { get; set; }
    }
    public class OwnCrCasBranchPostVM
    {
        public OwnCrMasSupPostCityConcatenateVM CrCasBranchPostCityNavigation { get; set; }
    }

    public class OwnCrMasSupPostCityConcatenateVM
    {
        public string CrMasSupPostCityConcatenateArName { get; set; }
        public string CrMasSupPostCityConcatenateEnName { get; set; }
    }

    public class OwnCrCasRenterContractBasicVM
    {
        public string CrCasRenterContractBasicStatus { get; set; }
    }

    public class OwnCrCasCarInformationVM
    {
        public string CrCasCarInformationStatus { get; set; }
        public bool CrCasCarInformationPriceStatus { get; set; }
        public string CrCasCarInformationForSaleStatus { get; set; }
    }

    public class OwnCrCasRenterContractAlertVM
    {
        public string CrCasRenterContractAlertContractStatus { get; set; }
    }
}
