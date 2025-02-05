namespace Bnan.Ui.ViewModels.BS
{
    public class RenterInformationsVM
    {
        // Personal
        public string? RenterID { get; set; }
        public string? RenterIDType { get; set; }
        public int? IdCopyNumber { get; set; }
        public string? RenterIDTypeNameAr { get; set; }
        public string? RenterIDTypeNameEn { get; set; }
        public string? PersonalArName { get; set; }
        public string? PersonalEnName { get; set; }
        public string? PersonalArNationality { get; set; }
        public string? PersonalEnNationality { get; set; }
        public string? NationalityCode { get; set; }
        public string? GenderCode { get; set; }
        public string? PersonalArGender { get; set; }
        public string? PersonalEnGender { get; set; }
        public string? ProfessionsCode { get; set; }
        public string? PersonalArProfessions { get; set; }
        public string? PersonalEnProfessions { get; set; }
        public string? PersonalEmail { get; set; }
        public string? MobileNumber { get; set; }
        public string? KeyCountry { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? ExpiryIdDate { get; set; }
        public decimal? Balance { get; set; }
        public decimal? AvailableBalance { get; set; }
        public decimal? ReservedBalance { get; set; }

        //Licence
        public string? LicenseType { get; set; }
        public string? LicenseCode { get; set; }
        public string? LicenseArName { get; set; }
        public string? LicenseEnName { get; set; }
        public DateTime? LicenseIssuedDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        //Employeer
        public string? EmployerCode { get; set; }
        public string? EmployerArName { get; set; }
        public string? EmployerEnName { get; set; }
        //Post
        public string? CityCode { get; set; }
        public string? CityAr { get; set; }
        public string? CityEn { get; set; }
        public string? PostArNameConcenate { get; set; }
        public string? PostEnNameConcenate { get; set; }
        public string? PostArDistictName { get; set; }
        public string? PostEnDistictName { get; set; }
        //Cas Renter
        public DateTime? FirstVisit { get; set; }
        public DateTime? LastContract { get; set; }
        public int? ContractCount { get; set; }
        public int? RentalDays { get; set; }
        public int? KMCut { get; set; }
        public decimal? AmountsTraded { get; set; }
        public decimal? Evaluation { get; set; }
        public string? ArDealingMechanism { get; set; }
        public string? EnDealingMechanism { get; set; }
        public string? ArMembership { get; set; }
        public string? EnMembership { get; set; }
        public string? Signture { get; set; }
        public string? TaxNo { get; set; }
        public int? CountContracts { get; set; }
        public int? ActiveContractsCount { get; set; }
        public int? ClosedContractsCount { get; set; }
        public string? Reasons { get; set; }


    }
}
