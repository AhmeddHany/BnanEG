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
            ownersLayoutVM.OwnPaymentMethods = await ChartsPaymentMethod(lessorCode);
            ownersLayoutVM.AlertContracts = await _unitOfWork.CrCasRenterContractAlert.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractAlertLessor == lessorCode);

            ownersLayoutVM.CarPrices = await _unitOfWork.CrCasPriceCarBasic.FindAllAsNoTrackingAsync(x => x.CrCasPriceCarBasicLessorCode == lessorCode);
            var Renters = await _unitOfWork.CrCasRenterLessor.FindAllAsNoTrackingAsync(x => x.CrCasRenterLessorStatus == Status.Active && x.CrCasRenterLessorCode == lessorCode);
            ownersLayoutVM.Debtors = Renters.Where(x => x.CrCasRenterLessorAvailableBalance < 0).Sum(x => x.CrCasRenterLessorAvailableBalance);
            ownersLayoutVM.Creditors = Renters.Where(x => x.CrCasRenterLessorAvailableBalance > 0).Sum(x => x.CrCasRenterLessorAvailableBalance);

            ownersLayoutVM.BranchsInformations = await GetInfoForBranches(lessorCode);
            ownersLayoutVM.Employees = await GetInfoForEmployees(lessorCode);
            return View(ownersLayoutVM);
        }
        public async Task<List<OwnBranchVM>> GetInfoForBranches(string lessorCode)
        {
            List<OwnBranchVM> OwnBranches = new List<OwnBranchVM>();

            var branches = await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(
                x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus != Status.Deleted,
                new[] { "CrCasCarInformations", "CrCasRenterContractBasics", "CrCasBranchPost.CrCasBranchPostCityNavigation", "CrCasRenterContractAlerts" }
            );

            foreach (var branch in branches)
            {
                var OwnBranch = _mapper.Map<OwnBranchVM>(branch);

                OwnBranch.BranchPostAr = branch.CrCasBranchPost?.CrCasBranchPostCityNavigation?.CrMasSupPostCityConcatenateArName;
                OwnBranch.BranchPostEn = branch.CrCasBranchPost?.CrCasBranchPostCityNavigation?.CrMasSupPostCityConcatenateEnName;

                OwnBranch.ContractsCount = branch.CrCasRenterContractBasics?.Where(x => x.CrCasRenterContractBasicStatus != Status.Extension).Count() ?? 0;
                OwnBranch.ActiveContractsCount = branch.CrCasRenterContractBasics?.Where(x => x.CrCasRenterContractBasicStatus == Status.Active).Count() ?? 0;
                OwnBranch.ClosedContractsCount = branch.CrCasRenterContractBasics?.Where(x => x.CrCasRenterContractBasicStatus == Status.Closed).Count() ?? 0;
                OwnBranch.SavedContractsCount = branch.CrCasRenterContractBasics?.Where(x => x.CrCasRenterContractBasicStatus == Status.Saved).Count() ?? 0;
                OwnBranch.SuspendedContractsCount = branch.CrCasRenterContractBasics?.Where(x => x.CrCasRenterContractBasicStatus == Status.Suspend).Count() ?? 0;
                OwnBranch.ExpiredContractsCount = branch.CrCasRenterContractAlerts?.Where(x => x.CrCasRenterContractAlertContractStatus == Status.Expire).Count() ?? 0;

                var docsCompany = await _unitOfWork.CrCasBranchDocument.FindAllAsNoTrackingAsync(x => x.CrCasBranchDocumentsLessor == lessorCode && x.CrCasBranchDocumentsBranch == branch.CrCasBranchInformationCode);
                OwnBranch.DocsForCompanyExpireCount = docsCompany.Where(x => x.CrCasBranchDocumentsStatus == Status.Expire).Count();
                OwnBranch.DocsForCompanyAboutExpireCount = docsCompany.Where(x => x.CrCasBranchDocumentsStatus == Status.AboutToExpire).Count();

                var docsCar = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllAsNoTrackingAsync(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode &&
                                                                                                            x.CrCasCarDocumentsMaintenanceBranch == branch.CrCasBranchInformationCode);
                OwnBranch.DocsForCarExpireCount = docsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.Expire && x.CrCasCarDocumentsMaintenanceProceduresClassification == "12").Count();
                OwnBranch.DocsForCarAboutExpireCount = docsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire && x.CrCasCarDocumentsMaintenanceProceduresClassification == "12").Count();
                OwnBranch.MainForCarExpireCount = docsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.Expire && x.CrCasCarDocumentsMaintenanceProceduresClassification == "13").Count();
                OwnBranch.MainForCarAboutExpireCount = docsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire && x.CrCasCarDocumentsMaintenanceProceduresClassification == "13").Count();

                OwnBranch.CarsCount = branch.CrCasCarInformations?.Count(x => x.CrCasCarInformationStatus != Status.Sold && x.CrCasCarInformationStatus != Status.Deleted) ?? 0;
                OwnBranch.RentedCarsCount = branch.CrCasCarInformations?.Count(x => x.CrCasCarInformationStatus == Status.Rented) ?? 0;
                OwnBranch.ActiveCarsCount = branch.CrCasCarInformations?.Count(x => x.CrCasCarInformationStatus == Status.Active &&
                                                     x.CrCasCarInformationPriceStatus == true &&
                                                     x.CrCasCarInformationBranchStatus == Status.Active &&
                                                     x.CrCasCarInformationOwnerStatus == Status.Active &&
                                                     (x.CrCasCarInformationForSaleStatus == Status.Active || x.CrCasCarInformationForSaleStatus == Status.RendAndForSale)) ?? 0;
                OwnBranch.UnActiveCarsCount = OwnBranch.CarsCount - OwnBranch.RentedCarsCount - OwnBranch.ActiveCarsCount;
                var checkIfThereCustody = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllAsNoTrackingAsync(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                                                        x.CrCasSysAdministrativeProceduresBranch == branch.CrCasBranchInformationCode &&
                                                                                                                        x.CrCasSysAdministrativeProceduresCode == "304" &&
                                                                                                                        x.CrCasSysAdministrativeProceduresStatus == Status.Insert);
                OwnBranch.HaveCustodyNotAccepted = OwnBranch.CrCasBranchInformationReservedBalance > 0;
                if (OwnBranch.CrCasBranchInformationReservedBalance > 0 ||
                    OwnBranch.DocsForCompanyAboutExpireCount > 0 ||
                    OwnBranch.DocsForCompanyExpireCount > 0 ||
                    OwnBranch.MainForCarExpireCount > 0 ||
                    OwnBranch.UnActiveCarsCount > 0 ||
                    OwnBranch.ExpiredContractsCount > 0) OwnBranch.RedPointInBranch = false;
                else OwnBranch.RedPointInBranch = true;
                // Add the mapped OwnBranchVM to the list
                OwnBranches.Add(OwnBranch);
            }

            // Return the list of OwnBranchVM
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
        // Branches
        public async Task<IActionResult> Branches()
        {
            ViewBag.Branches = true;
            ViewBag.Dashboard = false;
            ViewBag.Employees = false;
            ViewBag.Indicators = false;
            ViewBag.Cars = false;
            ViewBag.Contracts = false;
            ViewBag.Tenants = false;

            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Owners", "Branches", "", "", "", userLogin.CrMasUserInformationEnName);
            else await ViewData.SetPageTitleAsync("الملاك", "الفروع", "", "", "", userLogin.CrMasUserInformationArName);
            var ownersLayoutVM = await OwnersDashboadInfo(lessorCode);
            var branchs = await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(x => x.CrCasBranchInformationLessor == lessorCode, new[] { "CrCasCarInformations", "CrCasRenterContractBasics", "CrCasBranchPost.CrCasBranchPostCityNavigation" });
            var BranchsInforamtions = _mapper.Map<List<OwnBranchVM>>(branchs);
            ownersLayoutVM.BranchsInformations = BranchsInforamtions;
            return View(ownersLayoutVM);
        }
        //Employees
        public async Task<IActionResult> Employees()
        {
            ViewBag.Branches = false;
            ViewBag.Dashboard = false;
            ViewBag.Employees = true;
            ViewBag.Indicators = false;
            ViewBag.Cars = false;
            ViewBag.Contracts = false;
            ViewBag.Tenants = false;

            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Owners", "Employees", "", "", "", userLogin.CrMasUserInformationEnName);
            else await ViewData.SetPageTitleAsync("الملاك", "الموظفين", "", "", "", userLogin.CrMasUserInformationArName);
            var ownersLayoutVM = await OwnersDashboadInfo(lessorCode);
            List<OwnEmployeesVM> employeesVM = new List<OwnEmployeesVM>();
            var Employees = await _unitOfWork.CrMasUserInformation.FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationStatus != Status.Deleted);
            foreach (var employee in Employees)
            {
                var contracts = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractBasicUserInsert == employee.CrMasUserInformationCode && x.CrCasRenterContractBasicLessor == lessorCode);
                var EmployeeMapper = _mapper.Map<OwnEmployeesVM>(employee);
                EmployeeMapper.ActiveContract = contracts.Count(x => x.CrCasRenterContractBasicStatus != Status.Closed && x.CrCasRenterContractBasicStatus != Status.Extension);
                EmployeeMapper.ClosedContract = contracts.Count(x => x.CrCasRenterContractBasicStatus == Status.Closed);
                EmployeeMapper.OwnUsersPaymentMethods = await ChartsPaymentMethod(lessorCode, employee.CrMasUserInformationCode);
                var contractPrimaryKeys = contracts.Select(c => c.CrCasRenterContractBasicNo).ToList();
                var AlertsContracts = await _unitOfWork.CrCasRenterContractAlert.FindAllAsNoTrackingAsync(x => contractPrimaryKeys.Contains(x.CrCasRenterContractAlertNo) && x.CrCasRenterContractAlertContractStatus != Status.Closed);
                EmployeeMapper.AlertContracts = AlertsContracts.Distinct().ToList();
                employeesVM.Add(EmployeeMapper);
            }
            ownersLayoutVM.Employees = employeesVM;

            return View(ownersLayoutVM);
        }
        private async Task<List<OwnPaymentMethodLessorVM>> ChartsPaymentMethod(string lessorCode)
        {
            List<OwnPaymentMethodLessorVM> paymentMethodLessor = new List<OwnPaymentMethodLessorVM>();
            var AccountReceipt = await _unitOfWork.CrCasAccountReceipt.FindAllAsNoTrackingAsync(x => x.CrCasAccountReceiptLessorCode == lessorCode && x.CrCasAccountReceiptIsPassing == "1" && x.CrCasAccountReceiptPaymentMethod != "30");
            if (AccountReceipt == null) return paymentMethodLessor;
            var PaymentMethods = await _unitOfWork.CrMasSupAccountPaymentMethod.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active && (x.CrMasSupAccountPaymentMethodClassification == "1" || x.CrMasSupAccountPaymentMethodClassification == "2"));
            foreach (var paymentMethod in PaymentMethods)
            {
                // Assuming you have properties for Arabic and English names in your CrMasSupAccountPaymentMethod model
                string code = paymentMethod.CrMasSupAccountPaymentMethodCode; // Replace with the actual property name
                string arabicName = paymentMethod.CrMasSupAccountPaymentMethodArName; // Replace with the actual property name
                string englishName = paymentMethod.CrMasSupAccountPaymentMethodEnName; // Replace with the actual property name

                // Get the corresponding balance based on payment method
                decimal balance = 0;

                switch (paymentMethod.CrMasSupAccountPaymentMethodCode)
                {
                    case "10":
                        var Cash = AccountReceipt.Where(x => x.CrCasAccountReceiptPaymentMethod == "10");
                        balance = (decimal)Cash.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Cash.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "20":
                        var Madda = AccountReceipt.Where(x => x.CrCasAccountReceiptPaymentMethod == "20");
                        balance = (decimal)Madda.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Madda.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "21":
                        var Visa = AccountReceipt.Where(x => x.CrCasAccountReceiptPaymentMethod == "21");
                        balance = (decimal)Visa.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Visa.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "22":
                        var Master = AccountReceipt.Where(x => x.CrCasAccountReceiptPaymentMethod == "22");
                        balance = (decimal)Master.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Master.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "23":
                        var Amercian = AccountReceipt.Where(x => x.CrCasAccountReceiptPaymentMethod == "23");
                        balance = (decimal)Amercian.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Amercian.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                        // Add more cases for other payment methods if needed
                }

                // Create PaymentMethodData object and add it to the list
                paymentMethodLessor.Add(new OwnPaymentMethodLessorVM
                {
                    Code = code,
                    ArName = arabicName,
                    EnName = englishName,
                    Value = balance
                });
            }

            return paymentMethodLessor;
        }
        private async Task<List<OwnPaymentMethodLessorVM>> ChartsPaymentMethod(string lessorCode, string UserCode)
        {
            List<OwnPaymentMethodLessorVM> paymentMethodLessor = new List<OwnPaymentMethodLessorVM>();
            var AccountReceipt = await _unitOfWork.CrCasAccountReceipt.FindAllAsNoTrackingAsync(x => x.CrCasAccountReceiptLessorCode == lessorCode && x.CrCasAccountReceiptIsPassing == "1" && x.CrCasAccountReceiptPaymentMethod != "30" && x.CrCasAccountReceiptUser == UserCode);
            if (AccountReceipt == null) return paymentMethodLessor;
            var PaymentMethods = await _unitOfWork.CrMasSupAccountPaymentMethod.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active && (x.CrMasSupAccountPaymentMethodClassification == "1" || x.CrMasSupAccountPaymentMethodClassification == "2"));
            foreach (var paymentMethod in PaymentMethods)
            {
                // Assuming you have properties for Arabic and English names in your CrMasSupAccountPaymentMethod model
                string code = paymentMethod.CrMasSupAccountPaymentMethodCode; // Replace with the actual property name
                string arabicName = paymentMethod.CrMasSupAccountPaymentMethodArName; // Replace with the actual property name
                string englishName = paymentMethod.CrMasSupAccountPaymentMethodEnName; // Replace with the actual property name

                // Get the corresponding balance based on payment method
                var accountReceiptForUser = AccountReceipt.Where(x => x.CrCasAccountReceiptUser == UserCode);

                decimal balance = 0;

                switch (paymentMethod.CrMasSupAccountPaymentMethodCode)
                {
                    case "10":
                        var Cash = accountReceiptForUser.Where(x => x.CrCasAccountReceiptPaymentMethod == "10");
                        balance = (decimal)Cash.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Cash.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "20":
                        var Madda = accountReceiptForUser.Where(x => x.CrCasAccountReceiptPaymentMethod == "20");
                        balance = (decimal)Madda.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Madda.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "21":
                        var Visa = accountReceiptForUser.Where(x => x.CrCasAccountReceiptPaymentMethod == "21");
                        balance = (decimal)Visa.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Visa.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "22":
                        var Master = accountReceiptForUser.Where(x => x.CrCasAccountReceiptPaymentMethod == "22");
                        balance = (decimal)Master.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Master.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                    case "23":
                        var Amercian = accountReceiptForUser.Where(x => x.CrCasAccountReceiptPaymentMethod == "23");
                        balance = (decimal)Amercian.Sum(x => x.CrCasAccountReceiptPayment) - (decimal)Amercian.Sum(x => x.CrCasAccountReceiptReceipt);
                        break;
                        // Add more cases for other payment methods if needed
                }

                // Create PaymentMethodData object and add it to the list
                paymentMethodLessor.Add(new OwnPaymentMethodLessorVM
                {
                    Code = code,
                    ArName = arabicName,
                    EnName = englishName,
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
