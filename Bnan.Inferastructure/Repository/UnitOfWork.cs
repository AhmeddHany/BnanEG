﻿using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BnanEGContext _context;


        public IGenric<CrMasSysMainTask> CrMasSysMainTasks { get; private set; }
        public IGenric<CrMasSysSubTask> CrMasSysSubTasks { get; private set; }
        public IGenric<CrMasUserLogin> CrMasUserLogins { get; private set; }
        public IGenric<CrMasSysSystem> CrMasSysSystems { get; private set; }
        public IGenric<CrMasLessorInformation> CrMasLessorInformation { get; private set; }
        public IGenric<CrMasSysProcedure> CrMasSysProcedure { get; private set; }
        public IGenric<CrMasUserMainValidation> CrMasUserMainValidations { get; private set; }
        public IGenric<CrMasUserSubValidation> CrMasUserSubValidations { get; private set; }
        public IGenric<CrMasUserProceduresValidation> CrMasUserProceduresValidations { get; private set; }
        public IGenric<CrMasSysProceduresTask> CrMasSysProceduresTasks { get; private set; }
        public IGenric<CrMasSysCallingKey> CrMasSysCallingKeys { get; private set; }
        public IGenric<CrMasSysQuestionsAnswer> CrMasSysQuestionsAnswer { get; private set; }
        public IGenric<CrMasLessorImage> CrMasLessorImage { get; private set; }
        public IGenric<CrCasOwner> CrCasOwners { get; private set; }
        public IGenric<CrCasBeneficiary> CrCasBeneficiary { get; private set; }
        public IGenric<CrCasLessorMembership> CrCasLessorMembership { get; private set; }
        public IGenric<CrMasSupRenterMembership> CrMasSupRenterMembership { get; private set; }
        public IGenric<CrCasLessorMechanism> CrCasLessorMechanism { get; private set; }
        public IGenric<CrMasContractCompany> CrMasContractCompany { get; private set; }
        public IGenric<CrCasBranchInformation> CrCasBranchInformation { get; private set; }
        public IGenric<CrCasBranchDocument> CrCasBranchDocument { get; private set; }
        public IGenric<CrMasSupPostCity> CrMasSupPostCity { get; private set; }
        public IGenric<CrCasBranchPost> CrCasBranchPost { get; private set; }
        public IGenric<CrCasAccountBank> CrCasAccountBank { get; private set; }
        public IGenric<CrCasAccountSalesPoint> CrCasAccountSalesPoint { get; private set; }
        public IGenric<CrMasSupAccountBank> CrMasSupAccountBanks { get; private set; }
        public IGenric<CrMasSupAccountPaymentMethod> CrMasSupAccountPaymentMethod { get; private set; }
        public IGenric<CrMasSupAccountReference> CrMasSupAccountReference { get; private set; }
        public IGenric<CrCasLessorClassification> CrCasLessorClassification { get; private set; }
        public IGenric<CrMasContractCompanyDetailed> CrMasContractCompanyDetailed { get; private set; }
        public IGenric<CrMasSupCarDistribution> CrMasSupCarDistribution { get; private set; }
        public IGenric<CrMasUserInformation> CrMasUserInformation { get; private set; }
        public IGenric<CrMasSupCarCategory> CrMasSupCarCategory { get; private set; }
        public IGenric<CrMasSupCarModel> CrMasSupCarModel { get; private set; }
        public IGenric<CrMasSupCarOil> CrMasSupCarOil { get; private set; }
        public IGenric<CrMasSupContractAdditional> CrMasSupContractAdditional { get; private set; }

        public IGenric<CrMasSupContractOption> CrMasSupContractOption { get; private set; }

        public IGenric<CrMasSupContractCarCheckup> CrMasSupContractCarCheckup { get; private set; }
        public IGenric<CrMasUserBranchValidity> CrMasUserBranchValidity { get; private set; }
        public IGenric<CrCasLessorWhatsupConnect> CrCasLessorWhatsupConnect { get; }

        public IGenric<CrMasSupPostRegion> CrMasSupPostRegion { get; private set; }
        public IGenric<CrCasRenterPrivateDriverInformation> CrCasRenterPrivateDriverInformation { get; private set; }


        public IGenric<CrMasSupRenterDrivingLicense> CrMasSupRenterDrivingLicense { get; private set; }

        public IGenric<CrMasSupRenterIdtype> CrMasSupRenterIdtype { get; private set; }

        public IGenric<CrMasSupRenterGender> CrMasSupRenterGender { get; private set; }

        public IGenric<CrMasSupRenterNationality> CrMasSupRenterNationality { get; private set; }
        public IGenric<CrMasUserContractValidity> CrMasUserContractValidity { get; private set; }
        public IGenric<CrCasSysAdministrativeProcedure> CrCasSysAdministrativeProcedure { get; private set; }
        public IGenric<CrCasCarInformation> CrCasCarInformation { get; private set; }
        public IGenric<CrMasSupCarRegistration> CrMasSupCarRegistration { get; private set; }
        public IGenric<CrMasSupCarCvt> CrMasSupCarCvt { get; private set; }
        public IGenric<CrMasSupCarFuel> CrMasSupCarFuel { get; private set; }
        public IGenric<CrMasSupCarColor> CrMasSupCarColor { get; private set; }
        public IGenric<CrCasCarDocumentsMaintenance> CrCasCarDocumentsMaintenance { get; private set; }
        public IGenric<CrMasSupCarAdvantage> CrMasSupCarAdvantage { get; private set; }
        public IGenric<CrCasCarAdvantage> CrCasCarAdvantage { get; private set; }
        public IGenric<CrCasPriceCarBasic> CrCasPriceCarBasic { get; private set; }
        public IGenric<CrCasPriceCarAdvantage> CrCasPriceCarAdvantage { get; private set; }
        public IGenric<CrCasPriceCarAdditional> CrCasPriceCarAdditional { get; private set; }
        public IGenric<CrCasPriceCarOption> CrCasPriceCarOption { get; private set; }
        public IGenric<CrMasSysProbabilityMembership> CrMasSysProbabilityMembership { get; private set; }
        public IGenric<CrMasRenterInformation> CrMasRenterInformation { get; private set; }
        public IGenric<CrMasSupRenterSector> CrMasSupRenterSector { get; private set; }
        public IGenric<CrMasSupRenterProfession> CrMasSupRenterProfession { get; private set; }
        public IGenric<CrMasSupRenterEmployer> CrMasSupRenterEmployer { get; private set; }
        public IGenric<CrMasRenterPost> CrMasRenterPost { get; private set; }
        public IGenric<CrCasRenterLessor> CrCasRenterLessor { get; private set; }
        public IGenric<CrCasRenterContractChoice> CrCasRenterContractChoice { get; private set; }
        public IGenric<CrCasRenterContractAdditional> CrCasRenterContractAdditional { get; private set; }
        public IGenric<CrCasRenterContractCarCheckup> CrCasRenterContractCarCheckup { get; private set; }
        public IGenric<CrMasSupContractCarCheckupDetail> CrMasSupContractCarCheckupDetail { get; private set; }

        public IGenric<CrCasRenterContractBasic> CrCasRenterContractBasic { get; private set; }
        public IGenric<CrCasRenterContractAdvantage> CrCasRenterContractAdvantage { get; private set; }
        public IGenric<CrCasRenterContractAuthorization> CrCasRenterContractAuthorization { get; private set; }
        public IGenric<CrCasAccountReceipt> CrCasAccountReceipt { get; private set; }
        public IGenric<CrCasRenterContractAlert> CrCasRenterContractAlert { get; private set; }
        public IGenric<CrMasSysEvaluation> CrMasSysEvaluation { get; private set; }
        public IGenric<CrMasSupCarBrand> CrMasSupCarBrand { get; private set; }
        public IGenric<CrCasRenterContractStatistic> CrCasRenterContractStatistic { get; private set; }
        public IGenric<CrCasAccountContractTaxOwed> CrCasAccountContractTaxOwed { get; private set; }
        public IGenric<CrCasAccountContractCompanyOwed> CrCasAccountContractCompanyOwed { get; private set; }
        public IGenric<CrCasAccountInvoice> CrCasAccountInvoice { get; private set; }
        public IGenric<CrMasSysConvertNoToText> CrMasSysConvertNoToText { get; private set; }
        public IGenric<CrMasSupCarYear> CrMasSupCarYear { get; private set; }
        public IGenric<CrMasSupCountryClassification> CrMasSupCountryClassification { get; private set; }

        public IGenric<CrCasRenterContractEvaluation> CrCasRenterContractEvaluation { get; private set; }
        public IGenric<CrMasSupContractSource> CrMasSupContractSource { get; private set; }
        public IGenric<CrCasLessorShomoosConnect> CrCasLessorShomoosConnect { get; private set; }
        public IGenric<CrCasLessorSmsConnect> CrCasLessorSmsConnect { get; private set; }
        public IGenric<CrCasLessorTgaConnect> CrCasLessorTgaConnect { get; private set; }
        public IGenric<CrCasLessorPolicy> CrCasLessorPolicy { get; private set; }
        public IGenric<CrMasSupContractCloseSuspension> CrMasSupContractCloseSuspension { get; private set; }
        public IGenric<CrMasSupContractCloseMechanism> CrMasSupContractCloseMechanism { get; private set; }


        public UnitOfWork(BnanEGContext context)
        {
            _context = context;
            CrMasSysMainTasks = new BaseRepository<CrMasSysMainTask>(_context);
            CrMasSysSubTasks = new BaseRepository<CrMasSysSubTask>(_context);
            CrMasUserLogins = new BaseRepository<CrMasUserLogin>(_context);
            CrMasSysSystems = new BaseRepository<CrMasSysSystem>(_context);
            CrMasLessorInformation = new BaseRepository<CrMasLessorInformation>(_context);
            CrMasSysProcedure = new BaseRepository<CrMasSysProcedure>(_context);
            CrMasUserMainValidations = new BaseRepository<CrMasUserMainValidation>(_context);
            CrMasUserSubValidations = new BaseRepository<CrMasUserSubValidation>(_context);
            CrMasUserProceduresValidations = new BaseRepository<CrMasUserProceduresValidation>(_context);
            CrMasSysProceduresTasks = new BaseRepository<CrMasSysProceduresTask>(_context);
            CrMasSysCallingKeys = new BaseRepository<CrMasSysCallingKey>(_context);
            CrMasSysQuestionsAnswer = new BaseRepository<CrMasSysQuestionsAnswer>(_context);
            CrMasLessorImage = new BaseRepository<CrMasLessorImage>(_context);
            CrCasOwners = new BaseRepository<CrCasOwner>(_context);
            CrCasBeneficiary = new BaseRepository<CrCasBeneficiary>(_context);
            CrCasLessorMembership = new BaseRepository<CrCasLessorMembership>(_context);
            CrMasSupRenterMembership = new BaseRepository<CrMasSupRenterMembership>(_context);
            CrCasLessorMechanism = new BaseRepository<CrCasLessorMechanism>(_context);
            CrMasContractCompany = new BaseRepository<CrMasContractCompany>(_context);
            CrCasBranchInformation = new BaseRepository<CrCasBranchInformation>(_context);
            CrCasBranchDocument = new BaseRepository<CrCasBranchDocument>(_context);
            CrMasSupPostCity = new BaseRepository<CrMasSupPostCity>(_context);
            CrCasBranchPost = new BaseRepository<CrCasBranchPost>(_context);
            CrCasAccountBank = new BaseRepository<CrCasAccountBank>(_context);
            CrCasAccountSalesPoint = new BaseRepository<CrCasAccountSalesPoint>(_context);
            CrMasSupAccountBanks = new BaseRepository<CrMasSupAccountBank>(_context);
            CrMasSupAccountPaymentMethod = new BaseRepository<CrMasSupAccountPaymentMethod>(_context);
            CrMasSupAccountReference = new BaseRepository<CrMasSupAccountReference>(_context);
            CrCasLessorClassification = new BaseRepository<CrCasLessorClassification>(_context);
            CrMasContractCompanyDetailed = new BaseRepository<CrMasContractCompanyDetailed>(_context);
            CrMasSupCarDistribution = new BaseRepository<CrMasSupCarDistribution>(_context);
            CrMasSupContractAdditional = new BaseRepository<CrMasSupContractAdditional>(_context);
            CrMasSupContractOption = new BaseRepository<CrMasSupContractOption>(_context);
            CrMasSupContractCarCheckup = new BaseRepository<CrMasSupContractCarCheckup>(_context);
            CrMasSupRenterDrivingLicense = new BaseRepository<CrMasSupRenterDrivingLicense>(_context);
            CrMasUserInformation = new BaseRepository<CrMasUserInformation>(_context);
            CrMasSupCarCategory = new BaseRepository<CrMasSupCarCategory>(_context);
            CrMasSupCarModel = new BaseRepository<CrMasSupCarModel>(_context);
            CrMasSupPostRegion = new BaseRepository<CrMasSupPostRegion>(_context);
            CrMasUserBranchValidity = new BaseRepository<CrMasUserBranchValidity>(_context);
            CrCasRenterPrivateDriverInformation = new BaseRepository<CrCasRenterPrivateDriverInformation>(_context);
            CrMasSupRenterIdtype = new BaseRepository<CrMasSupRenterIdtype>(_context);
            CrMasSupRenterGender = new BaseRepository<CrMasSupRenterGender>(_context);
            CrMasSupRenterNationality = new BaseRepository<CrMasSupRenterNationality>(_context);
            CrMasUserContractValidity = new BaseRepository<CrMasUserContractValidity>(_context);
            CrCasSysAdministrativeProcedure = new BaseRepository<CrCasSysAdministrativeProcedure>(_context);
            CrCasCarInformation = new BaseRepository<CrCasCarInformation>(_context);
            CrMasSupCarRegistration = new BaseRepository<CrMasSupCarRegistration>(_context);
            CrMasSupCarOil = new BaseRepository<CrMasSupCarOil>(_context);
            CrMasSupCarFuel = new BaseRepository<CrMasSupCarFuel>(_context);
            CrMasSupCarCvt = new BaseRepository<CrMasSupCarCvt>(_context);
            CrMasSupCarColor = new BaseRepository<CrMasSupCarColor>(_context);
            CrCasCarDocumentsMaintenance = new BaseRepository<CrCasCarDocumentsMaintenance>(_context);
            CrMasSupCarAdvantage = new BaseRepository<CrMasSupCarAdvantage>(_context);
            CrCasCarAdvantage = new BaseRepository<CrCasCarAdvantage>(_context);
            CrCasPriceCarBasic = new BaseRepository<CrCasPriceCarBasic>(_context);
            CrCasPriceCarAdvantage = new BaseRepository<CrCasPriceCarAdvantage>(_context);
            CrCasPriceCarOption = new BaseRepository<CrCasPriceCarOption>(_context);
            CrCasPriceCarAdditional = new BaseRepository<CrCasPriceCarAdditional>(_context);
            CrMasSysProbabilityMembership = new BaseRepository<CrMasSysProbabilityMembership>(_context);
            CrMasRenterInformation = new BaseRepository<CrMasRenterInformation>(_context);
            CrMasSupRenterSector = new BaseRepository<CrMasSupRenterSector>(_context);
            CrMasSupRenterProfession = new BaseRepository<CrMasSupRenterProfession>(_context);
            CrMasSupRenterEmployer = new BaseRepository<CrMasSupRenterEmployer>(_context);
            CrMasRenterPost = new BaseRepository<CrMasRenterPost>(_context);
            CrCasRenterLessor = new BaseRepository<CrCasRenterLessor>(_context);
            CrCasRenterContractChoice = new BaseRepository<CrCasRenterContractChoice>(_context);
            CrCasRenterContractAdditional = new BaseRepository<CrCasRenterContractAdditional>(_context);
            CrCasRenterContractCarCheckup = new BaseRepository<CrCasRenterContractCarCheckup>(_context);
            CrMasSupContractCarCheckupDetail = new BaseRepository<CrMasSupContractCarCheckupDetail>(_context);
            CrCasRenterContractBasic = new BaseRepository<CrCasRenterContractBasic>(_context);
            CrCasRenterContractAdvantage = new BaseRepository<CrCasRenterContractAdvantage>(_context);
            CrCasRenterContractAuthorization = new BaseRepository<CrCasRenterContractAuthorization>(_context);
            CrCasAccountReceipt = new BaseRepository<CrCasAccountReceipt>(_context);
            CrCasRenterContractAlert = new BaseRepository<CrCasRenterContractAlert>(_context);
            CrMasSysEvaluation = new BaseRepository<CrMasSysEvaluation>(_context);
            CrMasSupCarBrand = new BaseRepository<CrMasSupCarBrand>(_context);
            CrCasRenterContractStatistic = new BaseRepository<CrCasRenterContractStatistic>(_context);
            CrCasAccountContractTaxOwed = new BaseRepository<CrCasAccountContractTaxOwed>(_context);
            CrCasAccountContractCompanyOwed = new BaseRepository<CrCasAccountContractCompanyOwed>(_context);
            CrCasAccountInvoice = new BaseRepository<CrCasAccountInvoice>(_context);
            CrMasSysConvertNoToText = new BaseRepository<CrMasSysConvertNoToText>(_context);
            CrMasSupCarYear = new BaseRepository<CrMasSupCarYear>(_context);
            CrCasRenterContractEvaluation = new BaseRepository<CrCasRenterContractEvaluation>(_context);
            CrMasSupCountryClassification = new BaseRepository<CrMasSupCountryClassification>(_context);
            CrCasLessorWhatsupConnect = new BaseRepository<CrCasLessorWhatsupConnect>(_context);
            CrMasSupContractSource = new BaseRepository<CrMasSupContractSource>(_context);
            CrCasLessorTgaConnect = new BaseRepository<CrCasLessorTgaConnect>(_context);
            CrCasLessorSmsConnect = new BaseRepository<CrCasLessorSmsConnect>(_context);
            CrCasLessorShomoosConnect = new BaseRepository<CrCasLessorShomoosConnect>(_context);
            CrCasLessorPolicy = new BaseRepository<CrCasLessorPolicy>(_context);
            CrMasSupContractCloseSuspension = new BaseRepository<CrMasSupContractCloseSuspension>(_context);
            CrMasSupContractCloseMechanism = new BaseRepository<CrMasSupContractCloseMechanism>(_context);
        }
        public int Complete()
        {
            return _context.SaveChanges();
        }
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
