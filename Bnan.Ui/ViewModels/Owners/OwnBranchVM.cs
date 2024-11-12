namespace Bnan.Ui.ViewModels.Owners
{
    public class OwnBranchVM
    {
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
        public string? BranchPostAr { get; set; }
        public string? BranchPostEn { get; set; }
        public int? ContractsCount { get; set; }
        public int? ClosedContractsCount { get; set; }
        public int? ActiveContractsCount { get; set; }
        public int? SuspendedContractsCount { get; set; }
        public int? SavedContractsCount { get; set; }
        public int? ExpiredContractsCount { get; set; }
        public int? DocsForCompanyExpireCount { get; set; }
        public int? DocsForCompanyAboutExpireCount { get; set; }
        public int? DocsForCarExpireCount { get; set; }
        public int? DocsForCarAboutExpireCount { get; set; }
        public int? MainForCarExpireCount { get; set; }
        public int? MainForCarAboutExpireCount { get; set; }
        public int? CarsCount { get; set; }
        public int? ActiveCarsCount { get; set; }
        public int? RentedCarsCount { get; set; }
        public int? UnActiveCarsCount { get; set; }
        public bool? HaveCustodyNotAccepted { get; set; }
        public bool? RedPointInBranch { get; set; }
    }
}
