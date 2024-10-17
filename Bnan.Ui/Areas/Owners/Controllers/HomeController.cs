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
            var branchs = await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(x => x.CrCasBranchInformationLessor == lessorCode, new[] { "CrCasCarInformations", "CrCasRenterContractBasics", "CrCasBranchPost.CrCasBranchPostCityNavigation" });
            var BranchsInforamtions = _mapper.Map<List<OwnBranchVM>>(branchs);
            ownersLayoutVM.BranchsInformations = BranchsInforamtions;
            return View(ownersLayoutVM);
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
