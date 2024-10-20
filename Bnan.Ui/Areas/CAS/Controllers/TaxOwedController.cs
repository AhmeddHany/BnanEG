﻿using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Linq;

namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class TaxOwedController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IFinancialTransactionOfRenter _FinancialTransactionOfRenter;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<TaxOwedController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;


        public TaxOwedController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IAdminstritiveProcedures adminstritiveProcedures,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter FinancialTransactionOfRenter,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<TaxOwedController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _FinancialTransactionOfRenter = FinancialTransactionOfRenter;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _adminstritiveProcedures = adminstritiveProcedures;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllTaxOwed = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == true).OrderByDescending(x => x.CrCasAccountContractTaxOwedPayDate).ToList();
            var AllTaxOwed_Filtered_but = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == true, new[] { "CrCasAccountContractTaxOwedUserCodeNavigation", "CrCasAccountContractTaxOwedPayCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractTaxOwedPayCode).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();

            //if (AllTaxOwed?.Count() < 1)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}

            var AllTaxOwed_add = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == false);

            if (AllTaxOwed_add?.Count() < 1)
            {
                ViewBag.Data = "0";
            }
            else
            {
                ViewBag.Data = "1";
            }


            var MaxDate = AllTaxOwed.Max(x => x.CrCasAccountContractTaxOwedPayCodeNavigation?.CrCasSysAdministrativeProceduresDate);
            var minDate = MaxDate?.AddDays(-30);
            ViewBag.StartDate = minDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = MaxDate?.ToString("yyyy-MM-dd");

            var AllTaxOwed_Filtered = AllTaxOwed_Filtered_but.Where(x => x.CrCasAccountContractTaxOwedPayDate < MaxDate?.AddDays(1)
                && x.CrCasAccountContractTaxOwedPayDate >= minDate).ToList();

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            TaxOwed_VM taxOwed_VM = new TaxOwed_VM();
            taxOwed_VM.CrCasAccountContractTaxOwed_Filtered = AllTaxOwed_Filtered;
            taxOwed_VM.CrCasAccountContractTaxOwed = AllTaxOwed;

            return View(taxOwed_VM);
        }

        [HttpGet]
        public async Task<IActionResult> AddPaymentTaxValues()
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllTaxOwed = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == false).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();
            //var AllTaxOwed_Filtered = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == true, new[] { "CrCasAccountContractTaxOwedUserCodeNavigation", "CrCasAccountContractTaxOwedPayCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractTaxOwedPayCode).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();

            //if (AllTaxOwed?.Count() < 1)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}

            var firstYear = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == false).Min(x => x.CrCasAccountContractTaxOwedDate);
            var lastYear = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == false).Max(x => x.CrCasAccountContractTaxOwedDate);
            ViewBag.firstYear = Int16.Parse(firstYear?.ToString("yyyy", CultureInfo.InvariantCulture) ?? "0");
            ViewBag.lastYear = Int16.Parse(lastYear?.ToString("yyyy", CultureInfo.InvariantCulture) ?? "0");

            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == currentUser.CrMasUserInformationLessor &&
                x.CrCasSysAdministrativeProceduresCode == "307"
                && x.CrCasSysAdministrativeProceduresClassification == "30"
                && x.CrCasSysAdministrativeProceduresSector == "1"
                && x.CrCasSysAdministrativeProceduresYear == y).Max(x => x.CrCasSysAdministrativeProceduresNo.Substring(x.CrCasSysAdministrativeProceduresNo.Length - 6, 6));
            string Serial;
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                Serial = val.ToString("000000");
            }
            else
            {
                Serial = "000001";
            }
            var NewProcedureNo = y + "-" + "1" + "307" + "-" + currentUser.CrMasUserInformationLessor + "100" + "-" + Serial;


            TaxOwed_VM taxOwed_VM = new TaxOwed_VM();
            //taxOwed_VM.CrCasAccountContractTaxOwed_Filtered = AllTaxOwed_Filtered;
            taxOwed_VM.CrCasAccountContractTaxOwed = AllTaxOwed;
            //taxOwed_VM.New_TaxOwed_Tax_no = "0000-0000";
            taxOwed_VM.New_TaxOwed_Tax_no = NewProcedureNo;
            return View(taxOwed_VM);
        }

        [HttpPost]
        public async Task<IActionResult> AddPaymentTaxValues(List<string> values, string year, string quarter, string pay_date, string reasons, string Total_Money, string Serial_pay)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);


            if (!string.IsNullOrEmpty(quarter) && !string.IsNullOrEmpty(year) && quarter.Length > 0 && Total_Money != "0٫0" && Total_Money != "0.00")
            {
                var quarter_converted = Int16.Parse(quarter) * 3 + 1;
                var start = year + "/" + quarter_converted.ToString() + "/" + "01";
                var end = year + "/" + quarter_converted.ToString() + "/" + "01";
                var startDate = DateTime.Parse(start).AddMonths(-3).Date;
                var endDate = DateTime.Parse(end).Date;

                var AllTaxOwed_IsPaid_Before = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor
                && x.CrCasAccountContractTaxOwedIsPaid == true
                && x.CrCasAccountContractTaxOwedDate < endDate
               && x.CrCasAccountContractTaxOwedDate >= startDate).FirstOrDefault();

                if (AllTaxOwed_IsPaid_Before != null)
                {
                    //return RedirectToAction("FailedToast");
                    return Json(new { code = 2 });
                }

                var AllTaxOwed_Filtered = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor
               && x.CrCasAccountContractTaxOwedIsPaid == false).DistinctBy(x => x.CrCasAccountContractTaxOwedContractNo)
               .Where(x => x.CrCasAccountContractTaxOwedDate < endDate
               && x.CrCasAccountContractTaxOwedDate >= startDate).ToList();
                decimal Total_Money_Decimal = decimal.Parse(Total_Money, CultureInfo.InvariantCulture);
                //////////////////
                // Save Adminstrive Procedures
                await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "307", "30", currentUser.CrMasUserInformationLessor, "100",
                    Serial_pay, Total_Money_Decimal, Total_Money_Decimal, null, null, null, null, null, null, "تسديد مستحقات القيمة المضافة", "Payment Dues of Tax values", "I", reasons);
                //_toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                //return RedirectToAction("SalesPoints", "SalesPoints");

                foreach (var item in AllTaxOwed_Filtered)
                {
                    //var exist = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == false && x.CrCasAccountContractTaxOwedContractNo == item..Trim()).FirstOrDefault();
                    var exist = item;
                    if (exist != null)
                    {
                        exist.CrCasAccountContractTaxOwedIsPaid = true;
                        exist.CrCasAccountContractTaxOwedPayDate = DateTime.Parse(pay_date).Date;
                        exist.CrCasAccountContractTaxOwedPayCode = Serial_pay;
                        exist.CrCasAccountContractTaxOwedUserCode = currentUser.CrMasUserInformationCode;

                        _unitOfWork.CrCasAccountContractTaxOwed.Update(exist);
                    }
                }


                //////////////////

                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تم التسديد", "Payment Done", mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                //var result = new
                //{
                //    accountNo = account.CrCasAccountBankCode,
                //    accountIban = account.CrCasAccountBankIban,
                //    bankNo = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankCode,
                //    arBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankArName,
                //    enBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankEnName,
                //};
                //return Json(result);
                //RedirectToAction("SuccessToast", "TaxOwed");
                //return Json(null);
                return Json(new { code = 1 });
                //return View();

            }

            //return RedirectToAction("FailedToast");

            return Json(new { code = 0 });
        }


        [HttpGet]
        public async Task<IActionResult> EditPaymentTaxValues(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            //var AllTaxOwed = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == true).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();
            var AllTaxOwed_Filtered = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == true, new[] { "CrCasAccountContractTaxOwedUserCodeNavigation", "CrCasAccountContractTaxOwedPayCodeNavigation" }).Where(x => x.CrCasAccountContractTaxOwedPayCode == id).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();

            //if (AllTaxOwed_Filtered?.Count() < 1)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}

            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var ProcedureData = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == currentUser.CrMasUserInformationLessor &&
                x.CrCasSysAdministrativeProceduresCode == "307"
                && x.CrCasSysAdministrativeProceduresNo == id).FirstOrDefault();

            var NewProcedureNo = ProcedureData?.CrCasSysAdministrativeProceduresNo;
            var Invoices_Count = AllTaxOwed_Filtered?.Count();
            var Quarter_number = AllTaxOwed_Filtered?.FirstOrDefault()?.CrCasAccountContractTaxOwedDate?.ToString("MM", CultureInfo.InvariantCulture);
            var Quarter = "";
            if (Int16.Parse(Quarter_number ?? "0") < 4)
            {
                Quarter = _localizer["quarter_int1"];
            }
            else if (Int16.Parse(Quarter_number ?? "0") < 7)
            {
                Quarter = _localizer["quarter_int2"];
            }
            else if (Int16.Parse(Quarter_number ?? "0") < 10)
            {
                Quarter = _localizer["quarter_int3"];
            }
            else
            {
                Quarter = _localizer["quarter_int4"];
            }

            var AllInvoices = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == currentUser.CrMasUserInformationLessor && x.CrCasAccountInvoiceType == "309").ToList();

            TaxOwed_VM taxOwed_VM = new TaxOwed_VM();
            //taxOwed_VM.CrCasAccountContractTaxOwed_Filtered = AllTaxOwed_Filtered;
            taxOwed_VM.CrCasAccountContractTaxOwed = AllTaxOwed_Filtered;
            taxOwed_VM.CrCasSysAdministrativeProcedure_Data = ProcedureData;
            //taxOwed_VM.New_TaxOwed_Tax_no = "0000-0000";
            taxOwed_VM.New_TaxOwed_Tax_no = NewProcedureNo;
            taxOwed_VM.Quarter = Quarter;
            taxOwed_VM.Invoices_Count = Invoices_Count;
            taxOwed_VM.CrCasAccountInvoice = AllInvoices;

            return View(taxOwed_VM);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllContracts(string id, string reasons)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);


            var ProcedureData = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == currentUser.CrMasUserInformationLessor &&
    x.CrCasSysAdministrativeProceduresCode == "307"
    && x.CrCasSysAdministrativeProceduresNo == id).FirstOrDefault();

            if (ProcedureData != null)
            {
                ProcedureData.CrCasSysAdministrativeProceduresArDescription = "حذف مستحقات القيمة المضافة";
                ProcedureData.CrCasSysAdministrativeProceduresEnDescription = "Delete Dues of Tax values";
                ProcedureData.CrCasSysAdministrativeProceduresStatus = "D";
                ProcedureData.CrCasSysAdministrativeProceduresReasons = reasons;

                _unitOfWork.CrCasSysAdministrativeProcedure.Update(ProcedureData);
            }



            var AllTaxOwed_Filtered = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == true, new[] { "CrCasAccountContractTaxOwedUserCodeNavigation", "CrCasAccountContractTaxOwedPayCodeNavigation" }).Where(x => x.CrCasAccountContractTaxOwedPayCode == id).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();

            foreach (var item in AllTaxOwed_Filtered)
            {
                if (item != null)
                {
                    item.CrCasAccountContractTaxOwedIsPaid = false;
                    item.CrCasAccountContractTaxOwedPayDate = null;
                    item.CrCasAccountContractTaxOwedPayCode = null;
                    item.CrCasAccountContractTaxOwedUserCode = null;

                    _unitOfWork.CrCasAccountContractTaxOwed.Update(item);
                }
            }


            //////////////////

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تم الغاء التسديد", "Payment Removed", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            //var result = new
            //{
            //    accountNo = account.CrCasAccountBankCode,
            //    accountIban = account.CrCasAccountBankIban,
            //    bankNo = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankCode,
            //    arBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankArName,
            //    enBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankEnName,
            //};
            //return Json(result);
            //RedirectToAction("SuccessToast", "TaxOwed");
            //return Json(null);
            return Json(new { code = 1 });
            //return View();

        }


        [HttpGet]
        public async Task<IActionResult> GetAllContractsForAddwithQuarterAsync(string year, string quarter)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");

            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            if (!string.IsNullOrEmpty(quarter) && !string.IsNullOrEmpty(year) && quarter.Length > 0)
            {
                var quarter_converted = Int16.Parse(quarter) * 3;
                var start = year + "/" + quarter_converted.ToString() + "/" + "01";
                var end = year + "/" + quarter_converted.ToString() + "/" + "01";
                var startDate = DateTime.Parse(start).AddMonths(-2).Date;
                var endDate = DateTime.Parse(end).AddMonths(1).Date;
                decimal allInvoices_Value_new = 0.0m;
                decimal percentage_OfTax = 0.0m;
                decimal total_TaxValue = 0.0m;
                int invoices_Count = 0;
                var AllTaxOwed = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == false).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();
                var AllTaxOwed_Filtered = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor
                && x.CrCasAccountContractTaxOwedIsPaid == false, new[] { "CrCasAccountContractTaxOwedUserCodeNavigation", "CrCasAccountContractTaxOwedPayCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractTaxOwedContractNo)
                .Where(x => x.CrCasAccountContractTaxOwedDate < endDate
                && x.CrCasAccountContractTaxOwedDate >= startDate).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();

                allInvoices_Value_new = AllTaxOwed_Filtered?.Sum(x => x.CrCasAccountContractTaxOwedContractValue) ?? 0.0m;
                //percentage_OfTax = AllTaxOwed_Filtered?.Average(x => x.CrCasAccountContractTaxOwedPercentage) ?? 0.0m;
                percentage_OfTax = AllTaxOwed_Filtered?.FirstOrDefault()?.CrCasAccountContractTaxOwedPercentage ?? 0.0m;
                total_TaxValue = AllTaxOwed_Filtered?.Sum(x => x.CrCasAccountContractTaxOwedValue) ?? 0.0m;
                invoices_Count = AllTaxOwed_Filtered.Count();

                TaxOwed_VM taxOwed_VM = new TaxOwed_VM();
                taxOwed_VM.CrCasAccountContractTaxOwed_Filtered = AllTaxOwed_Filtered;
                taxOwed_VM.CrCasAccountContractTaxOwed = AllTaxOwed;
                taxOwed_VM.Add_AllInvoices_Value_new = allInvoices_Value_new;
                taxOwed_VM.Add_Percentage_OfTax = percentage_OfTax;
                taxOwed_VM.Total_TaxValue = total_TaxValue;
                taxOwed_VM.invoices_Count = invoices_Count;
                var result = new
                {
                    code = 1,
                    allInvoices_Value_new = allInvoices_Value_new.ToString("N2", CultureInfo.InvariantCulture),
                    percentage_OfTax = percentage_OfTax.ToString("N2", CultureInfo.InvariantCulture),
                    total_TaxValue = total_TaxValue.ToString("N2", CultureInfo.InvariantCulture),
                    invoices_Count = invoices_Count,
                };
                return Json(result);
                //return PartialView("_DataTableBasic", taxOwed_VM);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<PartialViewResult> GetAllContractsByDate_statusAsync(string _max, string _mini)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            if (!string.IsNullOrEmpty(_max) && !string.IsNullOrEmpty(_mini) && _max.Length > 0)
            {
                _max = DateTime.Parse(_max).Date.AddDays(1).ToString("yyyy-MM-dd");


                var AllTaxOwed = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor && x.CrCasAccountContractTaxOwedIsPaid == true).OrderByDescending(x => x.CrCasAccountContractTaxOwedPayDate).ToList();
                var AllTaxOwed_Filtered = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor
                && x.CrCasAccountContractTaxOwedIsPaid == true, new[] { "CrCasAccountContractTaxOwedUserCodeNavigation", "CrCasAccountContractTaxOwedPayCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractTaxOwedPayCode)
                .Where(x => x.CrCasAccountContractTaxOwedPayDate < DateTime.Parse(_max).Date
                && x.CrCasAccountContractTaxOwedPayDate >= DateTime.Parse(_mini).Date).OrderByDescending(x => x.CrCasAccountContractTaxOwedPayDate).ToList();

                //var AllTaxOwed = _unitOfWork.CrCasAccountContractTaxOwed.FindAll(x => x.CrCasAccountContractTaxOwedDate < DateTime.Parse(_max).Date && x.CrCasAccountContractTaxOwedDate >= DateTime.Parse(_mini).Date && currentUser.CrMasUserInformationLessor == x.CrCasAccountContractTaxOwedLessor).Where(x.CrCasAccountContractTaxOwedDate < DateTime.Parse(_max).Date && x.CrCasAccountContractTaxOwedDate >= DateTime.Parse(_mini).Date).OrderByDescending(x => x.CrCasAccountContractTaxOwedDate).ToList();

                TaxOwed_VM taxOwed_VM = new TaxOwed_VM();
                taxOwed_VM.CrCasAccountContractTaxOwed_Filtered = AllTaxOwed_Filtered;
                taxOwed_VM.CrCasAccountContractTaxOwed = AllTaxOwed;

                return PartialView("_DataTableBasic", taxOwed_VM);
            }
            return PartialView();
        }




        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            ViewBag.Data = "0";
            var titles = await setTitle("205", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            return View();

        }

        public IActionResult SuccessToast()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "TaxOwed");
        }

        public IActionResult No_Fatoura_Data_FailedToast()
        {
            _toastNotification.AddAlertToastMessage(_localizer["No_Fatoura_Data_FailedToast"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("AddPaymentTaxValues", "TaxOwed");
        }

        public IActionResult Fatoura_Quarter_IsPaidBefore_FailedToast()
        {
            _toastNotification.AddAlertToastMessage(_localizer["Fatoura_Quarter_IsPaidBefore_FailedToast"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("AddPaymentTaxValues", "TaxOwed");
        }

    }
}