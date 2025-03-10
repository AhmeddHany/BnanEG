﻿using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.BS.CreateContract;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.BS.Controllers
{
    [Area("BS")]
    [Authorize(Roles = "BS")]

    public class ContractSettlementController : BaseController
    {
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<ContractSettlementController> _localizer;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IContractSettlement _contractSettlement;
        private readonly IConvertedText _convertedText;
        private readonly string pageNumber = SubTasks.SettementContract;

        public ContractSettlementController(IStringLocalizer<ContractSettlementController> localizer, IUnitOfWork unitOfWork,
            UserManager<CrMasUserInformation> userManager, IMapper mapper, IToastNotification toastNotification, IContract contractServices,
            IWebHostEnvironment hostingEnvironment, IContractSettlement contractSettlement, IConvertedText convertedText) : base(userManager, unitOfWork, mapper)
        {
            _localizer = localizer;
            _toastNotification = toastNotification;
            _hostingEnvironment = hostingEnvironment;
            _contractSettlement = contractSettlement;
            _convertedText = convertedText;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lessorCode = user.CrMasUserInformationLessor;
            await SetPageTitleAsync(string.Empty, pageNumber);

            var bsLayoutVM = await GetBranchesAndLayout();

            var contracts = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x =>
                (x.CrCasRenterContractBasicStatus == Status.Active || x.CrCasRenterContractBasicStatus == Status.Expire) &&
                x.CrCasRenterContractBasicLessor == lessorCode &&
                x.CrCasRenterContractBasicBranch == bsLayoutVM.SelectedBranch,
                new[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" });

            var contractReceivingTheCarFromAnotherBranch = await _unitOfWork.CrCasRenterContractAdditional
                .FindAllAsNoTrackingAsync(x => x.CrCasRenterContractAdditionalCode == "5000000003" || x.CrCasRenterContractAdditionalCode == "5000000004");

            var contractNumbers = contractReceivingTheCarFromAnotherBranch.Select(x => x.CrCasRenterContractAdditionalNo).ToList();
            var additionalContracts = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x => contractNumbers.Contains(x.CrCasRenterContractBasicNo) && (x.CrCasRenterContractBasicStatus == Status.Active || x.CrCasRenterContractBasicStatus == Status.Expire) &&
                x.CrCasRenterContractBasicLessor == lessorCode, new[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" });

            contracts.AddRange(additionalContracts.Except(contracts));

            var contractMap = _mapper.Map<List<ContractSettlementVM>>(contracts);
            bsLayoutVM.ContractSettlements = contractMap.OrderBy(x => x.CrCasRenterContractBasicExpectedEndDate).ToList();
            return View(bsLayoutVM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetContractBySearch(string search)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var bsLayoutVM = await GetBranchesAndLayout();
            var contracts = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x => (x.CrCasRenterContractBasicStatus == Status.Active || x.CrCasRenterContractBasicStatus == Status.Expire) && x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicBranch == bsLayoutVM.SelectedBranch, new[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" });

            var additionalContracts = await _unitOfWork.CrCasRenterContractAdditional.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractAdditionalCode == "5000000003" || x.CrCasRenterContractAdditionalCode == "5000000004");
            var additionalContractNumbers = additionalContracts.Select(x => x.CrCasRenterContractAdditionalNo).ToList();

            var additionalContractsData = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x => additionalContractNumbers.Contains(x.CrCasRenterContractBasicNo) &&
              (x.CrCasRenterContractBasicStatus == Status.Active || x.CrCasRenterContractBasicStatus == Status.Expire) && x.CrCasRenterContractBasicLessor == lessorCode, new[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" });

            contracts = contracts.Union(additionalContractsData).ToList();
            var contractMap = _mapper.Map<List<ContractSettlementVM>>(contracts);

            // فلترة العقود بناءً على البحث
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                bsLayoutVM.ContractSettlements = contractMap
                    .Where(x => x.CrCasRenterContractBasicNo.Contains(search) ||
                                 x.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationArName.Contains(search) ||
                                 x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName.Contains(search) ||
                                 x.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationEnName.ToLower().Contains(search) ||
                                 x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName.ToLower().Contains(search)).ToList();

                return PartialView("_ContractSettlement", bsLayoutVM);
            }
            bsLayoutVM.ContractSettlements = contractMap.OrderBy(x => x.CrCasRenterContractBasicExpectedEndDate).ToList();
            return PartialView("_ContractSettlement", bsLayoutVM);
        }
        [HttpGet]
        public async Task<IActionResult> Create(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            var lessorCode = user.CrMasUserInformationLessor;
            await SetPageTitleAsync(string.Empty, pageNumber);
            var bsLayoutVM = await GetBranchesAndLayout();
            var contract = (await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractBasicNo == id,
                                                                                     new[] { "CrCasRenterContractBasic5.CrCasRenterLessorNavigation",
                                                                                             "CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarAdvantages",
                                                                                             "CrCasRenterContractBasic1","CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationDistributionNavigation"}))
                                                                                             .OrderByDescending(x => x.CrCasRenterContractBasicCopy).FirstOrDefault();
            if (contract == null) return RedirectToAction("Error", "Account", new { area = "Identity", statusCode = 500 });
            if (contract.CrCasRenterContractBasicStatus == Status.Closed) return RedirectToAction("Index", "Home");
            var CheckupCars = await _unitOfWork.CrMasSupContractCarCheckup.FindAllAsNoTrackingAsync(x => x.CrMasSupContractCarCheckupStatus == Status.Active, new[] { "CrMasSupContractCarCheckupDetails" });
            var authContract = await _unitOfWork.CrCasRenterContractAuthorization.FindAsync(x => x.CrCasRenterContractAuthorizationLessor == lessorCode && x.CrCasRenterContractAuthorizationContractNo == contract.CrCasRenterContractBasicNo);
            var contractMap = _mapper.Map<ContractSettlementVM>(contract);
            var PaymentMethod = await _unitOfWork.CrMasSupAccountPaymentMethod.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active && x.CrMasSupAccountPaymentMethodClassification != "4");
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsNoTrackingAsync(x => x.CrCasAccountSalesPointLessor == lessorCode &&
                                                                             x.CrCasAccountSalesPointBrn == bsLayoutVM.SelectedBranch &&
                                                                             x.CrCasAccountSalesPointBankStatus == Status.Active &&
                                                                             x.CrCasAccountSalesPointStatus == Status.Active &&
                                                                             x.CrCasAccountSalesPointBranchStatus == Status.Active);

            var CloseSuspendedContract = await _unitOfWork.CrMasSupContractCloseSuspension.FindAllAsNoTrackingAsync(x => x.CrMasSupContractCloseSuspensionStatus == Status.Active);
            var CloseDelayMechanism = await _unitOfWork.CrMasSupContractCloseMechanism.FindAllAsNoTrackingAsync(x => x.CrMasSupContractCloseMechanismStatus == Status.Active);
            contractMap.RentSystemHourValue = CloseDelayMechanism?.FirstOrDefault(x => x.CrMasSupContractCloseMechanismCode == "13")?.CrMasSupContractCloseMechanismValue?.ToString("N2", CultureInfo.InvariantCulture);
            contractMap.RentSystemDayValue = CloseDelayMechanism?.FirstOrDefault(x => x.CrMasSupContractCloseMechanismCode == "23")?.CrMasSupContractCloseMechanismValue?.ToString("N2", CultureInfo.InvariantCulture);

            var FuelCheckUp = await _unitOfWork.CrCasRenterContractCarCheckup.FindAsync(x => x.CrCasRenterContractCarCheckupNo == contract.CrCasRenterContractBasicNo &&
                                                                                            x.CrCasRenterContractCarCheckupCode == Procedures.FuelCodeInCheckUp,
                                                                                            new[] { "CrCasRenterContractCarCheckupC" });
            var UnlimitedKm = (await _unitOfWork.CrCasRenterContractChoice.FindAsync(x => x.CrCasRenterContractChoiceNo == contract.CrCasRenterContractBasicNo && x.CrCasRenterContractChoiceCode == Procedures.UnlimitedKm));
            ViewBag.AccountCatchReceiptNo = GenerateReceiptNumber(lessorCode, bsLayoutVM.SelectedBranch, "301");
            ViewBag.AccountPaymentReceiptNo = GenerateReceiptNumber(lessorCode, bsLayoutVM.SelectedBranch, "302");
            ViewBag.InvoiceAccount = GenerateInvoiceNumber(lessorCode, bsLayoutVM.SelectedBranch, "309");
            contractMap.AuthEndDate = authContract.CrCasRenterContractAuthorizationEndDate;
            contractMap.AuthType = authContract.CrCasRenterContractAuthorizationType;
            contractMap.CasRenterPreviousBalance = contract.CrCasRenterContractBasic5?.CrCasRenterLessorAvailableBalance;
            var advantages = await _unitOfWork.CrCasRenterContractAdvantage.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractAdvantagesNo == contract.CrCasRenterContractBasicNo);
            // Get BranchName 
            var branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationCode == contractMap.CrCasRenterContractBasicBranch);
            ViewBag.ArBranch = branch.CrCasBranchInformationArShortName;
            ViewBag.EnBranch = branch.CrCasBranchInformationEnShortName;
            //Invoice
            contractMap.InvoicePdfPath = (await _unitOfWork.CrCasAccountInvoice.FindAllAsNoTrackingAsync(x => x.CrCasAccountInvoiceReferenceContract == contract.CrCasRenterContractBasicNo)).LastOrDefault()?.CrCasAccountInvoicePdfFile;
            contractMap.AdvantagesValue = advantages?.Sum(x => x.CrCasContractAdvantagesValue)?.ToString("N2", CultureInfo.InvariantCulture);
            contractMap.AdvantagesValueTotal = (advantages?.Sum(x => x.CrCasContractAdvantagesValue) * contract.CrCasRenterContractBasicExpectedRentalDays)?.ToString("N0", CultureInfo.InvariantCulture);
            var ChoicesValuesAll = await _unitOfWork.CrCasRenterContractChoice.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractChoiceNo == contract.CrCasRenterContractBasicNo);
            contractMap.ChoicesValue = ChoicesValuesAll.Sum(x => x.CrCasContractChoiceValue)?.ToString("N2", CultureInfo.InvariantCulture);
            var ContractChoicesAll = await _unitOfWork.CrCasRenterContractChoice.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractChoiceNo == contract.CrCasRenterContractBasicNo && (x.CrCasRenterContractChoiceCode == "5100000003" || x.CrCasRenterContractChoiceCode == "5100000004"), new[] { "CrCasRenterContractChoiceCodeNavigation" });
            contractMap.ContractChoices = ContractChoicesAll;
            contractMap.CarCheckUpFuel = FuelCheckUp;
            contractMap.UnlimitedKm = UnlimitedKm != null;
            bsLayoutVM.ContractSettlement = contractMap;
            bsLayoutVM.SalesPoint = SalesPoint;
            bsLayoutVM.PaymentMethods = PaymentMethod;
            bsLayoutVM.CarsCheckUp = CheckupCars;
            bsLayoutVM.ContractCloseSuspensions = CloseSuspendedContract;
            bsLayoutVM.ContractCloseMechanism = CloseDelayMechanism;
            return View(bsLayoutVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(BSLayoutVM bSLayoutVM, string? SavePdfInvoice, string? SavePdfReceipt, string SavePdfContract, Dictionary<string, CarCheckupDetailsVM>? CheckupDetails, string StaticContractCardImg)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var ContractInfo = bSLayoutVM.ContractSettlement;
            var OldContract = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicNo == ContractInfo.CrCasRenterContractBasicNo).OrderByDescending(x => x.CrCasRenterContractBasicCopy).FirstOrDefault();

            var Renter = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == OldContract.CrCasRenterContractBasicRenterId);
            var RenterLessor = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == OldContract.CrCasRenterContractBasicRenterId);
            var Car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == OldContract.CrCasRenterContractBasicCarSerailNo);
            var CarPrice = await _unitOfWork.CrCasPriceCarBasic.FindAsync(x => x.CrCasPriceCarBasicNo == OldContract.CrCasRenterContractPriceReference);
            var Branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationCode == bSLayoutVM.SelectedBranch && x.CrCasBranchInformationLessor == lessorCode);

            if (userLogin != null && Renter != null && Car != null && CarPrice != null && Branch != null)
            {
                SavePdfContract = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfContract);
                var pdfContract = await SavePdfAsync(SavePdfContract, lessorCode, Branch.CrCasBranchInformationCode, OldContract.CrCasRenterContractBasicNo, "BnanContract");
                var UpdateSettlementContract = await UpdateRenterContractBasicAsync(lessorCode, Branch, OldContract.CrCasRenterContractBasicNo, ContractInfo, userLogin,
                                                                                    pdfContract, (decimal)RenterLessor.CrCasRenterLessorAvailableBalance);
                if (UpdateSettlementContract == null)
                {
                    await RemovePdfs(new[] { pdfContract });
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index", "Home");
                }

                //Account Receipt
                var CheckAccountReceipt = new CrCasAccountReceipt();
                var pdfReceipt = "";
                var CheckBalances = true;
                var CheckSalesPoint = true;
                var CheckBranchValidity = true;
                var CheckUserInformation = true;
                // Account Receipt
                if (UpdateSettlementContract.CrCasRenterContractBasicAmountPaid > 0)
                {
                    SavePdfReceipt = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfReceipt);
                    if (!string.IsNullOrEmpty(SavePdfReceipt)) pdfReceipt = await SavePdfAsync(SavePdfReceipt, lessorCode, Branch.CrCasBranchInformationCode, ContractInfo.AccountReceiptNo, "Receipt");
                    CheckAccountReceipt = await AddAccountReceiptAsync(UpdateSettlementContract, lessorCode, Branch, ContractInfo, userLogin, pdfReceipt);
                    CheckBalances = await UpdateBalancesAsync(Branch, UpdateSettlementContract, userLogin, ContractInfo);
                }

                // Invoice 
                SavePdfInvoice = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfInvoice);
                var pdfInvoice = await SavePdfAsync(SavePdfInvoice, lessorCode, Branch.CrCasBranchInformationCode, ContractInfo.InitialInvoiceNo, "TaxInvoice");
                var checkAccountInvoiceNo = await AddAccountInvoiceAsync(UpdateSettlementContract, CheckAccountReceipt, pdfInvoice);

                // Renter Balance 
                var TotalContractValue = UpdateSettlementContract.CrCasRenterContractBasicActualTotal - UpdateSettlementContract.CrCasRenterContractBasicExpensesValue + UpdateSettlementContract.CrCasRenterContractBasicCompensationValue;
                var CheckUpdateRenterBalance = await _contractSettlement.UpdateRenterLessor(UpdateSettlementContract.CrCasRenterContractBasicNo, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualAmountRequired,
                                                                                       (decimal)UpdateSettlementContract.CrCasRenterContractBasicAmountPaid, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualTotal,
                                                                                       (decimal)TotalContractValue, (int)UpdateSettlementContract.CrCasRenterContractBasicActualDays, int.Parse(ContractInfo.CurrentMeter));

                var CheckAddAccountContractTaxOwed = await _contractSettlement.AddAccountContractTaxOwed(UpdateSettlementContract.CrCasRenterContractBasicNo, checkAccountInvoiceNo, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualValueAfterDiscount);
                var CheckUpdateAlert = await _contractSettlement.RemoveContractAlert(UpdateSettlementContract.CrCasRenterContractBasicNo);
                var CheckUpdateAuthrization = await _contractSettlement.UpdateAuthrization(UpdateSettlementContract.CrCasRenterContractBasicNo);
                var CheckMasRenter = await _contractSettlement.UpdateMasRenter(Renter.CrMasRenterInformationId);

                var CheckDrivers = await CheckDriversAsync(UpdateSettlementContract);

                //Update DocAndMaintainance Of Car
                var CheckDocAndMaintainance = await _contractSettlement.UpdateCarDocMaintainance(UpdateSettlementContract.CrCasRenterContractBasicCarSerailNo, lessorCode,
                                                                                               Branch.CrCasBranchInformationCode, int.Parse(ContractInfo.CurrentMeter));
                //Update Information Of Car
                var CheckCarInfo = await _contractSettlement.UpdateCarInformation(UpdateSettlementContract.CrCasRenterContractBasicCarSerailNo, lessorCode, Branch.CrCasBranchInformationCode,
                                                                                                          int.Parse(ContractInfo.CurrentMeter), CheckDocAndMaintainance, (int)UpdateSettlementContract.CrCasRenterContractBasicActualDays);
                //CheckUp
                var CheckCheckUpCar = await CheckUpdateCheckUpCarAsync(UpdateSettlementContract, CheckupDetails);
                // add Account Contract Company Owed
                var ChechAddAccountContractCompanyOwed = true;
                var CompanyContract = await _unitOfWork.CrMasContractCompany.FindAsync(x => x.CrMasContractCompanyLessor == UpdateSettlementContract.CrCasRenterContractBasicLessor && x.CrMasContractCompanyProcedures == "112");//ForBnan Contract
                if (CompanyContract.CrMasContractCompanyActivation != "1")
                {
                    ChechAddAccountContractCompanyOwed = await _contractSettlement.AddAccountContractCompanyOwed(UpdateSettlementContract.CrCasRenterContractBasicNo, ContractInfo.ActualDaysNo, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualDailyRent);
                }

                var ChechUpdateRenterStatistics = await _contractSettlement.UpdateRenterStatistics(UpdateSettlementContract, userLogin.CrMasUserInformationCode, (decimal)CheckAddAccountContractTaxOwed);
                var pdfDictionaryPaths = GetPdfDictionaryWithPath(pdfContract, pdfInvoice, pdfReceipt, (decimal)UpdateSettlementContract.CrCasRenterContractBasicAmountPaid);
                bool checkPdf = CheckPdfs(pdfDictionaryPaths.Keys.ToArray());
                if (!checkPdf)
                {
                    await RemovePdfs(pdfDictionaryPaths.Keys.ToArray());
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index", "Home");
                }

                if (UpdateSettlementContract != null && CheckAccountReceipt != null && CheckBalances && CheckSalesPoint && CheckBranchValidity && CheckUserInformation && checkAccountInvoiceNo != null &&
                    CheckMasRenter && CheckUpdateAuthrization && CheckUpdateAlert && CheckAddAccountContractTaxOwed != null && CheckUpdateRenterBalance && CheckDrivers
                    && !string.IsNullOrEmpty(CheckDocAndMaintainance) && CheckCarInfo && CheckCheckUpCar && ChechAddAccountContractCompanyOwed && ChechUpdateRenterStatistics)
                {
                    if (await _unitOfWork.CompleteAsync() > 0)
                    {
                        var toNumber = GetPhoneNumber(userLogin, Renter);
                        if (StaticContractCardImg != null) await WhatsAppServicesExtension.SendMediaAsync(toNumber, " ", lessorCode, StaticContractCardImg, "ContractCard.png");
                        var pdfDictionaryBase64 = GetPdfDictionaryBase64(SavePdfContract, SavePdfInvoice, SavePdfReceipt, (decimal)UpdateSettlementContract.CrCasRenterContractBasicAmountPaid);
                        await SendPdfsToWhatsAppAsync(pdfDictionaryBase64, lessorCode, CheckAccountReceipt.CrCasAccountReceiptNo, checkAccountInvoiceNo, UpdateSettlementContract.CrCasRenterContractBasicNo,
                                                      toNumber, Renter.CrMasRenterInformationArName, Renter.CrMasRenterInformationEnName);
                        _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        this.SetRenterTempData(Renter.CrMasRenterInformationId, ContractInfo.CrCasRenterContractBasicNo);
                        return RedirectToAction("Index", "Home");

                    }
                    else
                    {
                        await RemovePdfs(pdfDictionaryPaths.Keys.ToArray());
                    }
                }
                else
                {
                    await RemovePdfs(pdfDictionaryPaths.Keys.ToArray());
                }
            }
            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home");
        }
        private async Task<CrCasRenterContractBasic> UpdateRenterContractBasicAsync(string lessorCode, CrCasBranchInformation branch, string contractNo, ContractSettlementVM ContractInfo, CrMasUserInformation userLogin, string pdfContract, decimal RenterLessorAvailableBalance)
        {
            return await _contractSettlement.UpdateRenterSettlementContract(contractNo, userLogin.CrMasUserInformationCode, ContractInfo.ActualDaysNo, ContractInfo.CrCasRenterContractBasicDelayMechanism, ContractInfo.CurrentMeter, ContractInfo.AdditionalKm,
                                                                                                       ContractInfo.TaxValue, ContractInfo.DiscountValue, ContractInfo.AmountRequired, ContractInfo.AmountPayed, ContractInfo.ExpensesValue,
                                                                                                       ContractInfo.ExpensesReasons, ContractInfo.CompensationValue, ContractInfo.CompensationReasons, ContractInfo.MaxHours,
                                                                                                       ContractInfo.MaxMinutes, ContractInfo.ExtraHoursValue, ContractInfo.PrivateDriverValueTotal, ContractInfo.ChoicesValueTotal,
                                                                                                       ContractInfo.AdvantagesValueTotal, ContractInfo.ContractValue, ContractInfo.ContractValueAfterDiscount, ContractInfo.TotalContract,
                                                                                                       (decimal)RenterLessorAvailableBalance, ContractInfo.CrCasRenterContractBasicCloseStatus, pdfContract, ContractInfo.CrCasRenterContractBasicReasons);
        }
        private async Task<CrCasAccountReceipt> AddAccountReceiptAsync(CrCasRenterContractBasic UpdateContractInfo, string lessorCode, CrCasBranchInformation branch, ContractSettlementVM ContractInfo,
            CrMasUserInformation userLogin, string pdfReceipt)
        {
            string passing = ContractInfo.PaymentMethod == "30" ? "4" : "1";
            if (ContractInfo.PaymentMethod == "30")
            {
                ContractInfo.AccountNo = ContractInfo.SalesPoint;
            }
            string procedureCode = "";

            // Catch Receipt سند قبض
            if (UpdateContractInfo.CrCasRenterContractBasicActualAmountRequired > 0) procedureCode = "301";
            else if (UpdateContractInfo.CrCasRenterContractBasicActualAmountRequired < 0) procedureCode = "302";
            return await _contractSettlement.AddAccountReceipt(UpdateContractInfo.CrCasRenterContractBasicNo, lessorCode, branch.CrCasBranchInformationCode,
                                                                          ContractInfo.PaymentMethod, ContractInfo.AccountNo, UpdateContractInfo.CrCasRenterContractBasicCarSerailNo,
                                                                          ContractInfo.SalesPoint, (decimal)UpdateContractInfo.CrCasRenterContractBasicAmountPaid,
                                                                          UpdateContractInfo.CrCasRenterContractBasicRenterId, userLogin.CrMasUserInformationCode, passing, ContractInfo.ReasonsPayment, pdfReceipt, procedureCode);
        }
        private async Task<string> AddAccountInvoiceAsync(CrCasRenterContractBasic basicContract, CrCasAccountReceipt accountReceipt, string pdfInvoice)
        {
            return await _contractSettlement.AddAccountInvoice(
                basicContract.CrCasRenterContractBasicNo, basicContract.CrCasRenterContractBasicRenterId,
                basicContract.CrCasRenterContractBasicLessor, basicContract.CrCasRenterContractBasicBranch,
                basicContract.CrCasRenterContractBasicUserInsert, accountReceipt.CrCasAccountReceiptNo, pdfInvoice);
        }
        private async Task<bool> CheckDriversAsync(CrCasRenterContractBasic contractInfo)
        {
            if (!string.IsNullOrEmpty(contractInfo.CrCasRenterContractBasicPrivateDriverId))
            {
                return await _contractSettlement.UpdatePrivateDriverStatus(contractInfo.CrCasRenterContractBasicPrivateDriverId, contractInfo.CrCasRenterContractBasicLessor);
            }

            //if (!string.IsNullOrEmpty(contractInfo.CrCasRenterContractBasicDriverId) && contractInfo.CrCasRenterContractBasicDriverId.Trim() != contractInfo.CrCasRenterContractBasicRenterId)
            //{
            //    if (!await _contractSettlement.UpdateDriverStatus(contractInfo.CrCasRenterContractBasicDriverId, contractInfo.CrCasRenterContractBasicLessor))
            //    {
            //        return false;
            //    }
            //}
            //if (!string.IsNullOrEmpty(contractInfo.CrCasRenterContractBasicAdditionalDriverId))
            //{
            //    if (!await _contractSettlement.UpdateDriverStatus(contractInfo.CrCasRenterContractBasicAdditionalDriverId, contractInfo.CrCasRenterContractBasicLessor))
            //    {
            //        return false;
            //    }
            //}
            return true;
        }
        private async Task<bool> SendPdfsToWhatsAppAsync(Dictionary<string, string> pdfDictionary, string lessorCode, string receiptNo, string invoiceNo, string contractNo, string toNumber, string renterArName, string renterEnName)
        {
            if (pdfDictionary != null)
            {
                foreach (var kvp in pdfDictionary)
                {
                    var fileNo = "";
                    string pdf = kvp.Key;
                    string documentType = kvp.Value;
                    string message = WhatsupExtension.GetMessage(documentType, renterArName, renterEnName);
                    if (documentType == "Receipt" && !string.IsNullOrEmpty(pdf)) fileNo = receiptNo;
                    else if (documentType == "Invoice" && !string.IsNullOrEmpty(pdf)) fileNo = invoiceNo;
                    else if (documentType == "Contract" && !string.IsNullOrEmpty(pdf)) fileNo = contractNo;
                    if (!string.IsNullOrEmpty(fileNo)) await WhatsAppServicesExtension.SendMediaAsync(toNumber, message, lessorCode, pdf, $"{fileNo}.pdf");
                }
                ;
                return true;
            }
            return false;
        }
        private Dictionary<string, string> GetPdfDictionaryWithPath(string Contract, string Invoice, string Receipt, decimal amountPaid)
        {
            var pdfDictionary = new Dictionary<string, string>();
            pdfDictionary = new Dictionary<string, string> { { Contract, "Contract" }, { Invoice, "Invoice" } };
            if (amountPaid > 0) pdfDictionary.Add(Receipt, "Receipt");
            return pdfDictionary;
        }
        private Dictionary<string, string> GetPdfDictionaryBase64(string Contract, string Invoice, string Receipt, decimal amountPaid)
        {
            var pdfDictionary = new Dictionary<string, string>();
            pdfDictionary = new Dictionary<string, string> { { Contract, "Contract" }, { Invoice, "Invoice" } };
            if (amountPaid > 0) pdfDictionary.Add(Receipt, "Receipt");
            return pdfDictionary;
        }
        private async Task<bool> UpdateBalancesAsync(CrCasBranchInformation Branch, CrCasRenterContractBasic UpdateSettlementContract, CrMasUserInformation userLogin, ContractSettlementVM ContractInfo)
        {
            string passing = ContractInfo.PaymentMethod == "30" ? "4" : "1";
            if (ContractInfo.PaymentMethod == "30")
            {
                ContractInfo.AccountNo = ContractInfo.SalesPoint;
            }
            //Update Branch Balance , But first Check if passing equal 4 or not 
            if (passing != "4")
            {
                if (!await _contractSettlement.UpdateBranchBalance(Branch.CrCasBranchInformationCode, UpdateSettlementContract.CrCasRenterContractBasicLessor, (decimal)UpdateSettlementContract.CrCasRenterContractBasicAmountPaid, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualAmountRequired))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(ContractInfo.SalesPoint) && !await _contractSettlement.UpdateSalesPointBalance(Branch.CrCasBranchInformationCode, UpdateSettlementContract.CrCasRenterContractBasicLessor, ContractInfo.SalesPoint, (decimal)UpdateSettlementContract.CrCasRenterContractBasicAmountPaid, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualAmountRequired))
                {
                    return false;
                }
                if (!await _contractSettlement.UpdateBranchValidity(Branch.CrCasBranchInformationCode, UpdateSettlementContract.CrCasRenterContractBasicLessor, userLogin.CrMasUserInformationCode, ContractInfo.PaymentMethod, (decimal)UpdateSettlementContract.CrCasRenterContractBasicAmountPaid, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualAmountRequired))
                {
                    return false;
                }
                if (!await _contractSettlement.UpdateUserBalance(Branch.CrCasBranchInformationCode, UpdateSettlementContract.CrCasRenterContractBasicLessor, userLogin.CrMasUserInformationCode, ContractInfo.PaymentMethod, (decimal)UpdateSettlementContract.CrCasRenterContractBasicAmountPaid, (decimal)UpdateSettlementContract.CrCasRenterContractBasicActualAmountRequired))
                {
                    return false;
                }
            }
            return true;
        }
        private async Task<bool> CheckUpdateCheckUpCarAsync(CrCasRenterContractBasic basicContract, Dictionary<string, CarCheckupDetailsVM>? reasons)
        {
            var cultureName = CultureInfo.CurrentCulture.Name;
            var isArabic = cultureName == "ar-EG";

            // التحقق من أن البيانات ليست فارغة
            if (reasons == null || !reasons.Any())
                return true;
            foreach (var item in reasons)
            {
                var checkUpDetail = await _unitOfWork.CrMasSupContractCarCheckupDetail.FindAsync(x =>
                    x.CrMasSupContractCarCheckupDetailsCode == item.Key &&
                    x.CrMasSupContractCarCheckupDetailsNo == item.Value.ReasonCheckCode &&
                    x.CrMasSupContractCarCheckupDetailsStatus == Status.Active);

                // التحقق إذا لم يتم العثور على التفاصيل
                if (checkUpDetail == null)
                {
                    continue; // أو يمكنك إرجاع false إذا كان ذلك مطلوباً
                }

                // التحقق من اسم الفحص
                string nameOfCheckUpDetail = isArabic
                    ? (checkUpDetail.CrMasSupContractCarCheckupDetailsArName ?? "اسم غير متوفر")
                    : (checkUpDetail.CrMasSupContractCarCheckupDetailsEnName ?? "Name not available");

                // التحقق من السبب وإضافة القيمة الافتراضية
                string reason = string.IsNullOrEmpty(item.Value.Reason)
                    ? nameOfCheckUpDetail
                    : $"{nameOfCheckUpDetail} - {item.Value.Reason}";

                // استدعاء الخدمة
                bool isUpdated = await _contractSettlement.UpdateRenterContractCheckUp(basicContract.CrCasRenterContractBasicLessor, basicContract?.CrCasRenterContractBasicNo, basicContract?.CrCasRenterContractBasicCarSerailNo,
                                                                                basicContract?.CrCasRenterContractPriceReference, item.Key, item.Value.ReasonCheckCode, item.Value.CheckUp, reason);

                if (!isUpdated)
                {
                    return false;
                }
            }

            return true;
        }
        private Dictionary<string, string> GetPdfDictionary(string culture, string Contract, string Invoice, string Receipt, decimal amountPaid)
        {
            var pdfDictionary = new Dictionary<string, string>();
            pdfDictionary = new Dictionary<string, string> { { Contract, "Contract" }, { Invoice, "Invoice" } };
            if (amountPaid > 0) pdfDictionary.Add(Receipt, "Receipt");
            return pdfDictionary;
        }
        private async Task<string> SavePdfAsync(string? savePdf, string lessorCode, string branchCode, string contractNo, string type)
        {
            if (!string.IsNullOrEmpty(savePdf))
            {
                return await FileExtensions.SavePdf(_hostingEnvironment, savePdf, lessorCode, branchCode, contractNo, type);
            }
            return string.Empty;
        }
        private async Task RemovePdfs(string[] pdfPaths)
        {
            foreach (var pdf in pdfPaths)
            {
                await FileExtensions.RemoveFile(_hostingEnvironment, pdf);
            }
        }
        private bool CheckPdfs(string[] PdfsPath)
        {
            if (PdfsPath == null || PdfsPath.Any(pdf => string.IsNullOrEmpty(pdf)))
            {
                return false;
            }
            return true;
        }
        [HttpGet]
        public async Task<IActionResult> Get_ConvertedNumber_Action(string our_No)
        {

            var (ArConcatenate, EnConcatenate) = _convertedText.ConvertNumber(our_No, "Ar");

            var result = new
            {
                ar_concatenate = ArConcatenate,
                en_concatenate = EnConcatenate,
            };
            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetSalesPoint(string PaymentMethod, string BranchCode)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            List<SalesPointsVM> SalesPointVMList = new List<SalesPointsVM>();
            List<AccountBankVM> AccountBankVMList = new List<AccountBankVM>();
            var Type = "0";
            if (PaymentMethod != null)
            {
                if (PaymentMethod == "10")
                {
                    var SalesPoints = _unitOfWork.CrCasAccountSalesPoint.FindAll(x => x.CrCasAccountSalesPointLessor == lessorCode && x.CrCasAccountSalesPointBrn == BranchCode &&
                                                                           x.CrCasAccountSalesPointStatus == Status.Active && x.CrCasAccountSalesPointBankStatus == Status.Active &&
                                                                           x.CrCasAccountSalesPointBranchStatus == Status.Active && x.CrCasAccountSalesPointBank == "00").ToList();
                    Type = "1";

                    foreach (var item in SalesPoints)
                    {
                        SalesPointsVM SalesPointVM = new SalesPointsVM
                        {
                            CrCasAccountSalesPointNo = item.CrCasAccountSalesPointNo,
                            CrCasAccountSalesPointCode = item.CrCasAccountSalesPointCode,
                            CrCasAccountSalesPointArName = item.CrCasAccountSalesPointArName,
                            CrCasAccountSalesPointEnName = item.CrCasAccountSalesPointEnName,
                            CrCasAccountSalesPointBank = item.CrCasAccountSalesPointBank,
                            CrCasAccountSalesPointAccountBank = item.CrCasAccountSalesPointAccountBank
                        };
                        SalesPointVMList.Add(SalesPointVM);
                    }
                }
                else if (PaymentMethod == "20" || PaymentMethod == "22" || PaymentMethod == "21" || PaymentMethod == "23")
                {
                    var SalesPoints = _unitOfWork.CrCasAccountSalesPoint.FindAll(x => x.CrCasAccountSalesPointLessor == lessorCode && x.CrCasAccountSalesPointBrn == BranchCode &&
                                                                           x.CrCasAccountSalesPointStatus == Status.Active && x.CrCasAccountSalesPointBankStatus == Status.Active &&
                                                                           x.CrCasAccountSalesPointBranchStatus == Status.Active && x.CrCasAccountSalesPointBank != "00").ToList();
                    Type = "1";
                    foreach (var item in SalesPoints)
                    {
                        SalesPointsVM SalesPointVM = new SalesPointsVM
                        {
                            CrCasAccountSalesPointNo = item.CrCasAccountSalesPointNo,
                            CrCasAccountSalesPointCode = item.CrCasAccountSalesPointCode,
                            CrCasAccountSalesPointArName = item.CrCasAccountSalesPointArName,
                            CrCasAccountSalesPointEnName = item.CrCasAccountSalesPointEnName,
                            CrCasAccountSalesPointBank = item.CrCasAccountSalesPointBank,
                            CrCasAccountSalesPointAccountBank = item.CrCasAccountSalesPointAccountBank
                        };
                        SalesPointVMList.Add(SalesPointVM);
                    }
                }
                else
                {
                    var AccountBanks = _unitOfWork.CrCasAccountBank.FindAll(x => x.CrCasAccountBankLessor == lessorCode && x.CrCasAccountBankStatus == Status.Active &&
                                                                         x.CrCasAccountBankNo != "00").ToList();
                    Type = "2";
                    foreach (var item in AccountBanks)
                    {
                        AccountBankVM AccountBankVM = new AccountBankVM
                        {
                            CrCasAccountBankNo = item.CrCasAccountBankNo,
                            CrCasAccountBankArName = item.CrCasAccountBankArName,
                            CrCasAccountBankEnName = item.CrCasAccountBankEnName,
                            CrCasAccountBankCode = item.CrCasAccountBankCode,
                        };
                        AccountBankVMList.Add(AccountBankVM);
                    }
                }
                return Json(new { SalesPoints = SalesPointVMList, AccountBank = AccountBankVMList, Type = Type });
            }
            return Json(null);
        }
        private CrCasAccountReceipt GetContractAccountReceipt(string LessorCode, string BranchCode, string Procedure)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == LessorCode &&
                                                                       x.CrCasAccountReceiptYear == y && x.CrCasAccountReceiptBranchCode == BranchCode && x.CrCasAccountReceiptType == Procedure)
                                                             .Max(x => x.CrCasAccountReceiptNo.Substring(x.CrCasAccountReceiptNo.Length - 6, 6));

            CrCasAccountReceipt c = new CrCasAccountReceipt();
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                c.CrCasAccountReceiptNo = val.ToString("000000");
            }
            else
            {
                c.CrCasAccountReceiptNo = "000001";
            }

            return c;
        }
        private CrCasAccountInvoice GetInvoiceAccount(string LessorCode, string BranchCode, string ProcedureActualInvoice)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == LessorCode &&
                                                                       x.CrCasAccountInvoiceYear == y && x.CrCasAccountInvoiceType == ProcedureActualInvoice && x.CrCasAccountInvoiceBranchCode == BranchCode)
                                                             .Max(x => x.CrCasAccountInvoiceNo.Substring(x.CrCasAccountInvoiceNo.Length - 6, 6));

            CrCasAccountInvoice c = new CrCasAccountInvoice();
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                c.CrCasAccountInvoiceNo = val.ToString("000000");
            }
            else
            {
                c.CrCasAccountInvoiceNo = "000001";
            }

            return c;
        }
        //This For Home , Index To Rate Renter
        [HttpPost]
        public async Task<JsonResult> UpdateRateForRenter(string ContractNo, string RenterId, string Rate)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var Renter = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == RenterId);
            var Evaluation = await _unitOfWork.CrCasRenterContractEvaluation.FindAsync(x => x.CrCasRenterContractEvaluationContract == ContractNo && x.CrCasRenterContractEvaluationType == "1");
            if (Renter != null && Evaluation != null)
            {
                var RateForRenter = await _contractSettlement.UpdateRateForRenter(RenterId, lessorCode, Rate);
                var RateForRenterEvaluation = await _contractSettlement.UpdateRateForRenterToEvalution(ContractNo, RenterId, lessorCode, Rate);
                if (RateForRenter && RateForRenterEvaluation && await _unitOfWork.CompleteAsync() > 0) return Json(true);
            }
            return Json(false);
        }
        private string GenerateReceiptNumber(string lessorCode, string branch, string procedureCode)
        {
            DateTime year = DateTime.Now;
            string y = year.ToString("yy");
            string autoinc = GetContractAccountReceipt(lessorCode, branch, procedureCode).CrCasAccountReceiptNo;
            return $"{y}-1{procedureCode}-{lessorCode}{branch}-{autoinc}";
        }
        private string GenerateInvoiceNumber(string lessorCode, string branch, string procedureCode)
        {
            DateTime year = DateTime.Now;
            string y = year.ToString("yy");
            string autoinc = GetInvoiceAccount(lessorCode, branch, procedureCode).CrCasAccountInvoiceNo;
            return $"{y}-1{procedureCode}-{lessorCode}{branch}-{autoinc}";
        }
        private string GetPhoneNumber(CrMasUserInformation userLogin, CrMasRenterInformation RenterInfo)
        {
            string userPhoneNumber = userLogin.CrMasUserInformationCallingKey + userLogin.CrMasUserInformationMobileNo;
            string renterPhoneNumber = RenterInfo.CrMasRenterInformationCountreyKey + RenterInfo.CrMasRenterInformationMobile;

            return int.Parse(userLogin.CrMasUserInformationLessor) > 4005 ? renterPhoneNumber : userPhoneNumber;
        }
    }
}
