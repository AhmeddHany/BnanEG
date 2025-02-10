using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.BS.CreateContract;
using Bnan.Ui.ViewModels.CAS;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.ComponentModel;
using System.Globalization;

namespace Bnan.Ui.Areas.BS.Controllers
{
    [Area("BS")]
    [Authorize(Roles = "BS")]
    public class ContractController : BaseController
    {
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<ContractController> _localizer;
        private readonly IContract _ContractServices;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConvertedText _convertedText;

        public ContractController(IStringLocalizer<ContractController> localizer, IUnitOfWork unitOfWork, UserManager<CrMasUserInformation> userManager, IMapper mapper, IToastNotification toastNotification, IContract contractServices, IWebHostEnvironment hostingEnvironment, IConvertedText convertedText) : base(userManager, unitOfWork, mapper)
        {
            _localizer = localizer;
            _toastNotification = toastNotification;
            _ContractServices = contractServices;
            _hostingEnvironment = hostingEnvironment;
            _convertedText = convertedText;
        }
        public async Task<IActionResult> CreateContract()
        {
            //To Set Title 
            var titles = await setTitle("502", "5502001", "5");
            await ViewData.SetPageTitleAsync(titles[0], "", titles[2], "", "", titles[3]);
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var cultureName = CultureInfo.CurrentCulture.Name;
            var isArabic = cultureName == "ar-EG";
            var bSLayoutVM = await GetBranchesAndLayout();

            var drivers = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAllAsNoTrackingAsync(x => x.CrCasRenterPrivateDriverInformationLessor == lessorCode && x.CrCasRenterPrivateDriverInformationStatus == Status.Active);

            var carsAvailable = await _unitOfWork.CrCasCarInformation.FindAllAsNoTrackingAsync(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == bSLayoutVM.SelectedBranch && x.CrCasCarInformationStatus == Status.Active &&
                                                                                 x.CrCasCarInformationPriceStatus == true && x.CrCasCarInformationOwnerStatus == Status.Active &&
                                                                                (x.CrCasCarInformationForSaleStatus == Status.Active || x.CrCasCarInformationForSaleStatus == Status.RendAndForSale),
                                                                                new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationCategoryNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics",
                                                                                        "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation" ,"CrCasCarInformationFuelNavigation","CrCasCarInformationCvtNavigation"});

            //var categoryCars = carsAvailable.Select(item => item.CrCasCarInformationCategoryNavigation).Distinct().OrderBy(item => item.CrMasSupCarCategoryCode).ToList();
            var categoryCars = carsAvailable
                .GroupBy(item => new { item.CrCasCarInformationCategoryNavigation?.CrMasSupCarCategoryCode })
                .Select(group => group.First().CrCasCarInformationCategoryNavigation)
                .OrderBy(category => category.CrMasSupCarCategoryCode)
                .ToList();
            var CheckupCars = await _unitOfWork.CrMasSupContractCarCheckup.FindAllAsNoTrackingAsync(x => x.CrMasSupContractCarCheckupStatus == Status.Active, new[] { "CrMasSupContractCarCheckupDetails" });
            ViewBag.StartDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);

