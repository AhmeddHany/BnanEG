namespace Bnan.Core.Extensions
{
    public static class SubTasks
    {
        //MAS  Renter

        public static string CrMasSupRenterIdtype { get; set; } = "1107001";
        public static string CrMasSupRenterDrivingLicense { get; set; } = "1107002";
        public static string CrMasSupRenterNationality { get; set; } = "1107003";
        public static string CrMasSupRenterGender { get; set; } = "1107004";
        public static string CrMasSupRenterProfession { get; set; } = "1107005";
        public static string CrMasSupRenterEmployer { get; set; } = "1107006";
        public static string CrMasSupRenterMembership { get; set; } = "1107007";
        public static string CrMasSupRenterSector { get; set; } = "1107008";

        ////////////
        //MAS  CAR
        public static string CrMasSupCarRegistration { get; set; } = "1108001";
        public static string CrMasSupCarFuel { get; set; } = "1108002";
        public static string CrMasSupCarOil { get; set; } = "1108003";
        public static string CrMasSupCarCvt { get; set; } = "1108004";
        public static string CrMasSupCarBrand { get; set; } = "1108005";
        public static string CrMasSupCarModel { get; set; } = "1108006";
        public static string CrMasSupCarCategory { get; set; } = "1108007";
        public static string CrMasSupCarDistribution { get; set; } = "1108008";
        public static string CrMasSupCarAdvantage { get; set; } = "1108009";
        public static string CrMasSupCarColor { get; set; } = "1108010";

        ////////////
        //MAS  Finantial
        public static string CrMasSupAccountBank { get; set; } = "1110001";
        public static string CrMasSupAccountPaymentMethod { get; set; } = "1110002";
        public static string CrMasSupAccountReference { get; set; } = "1110003";


        ////////////
        //MAS  Users
        public static string CrMasUserInformationForMAS { get; set; } = "1106001";
        public static string CrMasUserSystemValiditionsMAS { get; set; } = "1106002";
        public static string CrMasUserInformationFromMASToCAS { get; set; } = "1106003";

        ////////////
        //MAS  Services
        public static string TechnicalConnectivityMAS { get; set; } = "1112001";
        public static string CrMasSupCountryClassification { get; set; } = "1112002";
        public static string CrCasLessorClassification { get; set; } = "1112003";
        public static string Evaluation { get; set; } = "1112004";
        public static string Currency { get; set; } = "1112005";
        public static string CrMasSysQuestionsAnswer { get; set; } = "1112006";

        ////////////
        //MAS  Post services
        public static string CrMasSysCallingKeys { get; set; } = "1109001";
        public static string Region { get; set; } = "1109002";
        public static string City { get; set; } = "1109003";


        ////////////
        //MAS  Contract services
        public static string CrMasSupContractAdditional { get; set; } = "1111001";
        public static string CrMasSupContractOption { get; set; } = "1111002";
        public static string CrMasSupContractCarCheckup { get; set; } = "1111003";
        public static string CrMasSupContractCarCheckupDetail { get; set; } = "1111004";
        // Company Data
        public static string CrMasLessorInformation { get; set; } = "1101001";

        ////////////
        //MAS  Renters
        public static string CrMasRenterInformation { get; set; } = "1103001";
        public static string CrCasRenterContractBasic { get; set; } = "1103002";

        /////////
        // Mas bnan
        public static string lessorKSA { get; set; } = "1102001";
        public static string lessor_Marketing { get; set; } = "1102002";

        /////////
        // Mas Companies
        public static string Company_Data { get; set; } = "1101001";
        public static string Support_Photos { get; set; } = "1101002";
        public static string Contracts { get; set; } = "1101003";
        public static string Dues { get; set; } = "1101004";
        public static string Payment_Of_Dues { get; set; } = "1101005";
        public static string CompanyMessages { get; set; } = "1101006";

        /////////
        // Mas Reports 
        public static string MasReport1 { get; set; } = "1104001";
        public static string MasReport2 { get; set; } = "1104002";
        public static string MasReport3 { get; set; } = "1104003";
        public static string MasReport4 { get; set; } = "1104004";
        public static string MasReport5 { get; set; } = "1104005";
        public static string MasReport6 { get; set; } = "1104006";
        public static string MasReport7 { get; set; } = "1104007";
        public static string MasReport8 { get; set; } = "1104008";
        public static string MasReport9 { get; set; } = "1104009";


        /////////
        // Mas Statistics
        public static string MasStatistics_Renters { get; set; } = "1105001";
        public static string MasStatistics_Cars { get; set; } = "1105002";
        public static string MasStatistics_Contracts { get; set; } = "1105003";
        public static string MasStatistics_RenterContracts { get; set; } = "1105004";
        public static string MasStatistics_CarContracts { get; set; } = "1105005";
        /////////
        // CAS Employees

        ////////////
        //CAS  Users
        public static string CrMasUserInformationForCAS { get; set; } = "2207001";
        public static string CrMasUserSystemValiditionsCAS { get; set; } = "2207002";
        public static string CrMasUserContractValiditionsCAS { get; set; } = "2207003";
        public static string MyAccountCAS { get; set; } = "2207004";
        public static string ChangePasswordCAS { get; set; } = "2207005";

        //CAS  Company
        public static string Branches_CAS { get; set; } = "2201001";
        public static string Documents_CAS { get; set; } = "2201002";
        public static string Owners_CAS { get; set; } = "2201003";

        //MAS  Services
        public static string TechnicalConnectivityCAS { get; set; } = "2208004";
        public static string LessorMechanizmCAS { get; set; } = "2208005";
        public static string LessorMembershipCAS { get; set; } = "2208006";
        //public static string CrMasUserInformationFromMASToCAS { get; set; } = "1106003";

        // CAS Reports
        public static string Administrative_procedures_Report_Cas { get; set; } = "2205001";
        public static string Daily_Report_Cas { get; set; } = "2205002";
        public static string Active_contracts_Report_Cas { get; set; } = "2205003";
        public static string Closed_contracts_Report_Cas { get; set; } = "2205004";
        public static string SavedAndCanceled_contracts_Report_Cas { get; set; } = "2205005";
        public static string Shomoos_contracts_Report_Cas { get; set; } = "2205006";
        public static string Car_contracts_Report_Cas { get; set; } = "2205007";
        public static string Employers_contracts_Report_Cas { get; set; } = "2205008";
        public static string Drivers_contracts_Report_Cas { get; set; } = "2205009";
        public static string FT_Employers_Report_Cas { get; set; } = "2205010";
        public static string FT_Renters_Report_Cas { get; set; } = "2205011";
        public static string FT_SalesPoints_Report_Cas { get; set; } = "2205012";


        // CAS Statistics
        public static string CasStatistics_Cars { get; set; } = "2206001";
        public static string CasStatistics_Renters { get; set; } = "2206002";
        public static string CasStatistics_Contracts { get; set; } = "2206003";
        public static string CasStatistics_CarContracts { get; set; } = "2206004";
        public static string CasStatistics_RenterContracts { get; set; } = "2206005";

        // CAS Services
        public static string ServicesCas_Banks { get; set; } = "2208001";
        public static string ServicesCas_SalesPoints { get; set; } = "2208002";
        public static string ServicesCas_Drivers { get; set; } = "2208003";
        public static string ServicesCas_TechnicalConnectivity { get; set; } = "2208004";
        public static string ServicesCas_Alerts { get; set; } = "2208005";
        public static string ServicesCas_Memberships { get; set; } = "2208006";

        // CAS Renters
        public static string RentersCas_RentersData { get; set; } = "2203001";
        public static string RentersCas_RentersContracts { get; set; } = "2203002";
        public static string RentersCas_RentersDebits { get; set; } = "2203003";
        public static string RentersCas_RentersMessages { get; set; } = "2203004";
    }
}


