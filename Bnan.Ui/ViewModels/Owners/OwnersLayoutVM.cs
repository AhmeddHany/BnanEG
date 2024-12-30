using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.Owners
{
    public class OwnersLayoutVM
    {
        //Dashboard
        // Contract
        public List<OwnContractsVM>? OwnContracts;
        public double? RateContractsMonthBefore;
        // Cars
        public List<OwnCarsInfoVM>? OwnCars;
        public double? RateCarsMonthBefore;
        // renters
        public List<OwnRentersVM>? OwnRenters;
        public int RentersWithContracts;
        public int RentersWithountContracts;
        public double? RateRentersMonthBefore;
        // Balances
        public decimal? TotalBalance;
        public decimal? AvaliableBalance;
        public decimal? ReservedBalance;
        // Charts 
        public List<OwnPaymentMethodLessorVM>? OwnPaymentMethods;
        public List<OwnAlertContractsVM>? AlertContracts;
        // Documents And Maintaince And Price And Document Of Lessor
        public List<CrCasBranchDocument>? BranchDocuments;
        //public List<CrCasCarDocumentsMaintenance>? CarMaintainces;
        public List<CrCasCarDocumentsMaintenance>? CarDocumentsAndMaintainces;
        public List<CrCasPriceCarBasic>? CarPrices;
        // Branches
        public List<OwnBranchVM>? BranchsInformations;
        //Employees
        public List<OwnEmployeesVM>? Employees;
        //Indicators
        //Renters
        public List<OwnStatictsVM>? NationalityStaticitis;
        public List<OwnStatictsVM>? MembershipStaticitis;
        public List<OwnStatictsVM>? ProfessionStaticitis;
        public List<OwnStatictsVM>? BranchCityStaticitis;
        public List<OwnStatictsVM>? CityStaticitis;
        public List<OwnStatictsVM>? AgeStaticitis;
        public int? RentersCount;
        //Contracts
        public List<OwnStatictsVM>? BranchStaticitis;
        public List<OwnStatictsVM>? AmountValueStaticitis;
        public List<OwnStatictsVM>? DistanceKMStaticitis;
        public List<OwnStatictsVM>? DaysStaticitis;
        public List<OwnStatictsVM>? DaysCountStaticitis;
        public List<OwnStatictsVM>? TimeStaticitis;
        public int? ContractsCount;
        //Cars
        public List<OwnStatictsVM>? ModelCarStaticitis;
        public List<OwnStatictsVM>? CategoryCarStaticitis;
        public List<OwnStatictsVM>? YearCarStaticitis;
        public List<OwnStatictsVM>? BrandCarStaticitis;
        public int? CarsCount;
        // Liabilities Renter
        public decimal? Creditors;
        public decimal? Debtors;





    }
}
