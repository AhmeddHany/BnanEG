using Bnan.Core;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Interfaces.MAS.Users;
using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Bnan.Core.Models;
using Bnan.Core.Repository;
using Bnan.Inferastructure.Filters;
using Bnan.Inferastructure.Repository;
using Bnan.Inferastructure.Repository.Base;
using Bnan.Inferastructure.Repository.CAS;
using Bnan.Inferastructure.Repository.MAS;
using Bnan.Inferastructure.Repository.UpdateDataBaseJobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Quartz;
using System.Diagnostics;
using System.Globalization;

namespace Bnan.Inferastructure
{
    public static class PersistenceContainer
    {

        public static WebApplicationBuilder AddPersistenceServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddLocalization();
            builder.Services.AddDbContext<BnanSCContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("constring"));
                /* options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);*/
            });

            builder.Services.AddIdentity<CrMasUserInformation, IdentityRole>(opt =>
            {
                opt.Password.RequireDigit = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequiredLength = 1;
            }).AddEntityFrameworkStores<BnanSCContext>()
              .AddDefaultTokenProviders();

            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ar-EG")
                };
                foreach (var culture in supportedCultures)
                {
                    // Customize the number format for each supported culture
                    culture.NumberFormat.CurrencyDecimalDigits = 2;
                    culture.NumberFormat.CurrencyDecimalSeparator = ".";
                    culture.NumberFormat.CurrencyGroupSeparator = ",";
                }
                options.DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0], uiCulture: supportedCultures[0]);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            });


            builder.Services.AddStackExchangeRedisCache(opt =>
            {
                opt.Configuration = "localhost,abortConnect=false,connectTimeout=10000";
                opt.InstanceName = "redisUserstat";

            });

            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "MySessionCookie";
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.LoginPath = "/Identity/Account/Login";
                   options.ExpireTimeSpan = TimeSpan.FromSeconds(6);

                   options.Events = new CookieAuthenticationEvents()
                   {
                       OnSigningOut = (context) =>
                       {
                           Debug.WriteLine("test");
                           return Task.CompletedTask;
                       }
                   };
               });

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<SetCurrentPathMASFilter>();
                options.Filters.Add<SetCurrentPathCASFilter>();
            });
            builder.Services.AddScoped<SetCurrentPathMASFilter>();
            builder.Services.AddScoped<SetCurrentPathCASFilter>();

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            // New Mas
            builder.Services.AddScoped<IBaseRepo, BaseRepo>();
            // New Mas
            builder.Services.AddScoped<IUpdateStatusForContracts, UpdateStatusForContracts>();
            builder.Services.AddScoped<IUpdateStatusForDocsAndPriceCar, UpdateStatusForDocsAndPriceCar>();
            builder.Services.AddScoped<IUpdateStatusForUser, UpdateStatusForUser>();
            builder.Services.AddScoped<IUpdateCountOfTypeRenter, UpdateCountOfTypeRenter>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserLoginsService, UserLoginsService>();
            builder.Services.AddScoped<IUserMainValidtion, UserMainValidtionService>();
            builder.Services.AddScoped<IUserSubValidition, UserSubValiditionService>();
            builder.Services.AddScoped<IUserProcedureValidition, UserProcedureValiditionService>();
            builder.Services.AddScoped<ILessorImage, LessorImage>();
            builder.Services.AddScoped<IOwner, Owner>();
            builder.Services.AddScoped<IBeneficiary, Beneficiary>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ILessorMembership, LessorMembership>();
            builder.Services.AddScoped<ILessorMechanism, LessorMechanism>();
            builder.Services.AddScoped<ICompnayContract, CompnayContract>();
            builder.Services.AddScoped<IBranchInformation, BranchInformation>();
            builder.Services.AddScoped<IBranchDocument, BranchDocument>();
            builder.Services.AddScoped<IPostBranch, PostBranch>();
            builder.Services.AddScoped<IAccountBank, AccountBank>();
            builder.Services.AddScoped<ISalesPoint, SalesPoint>();
            builder.Services.AddScoped<IBankService, BankService>();
            builder.Services.AddScoped<IMasAccountPaymentMethod, MasAccountPaymentMethod>();
            builder.Services.AddScoped<ICarDistribution, CarDistribution>();

            builder.Services.AddScoped<ILessorOwners_CAS, LessorOwners_CAS>();
            builder.Services.AddScoped<IAccountBank_CAS, AccountBank_CAS>();
            builder.Services.AddScoped<IAccountSalesPoint_CAS, AccountSalesPoint_CAS>();


            builder.Services.AddScoped<IMasContractAdditional, MasContractAdditional>();
            builder.Services.AddScoped<IMasContractOptions, MasContractOptions>();
            builder.Services.AddScoped<IPostCity, PostCity>();
            builder.Services.AddScoped<IMasContractCarCheckup, MasContractCarCheckup>();
            builder.Services.AddScoped<IMasContractCarCheckupDetails, MasContractCarCheckupDetails>();
            builder.Services.AddScoped<IPostRegion, PostRegion>();

            builder.Services.AddScoped<IUserBranchValidity, UserBranchValidity>();
            builder.Services.AddScoped<IRenterDriver, RenterDriver>();
            builder.Services.AddScoped<IDrivingLicense, DrivingLicense>();


            builder.Services.AddScoped<IMasRenterGender, MasRenterGender>();
            builder.Services.AddScoped<IMasRenterNationality, MasRenterNationality>();
            builder.Services.AddScoped<IUserContractValididation, UserContractValididation>();
            builder.Services.AddScoped<IAdminstritiveProcedures, AdminstritiveProcedures>();
            builder.Services.AddScoped<ICarInformation, CarInformation>();
            builder.Services.AddScoped<IDocumentsMaintainanceCar, DocumentsMaintainanceCar>();
            builder.Services.AddScoped<ICarPrice, CarPrice>();
            builder.Services.AddScoped<IMembershipConditions, MembershipConditions>();
            builder.Services.AddScoped<IContract, Contract>();
            builder.Services.AddScoped<ICustody, Custody>();
            builder.Services.AddScoped<ITransferToFromRenter, TransferToFromRenter>();
            builder.Services.AddScoped<IContractExtension, ContractExtension>();
            builder.Services.AddScoped<IFeedBoxBS, FeedBoxBS>();



            builder.Services.AddScoped<IMasBase, MasBase>();
            builder.Services.AddScoped<IMasRenterIdtype, MasRenterIdtype>();
            builder.Services.AddScoped<IMasCarFuel, MasCarFuel>();
            builder.Services.AddScoped<IMasCarCvt, MasCarCvt>();
            builder.Services.AddScoped<IMasCarOil, MasCarOil>();
            builder.Services.AddScoped<IMasCarCategory, MasCarCategory>();
            builder.Services.AddScoped<IMasCarAdvantage, MasCarAdvantage>();
            builder.Services.AddScoped<IMasCarRegistration, MasCarRegistration>();
            builder.Services.AddScoped<IMasCarBrand, MasCarBrand>();
            builder.Services.AddScoped<IRenterInformation, RenterInformation>();

            builder.Services.AddScoped<IMasAccountBank, MasAccountBank>();
            builder.Services.AddScoped<IMasAccountReference, MasAccountReference>();
            builder.Services.AddScoped<IMasLessorClassification, MasLessorClassification>();
            builder.Services.AddScoped<IMasRate, MasRate>();
            builder.Services.AddScoped<IMasCurrency, MasCurrency>();
            builder.Services.AddScoped<IMasCountries, MasCountries>();
            builder.Services.AddScoped<IMasPostRegions, MasPostRegions>();
            builder.Services.AddScoped<IMasPostCity, MasPostCity>();


            //CAS
            builder.Services.AddScoped<IRenterLessorInformation, CasRenterLessorInformation>();
            builder.Services.AddScoped<IRenterDriver_CAS, RenterDriver_CAS>();

            builder.Services.AddScoped<IRenterContract, CasRenterContract>();
            builder.Services.AddScoped<IDriverContract, DriverContract>();
            builder.Services.AddScoped<IUserContract, UserContract>();
            builder.Services.AddScoped<ICarContract, CarContract>();
            builder.Services.AddScoped<IFinancialTransactionOfRenter, FinancialTransactionOfRenter>();
            builder.Services.AddScoped<IFinancialTransactionOfSalesPoint, FinancialTransactionOfSalesPoint>();
            builder.Services.AddScoped<IFinancialTransactionOfEmployee, FinancialTransactionOfEmployee>();
            builder.Services.AddScoped<IContractSettlement, ContractSettlement>();
            builder.Services.AddScoped<IConvertedText, ConvertedText>();
            builder.Services.AddScoped<IWhatsupConnect, WhatsupConnect>();
            //MAS
            builder.Services.AddScoped<IMasCarColor, MasCarColor>();
            builder.Services.AddScoped<IMasRenterDrivingLicense, MasRenterDrivingLicense>();
            builder.Services.AddScoped<IMasRenterProfession, MasRenterProfession>();
            builder.Services.AddScoped<IMasRenterEmployer, MasRenterEmployer>();
            builder.Services.AddScoped<IMasRenterSector, MasRenterSector>();
            builder.Services.AddScoped<IMasCarModel, MasCarModel>();
            builder.Services.AddScoped<IMasRenterMembership, MasRenterMembership>();
            builder.Services.AddScoped<IMasUser, MasUser>();
            builder.Services.AddScoped<IMasQuestionsAnswer, MasQuestionsAnswer>();
            //builder.Services.AddScoped<IMasWhatsupConnect, MasWhatsupConnect>();
            builder.Services.AddScoped<IMasLessor, MasLessor>();
            builder.Services.AddScoped<IMasCountryClassification, MasCountryClassification>();
            builder.Services.AddScoped<IMasRenterInformation, MasRenterInformation>();
            builder.Services.AddScoped<IMasLessorMarketing, MasLessorMarketing>();
            builder.Services.AddScoped<IMasCompanyUsers, MasCompanyUsers>();
            builder.Services.AddScoped<ITGAConnect, TGAConnect>();
            builder.Services.AddScoped<IShomoosConnect, ShomoosConnect>();
            builder.Services.AddScoped<ISMSConnect, SMSConnect>();


            return builder;
        }



    }
}