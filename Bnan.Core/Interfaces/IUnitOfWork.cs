using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenric<CrMasSysMainTask> CrMasSysMainTasks { get; }
        public IGenric<CrMasSysSubTask> CrMasSysSubTasks { get; }
        public IGenric<CrMasUserLogin> CrMasUserLogins { get; }
        public IGenric<CrMasSysSystem> CrMasSysSystems { get; }
        public IGenric<CrMasLessorInformation> CrMasLessorInformation { get; }
        public IGenric<CrMasUserMainValidation> CrMasUserMainValidations { get; }
        public IGenric<CrMasUserSubValidation> CrMasUserSubValidations { get; }
        public IGenric<CrMasUserProceduresValidation> CrMasUserProceduresValidations { get; }
        public IGenric<CrMasSysProcedure> CrMasSysProcedure { get; }
        public IGenric<CrMasSysProceduresTask> CrMasSysProceduresTasks { get; }
        public IGenric<CrMasSysCallingKey> CrMasSysCallingKeys { get; }
        public IGenric<CrMasSysQuestionsAnswer> CrMasSysQuestionsAnswer { get; }
        public IGenric<CrMasLessorImage> CrMasLessorImage { get; }
        public IGenric<CrCasOwner> CrCasOwners { get; }
        public IGenric<CrCasBeneficiary> CrCasBeneficiary { get; }
        public IGenric<CrCasLessorMembership> CrCasLessorMembership { get; }
        public IGenric<CrMasSupRenterMembership> CrMasSupRenterMembership { get; }
        public IGenric<CrCasLessorMechanism> CrCasLessorMechanism { get; }
        public IGenric<CrMasContractCompany> CrMasContractCompany { get; }
        public IGenric<CrCasBranchInformation> CrCasBranchInformation { get; }
        public IGenric<CrCasBranchDocument> CrCasBranchDocument { get; }
        public IGenric<CrMasSupPostCity> CrMasSupPostCity { get; }
        public IGenric<CrCasBranchPost> CrCasBranchPost { get; }
        public IGenric<CrCasAccountBank> CrCasAccountBank { get; }
        public IGenric<CrCasAccountSalesPoint> CrCasAccountSalesPoint { get; }
        public IGenric<CrMasSupAccountBank> CrMasSupAccountBanks { get; }
        public IGenric<CrCasLessorClassification> CrCasLessorClassification { get; }
        public IGenric<CrMasContractCompanyDetailed> CrMasContractCompanyDetailed { get; }
        public IGenric<CrMasSupCarDistribution> CrMasSupCarDistribution { get; }
        public IGenric<CrMasSupAccountPaymentMethod> CrMasSupAccountPaymentMethod { get; }
        public IGenric<CrMasSupAccountReference> CrMasSupAccountReference { get; }
        public IGenric<CrMasSupCarCategory> CrMasSupCarCategory { get; }
        public IGenric<CrMasSupCarModel> CrMasSupCarModel { get; }
        public IGenric<CrMasUserInformation> CrMasUserInformation { get; }
        public IGenric<CrMasSupContractAdditional> CrMasSupContractAdditional { get; }
        public IGenric<CrMasSupContractOption> CrMasSupContractOption { get; }
        public IGenric<CrMasSupContractCarCheckup> CrMasSupContractCarCheckup { get; }
        public IGenric<CrMasUserBranchValidity> CrMasUserBranchValidity { get; }
        public IGenric<CrCasLessorWhatsupConnect> CrCasLessorWhatsupConnect { get; }

        public IGenric<CrMasSupPostRegion> CrMasSupPostRegion { get; }

        public IGenric<CrCasRenterPrivateDriverInformation> CrCasRenterPrivateDriverInformation { get; }
        public IGenric<CrMasSupRenterDrivingLicense> CrMasSupRenterDrivingLicense { get; }

        public IGenric<CrMasSupRenterIdtype> CrMasSupRenterIdtype { get; }

        public IGenric<CrMasSupRenterGender> CrMasSupRenterGender { get; }

        public IGenric<CrMasSupRenterNationality> CrMasSupRenterNationality { get; }
        public IGenric<CrMasUserContractValidity> CrMasUserContractValidity { get; }
        public IGenric<CrCasSysAdministrativeProcedure> CrCasSysAdministrativeProcedure { get; }
        public IGenric<CrCasCarInformation> CrCasCarInformation { get; }
        public IGenric<CrMasSupCarRegistration> CrMasSupCarRegistration { get; }
        public IGenric<CrMasSupCarFuel> CrMasSupCarFuel { get; }
        public IGenric<CrMasSupCarOil> CrMasSupCarOil { get; }
        public IGenric<CrMasSupCarCvt> CrMasSupCarCvt { get; }
        public IGenric<CrMasSupCarColor> CrMasSupCarColor { get; }
        public IGenric<CrCasCarDocumentsMaintenance> CrCasCarDocumentsMaintenance { get; }
        public IGenric<CrMasSupCarAdvantage> CrMasSupCarAdvantage { get; }
        public IGenric<CrCasCarAdvantage> CrCasCarAdvantage { get; }
        public IGenric<CrCasPriceCarBasic> CrCasPriceCarBasic { get; }
        public IGenric<CrCasPriceCarAdvantage> CrCasPriceCarAdvantage { get; }
        public IGenric<CrCasPriceCarAdditional> CrCasPriceCarAdditional { get; }
        public IGenric<CrCasPriceCarOption> CrCasPriceCarOption { get; }
        public IGenric<CrMasSysProbabilityMembership> CrMasSysProbabilityMembership { get; }
        public IGenric<CrMasRenterInformation> CrMasRenterInformation { get; }
        public IGenric<CrMasSupRenterSector> CrMasSupRenterSector { get; }
        public IGenric<CrMasSupRenterProfession> CrMasSupRenterProfession { get; }
        public IGenric<CrMasSupRenterEmployer> CrMasSupRenterEmployer { get; }
        public IGenric<CrMasRenterPost> CrMasRenterPost { get; }
        public IGenric<CrCasRenterLessor> CrCasRenterLessor { get; }
        public IGenric<CrCasRenterContractChoice> CrCasRenterContractChoice { get; }
        public IGenric<CrCasRenterContractAdditional> CrCasRenterContractAdditional { get; }
        public IGenric<CrCasRenterContractCarCheckup> CrCasRenterContractCarCheckup { get; }
        public IGenric<CrMasSupContractCarCheckupDetail> CrMasSupContractCarCheckupDetail { get; }
        public IGenric<CrCasRenterContractBasic> CrCasRenterContractBasic { get; }
        public IGenric<CrCasRenterContractAdvantage> CrCasRenterContractAdvantage { get; }
        public IGenric<CrCasRenterContractAuthorization> CrCasRenterContractAuthorization { get; }
        public IGenric<CrCasAccountReceipt> CrCasAccountReceipt { get; }
        public IGenric<CrCasRenterContractAlert> CrCasRenterContractAlert { get; }
        public IGenric<CrMasSysEvaluation> CrMasSysEvaluation { get; }
        public IGenric<CrMasSupCarBrand> CrMasSupCarBrand { get; }
        public IGenric<CrCasRenterContractStatistic> CrCasRenterContractStatistic { get; }
        public IGenric<CrCasAccountContractTaxOwed> CrCasAccountContractTaxOwed { get; }
        public IGenric<CrCasAccountContractCompanyOwed> CrCasAccountContractCompanyOwed { get; }
        public IGenric<CrCasAccountInvoice> CrCasAccountInvoice { get; }
        public IGenric<CrMasSysConvertNoToText> CrMasSysConvertNoToText { get; }
        public IGenric<CrMasSupCarYear> CrMasSupCarYear { get; }
        public IGenric<CrMasSupCountryClassification> CrMasSupCountryClassification { get; }

        public IGenric<CrCasRenterContractEvaluation> CrCasRenterContractEvaluation { get; }
        public IGenric<CrMasSupContractSource> CrMasSupContractSource { get; }
        public IGenric<CrCasLessorTgaConnect> CrCasLessorTgaConnect { get; }
        public IGenric<CrCasLessorSmsConnect> CrCasLessorSmsConnect { get; }
        public IGenric<CrCasLessorShomoosConnect> CrCasLessorShomoosConnect { get; }
        public IGenric<CrCasLessorPolicy> CrCasLessorPolicy { get; }
        int Complete();
        Task<int> CompleteAsync();
    }
}
