using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.Owners
{
    public class OwnBranchVM
    {
        public OwnBranchVM()
        {

            CrCasCarInformations = new HashSet<CrCasCarInformation>();
            CrCasRenterContractBasics = new HashSet<CrCasRenterContractBasic>();

        }

        public string CrCasBranchInformationLessor { get; set; } = null!;
        public string CrCasBranchInformationCode { get; set; } = null!;
        public string? CrCasBranchInformationArName { get; set; }
        public string? CrCasBranchInformationArShortName { get; set; }
        public string? CrCasBranchInformationEnName { get; set; }
        public string? CrCasBranchInformationEnShortName { get; set; }
        public string? CrCasBranchInformationDirectorArName { get; set; }
        public string? CrCasBranchInformationDirectorEnName { get; set; }
        public decimal? CrCasBranchInformationTotalBalance { get; set; }
        public decimal? CrCasBranchInformationReservedBalance { get; set; }
        public decimal? CrCasBranchInformationAvailableBalance { get; set; }
        public string? CrCasBranchInformationStatus { get; set; }
        public string? CrCasBranchInformationReasons { get; set; }
        public virtual CrCasBranchPost CrCasBranchPost { get; set; } = null!;
        public virtual ICollection<CrCasCarInformation> CrCasCarInformations { get; set; }
        public virtual ICollection<CrCasRenterContractBasic> CrCasRenterContractBasics { get; set; }
    }
}