            //Check Account Bank And Sales Point and payment method
            var AccountBanks = await _unitOfWork.CrCasAccountBank.FindAllAsNoTrackingAsync(x => x.CrCasAccountBankLessor == lessorCode && x.CrCasAccountBankStatus == Status.Active, new[] { "CrCasAccountSalesPoints" });
            var PaymentMethod = await _unitOfWork.CrMasSupAccountPaymentMethod.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active);
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsNoTrackingAsync(x => x.CrCasAccountSalesPointLessor == lessorCode &&
                                                                             x.CrCasAccountSalesPointBrn == bSLayoutVM.SelectedBranch &&
                                                                             x.CrCasAccountSalesPointBankStatus == Status.Active &&
                                                                             x.CrCasAccountSalesPointStatus == Status.Active &&
                                                                             x.CrCasAccountSalesPointBranchStatus == Status.Active);

            // Fetch all data
            var RenterIdTypes = await _unitOfWork.CrMasSupRenterIdtype.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterIdtypeStatus != Status.Deleted);
            var RenterProffesions = await _unitOfWork.CrMasSupRenterProfession.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterProfessionsStatus != Status.Deleted &&
                                                                                                           x.CrMasSupRenterProfessionsCode != "1400000001" && x.CrMasSupRenterProfessionsCode != "1400000002");


            var Nationailties = await _unitOfWork.CrMasSupRenterNationality.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted);
            var Cities = await _unitOfWork.CrMasSupPostCity.FindAllAsNoTrackingAsync(l => l.CrMasSupPostCityStatus != Status.Deleted);
            var Workplaces = await _unitOfWork.CrMasSupRenterEmployer.FindAllAsNoTrackingAsync(l => l.CrMasSupRenterEmployerStatus != Status.Deleted && l.CrMasSupRenterEmployerCode != "1800000001" && l.CrMasSupRenterEmployerCode != "1800000002");
            var Genders = await _unitOfWork.CrMasSupRenterGender.FindAllAsNoTrackingAsync(l => l.CrMasSupRenterGenderStatus != Status.Deleted && l.CrMasSupRenterGenderCode != "1100000002" && l.CrMasSupRenterGenderCode != "1100000001");
            var CallingKeysAll = await _unitOfWork.CrMasSysCallingKeys.FindAllAsNoTrackingAsync(l => l.CrMasSysCallingKeysStatus != Status.Deleted);
            var CallingKeys = CallingKeysAll.OrderByDescending(x => x.CrMasSysCallingKeysCount).ToList();
            var DrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.FindAllAsNoTrackingAsync(l => l.CrMasSupRenterDrivingLicenseStatus != Status.Deleted && l.CrMasSupRenterDrivingLicenseCode != "1");
            var Policies = await _unitOfWork.CrCasLessorPolicy.FindAllAsNoTrackingAsync(l => l.CrCasLessorPolicyStatus != Status.Deleted);
            // Transform data based on culture
            var nationalitiesArray = Nationailties.Select(c => new
            {
                textAr = c.CrMasSupRenterNationalitiesArName ,
                textEn = c.CrMasSupRenterNationalitiesEnName, // الاسم العربي أو الإنجليزي
                value = c.CrMasSupRenterNationalitiesCode, // كود الجنسية
                naqlGCC = c.CrMasSupRenterNationalitiesNaqlGcc // إضافة العمود الإضافي (مثلاً CrMasSupRenterNationalitiesNaqlGcc)
            }).ToList();
            var citiesArray = Cities.Select(c => new { textAr = c.CrMasSupPostCityConcatenateArName ,textEn = c.CrMasSupPostCityConcatenateEnName, value = c.CrMasSupPostCityCode }).ToList();
            var workplacesArray = Workplaces.Select(c => new { textAr =  c.CrMasSupRenterEmployerArName, textEn = c.CrMasSupRenterEmployerEnName, value = c.CrMasSupRenterEmployerCode }).ToList();

            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var sector = "1";
            var autoinc = GetContractLastRecord("1", lessorCode, bSLayoutVM.SelectedBranch).CrCasRenterContractBasicNo;
            var BasicContractNo = y + "-" + sector + "401" + "-" + lessorCode + bSLayoutVM.SelectedBranch + "-" + autoinc;
            ViewBag.ContractNo = BasicContractNo;

            // get data and send it to View And Create image  
            //var sector = Renter.CrCasRenterLessorSector;
            var autoinc1 = GetContractAccountReceipt(lessorCode, bSLayoutVM.SelectedBranch).CrCasAccountReceiptNo;
            var AccountReceiptNo = y + "-" + "1" + "301" + "-" + lessorCode + bSLayoutVM.SelectedBranch + "-" + autoinc1;
            ViewBag.AccountReceiptNo = AccountReceiptNo;

            // get data and send it to View And Create image  

            //var sector = Renter.CrCasRenterLessorSector;
            var autoinc2 = GetInvoiceAccount(lessorCode, bSLayoutVM.SelectedBranch).CrCasAccountInvoiceNo;
            var InvoiceAccount = y + "-" + "1" + "308" + "-" + lessorCode + bSLayoutVM.SelectedBranch + "-" + autoinc2;
            ViewBag.InvoiceAccount = InvoiceAccount;

            bSLayoutVM.Drivers = drivers;
            bSLayoutVM.CarCategories = categoryCars;
            bSLayoutVM.CarsFilter = carsAvailable;
            bSLayoutVM.CarsCheckUp = CheckupCars;
            bSLayoutVM.SalesPoint = SalesPoint;
            bSLayoutVM.AccountBanks = AccountBanks;
            bSLayoutVM.PaymentMethods = PaymentMethod;
            bSLayoutVM.RenterIdtypes = RenterIdTypes;
            bSLayoutVM.RenterGender = Genders;
            bSLayoutVM.CallingKeys = CallingKeys;
            bSLayoutVM.DrivingLicense = DrivingLicenses;
            bSLayoutVM.RenterProfession = RenterProffesions;
            bSLayoutVM.Policies = Policies;
            ViewBag.RenterNationalities = nationalitiesArray;
            ViewBag.RenterCities = citiesArray;
            ViewBag.RenterWorkplaces = workplacesArray;
            return View(bSLayoutVM);
        }
        [HttpPost]
        public async Task<IActionResult> CreateContract(BSLayoutVM? bSLayoutVM, string ChoicesList, string AdditionalsList, Dictionary<string, CarCheckupDetailsVM>? CheckupDetails, bool Contract_OutFeesTmm,
                                                 string? SavePdfInvoice, string? SavePdfReceipt, string? SavePdfContract, string StaticContractCardImg)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (bSLayoutVM?.Contract == null)
            {
                _toastNotification.AddErrorToastMessage("فشلت العملية لعدم استقبال اي بيانات", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            var contractInfo = bSLayoutVM.Contract;
            var branch = _unitOfWork.CrCasBranchInformation.Find(x => x.CrCasBranchInformationCode == bSLayoutVM.SelectedBranch && x.CrCasBranchInformationLessor == lessorCode);
            var sectorCodeForRenter = GetSector(contractInfo.RenterInfo.CrMasRenterInformationId.Substring(0, 1), "");
            if (userLogin == null || branch == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            bool carStatus = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationSerailNo == contractInfo.SerialNo).CrCasCarInformationStatus == Status.Active;
            if (!carStatus)
            {
                _toastNotification.AddErrorToastMessage(_localizer["CarIsRented"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }

            //Renter,Driver,Add Driver
            var isSaved = await SaveRenterDriverInfoAsync(contractInfo);
            if (!isSaved)
            {
                _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء عملية حفظ المستأجر", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }

            var basicContractNo = GenerateContractNumber(lessorCode, branch.CrCasBranchInformationCode, sectorCodeForRenter);
            //Contract
            if (string.IsNullOrEmpty(SavePdfContract))
            {
                _toastNotification.AddErrorToastMessage("فشلت العملية لعدم استقبال ملف العقد، يجب ان تكون الصورة الواحدة لا تزيد 500 كيلو بايت", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            SavePdfContract = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfContract);
            if (string.IsNullOrEmpty(SavePdfContract))
            {
                _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء عملية حفظ العقد", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            var pdfContract = await SavePdfAsync(SavePdfContract, lessorCode, branch.CrCasBranchInformationCode, basicContractNo, "BnanContract");

            contractInfo.SourceCode = "10";//Static Data
            var basicContract = await AddRenterContractBasicAsync(lessorCode, branch, basicContractNo, contractInfo, userLogin, pdfContract , sectorCodeForRenter);
            if (basicContract == null)
            {
                await RemovePdfs(new[] { pdfContract });
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            //Receipt
            var checkAccountReceipt = new CrCasAccountReceipt();
            var amountPaid = (decimal)basicContract.CrCasRenterContractBasicAmountPaidAdvance;
            var pdfReceiptPath = "";
            if (amountPaid > 0)
            {
                SavePdfReceipt = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfReceipt);
                if (string.IsNullOrEmpty(SavePdfReceipt))
                {
                    await RemovePdfs(new[] { pdfContract });
                    _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء عملية حفظ السند", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index", "Home");
                }
                pdfReceiptPath = await SavePdfIfNotEmptyAsync(SavePdfReceipt, lessorCode, branch.CrCasBranchInformationCode, contractInfo.AccountReceiptNo, "Receipt", amountPaid > 0);
                checkAccountReceipt = await AddAccountReceiptAsync(basicContract, lessorCode, sectorCodeForRenter, branch, contractInfo, userLogin, amountPaid, pdfReceiptPath);
            }


            if (amountPaid > 0 && checkAccountReceipt != null)
            {
                await UpdateBalancesAsync(branch, lessorCode, userLogin, amountPaid, contractInfo);
            }


            SavePdfInvoice = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfInvoice);
            if (string.IsNullOrEmpty(SavePdfInvoice))
            {
                await RemovePdfs(new[] { SavePdfReceipt });
                _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء عملية حفظ الفاتورة", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            var pdfInvoicePath = await SavePdfAsync(SavePdfInvoice, lessorCode, branch.CrCasBranchInformationCode, contractInfo.InitialInvoiceNo, "ProformaInvoice");

            var checkAccountInvoice = await AddAccountInvoiceAsync(basicContract, checkAccountReceipt, sectorCodeForRenter, pdfInvoicePath);

            var checks = await PerformAdditionalChecksAsync(basicContract, lessorCode, contractInfo, ChoicesList, AdditionalsList, CheckupDetails);

            var pdfDictionaryPaths = GetPdfDictionaryWithPath(pdfContract, pdfInvoicePath, pdfReceiptPath, amountPaid);

            bool checkPdf = CheckPdfs(pdfDictionaryPaths.Keys.ToArray());
            if (/*!checkPdf ||*/ !checks || !checkAccountInvoice)
            {
                await RemovePdfs(pdfDictionaryPaths.Keys.ToArray());
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                if (StaticContractCardImg != null) await WhatsAppServicesExtension.SendMediaAsync(userLogin.CrMasUserInformationCallingKey + userLogin.CrMasUserInformationMobileNo, " ", lessorCode, StaticContractCardImg, "CreateContractCard.png");
                var pdfDictionaryBase64 = GetPdfDictionaryBase64(SavePdfContract, SavePdfInvoice, SavePdfReceipt, amountPaid);
                await SendPdfsToWhatsAppAsync(pdfDictionaryBase64, lessorCode,contractInfo.AccountReceiptNo,contractInfo.InitialInvoiceNo, basicContract.CrCasRenterContractBasicNo,
                                              userLogin.CrMasUserInformationCallingKey + userLogin.CrMasUserInformationMobileNo, contractInfo.RenterInfo.CrMasRenterInformationArName, contractInfo.RenterInfo.CrMasRenterInformationEnName);
                _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            else
            {
                await RemovePdfs(pdfDictionaryPaths.Keys.ToArray());
            }

            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home");
        }

        private string GenerateContractNumber(string lessorCode, string branchCode, string sector)
        {
            DateTime now = DateTime.Now;
            var year = now.ToString("yy");
            var autoinc = GetContractLastRecord(sector, lessorCode, branchCode).CrCasRenterContractBasicNo;
            return $"{year}-{sector}401-{lessorCode}{branchCode}-{autoinc}";
        }

        private async Task<string> SavePdfAsync(string? savePdf, string lessorCode, string branchCode, string contractNo, string type)
        {
            if (!string.IsNullOrEmpty(savePdf))
            {
                return await FileExtensions.SavePdf(_hostingEnvironment, savePdf, lessorCode, branchCode, contractNo, type);
            }
            return string.Empty;
        }

        private async Task<string> SavePdfIfNotEmptyAsync(string? savePdf, string lessorCode, string branchCode, string receiptNo, string type, bool condition)
        {
            return condition ? await SavePdfAsync(savePdf, lessorCode, branchCode, receiptNo, type) : string.Empty;
        }

        private async Task<bool> SendPdfsToWhatsAppAsync(Dictionary<string, string> pdfDictionary,string lessorCode ,string receiptNo, string invoiceNo,string contractNo, string toNumber, string renterArName, string renterEnName)
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
                    else if (documentType == "Contract"&&!string.IsNullOrEmpty(pdf)) fileNo = contractNo;
                    if (!string.IsNullOrEmpty(fileNo)) await WhatsAppServicesExtension.SendMediaAsync(toNumber, message, lessorCode, pdf, $"{fileNo}.pdf");
                };
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

        private async Task<CrCasRenterContractBasic> AddRenterContractBasicAsync(string lessorCode, CrCasBranchInformation branch, string contractNo, ContractVM contractInfo, CrMasUserInformation userLogin,
             string ContractPdf, string sectorCodeForRenter)
        {
            return await _ContractServices.AddRenterContractBasic(
                lessorCode, branch.CrCasBranchInformationCode, contractNo, contractInfo.RenterInfo.CrMasRenterInformationId, sectorCodeForRenter, contractInfo.DriverInfo.CrMasRenterInformationId,
                contractInfo.PrivateDriverId, contractInfo.AddDriverInfo.CrMasRenterInformationId, contractInfo.SerialNo, contractInfo.PriceNo,
                contractInfo.DaysNo, contractInfo.UserAddHours, contractInfo.UserAddKm, contractInfo.CurrentMeter, contractInfo.OptionTotal,
                contractInfo.AdditionalTotal, contractInfo.ContractValueAfterDiscount, contractInfo.DiscountValue, contractInfo.ContractValueBeforeDiscount,
                contractInfo.TaxValue, contractInfo.TotalContractAmount, userLogin.CrMasUserInformationCode, contractInfo.OutFeesTmm,
                contractInfo.UserDiscount, contractInfo.AmountPayed, ContractPdf,contractInfo.PolicyCode,contractInfo.SourceCode, contractInfo.RenterReasons);
        }

        private async Task<CrCasAccountReceipt> AddAccountReceiptAsync(CrCasRenterContractBasic basicContract, string lessorCode, string sectorCode, CrCasBranchInformation branch,
            ContractVM contractInfo, CrMasUserInformation userLogin, decimal amountPaid, string pdfReceipt)
        {
            if (amountPaid <= 0) return null;

            string passing = contractInfo.PaymentMethod == "30" ? "4" : "1";
            if (contractInfo.PaymentMethod == "30")
            {
                contractInfo.AccountNo = contractInfo.SalesPoint;
            }

            var receipt = await _ContractServices.AddAccountReceipt(
                basicContract.CrCasRenterContractBasicNo, lessorCode, basicContract.CrCasRenterContractBasicBranch,
                contractInfo.PaymentMethod, contractInfo.AccountNo, basicContract.CrCasRenterContractBasicCarSerailNo,
                contractInfo.SalesPoint, amountPaid, basicContract.CrCasRenterContractBasicRenterId, sectorCode, userLogin.CrMasUserInformationCode,
                passing, contractInfo.PaymentReasons, pdfReceipt);
            return receipt;
        }

        private async Task UpdateBalancesAsync(CrCasBranchInformation branch, string lessorCode, CrMasUserInformation userLogin, decimal amountPaid, ContractVM contractInfo)
        {
            if (contractInfo.PaymentMethod != "30")
            {
                await _ContractServices.UpdateBranchBalance(branch.CrCasBranchInformationCode, lessorCode, amountPaid);
                if (!string.IsNullOrEmpty(contractInfo.SalesPoint))
                {
                    await _ContractServices.UpdateSalesPointBalance(branch.CrCasBranchInformationCode, lessorCode, contractInfo.SalesPoint, amountPaid);
                }
                await _ContractServices.UpdateBranchValidity(branch.CrCasBranchInformationCode, lessorCode, userLogin.CrMasUserInformationCode, contractInfo.PaymentMethod, amountPaid);
                await _ContractServices.UpdateUserBalance(branch.CrCasBranchInformationCode, lessorCode, userLogin.CrMasUserInformationCode, contractInfo.PaymentMethod, amountPaid);
            }
        }

        private async Task<bool> AddAccountInvoiceAsync(CrCasRenterContractBasic basicContract, CrCasAccountReceipt accountReceipt, string sector, string pdfInvoice)
        {
            return await _ContractServices.AddAccountInvoice(
                basicContract.CrCasRenterContractBasicNo, basicContract.CrCasRenterContractBasicRenterId, sector,
                basicContract.CrCasRenterContractBasicLessor, basicContract.CrCasRenterContractBasicBranch,
                basicContract.CrCasRenterContractBasicUserInsert, accountReceipt.CrCasAccountReceiptNo, pdfInvoice);
        }

        private async Task<bool> PerformAdditionalChecksAsync(CrCasRenterContractBasic basicContract, string lessorCode, ContractVM contractInfo, string choicesList, string additionalsList, Dictionary<string, CarCheckupDetailsVM> reasons)
        {
            var checkChoices = await CheckChoicesAsync(basicContract, lessorCode, contractInfo, choicesList);
            var checkAdditional = await CheckAdditionalsAsync(basicContract, lessorCode, contractInfo, additionalsList);
            var checkAdvantages = await CheckAdvantagesAsync(basicContract, contractInfo);
            var checkCheckUpCar = await CheckCheckUpCarAsync(basicContract, lessorCode, contractInfo, reasons);
            var checkAuthrization = await _ContractServices.AddRenterContractAuthrization(basicContract.CrCasRenterContractBasicNo, lessorCode, contractInfo.OutFeesTmm, contractInfo.FeesTmmValue);
            var checkDocAndMaintainance = await _ContractServices.UpdateCarDocMaintainance(basicContract.CrCasRenterContractBasicCarSerailNo, lessorCode, basicContract.CrCasRenterContractBasicBranch, int.Parse(contractInfo.CurrentMeter));
            var checkCarInfo = await _ContractServices.UpdateCarInformation(basicContract.CrCasRenterContractBasicCarSerailNo, lessorCode, basicContract.CrCasRenterContractBasicBranch, (DateTime)basicContract.CrCasRenterContractBasicIssuedDate, (int)basicContract.CrCasRenterContractBasicExpectedRentalDays, int.Parse(contractInfo.CurrentMeter), checkDocAndMaintainance);
            var checkRenterLessor = await _ContractServices.UpdateRenterLessor(contractInfo.RenterInfo.CrMasRenterInformationId, lessorCode, (DateTime)basicContract.CrCasRenterContractBasicIssuedDate, (decimal)basicContract.CrCasRenterContractBasicAmountPaidAdvance, (decimal)basicContract.CrCasRenterContractBasicExpectedTotal, contractInfo.RenterInfo.CrMasRenterInformationReasons);
            //var checkMasRenter = await _ContractServices.UpdateMasRenter(contractInfo.RenterInfo.CrMasRenterInformationId);
            var checkDrivers = await CheckDriversAsync(contractInfo, lessorCode);
            var checkRenterAlert = await _ContractServices.AddRenterAlert(basicContract.CrCasRenterContractBasicNo, lessorCode, basicContract.CrCasRenterContractBasicBranch, (int)basicContract.CrCasRenterContractBasicExpectedRentalDays, (DateTime)basicContract.CrCasRenterContractBasicExpectedEndDate, basicContract.CrCasRenterContractBasicCarSerailNo, basicContract.CrCasRenterContractPriceReference);
            var checkRenterStatistics = await _ContractServices.AddRenterStatistics(basicContract);
            var checkRenterEvaluation = await _ContractServices.AddRenterEvaluation(basicContract.CrCasRenterContractBasicNo, lessorCode, contractInfo.RenterInfo.CrMasRenterInformationId, "1", basicContract.CrCasRenterContractBasicUserInsert);
            return checkChoices && checkAdditional && checkAdvantages && checkCheckUpCar && checkAuthrization && checkCarInfo && checkDocAndMaintainance != null && checkRenterLessor /*&& checkMasRenter*/ && checkDrivers && checkRenterAlert && checkRenterStatistics && checkRenterEvaluation;
        }

        private async Task<bool> CheckChoicesAsync(CrCasRenterContractBasic basicContract, string lessorCode, ContractVM contractInfo, string choicesList)
        {
            if (string.IsNullOrEmpty(choicesList)) return true;

            var choiceCodes = choicesList.Split(',').ToList();
            foreach (var item in choiceCodes)
            {
                if (!await _ContractServices.AddRenterContractChoice(lessorCode, basicContract.CrCasRenterContractBasicNo, contractInfo.SerialNo, contractInfo.PriceNo, item.Trim()))
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> CheckAdditionalsAsync(CrCasRenterContractBasic basicContract, string lessorCode, ContractVM contractInfo, string additionalsList)
        {
            if (string.IsNullOrEmpty(additionalsList)) return true;

            var additionalCodes = additionalsList.Split(',').ToList();
            foreach (var item in additionalCodes)
            {
                if (!await _ContractServices.AddRenterContractAdditional(lessorCode, basicContract.CrCasRenterContractBasicNo, contractInfo.SerialNo, contractInfo.PriceNo, item.Trim()))
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> CheckAdvantagesAsync(CrCasRenterContractBasic basicContract, ContractVM contractInfo)
        {
            if (decimal.Parse(contractInfo.AdvantagesTotalValue, CultureInfo.InvariantCulture) <= 0) return true;

            var advantagesCar = _unitOfWork.CrCasPriceCarAdvantage.FindAll(x => x.CrCasPriceCarAdvantagesNo == contractInfo.PriceNo);
            foreach (var item in advantagesCar)
            {
                if (!await _ContractServices.AddRenterContractAdvantages(item, basicContract.CrCasRenterContractBasicNo))
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> CheckCheckUpCarAsync(CrCasRenterContractBasic basicContract, string lessorCode, ContractVM contractInfo, Dictionary<string, CarCheckupDetailsVM> reasons)
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
                var result = await _ContractServices.AddRenterContractCheckUp(lessorCode, basicContract?.CrCasRenterContractBasicNo, contractInfo?.SerialNo,
                                                                                contractInfo?.PriceNo, item.Key, item.Value.ReasonCheckCode, item.Value.CheckUp, reason);

                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> CheckDriversAsync(ContractVM contractInfo, string lessorCode)
        {
            if (!string.IsNullOrEmpty(contractInfo.PrivateDriverId))
            {
                return await _ContractServices.UpdatePrivateDriverStatus(contractInfo.PrivateDriverId, lessorCode);
            }

            if (!string.IsNullOrEmpty(contractInfo.DriverInfo.CrMasRenterInformationId) && contractInfo.DriverInfo.CrMasRenterInformationId.Trim() != contractInfo.RenterInfo.CrMasRenterInformationId)
            {
                if (!await _ContractServices.UpdateDriverStatus(contractInfo.DriverInfo.CrMasRenterInformationId, lessorCode, contractInfo.DriverInfo.CrMasRenterInformationReasons))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(contractInfo.AddDriverInfo.CrMasRenterInformationId))
            {
                if (!await _ContractServices.UpdateDriverStatus(contractInfo.AddDriverInfo.CrMasRenterInformationId, lessorCode, contractInfo.AddDriverInfo.CrMasRenterInformationReasons))
                {
                    return false;
                }
            }
            return true;
        }

        [HttpGet]
        public IActionResult Get_ConvertedNumber_Action(string our_No)
        {

            var (ArConcatenate, EnConcatenate) = _convertedText.ConvertNumber(our_No, "Ar");

            var result = new
            {
                ar_concatenate = ArConcatenate,
                en_concatenate = EnConcatenate,
            };
            return Json(result);
        }


        private bool CheckPdfs(string[] PdfsPath)
        {
            if (PdfsPath == null || PdfsPath.Any(pdf => string.IsNullOrEmpty(pdf)))
            {
                return false;
            }
            return true;
        }
        private async Task RemovePdfs(string[] pdfPaths)
        {
            foreach (var pdf in pdfPaths)
            {
                await FileExtensions.RemoveFile(_hostingEnvironment, pdf);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetRenter(string RenterId)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            //First check if bnan have this renter id or not 
            var BnanRenterInfo = _unitOfWork.CrMasRenterInformation.Find(x => x.CrMasRenterInformationId == RenterId, new[] { "CrMasRenterInformationGenderNavigation",
                                                                                                                           "CrMasRenterInformationNationalityNavigation",
                                                                                                                           "CrMasRenterInformationProfessionNavigation",
                                                                                                                           "CrMasRenterInformationEmployerNavigation",
                                                                                                                           "CrMasRenterInformationDrivingLicenseTypeNavigation"});
            RenterInformationsVM renterInformationsVM = null;

            if (BnanRenterInfo != null)
            {
                var RenterPost = _unitOfWork.CrMasRenterPost.Find(x => x.CrMasRenterPostCode == RenterId);

                var LessorRenterInfo = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == RenterId && x.CrCasRenterLessorCode == lessorCode,
                                                                         new[] { "CrCasRenterContractBasicCrCasRenterContractBasic5s", "CrCasRenterLessorMembershipNavigation" });
                if (LessorRenterInfo == null)
                {
                    //this model for View Only 
                    LessorRenterInfo = await _ContractServices.AddRenterToCasRenterLessor(lessorCode, BnanRenterInfo, RenterPost);
                    if (await _unitOfWork.CompleteAsync() > 0)
                    {
                        renterInformationsVM = MapRenterInfoToViewModel(BnanRenterInfo, LessorRenterInfo, RenterPost);
                        return Json(renterInformationsVM);
                    }
                    else return Json(null);
                }
                else
                {
                    renterInformationsVM = MapRenterInfoToViewModel(BnanRenterInfo, LessorRenterInfo, RenterPost);
                    return Json(renterInformationsVM);
                }

            }
            return Json(null);
        }
        [HttpGet]
        public async Task<IActionResult> GetPrivateDriverInfo(string id)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            //First check if bnan have this renter id or not 
            var PrivateDriverInfo = _unitOfWork.CrCasRenterPrivateDriverInformation.Find(x => x.CrCasRenterPrivateDriverInformationId == id, new[] { "CrCasRenterPrivateDriverInformationGenderNavigation",
                                                                                                                           "CrCasRenterPrivateDriverInformationNationalityNavigation",
                                                                                                                           "CrCasRenterPrivateDriverInformationLicenseTypeNavigation",
                                                                                                                           "CrCasRenterPrivateDriverInformationIdtrypeNavigation","CrCasRenterContractBasics"});
            if (PrivateDriverInfo == null) return Json(null);
            var RenterInfoVM = new RenterInformationsVM
            {
                RenterID = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationId,
                RenterIDType = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationLicenseType,
                RenterIDTypeNameAr = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationIdtrypeNavigation?.CrMasSupRenterIdtypeArName,
                RenterIDTypeNameEn = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationIdtrypeNavigation?.CrMasSupRenterIdtypeEnName,
                PersonalArName = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationArName,
                PersonalEnName = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationEnName,
                GenderCode = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationGender,
                PersonalArGender = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationGenderNavigation?.CrMasSupRenterGenderArName,
                PersonalEnGender = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationGenderNavigation?.CrMasSupRenterGenderEnName,
                PersonalArNationality = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationNationalityNavigation?.CrMasSupRenterNationalitiesArName,
                PersonalEnNationality = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationNationalityNavigation?.CrMasSupRenterNationalitiesEnName,
                NationalityCode = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationNationality,
                PersonalEmail = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationEmail,
                LicenseType = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationLicenseType,
                LicenseCode = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationLicenseNo,
                LicenseArName = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationLicenseTypeNavigation?.CrMasSupRenterDrivingLicenseArName,
                LicenseEnName = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationLicenseTypeNavigation?.CrMasSupRenterDrivingLicenseEnName,
                LicenseExpiryDate = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationLicenseExpiry,
                LicenseIssuedDate = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationIssueIdDate,
                MobileNumber = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationMobile,
                BirthDate = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationBirthDate,
                KeyCountry = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationKeyMobile,
                LastContract = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationLastContract,
                ContractCount = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationContractCount,
                RentalDays = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationDaysCount,
                CountContracts = PrivateDriverInfo?.CrCasRenterContractBasics.Count(),
                ActiveContractsCount = PrivateDriverInfo?.CrCasRenterContractBasics.Count(x => x.CrCasRenterContractBasicStatus != Status.Closed),
                ClosedContractsCount = PrivateDriverInfo?.CrCasRenterContractBasics.Count(x => x.CrCasRenterContractBasicStatus == Status.Closed),
                Signture = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationSignature,
                Reasons = PrivateDriverInfo?.CrCasRenterPrivateDriverInformationReasons
            };
            return Json(RenterInfoVM);
        }

        //[HttpPost]
        //public async Task<IActionResult> AddRenterInformation([FromBody] RenterInfoVM RenterVM)
        //{

        //    var userLogin = await _userManager.GetUserAsync(User);
        //    var lessorCode = userLogin.CrMasUserInformationLessor;
        //    if (!string.IsNullOrEmpty(RenterVM.BirthDate)) RenterVM.CrMasRenterInformationBirthDate = DateTime.Parse(RenterVM.BirthDate).Date;
        //    if (!string.IsNullOrEmpty(RenterVM.ExpiryLicenseDate)) RenterVM.CrMasRenterInformationExpiryDrivingLicenseDate = DateTime.Parse(RenterVM.ExpiryLicenseDate).Date;
        //    var RenterModel = _mapper.Map<CrMasRenterInformation>(RenterVM);
        //    var RenterAfterSaved = await _ContractServices.AddRenterToMASRenterInformation(RenterModel, RenterVM.CrMasRenterInformationEmployerName);
        //    if (RenterAfterSaved != null)
        //    {
        //        var RenterPostAfterSaved = await _ContractServices.AddRenterForMASRenterPost(RenterAfterSaved.CrMasRenterInformationId, RenterVM.CrMasRenterInformationCity, RenterVM.CrMasRenterInformationArAddress, RenterVM.CrMasRenterInformationEnAddress);
        //        var CasRenterLessorAfterSaved = await _ContractServices.AddRenterToCasRenterLessor(lessorCode, RenterAfterSaved, RenterPostAfterSaved);
        //        if (await _unitOfWork.CompleteAsync() > 0)
        //        {
        //            var renterInformationsVM = MapRenterInfoToViewModel(RenterAfterSaved, CasRenterLessorAfterSaved, RenterPostAfterSaved);
        //            return Json(renterInformationsVM);
        //        }
        //    }

        //    return Json(null);

        //}
        private async Task<bool> SaveRenterDriverInfoAsync(ContractVM contractInfo)
        {
            var renterSaved = true;
            var driverSaved = true;
            var addDriverSaved = true;

            // First Check if renter is driver
            if (!string.IsNullOrEmpty(contractInfo.RenterInfo?.CrMasRenterInformationId) &&
                !string.IsNullOrEmpty(contractInfo.DriverInfo?.CrMasRenterInformationId) &&
                contractInfo.RenterInfo?.CrMasRenterInformationId == contractInfo.DriverInfo?.CrMasRenterInformationId)
            {
                // Here New Person Registered
                if (!string.IsNullOrEmpty(contractInfo.RenterInfo.CrMasRenterInformationSignature))
                {
                    contractInfo.DriverInfo.CrMasRenterInformationSignature = contractInfo.RenterInfo.CrMasRenterInformationSignature;
                }
                driverSaved = await AddRenterInformation(contractInfo.DriverInfo);
                if (!driverSaved) driverSaved = await UpdateRenterInformation(contractInfo.DriverInfo, PersonalType.Driver);
                if (!driverSaved)
                {
                    _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء حفظ بيانات السائق", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return false;
                }
                if (!string.IsNullOrEmpty(contractInfo.AddDriverInfo?.CrMasRenterInformationId))
                {
                    addDriverSaved = await AddRenterInformation(contractInfo.AddDriverInfo);
                    if (!addDriverSaved) addDriverSaved = await UpdateRenterInformation(contractInfo.AddDriverInfo, PersonalType.AddDriver);
                    if (!addDriverSaved)
                    {
                        _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء حفظ بيانات السائق الاضافي", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return false;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(contractInfo.RenterInfo?.CrMasRenterInformationId))
                {
                    renterSaved = await AddRenterInformation(contractInfo.RenterInfo);
                    if (!renterSaved) renterSaved = await UpdateRenterInformation(contractInfo.RenterInfo, PersonalType.Renter);
                    if (!renterSaved)
                    {
                        _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء حفظ بيانات المستأجر", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(contractInfo.DriverInfo?.CrMasRenterInformationId))
                {
                    driverSaved = await AddRenterInformation(contractInfo.DriverInfo);
                    if (!driverSaved) driverSaved = await UpdateRenterInformation(contractInfo.DriverInfo, PersonalType.Driver);
                    if (!driverSaved)
                    {
                        _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء حفظ بيانات السائق", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(contractInfo.AddDriverInfo?.CrMasRenterInformationId))
                {
                    addDriverSaved = await AddRenterInformation(contractInfo.AddDriverInfo);
                    if (!addDriverSaved) addDriverSaved = await UpdateRenterInformation(contractInfo.AddDriverInfo, PersonalType.AddDriver);
                    if (!addDriverSaved)
                    {
                        _toastNotification.AddErrorToastMessage("حدث خطأ ما اثناء حفظ بيانات السائق الاضافي", new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return false;
                    }
                }
            }

            if (!renterSaved || !driverSaved || !addDriverSaved)
            {
                return false;
            }
            else
            {
                await _unitOfWork.CompleteAsync();
                return true;
            }
        }
        private async Task<bool> AddRenterInformation(RenterInfoVM renterVM)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var renter = _unitOfWork.CrMasRenterInformation.Find(x => x.CrMasRenterInformationId == renterVM.CrMasRenterInformationId);
            string pathSignature = string.Empty;
            if (renter != null) return false;

            string foldername = $"{"images\\Bnan\\Renters"}\\{renterVM.CrMasRenterInformationId}";

            // Handle signature saving
            if (!string.IsNullOrEmpty(renterVM.CrMasRenterInformationSignature))
            {
                pathSignature = await FileExtensions.SaveSigntureImage(_hostingEnvironment, renterVM.CrMasRenterInformationSignature, renterVM.CrMasRenterInformationId, string.Empty, foldername);
                if (!string.IsNullOrEmpty(pathSignature)) renterVM.CrMasRenterInformationSignature = pathSignature;
            }

            // Map and save renter information
            var renterModel = _mapper.Map<CrMasRenterInformation>(renterVM);
            var renterAfterSaved = await _ContractServices.AddRenterToMASRenterInformation(renterModel, renterVM.CrMasRenterInformationEmployerName, renterVM.DayDate, renterVM.MonthDate, renterVM.YearDate);

            if (renterAfterSaved == null)
            {
                await FileExtensions.RemoveFile(_hostingEnvironment, pathSignature);
                return false;
            }

            // Add renter post and update lessor information
            var renterPostAfterSaved = await _ContractServices.AddRenterForMASRenterPost(renterAfterSaved.CrMasRenterInformationId, renterVM.CrMasRenterInformationCity, renterVM.CrMasRenterInformationArAddress, renterVM.CrMasRenterInformationEnAddress);
            var casRenterLessorAfterSaved = await _ContractServices.AddRenterToCasRenterLessor(lessorCode, renterAfterSaved, renterPostAfterSaved);
            if (renterPostAfterSaved != null && casRenterLessorAfterSaved != null) return true;

            // Clean up in case of failure
            await FileExtensions.RemoveFile(_hostingEnvironment, pathSignature);
            return false;
        }
        private async Task<bool> UpdateRenterInformation(RenterInfoVM renterVM, string personalType)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var renter = _unitOfWork.CrMasRenterInformation.Find(x => x.CrMasRenterInformationId == renterVM.CrMasRenterInformationId);

            if (renter == null) return false;

            var renterModel = _mapper.Map<CrMasRenterInformation>(renterVM);
            var renterAfterSaved = await _ContractServices.UpdateRenterToMASRenterInformation(renterModel, renterVM.CrMasRenterInformationEmployerName, personalType);

            if (renterAfterSaved == null) return false;

            var renterPost = _unitOfWork.CrMasRenterPost.Find(x => x.CrMasRenterPostCode == renter.CrMasRenterInformationId);
            var renterPostAfterSaved = renterPost != null
                ? await _ContractServices.UpdateRenterForMASRenterPost(renterAfterSaved.CrMasRenterInformationId, renterVM.CrMasRenterInformationCity, renterVM.CrMasRenterInformationArAddress, renterVM.CrMasRenterInformationEnAddress)
                : await _ContractServices.AddRenterForMASRenterPost(renterAfterSaved.CrMasRenterInformationId, renterVM.CrMasRenterInformationCity, renterVM.CrMasRenterInformationArAddress, renterVM.CrMasRenterInformationEnAddress);
            var casRenterLessorAfterSaved = await _ContractServices.UpdateRenterToCasRenterLessor(lessorCode, renterAfterSaved, renterPostAfterSaved);
            return renterPostAfterSaved != null && casRenterLessorAfterSaved != null;
        }
        private RenterInformationsVM MapRenterInfoToViewModel(CrMasRenterInformation renterInfo, CrCasRenterLessor renterLessorInfo, CrMasRenterPost renterPost)
        {
            var dealingMechanism = _unitOfWork.CrMasSysEvaluation.Find(x =>
                x.CrMasSysEvaluationsCode == renterLessorInfo.CrCasRenterLessorDealingMechanism);
            var City = _unitOfWork.CrMasSupPostCity.Find(x => x.CrMasSupPostCityCode == renterPost.CrMasRenterPostCity);
            return new RenterInformationsVM
            {
                RenterID = renterInfo?.CrMasRenterInformationId,
                RenterIDType = renterInfo?.CrMasRenterInformationIdtype,
                RenterIDTypeNameAr = renterInfo?.CrMasRenterInformationIdtypeNavigation?.CrMasSupRenterIdtypeArName,
                RenterIDTypeNameEn = renterInfo?.CrMasRenterInformationIdtypeNavigation?.CrMasSupRenterIdtypeEnName,
                PersonalArName = renterInfo?.CrMasRenterInformationArName,
                PersonalEnName = renterInfo?.CrMasRenterInformationEnName,
                GenderCode = renterInfo?.CrMasRenterInformationGender,
                PersonalArGender = renterInfo?.CrMasRenterInformationGenderNavigation?.CrMasSupRenterGenderArName,
                PersonalEnGender = renterInfo?.CrMasRenterInformationGenderNavigation?.CrMasSupRenterGenderEnName,
                PersonalArNationality = renterInfo?.CrMasRenterInformationNationalityNavigation?.CrMasSupRenterNationalitiesArName,
                PersonalEnNationality = renterInfo?.CrMasRenterInformationNationalityNavigation?.CrMasSupRenterNationalitiesEnName,
                NationalityCode = renterInfo?.CrMasRenterInformationNationality,
                ProfessionsCode = renterInfo?.CrMasRenterInformationProfession,
                PersonalArProfessions = renterInfo?.CrMasRenterInformationProfessionNavigation?.CrMasSupRenterProfessionsArName,
                PersonalEnProfessions = renterInfo?.CrMasRenterInformationProfessionNavigation?.CrMasSupRenterProfessionsEnName,
                PersonalEmail = renterInfo?.CrMasRenterInformationEmail,
                EmployerCode = renterInfo?.CrMasRenterInformationEmployerNavigation?.CrMasSupRenterEmployerCode,
                EmployerArName = renterInfo?.CrMasRenterInformationEmployerNavigation?.CrMasSupRenterEmployerArName,
                EmployerEnName = renterInfo?.CrMasRenterInformationEmployerNavigation?.CrMasSupRenterEmployerEnName,
                LicenseType = renterInfo?.CrMasRenterInformationDrivingLicenseType,
                LicenseCode = renterInfo?.CrMasRenterInformationDrivingLicenseNo,
                LicenseArName = renterInfo?.CrMasRenterInformationDrivingLicenseTypeNavigation?.CrMasSupRenterDrivingLicenseArName,
                LicenseEnName = renterInfo?.CrMasRenterInformationDrivingLicenseTypeNavigation?.CrMasSupRenterDrivingLicenseEnName,
                LicenseExpiryDate = renterInfo?.CrMasRenterInformationExpiryDrivingLicenseDate,
                LicenseIssuedDate = renterInfo?.CrMasRenterInformationIssueIdDate,
                PostArNameConcenate = renterPost?.CrMasRenterPostArConcatenate,
                PostEnNameConcenate = renterPost?.CrMasRenterPostEnConcatenate,
                PostArDistictName = renterPost?.CrMasRenterPostArDistrict,
                PostEnDistictName = renterPost?.CrMasRenterPostEnDistrict,
                CityCode = City?.CrMasSupPostCityCode,
                CityAr = City?.CrMasSupPostCityConcatenateArName,
                CityEn = City?.CrMasSupPostCityConcatenateEnName,
                MobileNumber = renterInfo?.CrMasRenterInformationMobile,
                BirthDate = renterInfo?.CrMasRenterInformationBirthDate,
                ExpiryIdDate = renterInfo?.CrMasRenterInformationExpiryIdDate,
                KeyCountry = renterInfo?.CrMasRenterInformationCountreyKey,
                Balance = renterLessorInfo?.CrCasRenterLessorBalance,
                AvailableBalance = renterLessorInfo?.CrCasRenterLessorAvailableBalance,
                ReservedBalance = renterLessorInfo?.CrCasRenterLessorReservedBalance,
                LastContract = renterLessorInfo?.CrCasRenterLessorDateLastContractual,
                FirstVisit = renterLessorInfo?.CrCasRenterLessorDateFirstInteraction,
                ContractCount = renterLessorInfo?.CrCasRenterLessorContractCount,
                RentalDays = renterLessorInfo?.CrCasRenterLessorContractDays,
                AmountsTraded = renterLessorInfo?.CrCasRenterLessorContractTradedAmount,
                KMCut = renterLessorInfo?.CrCasRenterLessorContractKm,
                ArDealingMechanism = dealingMechanism?.CrMasSysEvaluationsArDescription,
                EnDealingMechanism = dealingMechanism?.CrMasSysEvaluationsEnDescription,
                ArMembership = renterLessorInfo?.CrCasRenterLessorMembershipNavigation?.CrMasSupRenterMembershipArName ?? "مشترك",
                EnMembership = renterLessorInfo?.CrCasRenterLessorMembershipNavigation?.CrMasSupRenterMembershipEnName ?? "Subscriber",
                CountContracts = renterLessorInfo?.CrCasRenterContractBasicCrCasRenterContractBasic5s.Count(),
                ActiveContractsCount = renterLessorInfo?.CrCasRenterContractBasicCrCasRenterContractBasic5s.Count(x => x.CrCasRenterContractBasicStatus != Status.Closed),
                ClosedContractsCount = renterLessorInfo?.CrCasRenterContractBasicCrCasRenterContractBasic5s.Count(x => x.CrCasRenterContractBasicStatus == Status.Closed),
                Signture = renterInfo?.CrMasRenterInformationSignature,
                Reasons = renterInfo?.CrMasRenterInformationReasons
            };
        }
        [HttpGet]
        public async Task<IActionResult> CheckAuthUser(bool id, bool address, bool employeer)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var userContractValidity = _unitOfWork.CrMasUserContractValidity.Find(x => x.CrMasUserContractValidityUserId == userLogin.Id);
            var check = "true";
            if (id == false && userContractValidity.CrMasUserContractValidityId == false)
            {
                check = "id";
            }
            else if (address == false && userContractValidity.CrMasUserContractValidityRenterAddress == false)
            {
                check = "address";
            }
            else if (employeer == false && userContractValidity.CrMasUserContractValidityEmployer == false)
            {
                check = "employeer";
            }
            return Json(check);

        }

        [HttpGet]
        public async Task<IActionResult> CheckAuthDriver(bool id, bool address, bool license, bool age, bool employer)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var userContractValidity = _unitOfWork.CrMasUserContractValidity.Find(x => x.CrMasUserContractValidityUserId == userLogin.Id);
            var check = "true";
            if (id == false && userContractValidity.CrMasUserContractValidityId == false)
            {
                check = "id";
                return Json(check);
            }
            else if (address == false && userContractValidity.CrMasUserContractValidityRenterAddress == false)
            {
                check = "address";
                return Json(check);
            }
            else if (license == false && userContractValidity.CrMasUserContractValidityDrivingLicense == false)
            {
                check = "license";
                return Json(check);
            }
            else if (age == false && userContractValidity.CrMasUserContractValidityAge == false)
            {
                check = "age";
                return Json(check);
            }
            else if (employer == false && userContractValidity.CrMasUserContractValidityEmployer == false)
            {
                check = "employer";
                return Json(check);
            }
            else { return Json(check); }
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarsByCategory(string selectedCategory, string selectedBranch)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationBranch == selectedBranch && x.CrCasCarInformationStatus == Status.Active &&
                                                                                 x.CrCasCarInformationPriceStatus == true && x.CrCasCarInformationOwnerStatus == Status.Active &&
                                                                                (x.CrCasCarInformationForSaleStatus == Status.Active || x.CrCasCarInformationForSaleStatus == Status.RendAndForSale),
                                                                                new[] { "CrCasCarInformationDistributionNavigation", "CrCasCarInformationCategoryNavigation", "CrCasCarInformationDistributionNavigation.CrCasPriceCarBasics",
                                                                                   "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation" ,"CrCasCarInformationFuelNavigation","CrCasCarInformationCvtNavigation"}).ToList();
            if (selectedCategory == "3400000000")
            {
                BSLayoutVM bSLayoutVM = new BSLayoutVM()
                {
                    CarsFilter = cars
                };
                return PartialView("_CarsList", bSLayoutVM);
            }
            BSLayoutVM bSLayout = new BSLayoutVM()
            {
                CarsFilter = cars.Where(x => x.CrCasCarInformationCategory == selectedCategory).ToList(),
            };
            return PartialView("_CarsList", bSLayout);


        }

        [HttpGet]
        public async Task<IActionResult> GetCarInformation(string serialNumber)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            BSCarsInformationVM carVM = new BSCarsInformationVM();
            var carInfo = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationSerailNo == serialNumber);

            carVM.CarInformation = carInfo;
            var carPrice = _unitOfWork.CrCasPriceCarBasic.Find(x => x.CrCasPriceCarBasicNo == carInfo.CrCasCarInformationPriceNo);
            if (carPrice != null)
            {
                carVM.CarPrice = carPrice;
                return Json(carVM);
            }
            return Json(null);

        }
        [HttpGet]
        public async Task<IActionResult> GetCarInfoForContract(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
                return BadRequest("Serial number is required.");

            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin?.CrMasUserInformationLessor;

            if (lessorCode == null)
                return Unauthorized();

            var carInfo = await _unitOfWork.CrCasCarInformation.FindAsync(x =>
                x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationSerailNo == serialNumber);

            if (carInfo == null)
                return NotFound("Car information not found.");

            var carVM = _mapper.Map<CarInfomationVM>(carInfo);

            // الحصول على جميع البيانات المطلوبة دفعة واحدة
            var fuel =await _unitOfWork.CrMasSupCarFuel.FindAsync(x => x.CrMasSupCarFuelCode == carInfo.CrCasCarInformationFuel);
            var cvt = await _unitOfWork.CrMasSupCarCvt.FindAsync(x => x.CrMasSupCarCvtCode == carInfo.CrCasCarInformationCvt);
            var oil = await _unitOfWork.CrMasSupCarOil.FindAsync(x => x.CrMasSupCarOilCode == carInfo.CrCasCarInformationOil);
            var registration =await _unitOfWork.CrMasSupCarRegistration.FindAsync(x => x.CrMasSupCarRegistrationCode == carInfo.CrCasCarInformationRegistration);

            // استعلامات الصيانة دفعة واحدة
            var maintenances = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllAsync(x =>
                x.CrCasCarDocumentsMaintenanceSerailNo == serialNumber &&
                (x.CrCasCarDocumentsMaintenanceProceduresClassification == "12" || x.CrCasCarDocumentsMaintenanceProceduresClassification == "13"));

            var maintenanceDocs = maintenances.ToList();

            // تعبئة البيانات
            carVM.FuelAr = fuel?.CrMasSupCarFuelArName;
            carVM.FuelEn = fuel?.CrMasSupCarFuelEnName;

            carVM.CVTAr = cvt?.CrMasSupCarCvtArName;
            carVM.CVTEn = cvt?.CrMasSupCarCvtEnName;

            carVM.OilAr = oil?.CrMasSupCarOilArName;
            carVM.OilEn = oil?.CrMasSupCarOilEnName;

            carVM.RegisterationAr = registration?.CrMasSupCarRegistrationArName;
            carVM.RegisterationEn = registration?.CrMasSupCarRegistrationEnName;

            // استخراج التواريخ من الصيانة
            carVM.ChangeOilDate = GetMaintenanceRecord(maintenanceDocs.ToList(), "13", "131").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd",CultureInfo.InvariantCulture);
            carVM.EndDrivinglicence = GetMaintenanceRecord(maintenanceDocs, "12", "120").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            carVM.EndPeriodicInspection = GetMaintenanceRecord(maintenanceDocs, "12", "123").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            carVM.TiresDate = GetMaintenanceRecord(maintenanceDocs, "13", "130").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            carVM.PeriodicMaintenanceDate = GetMaintenanceRecord(maintenanceDocs, "13", "132").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            carVM.FrontBrakeDate = GetMaintenanceRecord(maintenanceDocs, "13", "133").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            carVM.RearBrakeDate = GetMaintenanceRecord(maintenanceDocs, "13", "134").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            carVM.ChangeOilKm = GetMaintenanceRecord(maintenanceDocs, "13", "131").CrCasCarDocumentsMaintenanceKmEndsAt?.ToString("N0", CultureInfo.InvariantCulture);
            carVM.TiresKm = GetMaintenanceRecord(maintenanceDocs, "13", "130").CrCasCarDocumentsMaintenanceKmEndsAt?.ToString("N0", CultureInfo.InvariantCulture);
            carVM.PeriodicMaintenanceKm = GetMaintenanceRecord(maintenanceDocs, "13", "132").CrCasCarDocumentsMaintenanceKmEndsAt?.ToString("N0", CultureInfo.InvariantCulture);
            carVM.FrontBrakeKm = GetMaintenanceRecord(maintenanceDocs, "13", "133").CrCasCarDocumentsMaintenanceKmEndsAt?.ToString("N0", CultureInfo.InvariantCulture);
            carVM.RearBrakeKm = GetMaintenanceRecord(maintenanceDocs, "13", "134").CrCasCarDocumentsMaintenanceKmEndsAt?.ToString("N0", CultureInfo.InvariantCulture);

            // استخراج بيانات الوثائق
            carVM.PeriodicInspectionNo = GetMaintenanceRecord(maintenanceDocs, "12", "123").CrCasCarDocumentsMaintenanceNo;
            carVM.RunningCardNo = GetMaintenanceRecord(maintenanceDocs, "12", "122").CrCasCarDocumentsMaintenanceNo;
            carVM.RunningCardEndDate = GetMaintenanceRecord(maintenanceDocs, "12", "122").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            carVM.InsurancePolicyNo = GetMaintenanceRecord(maintenanceDocs, "12", "121").CrCasCarDocumentsMaintenanceNo;
            carVM.InsurancePolicyEndDate = GetMaintenanceRecord(maintenanceDocs, "12", "121").CrCasCarDocumentsMaintenanceEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

            return Json(carVM);
        }

        // دالة مساعدة لاستخراج التواريخ
        private CrCasCarDocumentsMaintenance GetMaintenanceRecord(List<CrCasCarDocumentsMaintenance> docs, string classification, string procedure)
        {
            return docs.FirstOrDefault(x => x.CrCasCarDocumentsMaintenanceProceduresClassification == classification &&
                                            x.CrCasCarDocumentsMaintenanceProcedures == procedure);
        }
        [HttpGet]
        public async Task<IActionResult> GetAdvantagesValue(string priceNumber)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            BSCarsInformationVM carVM = new BSCarsInformationVM();
            var AdvantagesTotalValue = _unitOfWork.CrCasPriceCarAdvantage.FindAll(x => x.CrCasPriceCarAdvantagesNo == priceNumber).Select(x => x.CrCasPriceCarAdvantagesValue).Sum();
            if (AdvantagesTotalValue != null) return Json(AdvantagesTotalValue);
            return Json(null);
        }
        [HttpGet]
        public async Task<IActionResult> GetAdvantages(string priceNumber)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            List<AdvantagesVM> AdvantagesList = new List<AdvantagesVM>();

            var Advantages = _unitOfWork.CrCasPriceCarAdvantage.FindAll(x => x.CrCasPriceCarAdvantagesNo == priceNumber);


            foreach (var adv in Advantages)
            {
                var supCarAdvantages = _unitOfWork.CrMasSupCarAdvantage.Find(x => x.CrMasSupCarAdvantagesCode == adv.CrCasPriceCarAdvantagesCode);

                AdvantagesVM advantagesVM = new AdvantagesVM
                {
                    AdvantagesNo = adv.CrCasPriceCarAdvantagesNo,
                    AdvantagesCode = adv.CrCasPriceCarAdvantagesCode,
                    AdvantagesValue = adv.CrCasPriceCarAdvantagesValue,
                    ArName = supCarAdvantages.CrMasSupCarAdvantagesArName,
                    EnName = supCarAdvantages.CrMasSupCarAdvantagesEnName
                };

                AdvantagesList.Add(advantagesVM);
            }
            return Json(AdvantagesList);
        }

        [HttpGet]
        public async Task<IActionResult> GetCarChoices(string priceNumber)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;

            List<OptionsVM> optionsList = new List<OptionsVM>();

            var carOptions = _unitOfWork.CrCasPriceCarOption.FindAll(x => x.CrCasPriceCarOptionsNo == priceNumber);

            foreach (var option in carOptions)
            {
                var supCarOptions = _unitOfWork.CrMasSupContractOption.Find(x => x.CrMasSupContractOptionsCode == option.CrCasPriceCarOptionsCode);

                OptionsVM optionsVM = new OptionsVM
                {
                    OptionsNo = option.CrCasPriceCarOptionsNo,
                    OptionsCode = option.CrCasPriceCarOptionsCode,
                    OptionsValue = option.CrCasPriceCarOptionsValue,
                    ArName = supCarOptions.CrMasSupContractOptionsArName,
                    EnName = supCarOptions.CrMasSupContractOptionsEnName
                };

                optionsList.Add(optionsVM);
            }
            var result = new
            {
                OptionsList = optionsList,
                Count = carOptions.Count(),
                Total = optionsList.Select(x => x.OptionsValue).Sum()
            };

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetCarAdditional(string priceNumber)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            List<AdditionalVM> additionalList = new List<AdditionalVM>();

            var carAdditionals = _unitOfWork.CrCasPriceCarAdditional.FindAll(x => x.CrCasPriceCarAdditionalNo == priceNumber);

            foreach (var additional in carAdditionals)
            {
                var supCarAdditional = _unitOfWork.CrMasSupContractAdditional.Find(x => x.CrMasSupContractAdditionalCode == additional.CrCasPriceCarAdditionalCode);

                AdditionalVM additionalVM = new AdditionalVM
                {
                    AddNo = additional.CrCasPriceCarAdditionalNo,
                    AddCode = additional.CrCasPriceCarAdditionalCode,
                    AddValue = additional.CrCasPriceCarAdditionalValue,
                    ArName = supCarAdditional.CrMasSupContractAdditionalArName,
                    EnName = supCarAdditional.CrMasSupContractAdditionalEnName
                };
                additionalList.Add(additionalVM);

            }
            var result = new
            {
                addList = additionalList,
                Count = carAdditionals.Count(),
                Total = additionalList.Select(x => x.AddValue).Sum()
            };
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCarCheckUp()
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            List<CheckUpVM> checkUpList = new List<CheckUpVM>();
            var CheckUps = _unitOfWork.CrMasSupContractCarCheckup.FindAll(x => x.CrMasSupContractCarCheckupStatus == Status.Active).ToList();
            foreach (var carCheckup in CheckUps)
            {

                CheckUpVM carCheckupVM = new CheckUpVM
                {
                    Code = carCheckup.CrMasSupContractCarCheckupCode,
                    ArName = carCheckup.CrMasSupContractCarCheckupArName,
                    EnName = carCheckup.CrMasSupContractCarCheckupEnName
                };
                checkUpList.Add(carCheckupVM);
            }
            return Json(checkUpList);
        }
        [HttpPost]
        public async Task<JsonResult> UpdateSigntureForRenter(string img, string renterId)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var Renter = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == renterId);
            string foldername = $"{"images\\Bnan\\Renters"}\\{Renter.CrMasRenterInformationId}";
            if (Renter != null && !string.IsNullOrEmpty(img))
            {
                var path = await FileExtensions.SaveSigntureImage(_hostingEnvironment, img, Renter.CrMasRenterInformationId, Renter.CrMasRenterInformationSignature, foldername);
                if (!string.IsNullOrEmpty(path) && await _ContractServices.UpdateRenterSignture(Renter.CrMasRenterInformationId, path) && await _unitOfWork.CompleteAsync() > 0) return Json(path);
            }
            return Json(null);
        }
        private CrCasRenterContractBasic GetContractLastRecord(string Sector, string LessorCode, string BranchCode)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var SectorCode = Sector;
            var Lrecord = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicLessor == LessorCode &&
                x.CrCasRenterContractBasicSector == SectorCode
                && x.CrCasRenterContractBasicYear == y && x.CrCasRenterContractBasicBranch == BranchCode)
                .Max(x => x.CrCasRenterContractBasicNo.Substring(x.CrCasRenterContractBasicNo.Length - 6, 6));

            CrCasRenterContractBasic c = new CrCasRenterContractBasic();
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                c.CrCasRenterContractBasicNo = val.ToString("000000");
            }
            else
            {
                c.CrCasRenterContractBasicNo = "000001";
            }

            return c;
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
        private string GetSector(string firstChar, string sectorType)
        {
            var sectorCode = "";
            var sector = _unitOfWork.CrMasSupRenterSector.Find(x => x.CrMasSupRenterSectorArName.Trim() == sectorType.Trim());
            if (sector != null) sectorCode = sector.CrMasSupRenterSectorCode;
            else
            {
                if (firstChar == "7") sectorCode = "0";
                else sectorCode = "1";
            }
            return sectorCode;
        }
        public IActionResult FailedToast(string type)
        {
            var message = GetTypeMessage(type);
            _toastNotification.AddErrorToastMessage(message, new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home", new { area = "BS" });
        }
        private string GetTypeMessage(string type)
        {
            var message = "";
            if (type == "arreceipt") message = "هناك خطأ في انشاء السند عربي يرجى التواصل مع الادارة";
            else if (type == "enreceipt") message = "هناك خطأ في انشاء السند انجليزي يرجى التواصل مع الادارة";
            else if (type == "arinvoice") message = "هناك خطأ في انشاء الفاتورة عربي يرجى التواصل مع الادارة";
            else if (type == "eninvoice") message = "هناك خطأ في انشاء الفاتورة انجليزي يرجى التواصل مع الادارة";
            else if (type == "arcontract") message = "هناك خطأ في انشاء العقد عربي يرجى التواصل مع الادارة";
            else if (type == "encontract") message = "هناك خطأ في انشاء العقد انجليزي يرجى التواصل مع الادارة";
            else if (type == "createpdf") message = "هناك خطأ في ارسال الملفات يرجى التواصل مع الادارة";
            return message;
        }
    }
}
