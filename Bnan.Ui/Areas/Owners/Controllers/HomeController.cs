using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.Owners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Bnan.Ui.Areas.Owners.Controllers
{
    [Area("Owners")]
    [Authorize(Roles = "OWN")]
    public class HomeController : BaseController
    {
        public HomeController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper) : base(userManager, unitOfWork, mapper)
        {
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            ViewBag.Dashboard = true;
            ViewBag.Branches = false;
            ViewBag.Employees = false;
            ViewBag.Indicators = false;
            ViewBag.Cars = false;
            ViewBag.Contracts = false;
            ViewBag.Tenants = false;
            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Owners", "Dashboard", "", "", "", userLogin.CrMasUserInformationEnName);
            else await ViewData.SetPageTitleAsync("الملاك", "لوحة التحكم", "", "", "", userLogin.CrMasUserInformationArName);
            var ownersLayoutVM = await OwnersDashboadInfo(lessorCode);


            var allContractAlerts = await _unitOfWork.CrCasRenterContractAlert
                    .FindAllWithSelectAsNoTrackingAsync(x => x.CrCasRenterContractAlertLessor == lessorCode, query => query
                        .Select(x => new OwnAlertContractsVM
                        {
                            CrCasRenterContractAlertContractActiviteStatus = x.CrCasRenterContractAlertContractActiviteStatus,
                            CrCasRenterContractAlertContractStatus = x.CrCasRenterContractAlertContractStatus
                        }));
            ownersLayoutVM.AlertContracts = allContractAlerts;

            var Renters = await _unitOfWork.CrCasRenterLessor
                    .FindAllWithSelectAsNoTrackingAsync(
                        x => x.CrCasRenterLessorStatus == Status.Active && x.CrCasRenterLessorCode == lessorCode,
                        query => query.Select(x => new
                        {
                            x.CrCasRenterLessorAvailableBalance
                        })
                    );
            ownersLayoutVM.Debtors = Renters.Where(x => x.CrCasRenterLessorAvailableBalance < 0).Sum(x => x.CrCasRenterLessorAvailableBalance);
            ownersLayoutVM.Creditors = Renters.Where(x => x.CrCasRenterLessorAvailableBalance > 0).Sum(x => x.CrCasRenterLessorAvailableBalance);



            ownersLayoutVM.OwnPaymentMethods = await ChartsPaymentMethod(lessorCode);
            ownersLayoutVM.BranchsInformations = await GetInfoForBranches(lessorCode);
            ownersLayoutVM.Employees = await GetInfoForEmployees(lessorCode);
            return View(ownersLayoutVM);
        }
        public async Task<List<OwnBranchVM>> GetInfoForBranches(string lessorCode)
        {
            List<OwnBranchVM> OwnBranches = new List<OwnBranchVM>();

            var branches = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                     x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus != Status.Deleted,
                     query => query.Select(x => new OwnBranchVM
                     {
                         CrCasBranchInformationLessor = x.CrCasBranchInformationLessor,
                         CrCasBranchInformationCode = x.CrCasBranchInformationCode,
                         CrCasBranchInformationArName = x.CrCasBranchInformationArName,
                         CrCasBranchInformationArShortName = x.CrCasBranchInformationArShortName,
                         CrCasBranchInformationEnName = x.CrCasBranchInformationEnName,
                         CrCasBranchInformationEnShortName = x.CrCasBranchInformationEnShortName,
                         CrCasBranchInformationDirectorArName = x.CrCasBranchInformationDirectorArName,
                         CrCasBranchInformationDirectorEnName = x.CrCasBranchInformationDirectorEnName,
                         CrCasBranchInformationTotalBalance = x.CrCasBranchInformationTotalBalance,
                         CrCasBranchInformationReservedBalance = x.CrCasBranchInformationReservedBalance,
                         CrCasBranchInformationAvailableBalance = x.CrCasBranchInformationAvailableBalance,
                         CrCasBranchInformationStatus = x.CrCasBranchInformationStatus
                     })
                 );

            // Use a temporary list to accumulate the branches
            List<OwnBranchVM> tempBranches = new List<OwnBranchVM>();

            foreach (var branch in branches)
            {
                var CrCasBranchPost = await _unitOfWork.CrCasBranchPost.FindAsync(x => x.CrCasBranchPostLessor == branch.CrCasBranchInformationLessor && x.CrCasBranchPostBranch == branch.CrCasBranchInformationCode,
                                                                                                         new[] { "CrCasBranchPostCityNavigation" });
                branch.BranchPostEn = CrCasBranchPost?.CrCasBranchPostCityNavigation?.CrMasSupPostCityConcatenateEnName;
                branch.BranchPostAr = CrCasBranchPost?.CrCasBranchPostCityNavigation?.CrMasSupPostCityConcatenateArName;

                var ContractBasics = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractBasicLessor == branch.CrCasBranchInformationLessor &&
                                                                                                             x.CrCasRenterContractBasicBranch == branch.CrCasBranchInformationCode);
                branch.ContractsCount = ContractBasics.Count(x => x.CrCasRenterContractBasicStatus != Status.Extension);
                branch.ActiveContractsCount = ContractBasics.Count(x => x.CrCasRenterContractBasicStatus == Status.Active);
                branch.ClosedContractsCount = ContractBasics.Count(x => x.CrCasRenterContractBasicStatus == Status.Closed);
                branch.SavedContractsCount = ContractBasics.Count(x => x.CrCasRenterContractBasicStatus == Status.Saved);
                branch.SuspendedContractsCount = ContractBasics.Count(x => x.CrCasRenterContractBasicStatus == Status.Suspend);
                branch.ExpiredContractsCount = await _unitOfWork.CrCasRenterContractAlert.CountAsync(x => x.CrCasRenterContractAlertContractStatus == Status.Expire &&
                                                                                                          x.CrCasRenterContractAlertLessor==branch.CrCasBranchInformationLessor&&
                                                                                                          x.CrCasRenterContractAlertBranch==branch.CrCasBranchInformationCode);

                var docsCompany = await _unitOfWork.CrCasBranchDocument.FindAllAsNoTrackingAsync(x => x.CrCasBranchDocumentsLessor == lessorCode && x.CrCasBranchDocumentsBranch == branch.CrCasBranchInformationCode);
                branch.DocsForCompanyExpireCount = docsCompany.Where(x => x.CrCasBranchDocumentsStatus == Status.Expire|| x.CrCasBranchDocumentsStatus == Status.Renewed).Count();
                branch.DocsForCompanyAboutExpireCount = docsCompany.Where(x => x.CrCasBranchDocumentsStatus == Status.AboutToExpire).Count();

                var docsCar = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllAsNoTrackingAsync(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode &&
                                                                                                             x.CrCasCarDocumentsMaintenanceBranch == branch.CrCasBranchInformationCode);
                branch.DocsForCarExpireCount = docsCar.Where(x => (x.CrCasCarDocumentsMaintenanceStatus == Status.Expire || x.CrCasCarDocumentsMaintenanceStatus == Status.Renewed) && x.CrCasCarDocumentsMaintenanceProceduresClassification == "12").Count();
                branch.DocsForCarAboutExpireCount = docsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire && x.CrCasCarDocumentsMaintenanceProceduresClassification == "12").Count();
                branch.MainForCarExpireCount = docsCar.Where(x =>( x.CrCasCarDocumentsMaintenanceStatus == Status.Expire || x.CrCasCarDocumentsMaintenanceStatus == Status.Renewed) && x.CrCasCarDocumentsMaintenanceProceduresClassification == "13").Count();
                branch.MainForCarAboutExpireCount = docsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire && x.CrCasCarDocumentsMaintenanceProceduresClassification == "13").Count();
                branch.CarsCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == branch.CrCasBranchInformationCode &&
                                                                                                           x.CrCasCarInformationStatus != Status.Sold && x.CrCasCarInformationStatus != Status.Deleted);
                branch.RentedCarsCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == branch.CrCasBranchInformationCode &&
                                                                                                           x.CrCasCarInformationStatus == Status.Rented);
                branch.ActiveCarsCount = await _unitOfWork.CrCasCarInformation.CountAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == branch.CrCasBranchInformationCode &&
                                                                                                           x.CrCasCarInformationStatus == Status.Active &&
                                                                                                          x.CrCasCarInformationPriceStatus == true &&
                                                                                                          x.CrCasCarInformationBranchStatus == Status.Active &&
                                                                                                          x.CrCasCarInformationOwnerStatus == Status.Active &&
                                                                                                          (x.CrCasCarInformationForSaleStatus == Status.Active || x.CrCasCarInformationForSaleStatus == Status.RendAndForSale));

                branch.UnActiveCarsCount = branch.CarsCount - branch.RentedCarsCount - branch.ActiveCarsCount;
                var checkIfThereCustody = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllAsNoTrackingAsync(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                                                         x.CrCasSysAdministrativeProceduresBranch == branch.CrCasBranchInformationCode &&
                                                                                                                         x.CrCasSysAdministrativeProceduresCode == "304" &&
                                                                                                                         x.CrCasSysAdministrativeProceduresStatus == Status.Insert);
                branch.HaveCustodyNotAccepted = branch.CrCasBranchInformationReservedBalance > 0;
                if (branch.CrCasBranchInformationReservedBalance > 0 ||
                     branch.DocsForCompanyAboutExpireCount > 0 ||
                     branch.DocsForCompanyExpireCount > 0 ||
                     branch.MainForCarExpireCount > 0 ||
                     branch.UnActiveCarsCount > 0 ||
                     branch.ExpiredContractsCount > 0) branch.RedPointInBranch = false;
                else branch.RedPointInBranch = true;

                // Add the processed branchVM to the temporary list
                tempBranches.Add(branch);
            }

            // Assign the accumulated list to the original variable
            OwnBranches = tempBranches;

            // Return the list of branchVM
            return OwnBranches;
        }
        public async Task<List<OwnEmployeesVM>> GetInfoForEmployees(string lessorCode)
        {
            List<OwnEmployeesVM> OwnEmployees = new List<OwnEmployeesVM>();

            var employees = await _unitOfWork.CrMasUserInformation.FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationStatus != Status.Deleted);
            foreach (var employee in employees)
            {
                var OwnEmployee = _mapper.Map<OwnEmployeesVM>(employee);
                var contracts = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractBasicUserInsert == employee.CrMasUserInformationCode && x.CrCasRenterContractBasicLessor == lessorCode);
                OwnEmployee.ActiveContract = contracts.Count(x => x.CrCasRenterContractBasicStatus != Status.Closed && x.CrCasRenterContractBasicStatus != Status.Extension);
                OwnEmployee.ClosedContract = contracts.Count(x => x.CrCasRenterContractBasicStatus == Status.Closed);
                OwnEmployee.ContractsCount = OwnEmployee.ActiveContract + OwnEmployee.ClosedContract;
                var checkIfThereCustody = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllAsNoTrackingAsync(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                                                       x.CrCasSysAdministrativeProceduresCode == "304" &&
                                                                                                                       x.CrCasSysAdministrativeProceduresUserInsert == employee.CrMasUserInformationCode &&
                                                                                                                       x.CrCasSysAdministrativeProceduresStatus == Status.Insert);
                OwnEmployee.HaveCustodyNotAccepted = OwnEmployee.CrMasUserInformationReservedBalance > 0;
                OwnEmployee.CreditLimitExceeded = OwnEmployee.CrMasUserInformationCreditLimit < OwnEmployee.CrMasUserInformationAvailableBalance;
                OwnEmployee.EntryLastDateString = OwnEmployee.CrMasUserInformationEntryLastDate?.ToString("yyyy/MM/dd");
                if (OwnEmployee.CrMasUserInformationEntryLastTime != null)
                {
                    var time = DateTime.Today.Add(OwnEmployee.CrMasUserInformationEntryLastTime.Value);
                    OwnEmployee.EntryLastTimeString = time.ToString("hh:mm tt");
                }
                if (OwnEmployee.CrMasUserInformationLastActionDate == null) OwnEmployee.OnlineOrOflline = false;
                else
                {
                    var timeDifference = DateTime.Now - OwnEmployee.CrMasUserInformationLastActionDate;
                    if (timeDifference?.TotalMinutes > 10) OwnEmployee.OnlineOrOflline = false;
                    else OwnEmployee.OnlineOrOflline = true;

                }
                OwnEmployees.Add(OwnEmployee);
            }
            return OwnEmployees;
        }
        private async Task<List<OwnPaymentMethodLessorVM>> ChartsPaymentMethod(string lessorCode)
        {
            List<OwnPaymentMethodLessorVM> paymentMethodLessor = new List<OwnPaymentMethodLessorVM>();

            // استعلام لتحميل بيانات الحسابات فقط بالمعلومات المطلوبة
            var AccountReceipt = await _unitOfWork.CrCasAccountReceipt
                .FindAllWithSelectAsNoTrackingAsync(
                    x => x.CrCasAccountReceiptLessorCode == lessorCode && x.CrCasAccountReceiptIsPassing == "1" && x.CrCasAccountReceiptPaymentMethod != "30",
                    query => query.Select(x => new
                    {
                        x.CrCasAccountReceiptPaymentMethod,
                        x.CrCasAccountReceiptPayment,
                        x.CrCasAccountReceiptReceipt
                    })
                );

            // في حالة عدم وجود أي سجلات
            if (AccountReceipt == null || !AccountReceipt.Any()) return paymentMethodLessor;

            // استعلام لتحميل بيانات طرق الدفع فقط بالمعلومات المطلوبة
            var PaymentMethods = await _unitOfWork.CrMasSupAccountPaymentMethod
                .FindAllWithSelectAsNoTrackingAsync(
                    x => x.CrMasSupAccountPaymentMethodStatus == Status.Active && (x.CrMasSupAccountPaymentMethodClassification == "1" || x.CrMasSupAccountPaymentMethodClassification == "2"),
                    query => query.Select(x => new
                    {
                        x.CrMasSupAccountPaymentMethodCode,
                        x.CrMasSupAccountPaymentMethodArName,
                        x.CrMasSupAccountPaymentMethodEnName
                    })
                );

            // بناء بيانات طرق الدفع
            foreach (var paymentMethod in PaymentMethods)
            {
                decimal balance = AccountReceipt
                    .Where(x => x.CrCasAccountReceiptPaymentMethod == paymentMethod.CrMasSupAccountPaymentMethodCode)
                    .Sum(x => (decimal)x.CrCasAccountReceiptPayment - (decimal)x.CrCasAccountReceiptReceipt);

                // إضافة البيانات إلى القائمة النهائية
                paymentMethodLessor.Add(new OwnPaymentMethodLessorVM
                {
                    Code = paymentMethod.CrMasSupAccountPaymentMethodCode,
                    ArName = paymentMethod.CrMasSupAccountPaymentMethodArName,
                    EnName = paymentMethod.CrMasSupAccountPaymentMethodEnName,
                    Value = balance
                });
            }

            return paymentMethodLessor;
        }

        [HttpGet]
        public IActionResult SetLanguage(string returnUrl, string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );

            return LocalRedirect(returnUrl);
        }
    }
}
