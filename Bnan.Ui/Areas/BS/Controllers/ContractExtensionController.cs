﻿using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
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
    public class ContractExtensionController : BaseController
    {
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<ContractExtensionController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly IContractExtension _contractExtension;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConvertedText _convertedText;
        private readonly string pageNumber = SubTasks.ExtensionContract;


        public ContractExtensionController(IStringLocalizer<ContractExtensionController> localizer, IUnitOfWork unitOfWork, UserManager<CrMasUserInformation> userManager, IMapper mapper, IToastNotification toastNotification, IAdminstritiveProcedures adminstritiveProcedures, IContractExtension contractExtension, IWebHostEnvironment hostingEnvironment, IConvertedText convertedText) : base(userManager, unitOfWork, mapper)
        {
            _localizer = localizer;
            _toastNotification = toastNotification;
            _adminstritiveProcedures = adminstritiveProcedures;
            _contractExtension = contractExtension;
            _hostingEnvironment = hostingEnvironment;
            _convertedText = convertedText;
        }
        public async Task<IActionResult> Index()
        {
            var userLogin = await _userManager.GetUserAsync(User);
            if (userLogin == null) return RedirectToAction("Login", "Account");

            await SetPageTitleAsync(string.Empty, pageNumber);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var bsLayoutVM = await GetBranchesAndLayout();

            var contracts = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(
                x => x.CrCasRenterContractBasicLessor == lessorCode &&
                     x.CrCasRenterContractBasicBranch == bsLayoutVM.SelectedBranch &&
                     x.CrCasRenterContractBasicStatus == Status.Active &&
                     x.CrCasRenterContractBasicExpectedEndDate >= DateTime.Now,
                new[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" }
            );

            var contractMap = _mapper.Map<List<ContractForExtensionVM>>(contracts);

            var contractNos = contractMap.Select(c => c.CrCasRenterContractBasicNo).ToList();
            var authContracts = await _unitOfWork.CrCasRenterContractAuthorization.FindAllAsNoTrackingAsync(
                x => contractNos.Contains(x.CrCasRenterContractAuthorizationContractNo) &&
                     x.CrCasRenterContractAuthorizationLessor == lessorCode
            );

            foreach (var contract in contractMap)
            {
                contract.AuthEndDate = authContracts.FirstOrDefault(a => a.CrCasRenterContractAuthorizationContractNo == contract.CrCasRenterContractBasicNo)?.CrCasRenterContractAuthorizationEndDate;
            }

            bsLayoutVM.ExtensionContracts = contractMap.Where(x => x.AuthEndDate > DateTime.Now)
                                                       .OrderBy(x => x.CrCasRenterContractBasicExpectedEndDate)
                                                       .ToList();

            return View(bsLayoutVM);
        }
        //[HttpGet]
        //public async Task<PartialViewResult> GetContractBySearch(string search)
        //{
        //    var userLogin = await _userManager.GetUserAsync(User);
        //    var lessorCode = userLogin.CrMasUserInformationLessor;
        //    var bsLayoutVM = await GetBranchesAndLayout();
        //    var contracts = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicStatus == Status.Active &&
        //                                                                   x.CrCasRenterContractBasicBranch == bsLayoutVM.SelectedBranch && x.CrCasRenterContractBasicExpectedEndDate > DateTime.Now &&
        //                                                                   x.CrCasRenterContractBasicLessor == lessorCode, new[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" });
        //    var contractMap = _mapper.Map<List<ContractForExtensionVM>>(contracts);

        //    if (!string.IsNullOrEmpty(search))
        //    {

        //        bsLayoutVM.ExtensionContracts = contractMap.Where(x => x.CrCasRenterContractBasicNo.Contains(search) ||
        //                                                                x.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationArName.Contains(search) ||
        //                                                                x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName.Contains(search) ||
        //                                                                x.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationEnName.ToLower().Contains(search) ||
        //                                                                x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName.ToLower().Contains(search)).OrderBy(x => x.CrCasRenterContractBasicExpectedEndDate).ToList();
        //        return PartialView("_ContractExtension", bsLayoutVM);
        //    }
        //    bsLayoutVM.ExtensionContracts = contractMap.OrderBy(x => x.CrCasRenterContractBasicExpectedEndDate).ToList();
        //    return PartialView("_ContractExtension", bsLayoutVM);
        //}
        [HttpGet]
        public async Task<PartialViewResult> GetContractBySearch(string search)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var bsLayoutVM = await GetBranchesAndLayout();

            var contracts = await _unitOfWork.CrCasRenterContractBasic.FindAllAsNoTrackingAsync(
                x => x.CrCasRenterContractBasicStatus == Status.Active &&
                     x.CrCasRenterContractBasicBranch == bsLayoutVM.SelectedBranch &&
                     x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicExpectedEndDate > DateTime.Now,
                new[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" }
            );

            var contractMap = _mapper.Map<List<ContractForExtensionVM>>(contracts);
            if (!string.IsNullOrEmpty(search))
            {
                bsLayoutVM.ExtensionContracts = contractMap.Where(x => x.CrCasRenterContractBasicNo.Contains(search) ||
                                                                         x.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationArName.Contains(search) ||
                                                                         x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName.Contains(search) ||
                                                                         x.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationEnName.ToLower().Contains(search) ||
                                                                         x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName.ToLower().Contains(search))
                                                       .OrderBy(x => x.CrCasRenterContractBasicExpectedEndDate)
                                                       .ToList();
            }
            else
            {
                bsLayoutVM.ExtensionContracts = contractMap.OrderBy(x => x.CrCasRenterContractBasicExpectedEndDate).ToList();
            }

            return PartialView("_ContractExtension", bsLayoutVM);
        }
        [HttpGet]
        public async Task<IActionResult> Create(string id)
        {
            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            if (userLogin == null) return RedirectToAction("Login", "Account");
            await SetPageTitleAsync(string.Empty, pageNumber);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var bsLayoutVM = await GetBranchesAndLayout();
            var contract = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicNo == id,
                                                                                     new[] { "CrCasRenterContractBasic5.CrCasRenterLessorNavigation",
                                                                                             "CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationDistributionNavigation"}).OrderByDescending(x => x.CrCasRenterContractBasicCopy).FirstOrDefault();
            if (contract == null) return RedirectToAction("Error", "Account", new { area = "Identity", statusCode = 500 });
            if (contract.CrCasRenterContractBasicStatus == Status.Closed || contract.CrCasRenterContractBasicExpectedEndDate <= DateTime.Now) return RedirectToAction("Index", "Home");
            var authContract = _unitOfWork.CrCasRenterContractAuthorization.Find(x => x.CrCasRenterContractAuthorizationLessor == lessorCode &&
                x.CrCasRenterContractAuthorizationContractNo == contract.CrCasRenterContractBasicNo);
            var contractMap = _mapper.Map<ContractForExtensionVM>(contract);
            var PaymentMethod = _unitOfWork.CrMasSupAccountPaymentMethod.FindAll(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active && x.CrMasSupAccountPaymentMethodClassification != "4").ToList();
            var SalesPoint = _unitOfWork.CrCasAccountSalesPoint.FindAll(x => x.CrCasAccountSalesPointLessor == lessorCode &&
                                                                             x.CrCasAccountSalesPointBrn == bsLayoutVM.SelectedBranch &&
                                                                             x.CrCasAccountSalesPointBankStatus == Status.Active &&
                                                                             x.CrCasAccountSalesPointStatus == Status.Active &&
                                                                             x.CrCasAccountSalesPointBranchStatus == Status.Active).ToList();
            var advatagesContract = _unitOfWork.CrCasRenterContractAdvantage.FindAll(x => x.CrCasRenterContractAdvantagesNo == contract.CrCasRenterContractBasicNo).Sum(x => x.CrCasContractAdvantagesValue);
            var ChoicesContract = _unitOfWork.CrCasRenterContractChoice.FindAll(x => x.CrCasRenterContractChoiceNo == contract.CrCasRenterContractBasicNo).Sum(x => x.CrCasContractChoiceValue);
            var Invoice = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceReferenceContract == contract.CrCasRenterContractBasicNo).LastOrDefault();
            //Get ACcount Receipt
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var autoinc1 = GetContractAccountReceipt(lessorCode, bsLayoutVM.SelectedBranch).CrCasAccountReceiptNo;
            var AccountReceiptNo = y + "-" + "1" + "301" + "-" + lessorCode + bsLayoutVM.SelectedBranch + "-" + autoinc1;
            ViewBag.AccountReceiptNo = AccountReceiptNo;
            ///////////////////////////////////
            //var sector = Renter.CrCasRenterLessorSector;
            var autoinc2 = GetInvoiceAccount(lessorCode, bsLayoutVM.SelectedBranch).CrCasAccountInvoiceNo;
            var InvoiceAccount = y + "-" + "1" + "308" + "-" + lessorCode + bsLayoutVM.SelectedBranch + "-" + autoinc2;
            ViewBag.InvoiceAccount = InvoiceAccount;
            //////////////////////////////////
            contractMap.AuthEndDate = authContract.CrCasRenterContractAuthorizationEndDate;
            contractMap.AuthType = authContract.CrCasRenterContractAuthorizationType;
            contractMap.CasRenterPreviousBalance = contract.CrCasRenterContractBasic5?.CrCasRenterLessorAvailableBalance;
            contractMap.AdvatagesValue = (advatagesContract * contract.CrCasRenterContractBasicExpectedRentalDays)?.ToString("N0", CultureInfo.InvariantCulture);
            contractMap.ChoicesValue = ChoicesContract?.ToString("N2", CultureInfo.InvariantCulture);
            contractMap.InvoicePdf = Invoice?.CrCasAccountInvoicePdfFile;
            bsLayoutVM.ExtensionContract = contractMap;
            bsLayoutVM.SalesPoint = SalesPoint;
            bsLayoutVM.PaymentMethods = PaymentMethod;
            return View(bsLayoutVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(BSLayoutVM bSLayoutVM, string ExtensionValue, string reasons, string? SavePdfInvoice, string? SavePdfReceipt, string StaticContractCardImg)
        {

            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var ContractInfo = bSLayoutVM.ExtensionContract;
            var Contract = await _unitOfWork.CrCasRenterContractBasic.FindAsync(x => x.CrCasRenterContractBasicNo == ContractInfo.CrCasRenterContractBasicNo && x.CrCasRenterContractBasicStatus == Status.Active);
            if (Contract != null)
            {
                var Renter = _unitOfWork.CrMasRenterInformation.Find(x => x.CrMasRenterInformationId == Contract.CrCasRenterContractBasicRenterId);
                var RenterLessor = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == Contract.CrCasRenterContractBasicRenterId);
                var Car = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationSerailNo == Contract.CrCasRenterContractBasicCarSerailNo);
                var CarPrice = _unitOfWork.CrCasPriceCarBasic.Find(x => x.CrCasPriceCarBasicNo == Contract.CrCasRenterContractPriceReference);
                var Branch = _unitOfWork.CrCasBranchInformation.Find(x => x.CrCasBranchInformationCode == bSLayoutVM.SelectedBranch && x.CrCasBranchInformationLessor == lessorCode);
                if (string.IsNullOrEmpty(ContractInfo.AmountPayed)) ContractInfo.AmountPayed = "0";
                var NewContract = await _contractExtension.AddRenterExtensionContract(Contract.CrCasRenterContractBasicNo, ContractInfo.DaysNo, userLogin.CrMasUserInformationCode, ContractInfo.AmountPayed, reasons);
                if (NewContract != null)
                {
                    var CheckUpdateContractStatus = true;
                    CheckUpdateContractStatus = await _contractExtension.UpdateStatusOldContract(Contract.CrCasRenterContractBasicNo);

                    //Account Receipt
                    var CheckAccountReceipt = new CrCasAccountReceipt();
                    var CheckRenterLessor = true;
                    var CheckBranch = true;
                    var CheckSalesPoint = true;
                    var CheckBranchValidity = true;
                    var CheckUserInformation = true;

                    var passing = "";
                    var PdfReceipt = "";
                    if (decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture) != null && decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture) > 0)
                    {

                        if (ContractInfo.PaymentMethod == "30")
                        {
                            passing = "4";
                            ContractInfo.AccountNo = ContractInfo.SalesPoint;
                        }
                        else
                        {
                            passing = "1";
                        }
                        SavePdfReceipt = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfReceipt);
                        if (!string.IsNullOrEmpty(SavePdfReceipt)) PdfReceipt = await FileExtensions.SavePdf(_hostingEnvironment, SavePdfReceipt, lessorCode, Branch.CrCasBranchInformationCode, ContractInfo.AccountReceiptNo, "Receipt");
                        CheckAccountReceipt = await _contractExtension.AddAccountReceipt(NewContract.CrCasRenterContractBasicNo, lessorCode, NewContract.CrCasRenterContractBasicBranch,
                                                                                      ContractInfo.PaymentMethod, ContractInfo.AccountNo, NewContract.CrCasRenterContractBasicCarSerailNo,
                                                                                      ContractInfo.SalesPoint, decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture),
                                                                                      NewContract.CrCasRenterContractBasicRenterId, userLogin.CrMasUserInformationCode, passing, reasons, PdfReceipt);
                        //Update Branch Balance , But first Check if passing equal 4 or not 
                        if (passing != "4") CheckBranch = await _contractExtension.UpdateBranchBalance(Branch.CrCasBranchInformationCode, lessorCode, decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture));
                        //Update SalesPoint Balance , But first Check if passing equal 4 or not 
                        if (!string.IsNullOrEmpty(ContractInfo.SalesPoint) && passing != "4") CheckSalesPoint = await _contractExtension.UpdateSalesPointBalance(Branch.CrCasBranchInformationCode, lessorCode, ContractInfo.SalesPoint, decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture));
                        // UpdateBranchValidity
                        if (passing != "4") CheckBranchValidity = await _contractExtension.UpdateBranchValidity(Branch.CrCasBranchInformationCode, lessorCode, userLogin.CrMasUserInformationCode, ContractInfo.PaymentMethod, decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture));
                        // UpdateUserBalance
                        if (passing != "4") CheckUserInformation = await _contractExtension.UpdateUserBalance(Branch.CrCasBranchInformationCode, lessorCode, userLogin.CrMasUserInformationCode, ContractInfo.PaymentMethod, decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture));
                    }


                    // Invoice 
                    var PdfInovice = "";
                    SavePdfInvoice = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfInvoice);
                    if (!string.IsNullOrEmpty(SavePdfInvoice)) PdfInovice = await FileExtensions.SavePdf(_hostingEnvironment, SavePdfInvoice, lessorCode, Branch.CrCasBranchInformationCode, ContractInfo.InitialInvoiceNo, "ProformaInvoice");
                    var CheckAccountInvoice = await _contractExtension.AddAccountInvoice(NewContract.CrCasRenterContractBasicNo, NewContract.CrCasRenterContractBasicRenterId, lessorCode, Branch.CrCasBranchInformationCode,
                        NewContract.CrCasRenterContractBasicUserInsert, CheckAccountReceipt.CrCasAccountReceiptNo, PdfInovice);



                    //Update RenterLessor Of Car Renter
                    CheckRenterLessor = await _contractExtension.UpdateRenterLessor(NewContract.CrCasRenterContractBasicRenterId, lessorCode, decimal.Parse(ContractInfo.AmountPayed, CultureInfo.InvariantCulture), decimal.Parse(ExtensionValue, CultureInfo.InvariantCulture));

                    // Add Renter Alert
                    var CheckRenterAlert = true;
                    CheckRenterAlert = await _contractExtension.UpdateAlertContract(NewContract);

                    var pdfDictionary = GetPdfDictionaryWithPath(PdfInovice, PdfReceipt, (decimal)NewContract.CrCasRenterContractBasicAmountPaidAdvance);
                    bool checkPdf = CheckPdfs(pdfDictionary.Keys.ToArray());
                    if (!checkPdf)
                    {
                        await RemovePdfs(pdfDictionary.Keys.ToArray());
                        _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return RedirectToAction("Index", "Home");
                    }


                    if (NewContract != null && CheckUpdateContractStatus && CheckRenterLessor && CheckAccountReceipt != null && CheckBranch && CheckSalesPoint &&
                        CheckUserInformation && CheckBranchValidity && CheckRenterAlert && CheckAccountInvoice)
                    {

                        if (await _unitOfWork.CompleteAsync() > 0)
                        {
                            var toNumber = GetPhoneNumber(userLogin, Renter);
                            if (StaticContractCardImg != null) await WhatsAppServicesExtension.SendMediaAsync(toNumber, " ", lessorCode, StaticContractCardImg, "ExtensionContractCard.png");
                            var pdfDictionaryBase64 = GetPdfDictionaryBase64(SavePdfInvoice, SavePdfReceipt, (decimal)NewContract.CrCasRenterContractBasicAmountPaidAdvance);
                            await SendPdfsToWhatsAppAsync(pdfDictionaryBase64, lessorCode, ContractInfo.AccountReceiptNo, ContractInfo.InitialInvoiceNo,
                                                          NewContract.CrCasRenterContractBasicNo, toNumber, Renter.CrMasRenterInformationArName, Renter.CrMasRenterInformationEnName);
                            _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            await RemovePdfs(pdfDictionary.Keys.ToArray());
                        }
                    }
                    else
                    {
                        await RemovePdfs(pdfDictionary.Keys.ToArray());
                    }
                }
            }
            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index");
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
                    if (!string.IsNullOrEmpty(fileNo)) await WhatsAppServicesExtension.SendMediaAsync(toNumber, message, lessorCode, pdf, $"{fileNo}.pdf");
                }
                ;
                return true;
            }
            return false;
        }
        private Dictionary<string, string> GetPdfDictionaryWithPath(string Invoice, string Receipt, decimal amountPaid)
        {
            var pdfDictionary = new Dictionary<string, string>();
            pdfDictionary = new Dictionary<string, string> { { Invoice, "Invoice" } };
            if (amountPaid > 0) pdfDictionary.Add(Receipt, "Receipt");
            return pdfDictionary;
        }
        private Dictionary<string, string> GetPdfDictionaryBase64(string Invoice, string Receipt, decimal amountPaid)
        {
            var pdfDictionary = new Dictionary<string, string>();
            pdfDictionary = new Dictionary<string, string> { { Invoice, "Invoice" } };
            if (amountPaid > 0) pdfDictionary.Add(Receipt, "Receipt");
            return pdfDictionary;
        }
        //private async Task<string> SavePdfAsync(string? savePdf, string lessorCode, string branchCode, string contractNo, string lang, string type)
        //{
        //    if (!string.IsNullOrEmpty(savePdf))
        //    {
        //        return await FileExtensions.SavePdf(_hostingEnvironment, savePdf, lessorCode, branchCode, contractNo, lang, type);
        //    }
        //    return string.Empty;
        //}

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
        private CrCasAccountReceipt GetContractAccountReceipt(string LessorCode, string BranchCode)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == LessorCode &&
                                                                       x.CrCasAccountReceiptYear == y && x.CrCasAccountReceiptBranchCode == BranchCode)
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
        private CrCasAccountInvoice GetInvoiceAccount(string LessorCode, string BranchCode)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == LessorCode &&
                                                                       x.CrCasAccountInvoiceYear == y && x.CrCasAccountInvoiceBranchCode == BranchCode)
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
        private string GetPhoneNumber(CrMasUserInformation userLogin, CrMasRenterInformation RenterInfo)
        {
            string userPhoneNumber = userLogin.CrMasUserInformationCallingKey + userLogin.CrMasUserInformationMobileNo;
            string renterPhoneNumber = RenterInfo.CrMasRenterInformationCountreyKey + RenterInfo.CrMasRenterInformationMobile;

            return int.Parse(userLogin.CrMasUserInformationLessor) > 4005 ? renterPhoneNumber : userPhoneNumber;
        }
    }

}
