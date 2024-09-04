using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Linq;

namespace Bnan.Ui.Areas.MAS.Controllers
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class CompanyOwedController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IFinancialTransactionOfRenter _FinancialTransactionOfRenter;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CompanyOwedController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;


        public CompanyOwedController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IAdminstritiveProcedures adminstritiveProcedures,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter FinancialTransactionOfRenter,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CompanyOwedController> localizer) : base(userManager, unitOfWork, mapper)
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
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "4";
            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101005", "1");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("101", "1101005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x =>  x.CrCasAccountContractCompanyOwedAccrualStatus == true).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDatePayment).ToList();
            var AllCompanyOwed_Filtered_but = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x =>  x.CrCasAccountContractCompanyOwedAccrualStatus == true, new[] { "CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation", "CrCasAccountContractCompanyOwedCompanyCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNo).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

            //if (AllCompanyOwed?.Count() < 1)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}
            var MaxDate = AllCompanyOwed.Max(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation?.CrCasSysAdministrativeProceduresDate);
            var minDate = MaxDate?.AddDays(-30);
            ViewBag.StartDate = minDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = MaxDate?.ToString("yyyy-MM-dd");

            var AllCompanyOwed_Filtered = AllCompanyOwed_Filtered_but.Where(x => x.CrCasAccountContractCompanyOwedDatePayment < MaxDate?.AddDays(1)
                && x.CrCasAccountContractCompanyOwedDatePayment >= minDate).ToList();

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            CompanyOwed_VM taxOwed_VM = new CompanyOwed_VM();
            taxOwed_VM.CrCasAccountContractCompanyOwed_Filtered = AllCompanyOwed_Filtered;
            taxOwed_VM.CrCasAccountContractCompanyOwed = AllCompanyOwed;
            return View(taxOwed_VM);
        }


        [HttpGet]
        public async Task<IActionResult> AddPaymentTaxValues()
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "4";
            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101005", "1");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("101", "1101005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllCompanies = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode != "0000").OrderBy(x => x.CrMasLessorInformationCode).ToList();
            //var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => x.CrCasAccountContractCompanyOwedAccrualStatus == false).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

            //if (AllCompanyOwed?.Count() < 1)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}

            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => 
                x.CrCasSysAdministrativeProceduresCode == "310"
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
            var NewProcedureNo = "";


            CompanyOwed_VM taxOwed_VM = new CompanyOwed_VM();
            //For No Error in List
            taxOwed_VM.CrCasAccountContractCompanyOwed = new List<CrCasAccountContractCompanyOwed> { };
            //taxOwed_VM.CrCasAccountContractCompanyOwed = AllCompanyOwed;
            taxOwed_VM.CrMasLessorInformation = AllCompanies;

            taxOwed_VM.New_CompanyOwed_Tax_no = NewProcedureNo;
            return View(taxOwed_VM);
        }


        [HttpGet]
        public async Task<IActionResult> AddPaymentTaxValues_AfterSelectComp(string company)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "4";
            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101005", "1");
            ViewBag.CurrentLessor = company;

            var titles = await setTitle("101", "1101005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllCompanies = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode != "0000").OrderBy(x => x.CrMasLessorInformationCode).ToList();
            var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => company == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == false).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();
            //var AllCompanyOwed_Filtered = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == true, new[] { "CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation", "CrCasAccountContractCompanyOwedCompanyCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNo).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

            if (AllCompanyOwed?.Count() < 1)
            {
                return RedirectToAction("FailedMessageReport_NoData");
            }

            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == company &&
                x.CrCasSysAdministrativeProceduresCode == "310"
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
            var NewProcedureNo = y + "-" + "1" + "310" + "-" + company + "100" + "-" + Serial;


            CompanyOwed_VM taxOwed_VM = new CompanyOwed_VM();
            //taxOwed_VM.CrCasAccountContractCompanyOwed_Filtered = AllCompanyOwed_Filtered;
            taxOwed_VM.CrCasAccountContractCompanyOwed = AllCompanyOwed;
            taxOwed_VM.CrMasLessorInformation = AllCompanies;

            //taxOwed_VM.New_CompanyOwed_Tax_no = "0000-0000";
            taxOwed_VM.New_CompanyOwed_Tax_no = NewProcedureNo;
            return Json(taxOwed_VM);
        }



        [HttpPost]
        public async Task<IActionResult> AddPaymentTaxValues(List<string> values, string pay_date, string reasons, string Total_Money, string Serial_pay,string Company ,string company_ContractNo)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "4";
            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101005", "1");
            ViewBag.CurrentLessor = Company;

            var titles = await setTitle("101", "1101005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            //if (Serial_pay == null)
            //{
            //    return Json(new { code = 0 });
            //}

            decimal Total_Money_Decimal = decimal.Parse(Total_Money, CultureInfo.InvariantCulture);
            //////////////////
            // Save Adminstrive Procedures
            await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "310", "30", Company, "100",
                company_ContractNo, Total_Money_Decimal, Total_Money_Decimal, null, null, null, null, null, null, "تسديد مستحقات بنان", "Payment Dues of BNAN", "I", reasons);
            //_toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            //return RedirectToAction("SalesPoints", "SalesPoints");

            foreach (var item in values)
            {
                var exist = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => Company == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == false && x.CrCasAccountContractCompanyOwedNo == item.Trim()).FirstOrDefault();
                if (exist != null)
                {
                    exist.CrCasAccountContractCompanyOwedAccrualStatus = true;
                    exist.CrCasAccountContractCompanyOwedDatePayment = DateTime.Parse(pay_date).Date;
                    exist.CrCasAccountContractCompanyOwedAccrualPaymentNo = Serial_pay;
                    exist.CrCasAccountContractCompanyOwedAmount = Total_Money_Decimal;
                    //exist.CrCasAccountContractCompanyOwedUserCode = currentUser.CrMasUserInformationCode;

                    _unitOfWork.CrCasAccountContractCompanyOwed.Update(exist);
                }
            }


            //////////////////

            //await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تم تسديد مستحقات بنان", "Payment BNAN Done ", mainTask.CrMasSysMainTasksCode,
            //subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            //subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            //var result = new
            //{
            //    accountNo = account.CrCasAccountBankCode,
            //    accountIban = account.CrCasAccountBankIban,
            //    bankNo = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankCode,
            //    arBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankArName,
            //    enBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankEnName,
            //};
            //return Json(result);
            //RedirectToAction("SuccessToast", "CompanyOwed");
            //return Json(null);
            return Json(new { code = 1 });
            //return View();

        }


        //    [HttpGet]
        //    public async Task<IActionResult> EditPaymentTaxValues(string id)
        //    {
        //        //sidebar Active
        //        ViewBag.id = "#sidebarCompany";
        //        ViewBag.no = "4";
        //        var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101005", "1");
        //        ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

        //        var titles = await setTitle("101", "1101005", "1");
        //        await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

        //        //var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == true).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();
        //        var AllCompanyOwed_Filtered = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == true, new[] { "CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation", "CrCasAccountContractCompanyOwedCompanyCodeNavigation" }).Where(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNo == id).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

        //        if (AllCompanyOwed_Filtered?.Count() < 1)
        //        {
        //            return RedirectToAction("FailedMessageReport_NoData");
        //        }

        //        DateTime year = DateTime.Now;
        //        var y = year.ToString("yy");
        //        var ProcedureData = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == currentUser.CrMasUserInformationLessor &&
        //            x.CrCasSysAdministrativeProceduresCode == "310"
        //            && x.CrCasSysAdministrativeProceduresNo == id).FirstOrDefault();

        //        var NewProcedureNo = ProcedureData?.CrCasSysAdministrativeProceduresNo;

        //        CompanyOwed_VM taxOwed_VM = new CompanyOwed_VM();
        //        //taxOwed_VM.CrCasAccountContractCompanyOwed_Filtered = AllCompanyOwed_Filtered;
        //        taxOwed_VM.CrCasAccountContractCompanyOwed = AllCompanyOwed_Filtered;
        //        taxOwed_VM.CrCasSysAdministrativeProcedure_Data = ProcedureData;
        //        //taxOwed_VM.New_CompanyOwed_Tax_no = "0000-0000";
        //        taxOwed_VM.New_CompanyOwed_Tax_no = NewProcedureNo;
        //        return View(taxOwed_VM);
        //    }

        //    [HttpPost]
        //    public async Task<IActionResult> DeleteAllContracts(string id ,string reasons)
        //    {
        //        //sidebar Active
        //        ViewBag.id = "#sidebarCompany";
        //        ViewBag.no = "4";
        //        var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101005", "1");
        //        ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

        //        var titles = await setTitle("101", "1101005", "1");
        //        await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);


        //        var ProcedureData = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == currentUser.CrMasUserInformationLessor &&
        //x.CrCasSysAdministrativeProceduresCode == "310"
        //&& x.CrCasSysAdministrativeProceduresNo == id).FirstOrDefault();

        //        if (ProcedureData != null)
        //        {
        //            ProcedureData.CrCasSysAdministrativeProceduresArDescription = "حذف مستحقات القيمة المضافة";
        //            ProcedureData.CrCasSysAdministrativeProceduresEnDescription = "Delete Dues of Tax values";
        //            ProcedureData.CrCasSysAdministrativeProceduresStatus = "D";
        //            ProcedureData.CrCasSysAdministrativeProceduresReasons = reasons;

        //            _unitOfWork.CrCasSysAdministrativeProcedure.Update(ProcedureData);
        //        }



        //        var AllCompanyOwed_Filtered = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == true, new[] { "CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation", "CrCasAccountContractCompanyOwedCompanyCodeNavigation" }).Where(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNo == id).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

        //        foreach (var item in AllCompanyOwed_Filtered)
        //        {
        //            if (item != null)
        //            {
        //                item.CrCasAccountContractCompanyOwedAccrualStatus = false;
        //                item.CrCasAccountContractCompanyOwedDatePayment = null;
        //                item.CrCasAccountContractCompanyOwedAccrualPaymentNo = null;
        //                item.CrCasAccountContractCompanyOwedUserCode = null;

        //                _unitOfWork.CrCasAccountContractCompanyOwed.Update(item);
        //            }
        //        }


        //        //////////////////

        //        await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تم الغاء التسديد", "Payment Removed", mainTask.CrMasSysMainTasksCode,
        //        subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
        //        subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

        //        //var result = new
        //        //{
        //        //    accountNo = account.CrCasAccountBankCode,
        //        //    accountIban = account.CrCasAccountBankIban,
        //        //    bankNo = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankCode,
        //        //    arBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankArName,
        //        //    enBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankEnName,
        //        //};
        //        //return Json(result);
        //        //RedirectToAction("SuccessToast", "CompanyOwed");
        //        //return Json(null);
        //        return Json(new { code = 1 });
        //        //return View();

        //    }

        //    [HttpGet]
        //    public async Task<PartialViewResult> GetAllContractsByDate_statusAsync(string _max, string _mini)
        //    {
        //        //sidebar Active
        //        ViewBag.id = "#sidebarCompany";
        //        ViewBag.no = "4";
        //        var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101005", "1");
        //        ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

        //        var titles = await setTitle("101", "1101005", "1");
        //        await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

        //        if (!string.IsNullOrEmpty(_max) && !string.IsNullOrEmpty(_mini) && _max.Length > 0)
        //        {
        //            _max = DateTime.Parse(_max).Date.AddDays(1).ToString("yyyy-MM-dd");


        //            var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == true).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDatePayment).ToList();
        //            var AllCompanyOwed_Filtered = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode 
        //            && x.CrCasAccountContractCompanyOwedAccrualStatus == true, new[] { "CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation", "CrCasAccountContractCompanyOwedCompanyCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNo)
        //            .Where(x=>x.CrCasAccountContractCompanyOwedDatePayment < DateTime.Parse(_max).Date 
        //            && x.CrCasAccountContractCompanyOwedDatePayment >= DateTime.Parse(_mini).Date).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDatePayment).ToList();

        //            //var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => x.CrCasAccountContractCompanyOwedDate < DateTime.Parse(_max).Date && x.CrCasAccountContractCompanyOwedDate >= DateTime.Parse(_mini).Date && currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode).Where(x.CrCasAccountContractCompanyOwedDate < DateTime.Parse(_max).Date && x.CrCasAccountContractCompanyOwedDate >= DateTime.Parse(_mini).Date).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

        //            CompanyOwed_VM taxOwed_VM = new CompanyOwed_VM();
        //            taxOwed_VM.CrCasAccountContractCompanyOwed_Filtered = AllCompanyOwed_Filtered;
        //            taxOwed_VM.CrCasAccountContractCompanyOwed = AllCompanyOwed;

        //            return PartialView("_DataTableBasic", taxOwed_VM);
        //        }
        //        return PartialView();
        //    }




        [HttpGet]
        public async Task<IActionResult> GetAllContractsByCom(string Company)
        {
            List<string> contracts = new List<string>();
            var Contract_Type = "";
            var NewProcedureNo = "00";
            CompanyOwed_VM taxOwed_VM = new CompanyOwed_VM();

            if (Company != null)
            {
                var list_All_contracts_DropDown = _unitOfWork.CrMasContractCompany.FindAll(x => x.CrMasContractCompanyLessor == Company && x.CrMasContractCompanyProcedures == "112" && x.CrMasContractCompanyStatus != "N").OrderByDescending(x => x.CrMasContractCompanyDate).ToList();
                foreach (var item in list_All_contracts_DropDown)
                {
                    contracts.Add(item.CrMasContractCompanyNo);
                }
                Contract_Type = list_All_contracts_DropDown?.FirstOrDefault()?.CrMasContractCompanyActivation;

                ////////////

                if(list_All_contracts_DropDown?.Count() < 1) 
                {
                    var result = new
                    {
                        contracts = contracts,
                        contract_1_Type = Contract_Type,
                        PayNo_New = "",
                    };
                    return Json(result);
                }

                var AllCompanies = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode != "0000").OrderBy(x => x.CrMasLessorInformationCode).ToList();
                var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => Company == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedContractCom == list_All_contracts_DropDown.FirstOrDefault().CrMasContractCompanyNo && x.CrCasAccountContractCompanyOwedAccrualStatus == false).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();
                //var AllCompanyOwed_Filtered = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == true, new[] { "CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation", "CrCasAccountContractCompanyOwedCompanyCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNo).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

                //if (AllCompanyOwed?.Count() < 1)
                //{
                //    return RedirectToAction("FailedMessageReport_NoData");
                //}

                DateTime year = DateTime.Now;
                var y = year.ToString("yy");
                var Lrecord = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == Company &&
                    x.CrCasSysAdministrativeProceduresCode == "310"
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
                NewProcedureNo = y + "-" + "1" + "310" + "-" + Company + "100" + "-" + Serial;


                //taxOwed_VM.CrCasAccountContractCompanyOwed_Filtered = AllCompanyOwed_Filtered;
                taxOwed_VM.CrCasAccountContractCompanyOwed = AllCompanyOwed;
                taxOwed_VM.CrMasLessorInformation = AllCompanies;

                //taxOwed_VM.New_CompanyOwed_Tax_no = "0000-0000";
                taxOwed_VM.New_CompanyOwed_Tax_no = NewProcedureNo;

                ///////////
            }
            //small in object is More More Important !!!!!
            var result2 = new
            {
                contracts = contracts,
                contract_1_Type = Contract_Type,
                payNo_New = NewProcedureNo,
                accountIban = "22",
                bankNo = "22",
                arBank = "22",
                enBank = "22",
            };
            return Json(result2);
            //RedirectToAction("SuccessToast", "TaxOwed");
            //return Json(null);
            //return Json(new { code = 1 });
            //return View();
        }



        [HttpGet]
        public async Task<PartialViewResult> GetAllContracts_ToDataTable(string Company, string company_ContractNo)
        {
            List<string> contracts = new List<string>();
            var Contract_Type = "";
            CompanyOwed_VM taxOwed_VM = new CompanyOwed_VM();

            if (Company != null)
            {
                ////////////

                var AllCompanies = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode != "0000").OrderBy(x => x.CrMasLessorInformationCode).ToList();
                var AllCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => Company == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedContractCom == company_ContractNo && x.CrCasAccountContractCompanyOwedAccrualStatus == false).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();
                //var AllCompanyOwed_Filtered = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountContractCompanyOwedCompanyCode && x.CrCasAccountContractCompanyOwedAccrualStatus == true, new[] { "CrCasAccountContractCompanyOwedAccrualPaymentNoNavigation", "CrCasAccountContractCompanyOwedCompanyCodeNavigation" }).DistinctBy(x => x.CrCasAccountContractCompanyOwedAccrualPaymentNo).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

                //if (AllCompanyOwed?.Count() < 1)
                //{
                //    return RedirectToAction("FailedMessageReport_NoData");
                //}



                //taxOwed_VM.CrCasAccountContractCompanyOwed_Filtered = AllCompanyOwed_Filtered;
                taxOwed_VM.CrCasAccountContractCompanyOwed = AllCompanyOwed;
                taxOwed_VM.CrMasLessorInformation = AllCompanies;

                ///////////
            }

            return PartialView("_DataTablePaymentTaxValues", taxOwed_VM);
            //RedirectToAction("SuccessToast", "TaxOwed");
            //return Json(null);
            //return Json(new { code = 1 });
            //return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNameTypeOfContract(string Contract)
        {
            var contract_type = "";

            if (Contract != null)
            {
                var NameTypeOfContract = _unitOfWork.CrMasContractCompany.Find(x => x.CrMasContractCompanyNo == Contract);
                contract_type = NameTypeOfContract?.CrMasContractCompanyActivation;
            }

            var result = new
            {
                contract_type = contract_type,

            };
            return Json(result);
            //RedirectToAction("SuccessToast", "TaxOwed");
            //return Json(null);
            //return Json(new { code = 1 });
            //return View();
        }



        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "4";
            ViewBag.Data = "0";
            var titles = await setTitle("101", "1101005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
           
            return View();
            
        }

        public IActionResult SuccessToast()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "CompanyOwed");
        }

        public IActionResult FailerToast()
        {
            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "CompanyOwed");
        }

    }
}