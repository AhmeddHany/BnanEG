using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.BS.Controllers
{
    [Area("BS")]
    [Authorize(Roles = "BS")]
    public class HomeController : BaseController
    {
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<HomeController> _localizer;
        public HomeController(IStringLocalizer<HomeController> localizer, IUnitOfWork unitOfWork, UserManager<CrMasUserInformation> userManager, IMapper mapper, IToastNotification toastNotification) : base(userManager, unitOfWork, mapper)
        {
            _localizer = localizer;
            _toastNotification = toastNotification;
        }
        public async Task<IActionResult> Index()
        {
            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Branches", "Home", "", "", "", userLogin.CrMasUserInformationEnName);
            else await ViewData.SetPageTitleAsync("الفروع", "الرئيسية", "", "", "", userLogin.CrMasUserInformationArName);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var bSLayoutVM = await GetBranchesAndLayout();
            var Cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold && x.CrCasCarInformationBranch == bSLayoutVM.SelectedBranch, new[] { "CrCasCarInformationDistributionNavigation" }).ToList();
            var carsRented = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == bSLayoutVM.SelectedBranch && x.CrCasCarInformationStatus == Status.Rented, new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics" }).ToList();

            var carsAvailable = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == bSLayoutVM.SelectedBranch && x.CrCasCarInformationStatus == Status.Active &&
                                                                                x.CrCasCarInformationPriceStatus == true && x.CrCasCarInformationBranchStatus == Status.Active &&
                                                                                x.CrCasCarInformationOwnerStatus == Status.Active &&
                                                                               (x.CrCasCarInformationForSaleStatus == Status.Active || x.CrCasCarInformationForSaleStatus == Status.RendAndForSale),
                                                                               new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics" }).ToList();
            var carsUnAvailable = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == bSLayoutVM.SelectedBranch && (x.CrCasCarInformationStatus == Status.Hold ||
                                                                                x.CrCasCarInformationStatus == Status.Maintaince || x.CrCasCarInformationPriceStatus == false ||
                                                                                x.CrCasCarInformationBranchStatus != Status.Active || x.CrCasCarInformationOwnerStatus != Status.Active ||
                                                                               (x.CrCasCarInformationStatus == Status.Active && x.CrCasCarInformationForSaleStatus == Status.ForSale)),
                                                                               new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics" }).ToList();
            //var documentsMaintenance = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode && x.CrCasCarDocumentsMaintenanceBranch == bSLayoutVM.SelectedBranch, new[] { "CrCasCarDocumentsMaintenanceProceduresNavigation" }).ToList();
            ViewBag.carCount = Cars.Count();
            ViewBag.AvaliableCars = carsAvailable.Count();
            ViewBag.UnAvaliableCars = carsUnAvailable.Count();
            ViewBag.RentedCars = carsRented.Count();
            var userInfo = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == userLogin.CrMasUserInformationCode, new[] { "CrMasUserBranchValidities" });
            var branchValidity = userInfo.CrMasUserBranchValidities.FirstOrDefault(x => x.CrMasUserBranchValidityBranch == bSLayoutVM.SelectedBranch);

            ViewBag.Adminstritive = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                x.CrCasSysAdministrativeProceduresTargeted == userLogin.CrMasUserInformationCode &&
                                                                                x.CrCasSysAdministrativeProceduresCode == "303" &&
                                                                                x.CrCasSysAdministrativeProceduresStatus == Status.Insert).Count();

            var Contracts = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicBranch == bSLayoutVM.SelectedBranch).ToList();
            var AlertContract = _unitOfWork.CrCasRenterContractAlert.FindAll(x => x.CrCasRenterContractAlertLessor == lessorCode && x.CrCasRenterContractAlertBranch == bSLayoutVM.SelectedBranch).ToList();

            //For Charts 
            var AccountReceipt = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == lessorCode && x.CrCasAccountReceiptBranchCode == bSLayoutVM.SelectedBranch &&
                                                                           x.CrCasAccountReceiptIsPassing == "1" && x.CrCasAccountReceiptPaymentMethod != "30").ToList();

            var PaymentMethods = _unitOfWork.CrMasSupAccountPaymentMethod.FindAll(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active &&
                                                                                       (x.CrMasSupAccountPaymentMethodClassification == "1" || x.CrMasSupAccountPaymentMethodClassification == "2")).ToList();

            var paymentMethodBranch = ChartsPaymentMethodBranch(AccountReceipt);
            var paymentMethodUser = ChartsPaymentMethodUser(AccountReceipt, userLogin.CrMasUserInformationCode);

            ViewBag.RenterLessorCount = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == lessorCode).Count();
            ViewBag.ContractForSettelment = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicLessor == lessorCode && (x.CrCasRenterContractBasicStatus == Status.Active || x.CrCasRenterContractBasicStatus == Status.Expire)).Count();
            ViewBag.ContractForExtension = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicBranch == bSLayoutVM.SelectedBranch && x.CrCasRenterContractBasicStatus == Status.Active).Count();

            ViewBag.AcccountReceiptCount = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == lessorCode &&
                                                                                     x.CrCasAccountReceiptBranchCode == bSLayoutVM.SelectedBranch && x.CrCasAccountReceiptUser == userLogin.CrMasUserInformationCode).Count();
            ViewBag.AcccountReceiptPassingCount = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == lessorCode &&
                                                                                     x.CrCasAccountReceiptBranchCode == bSLayoutVM.SelectedBranch && x.CrCasAccountReceiptUser == userLogin.CrMasUserInformationCode && x.CrCasAccountReceiptIsPassing == "1").Count();




            bSLayoutVM.RentedCars = null;
            bSLayoutVM.UnAvaliableCars = null;
            bSLayoutVM.AvaliableCars = null;
            bSLayoutVM.CrMasUserBranchValidity = branchValidity;
            bSLayoutVM.BasicContracts = Contracts;
            bSLayoutVM.AlertContract = AlertContract;
            bSLayoutVM.PaymentMethodsBranch = paymentMethodBranch.OrderBy(x => x.Code).ToList();
            bSLayoutVM.PaymentMethodsUser = paymentMethodUser.OrderBy(x => x.Code).ToList();
            // Check If Settlement Or Not
            if (this.ShouldShowModal(out string renterId, out string contractNo))
            {
                bSLayoutVM.RenterIdFromSettelementToRate = renterId;
                bSLayoutVM.ContractNoFromSettelementToRate = contractNo;
                ViewBag.ShowModal = true;
            }
            else ViewBag.ShowModal = false;
            //////////////////////////////////////////////////////////////////////////////////

            return View(bSLayoutVM);
        }
        public async Task<IActionResult> ChangeBranch(string selectedBranch)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var userInfo = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == userLogin.CrMasUserInformationCode);
            var lessorCode = userInfo.CrMasUserInformationLessor;
            var checkBranchCode = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationCode == selectedBranch);
            if (checkBranchCode != null) userInfo.CrMasUserInformationDefaultBranch = selectedBranch;
            else userLogin.CrMasUserInformationDefaultBranch = "100";
            await _unitOfWork.CompleteAsync();
            return Json(true);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRentedCars()
        {
            try
            {
                var userLogin = await _userManager.GetUserAsync(User);
                if (userLogin == null) throw new Exception("User not found.");


                var lessorCode = userLogin.CrMasUserInformationLessor;
                var BranchCode = userLogin.CrMasUserInformationDefaultBranch;

                var RentedCars = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == BranchCode && x.CrCasCarInformationStatus == Status.Rented,
                    new[] { "CrCasCarInformationDistributionNavigation","CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics", "CrCasRenterContractBasics.CrCasRenterContractBasic5.CrCasRenterLessorNavigation",
                "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation", "CrCasCarInformationCategoryNavigation" });

                if (RentedCars == null) throw new Exception("No rented cars found.");


                var branches = await _unitOfWork.CrCasBranchInformation.FindAllAsync(x =>
                    x.CrCasBranchInformationLessor == lessorCode &&
                    x.CrCasBranchInformationStatus != Status.Deleted);

                if (branches == null) throw new Exception("No branches found.");


                BSLayoutVM bSLayoutVM = new BSLayoutVM()
                {
                    CrCasBranchInformations = branches.ToList(),
                    RentedCars = RentedCars.OrderBy(x => x.CrCasRenterContractBasics.Select(c => c.CrCasRenterContractBasicExpectedEndDate).LastOrDefault()).ToList(),
                    DocumentsMaintenances = null,
                    UnAvaliableCars = null,
                    AvaliableCars = null,
                    SelectedBranch = BranchCode,
                };

                return PartialView("_RentedCars", bSLayoutVM);
            }
            catch (Exception ex)
            {
                return PartialView("_Error", ex.Message);
            }
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRentedCarsByCategory(string code)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var BranchCode = userLogin.CrMasUserInformationDefaultBranch;
            var RentedCars = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == BranchCode && x.CrCasCarInformationStatus == Status.Rented,
                                                                                 new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics",
                                                                                     "CrCasRenterContractBasics.CrCasRenterContractBasic5.CrCasRenterLessorNavigation",
                                                                                     "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation","CrCasCarInformationCategoryNavigation" });


            if (code != "00")
            {
                RentedCars = RentedCars.Where(x => x.CrCasCarInformationCategory == code);
            }


            var branches = await _unitOfWork.CrCasBranchInformation.FindAllAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus != Status.Deleted);

            BSLayoutVM bSLayoutVM = new BSLayoutVM()
            {
                CrCasBranchInformations = branches.ToList(),
                RentedCars = RentedCars.OrderBy(x => x.CrCasRenterContractBasics.Select(c => c.CrCasRenterContractBasicExpectedEndDate).LastOrDefault()).ToList(),
                DocumentsMaintenances = null,
                UnAvaliableCars = null,
                AvaliableCars = null,
                SelectedBranch = BranchCode,
            };
            return PartialView("_RentedCarByCategory", bSLayoutVM);
        }



        [HttpGet]
        public async Task<PartialViewResult> GetUnAvaliableCars()
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var BranchCode = userLogin.CrMasUserInformationDefaultBranch;
            var carsUnAvailable = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == BranchCode && (x.CrCasCarInformationStatus == Status.Hold ||
                                                                                           x.CrCasCarInformationStatus == Status.Maintaince || x.CrCasCarInformationPriceStatus == false ||
                                                                                           x.CrCasCarInformationBranchStatus != Status.Active || x.CrCasCarInformationOwnerStatus != Status.Active ||
                                                                                          (x.CrCasCarInformationStatus == Status.Active && x.CrCasCarInformationForSaleStatus == Status.ForSale)),
                                                                                          new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics",
                                                                                              "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation","CrCasCarInformationCategoryNavigation",
                                                                                              "CrCasCarInformationFuelNavigation","CrCasCarInformationCvtNavigation"});

            var branches = await _unitOfWork.CrCasBranchInformation.FindAllAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus != Status.Deleted);

            BSLayoutVM bSLayoutVM = new BSLayoutVM()
            {
                CrCasBranchInformations = branches.ToList(),
                RentedCars = null,
                UnAvaliableCars = carsUnAvailable.ToList(),
                AvaliableCars = null,
                SelectedBranch = BranchCode,
            };
            return PartialView("_UnAvailableCar", bSLayoutVM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetUnAvaliableCarsByCategory(string code)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var BranchCode = userLogin.CrMasUserInformationDefaultBranch;
            var carsUnAvailable = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == BranchCode && (x.CrCasCarInformationStatus == Status.Hold ||
                                                                                           x.CrCasCarInformationStatus == Status.Maintaince || x.CrCasCarInformationPriceStatus == false ||
                                                                                           x.CrCasCarInformationBranchStatus != Status.Active || x.CrCasCarInformationOwnerStatus != Status.Active ||
                                                                                          (x.CrCasCarInformationStatus == Status.Active && x.CrCasCarInformationForSaleStatus == Status.ForSale)),
                                                                                          new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics",
                                                                                              "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation","CrCasCarInformationCategoryNavigation",
                                                                                              "CrCasCarInformationFuelNavigation","CrCasCarInformationCvtNavigation" });

            if (code != "00")
            {
                carsUnAvailable = carsUnAvailable.Where(x => x.CrCasCarInformationCategory == code).ToList();
            }
            var branches = await _unitOfWork.CrCasBranchInformation.FindAllAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus != Status.Deleted);

            BSLayoutVM bSLayoutVM = new BSLayoutVM()
            {
                CrCasBranchInformations = branches.ToList(),
                RentedCars = null,
                UnAvaliableCars = carsUnAvailable.ToList(),
                AvaliableCars = null,
                SelectedBranch = BranchCode,
            };
            return PartialView("_UnAvailableCarByCategory", bSLayoutVM);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetAvaliableCars()
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var BranchCode = userLogin.CrMasUserInformationDefaultBranch;

            var carsAvailable = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == BranchCode && x.CrCasCarInformationStatus == Status.Active &&
                                                                                x.CrCasCarInformationPriceStatus == true && x.CrCasCarInformationBranchStatus == Status.Active &&
                                                                                x.CrCasCarInformationOwnerStatus == Status.Active &&
                                                                               (x.CrCasCarInformationForSaleStatus == Status.Active || x.CrCasCarInformationForSaleStatus == Status.RendAndForSale),
                                                                               new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationCategoryNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics",
                                                                                   "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation" ,"CrCasCarInformationFuelNavigation","CrCasCarInformationCvtNavigation"});
            var branches = await _unitOfWork.CrCasBranchInformation.FindAllAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus != Status.Deleted);
            BSLayoutVM bSLayoutVM = new BSLayoutVM()
            {
                CrCasBranchInformations = branches.ToList(),
                RentedCars = null,
                UnAvaliableCars = null,
                AvaliableCars = carsAvailable.ToList(),
                SelectedBranch = BranchCode,

            };
            return PartialView("_AvaliableCar", bSLayoutVM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAvaliableCarsByCategory(string code)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var BranchCode = userLogin.CrMasUserInformationDefaultBranch;

            var carsAvailable = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == BranchCode && x.CrCasCarInformationStatus == Status.Active &&
                                                                                x.CrCasCarInformationPriceStatus == true && x.CrCasCarInformationBranchStatus == Status.Active &&
                                                                                x.CrCasCarInformationOwnerStatus == Status.Active &&
                                                                               (x.CrCasCarInformationForSaleStatus == Status.Active || x.CrCasCarInformationForSaleStatus == Status.RendAndForSale),
                                                                               new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationCategoryNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics",
                                                                                   "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation","CrCasCarInformationFuelNavigation","CrCasCarInformationCvtNavigation" });
            if (code != "00")
            {
                carsAvailable = carsAvailable.Where(x => x.CrCasCarInformationCategory == code).ToList();
            }

            var branches = await _unitOfWork.CrCasBranchInformation.FindAllAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus != Status.Deleted);
            BSLayoutVM bSLayoutVM = new BSLayoutVM()
            {
                CrCasBranchInformations = branches.ToList(),
                RentedCars = null,
                UnAvaliableCars = null,
                AvaliableCars = carsAvailable.ToList(),
                SelectedBranch = BranchCode,

            };
            return PartialView("_AvaliableCarByCategory", bSLayoutVM);
        }
        [HttpGet]
        public async Task<IActionResult> CheckCompanyDocuments(string selectedBranch)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var documents = _unitOfWork.CrCasBranchDocument.FindAll(x => x.CrCasBranchDocumentsLessor == lessorCode && x.CrCasBranchDocumentsBranch == selectedBranch);
            var documentNotActive = documents.Where(x => x.CrCasBranchDocumentsStatus == Status.Renewed || x.CrCasBranchDocumentsStatus == Status.Expire).ToList();
            var userContractValidity = _unitOfWork.CrMasUserContractValidity.Find(x => x.CrMasUserContractValidityUserId == userLogin.Id);
            var check = "true";
            foreach (var document in documentNotActive)
            {
                if (document.CrCasBranchDocumentsProcedures == "100" && userContractValidity.CrMasUserContractValidityRegister == false) check = "100";
                else if (document.CrCasBranchDocumentsProcedures == "101" && userContractValidity.CrMasUserContractValidityChamber == false) check = "101";
                else if (document.CrCasBranchDocumentsProcedures == "102" && userContractValidity.CrMasUserContractValidityTransferPermission == false) check = "102";
                else if (document.CrCasBranchDocumentsProcedures == "103" && userContractValidity.CrMasUserContractValidityLicenceMunicipale == false) check = "103";
                else if (document.CrCasBranchDocumentsProcedures == "104" && userContractValidity.CrMasUserContractValidityCompanyAddress == false) check = "104";
                else check = "true";
            }
            return Json(check);
        }



        private List<PaymentMethodBranchDataVM> ChartsPaymentMethodBranch(List<CrCasAccountReceipt> AccountReceipt)
        {
            List<PaymentMethodBranchDataVM> paymentMethodBranch = new List<PaymentMethodBranchDataVM>();
            if (AccountReceipt == null) return paymentMethodBranch;
            var PaymentMethods = _unitOfWork.CrMasSupAccountPaymentMethod.FindAll(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active && (x.CrMasSupAccountPaymentMethodClassification == "1" || x.CrMasSupAccountPaymentMethodClassification == "2")).ToList();
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
                paymentMethodBranch.Add(new PaymentMethodBranchDataVM
                {
                    Code = code,
                    ArName = arabicName,
                    EnName = englishName,
                    Value = balance
                });
            }

            return paymentMethodBranch;
        }
        private List<PaymentMethodBranchDataVM> ChartsPaymentMethodUser(List<CrCasAccountReceipt> AccountReceipt, string UserCode)
        {
            List<PaymentMethodBranchDataVM> paymentMethodUser = new List<PaymentMethodBranchDataVM>();
            if (AccountReceipt == null) return paymentMethodUser;
            var PaymentMethods = _unitOfWork.CrMasSupAccountPaymentMethod.FindAll(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active && (x.CrMasSupAccountPaymentMethodClassification == "1" || x.CrMasSupAccountPaymentMethodClassification == "2")).ToList();
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
                paymentMethodUser.Add(new PaymentMethodBranchDataVM
                {
                    Code = code,
                    ArName = arabicName,
                    EnName = englishName,
                    Value = balance
                });
            }

            return paymentMethodUser;
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
