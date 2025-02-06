using AutoMapper;
using Bnan.Core.Models;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.BS.CreateContract;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.CAS.Employees;
using Bnan.Ui.ViewModels.CAS.Services;
using Bnan.Ui.ViewModels.Identitiy;
using Bnan.Ui.ViewModels.MAS;
using Bnan.Ui.ViewModels.MAS.Users;
using Bnan.Ui.ViewModels.Owners;

namespace Bnan.Inferastructure
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            CreateMap<CrMasLessorInformationVM, CrMasLessorInformation>();
            CreateMap<CrMasLessorInformation, CrMasLessorInformationVM>().ForMember(x => x.CrMasLessorInformationGovernmentNo, opt => opt.MapFrom(y => y.CrMasLessorInformationGovernmentNo.Trim()))
                                                                         .ForMember(x => x.CrMasLessorInformationTaxNo, opt => opt.MapFrom(y => y.CrMasLessorInformationTaxNo.Trim()))
                                                                         .ForMember(x => x.CrMasLessorInformationCommunicationMobile, opt => opt.MapFrom(y => y.CrMasLessorInformationCommunicationMobile.Trim()))
                                                                         .ForMember(x => x.CrMasLessorInformationCallFree, opt => opt.MapFrom(y => y.CrMasLessorInformationCallFree.Trim()))
                                                                         .ForMember(x => x.CrMasLessorInformationCommunicationMobile, opt => opt.MapFrom(y => y.CrMasLessorInformationCommunicationMobile.Trim()))
                                                                         .ForMember(x => x.CrMasLessorInformationTwiter, opt => opt.MapFrom(y => y.CrMasLessorInformationTwiter.Trim()));
            CreateMap<RegisterViewModel, CrMasUserInformation>().ReverseMap();

            CreateMap<CrMasSysProcedureVM, CrMasSysProcedure>().ReverseMap();

            CreateMap<LessorImagesVM, CrMasLessorImage>().ReverseMap();

            CreateMap<CrCasBranchPost, BranchPostVM>().ReverseMap();
            CreateMap<ContractCompanyVM, CrMasContractCompany>().ReverseMap();
            CreateMap<CrMasSupAccountBank, MasAccountBankVM>().ReverseMap();
            CreateMap<CrMasSupAccountPaymentMethod, AccountPaymentMethodVM>().ReverseMap();

            CreateMap<MasAccountReferenceVM, CrMasSupAccountReference>();
            CreateMap<CrMasSupAccountReference, MasAccountReferenceVM>();

            CreateMap<ContractAdditionalVM, CrMasSupContractAdditional>();
            CreateMap<CrMasSupContractAdditional, ContractAdditionalVM>();

            CreateMap<ContractOptionsVM, CrMasSupContractOption>();
            CreateMap<CrMasSupContractOption, ContractOptionsVM>();

            CreateMap<CrMasSupPostCity, PostCityVM>().ReverseMap();

            CreateMap<CrMasSupCarDistribution, CrMasSupCarDistributionVM>().ReverseMap();

            CreateMap<CrMasSupContractCarCheckup, ContractCarCheckupVM>().ReverseMap();

            CreateMap<CrMasSupContractCarCheckupDetail, ContractCarCheckupDetailsVM>().ReverseMap();

            CreateMap<CrCasAccountBank, AccountBankVM>().ReverseMap();
            CreateMap<CrCasAccountBank, CAS_AccountBankVM>().ReverseMap();
            CreateMap<CrCasAccountSalesPoint, CAS_AccountSalesPointVM>().ReverseMap();
            CreateMap<CrCasOwner, CASContractSourceVM>().ReverseMap();
            




            CreateMap<CrCasRenterPrivateDriverInformation, RenterDriverVM>().ReverseMap();

            CreateMap<BranchVM, CrCasBranchInformation>().ReverseMap();

            CreateMap<ContractValiditionsVM, CrMasUserContractValidity>().ReverseMap();

            CreateMap<CrCasBranchPost, BranchPost1VM>().ReverseMap();

            CreateMap<CrCasBranchDocument, BranchDocumentVM>().ReverseMap();
            CreateMap<SalesPointsVM, CrCasAccountSalesPoint>().ReverseMap();

            CreateMap<MechanismVM, CrCasLessorMechanism>().ReverseMap();
            CreateMap<OwnersVM, CrCasOwner>().ReverseMap();
            CreateMap<CrCasSysAdministrativeProcedure, AdminstritiveProceduresVM>().ReverseMap();
            CreateMap<CrCasCarInformation, CarsInforamtionVM>().ReverseMap();
            CreateMap<CrCasCarDocumentsMaintenance, DocumentsMaintainceCarVM>().ReverseMap();
            CreateMap<CarPriceVM, CrCasPriceCarBasic>().ReverseMap();

            CreateMap<RenterLessorVM, CrCasRenterLessor>().ReverseMap();

            CreateMap<ContractForExtensionVM, CrCasRenterContractBasic>().ReverseMap();

            CreateMap<CarBrandVM, CrMasSupCarBrand>().ReverseMap();

            CreateMap<RenterNationalityVM, CrMasSupRenterNationality>().ReverseMap();

            CreateMap<CarFuelVM, CrMasSupCarFuel>().ReverseMap();
            CreateMap<CarOilVM, CrMasSupCarOil>().ReverseMap();

            CreateMap<CrMasSupCarCvt, CarCvtVM>().ReverseMap();

            CreateMap<CarCategoryVM, CrMasSupCarCategory>().ReverseMap();

            CreateMap<CarAdvantageVM, CrMasSupCarAdvantage>().ReverseMap();

            CreateMap<CarRegistrationVM, CrMasSupCarRegistration>().ReverseMap();

            CreateMap<UserLoginVM, CrMasUserLogin>().ReverseMap();

            CreateMap<RenterIdtypeVM, CrMasSupRenterIdtype>().ReverseMap();

            CreateMap<RenterInformationsVM, CrMasRenterInformation>().ReverseMap();

            CreateMap<RenterLessorInformationVM, CrCasRenterLessor>().ReverseMap();
            CreateMap<RateVM, CrMasSysEvaluation>().ReverseMap();
            CreateMap<CurrencyVM, CrMasSysConvertNoToText>().ReverseMap();

            CreateMap<CasRenterContractVM, CrCasRenterLessor>().ReverseMap();
            CreateMap<RenterInfoVM, CrMasRenterInformation>().ReverseMap();

            CreateMap<DriverContractVM, CrCasRenterContractBasic>().ReverseMap();
            CreateMap<DetailsContractsForRenterVM, CrCasRenterContractBasic>().ReverseMap();
            CreateMap<CarInfomationVM, CrCasCarInformation>().ReverseMap();
            CreateMap<CrCasRenterContractBasic, ContractSettlementVM>().ReverseMap();
            //Owners
            CreateMap<CrCasRenterContractBasic, OwnContractsVM>().ReverseMap();
            CreateMap<CrCasCarInformation, OwnCarsInfoVM>().ReverseMap();
            CreateMap<CrCasRenterLessor, OwnRentersVM>().ReverseMap();
            CreateMap<CrCasBranchInformation, OwnBranchVM>().ReverseMap();
            CreateMap<CrMasUserInformation, OwnEmployeesVM>().ReverseMap();

            //MAS
            CreateMap<CarColorVM, CrMasSupCarColor>().ReverseMap();
            CreateMap<RenterDrivingLicenseVM, CrMasSupRenterDrivingLicense>().ReverseMap();
            CreateMap<RenterProfessionVM, CrMasSupRenterProfession>().ReverseMap();
            CreateMap<RenterEmployerVM, CrMasSupRenterEmployer>().ReverseMap();
            CreateMap<RenterSectorVM, CrMasSupRenterSector>().ReverseMap();
            CreateMap<CarModelVM, CrMasSupCarModel>().ReverseMap();
            CreateMap<RenterMembershipVM, CrMasSupRenterMembership>().ReverseMap();
            CreateMap<RenterGenderVM, CrMasSupRenterGender>().ReverseMap();
            CreateMap<LessorClassificationVM, CrCasLessorClassification>().ReverseMap();
            CreateMap<CountriesVM, CrMasSysCallingKey>().ReverseMap();
            CreateMap<PostRegionsVM, CrMasSupPostRegion>().ReverseMap();
            CreateMap<QuestionsAnswerVM, CrMasSysQuestionsAnswer>().ReverseMap();
            CreateMap<CountryClassificationVM, CrMasSupCountryClassification>().ReverseMap();
            CreateMap<CompanyUserVM, CrMasUserInformation>().ReverseMap();
            CreateMap<MASContractSourceVM, CrMasSupContractSource>().ReverseMap();
            CreateMap<EmployeesWithAuthrizationVM, CrMasUserInformation>().ReverseMap();

        }

    }
}
