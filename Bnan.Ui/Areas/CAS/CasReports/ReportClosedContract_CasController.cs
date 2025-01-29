using AutoMapper;
using Azure;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
//using Bnan.Inferastructure.Repository.CAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.CAS.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Numerics;
namespace Bnan.Ui.Areas.CAS.Controllers.CasReports
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class ReportClosedContract_CasController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportClosedContract_CasController> _localizer;
        private readonly string pageNumber = SubTasks.Closed_contracts_Report_Cas;


        public ReportClosedContract_CasController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportClosedContract_CasController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterInformation = masRenterInformation;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            // Set page titles
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var listmaxDate = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasRenterContractBasicStatus == Status.Closed,
                    selectProjection: query => query.Select(x => new Date_ReportClosedContract_CasVM
                    {
                        dates = x.CrCasRenterContractBasicExpectedStartDate,
                    }));

            if (listmaxDate?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var maxDate = listmaxDate.Max(x => x.dates)?.ToString("yyyy-MM-dd");

            var end = DateTime.Now.AddDays(1);
            var start = DateTime.Now.AddMonths(-1);
            if (maxDate != null)
            {
                end = DateTime.Parse(maxDate).AddDays(1);
                start = DateTime.Parse(maxDate).AddMonths(-1);
            }

            var all_RenterBasicContract = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                && x.CrCasRenterContractBasicStatus == Status.Closed
                && x.CrCasRenterContractBasicExpectedStartDate > start && x.CrCasRenterContractBasicExpectedStartDate <= end,
                selectProjection: query => query.Select(x => new ReportClosedContract_CasVM
                {
                    CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                    CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                    CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                    CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                    //CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                    CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                    CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                    CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                    CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                    branchAr = x.CrCasRenterContractBasic1.CrCasBranchInformationArShortName,
                    branchEn = x.CrCasRenterContractBasic1.CrCasBranchInformationEnShortName,
                    CrCasRenterContractBasicAmountPaidAdvance = x.CrCasRenterContractBasicAmountPaidAdvance,
                    CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                    CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                    //CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
                    //CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
                    CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                    CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                    CrCasRenterContractBasicCurrentReadingMeter = x.CrCasRenterContractBasicCurrentReadingMeter,
                    CrCasRenterContractBasicActualCurrentReadingMeter = x.CrCasRenterContractBasicActualCurrentReadingMeter,
                    CrCasRenterContractBasicActualTotal = x.CrCasRenterContractBasicActualTotal,
                    CrCasRenterContractBasicActualAmountRequired = x.CrCasRenterContractBasicActualAmountRequired,
                    CrCasRenterContractBasicExpensesValue = x.CrCasRenterContractBasicExpensesValue,
                    CrCasRenterContractBasicCompensationValue = x.CrCasRenterContractBasicCompensationValue,
                    CrCasRenterContractBasicActualExtraHours = x.CrCasRenterContractBasicActualExtraHours,
                })
                , includes: new string[] { "CrCasRenterContractBasic1" }
                );

            var all_Invoices = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasAccountInvoiceReferenceContract,
                    nameAr = x.CrCasAccountInvoicePdfFile,
                    str4 = x.CrCasAccountInvoiceUserCode,
                })
                //,includes: new string[] { "" } 
                );

            var Sum_ContractsValue = all_RenterBasicContract.Sum(x => x.CrCasRenterContractBasicActualTotal);
            var Sum_ExpensesValue = all_RenterBasicContract.Sum(x => x.CrCasRenterContractBasicExpensesValue);
            var Sum_CompensationsValue = all_RenterBasicContract.Sum(x => x.CrCasRenterContractBasicCompensationValue);
            
            listReportClosedContract_CasVM VM = new listReportClosedContract_CasVM();
            
            VM.all_contractBasic = all_RenterBasicContract;
            VM.all_Invoices = all_Invoices;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");
            VM.Sum_ContractsValue = Sum_ContractsValue?.ToString("N2",CultureInfo.InvariantCulture)??"0.00";
            VM.Sum_ExpensesValue = Sum_ExpensesValue?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
            VM.Sum_CompensationsValue = Sum_CompensationsValue?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";

            return View(VM);
        }

        [HttpGet]
        //[Route("/CAS/ReportClosedContract_Cas/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string start, string end)
        {
            var user = await _userManager.GetUserAsync(User);

            //sidebar Closed
            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start);
                var end_Date = DateTime.Parse(end).AddDays(1);
                var AllContracts = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                && x.CrCasRenterContractBasicStatus == Status.Closed
                && x.CrCasRenterContractBasicExpectedStartDate > start_Date && x.CrCasRenterContractBasicExpectedStartDate <= end_Date,
                    selectProjection: query => query.Select(x => new ReportClosedContract_CasVM
                    {
                        CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                        CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                        CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                        CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                        //CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                        CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                        CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                        CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                        CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                        branchAr = x.CrCasRenterContractBasic1.CrCasBranchInformationArShortName,
                        branchEn = x.CrCasRenterContractBasic1.CrCasBranchInformationEnShortName,
                        CrCasRenterContractBasicAmountPaidAdvance = x.CrCasRenterContractBasicAmountPaidAdvance,
                        CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                        CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                        //CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
                        //CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
                        CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                        CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                        CrCasRenterContractBasicCurrentReadingMeter = x.CrCasRenterContractBasicCurrentReadingMeter,
                        CrCasRenterContractBasicActualCurrentReadingMeter = x.CrCasRenterContractBasicActualCurrentReadingMeter,
                        CrCasRenterContractBasicActualTotal = x.CrCasRenterContractBasicActualTotal,
                        CrCasRenterContractBasicActualAmountRequired = x.CrCasRenterContractBasicActualAmountRequired,
                        CrCasRenterContractBasicExpensesValue = x.CrCasRenterContractBasicExpensesValue,
                        CrCasRenterContractBasicCompensationValue = x.CrCasRenterContractBasicCompensationValue,
                        CrCasRenterContractBasicActualExtraHours = x.CrCasRenterContractBasicActualExtraHours,
                    })
                , includes: new string[] { "CrCasRenterContractBasic1" }
                    );

                var all_Invoices = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: null,
                    selectProjection: query => query.Select(x => new cas_list_String_4
                    {
                        id_key = x.CrCasAccountInvoiceReferenceContract,
                        nameAr = x.CrCasAccountInvoicePdfFile,
                        str4 = x.CrCasAccountInvoiceUserCode,
                    })
                    //,includes: new string[] { "" } 
                    );
                var Sum_ContractsValue = AllContracts.Sum(x => x.CrCasRenterContractBasicActualTotal);
                var Sum_ExpensesValue = AllContracts.Sum(x => x.CrCasRenterContractBasicExpensesValue);
                var Sum_CompensationsValue = AllContracts.Sum(x => x.CrCasRenterContractBasicCompensationValue);
                listReportClosedContract_CasVM VM = new listReportClosedContract_CasVM();
                VM.all_Invoices = all_Invoices;
                VM.Sum_ContractsValue = Sum_ContractsValue?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
                VM.Sum_ExpensesValue = Sum_ExpensesValue?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
                VM.Sum_CompensationsValue = Sum_CompensationsValue?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
                var FilterByStatus = AllContracts.FindAll(x => x.CrCasRenterContractBasicStatus == Status.Closed);

                VM.all_contractBasic = FilterByStatus;
                return PartialView("_DataTableReportClosedContract_Cas", VM);
            }
            listReportClosedContract_CasVM VM2 = new listReportClosedContract_CasVM();
            VM2.Sum_ContractsValue = "0.00";
            VM2.Sum_ExpensesValue = "0.00";
            VM2.Sum_CompensationsValue = "0.00";
            return PartialView("_DataTableReportClosedContract_Cas", VM2);
            //return PartialView();
        }

        [HttpGet]
        //[Route("/CAS/ReportClosedContract_Cas/GetContractsByStatus")]
        public async Task<IActionResult> GetContractsData(string contractId)
        {
            var user = await _userManager.GetUserAsync(User);

          
                var this_Contracts = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                && x.CrCasRenterContractBasicNo == contractId.Trim()
                && x.CrCasRenterContractBasicStatus == Status.Closed,
                    selectProjection: query => query.Select(x => new ReportClosedContract_CasVM
                    {
                        CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                        CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                        CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                        CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                        //CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                        CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                        CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                        CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                        CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                        branchAr = x.CrCasRenterContractBasic1.CrCasBranchInformationArShortName,
                        branchEn = x.CrCasRenterContractBasic1.CrCasBranchInformationEnShortName,
                        CrCasRenterContractBasicAmountPaidAdvance = x.CrCasRenterContractBasicAmountPaidAdvance,
                        CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                        CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                        //CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
                        //CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
                        CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                        CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                        CrCasRenterContractBasicCurrentReadingMeter = x.CrCasRenterContractBasicCurrentReadingMeter,
                        CrCasRenterContractBasicActualCurrentReadingMeter = x.CrCasRenterContractBasicActualCurrentReadingMeter,
                        CrCasRenterContractBasicActualTotal = x.CrCasRenterContractBasicActualTotal,
                        CrCasRenterContractBasicActualAmountRequired = x.CrCasRenterContractBasicActualAmountRequired,
                        CrCasRenterContractBasicExpensesValue = x.CrCasRenterContractBasicExpensesValue,
                        CrCasRenterContractBasicCompensationValue = x.CrCasRenterContractBasicCompensationValue,
                        CrCasRenterContractBasicActualExtraHours = x.CrCasRenterContractBasicActualExtraHours,
                        Reasons = x.CrCasRenterContractBasicReasons,
                    })
                , includes: new string[] { "CrCasRenterContractBasic1" }
                    );

                var all_Invoices = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x=>x.CrCasAccountInvoiceReferenceContract == contractId.Trim(),
                    selectProjection: query => query.Select(x => new cas_list_String_4
                    {
                        id_key = x.CrCasAccountInvoiceReferenceContract,
                        nameAr = x.CrCasAccountInvoicePdfFile,
                        str4 = x.CrCasAccountInvoiceUserCode,
                    })
                    //,includes: new string[] { "" } 
                    );

            var single = this_Contracts?.FirstOrDefault();

            var AmountRequired = (single?.CrCasRenterContractBasicActualTotal??0) - (single?.CrCasRenterContractBasicExpensesValue??0) + (single?.CrCasRenterContractBasicCompensationValue??0);
            var AmountPaid = (single?.CrCasRenterContractBasicAmountPaidAdvance??0) + (single?.CrCasRenterContractBasicAmountPaid??0);
            var remain = (AmountRequired - AmountPaid).ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
            
            //##########
            var mod_ContractId = single?.CrCasRenterContractBasicNo?? " ";
            var mod_Paid = AmountPaid.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
            var mod_Remain = remain;
            var mod_KM = ((single?.CrCasRenterContractBasicActualCurrentReadingMeter??0) - (single?.CrCasRenterContractBasicCurrentReadingMeter??0)).ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
            var mod_additionHours = single?.CrCasRenterContractBasicActualExtraHours?.ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
            var mod_Reasons = single?.Reasons ?? " ";



            // #############

            var Contract_pdf = "#";
            var Contract_pdf_blank = "";
            var Invoice_pdf = "#";
            var Invoice_pdf_blank = "";
            var TGA_pdf = "#";
            var TGA_pdf_blank = "";
            var invoce_ar = all_Invoices?.Find(x => x.id_key == contractId.Trim())?.nameAr ?? "";
            var invoce_en = all_Invoices?.Find(x => x.id_key == contractId.Trim())?.nameEn ?? "";

                if (single.CrCasRenterContractBasicPdfFile != null && single.CrCasRenterContractBasicPdfFile != "") { Contract_pdf = single.CrCasRenterContractBasicPdfFile?.ToString().Replace("~", ""); Contract_pdf_blank = "_blank"; }
                if (single.CrCasRenterContractBasicPdfTga != null && single.CrCasRenterContractBasicPdfTga != "") { TGA_pdf = single.CrCasRenterContractBasicPdfTga?.ToString().Replace("~", ""); TGA_pdf_blank = "_blank"; }
                if (invoce_ar != null && invoce_ar != "") { Invoice_pdf = invoce_ar?.ToString().Replace("~", ""); Invoice_pdf_blank = "_blank"; }

            var response = new
                {
                    mod_ContractId = mod_ContractId,
                    mod_Paid = mod_Paid,
                    mod_Remain = mod_Remain,
                    mod_KM = mod_KM,
                    mod_additionHours = mod_additionHours,
                    mod_Reasons = mod_Reasons,
                    reciptPdf_href = Invoice_pdf,
                    reciptPdf_target = Invoice_pdf_blank,
                    ContractPdf_href = Contract_pdf,
                    ContractPdf_target = Contract_pdf_blank,
                    TGAPdf_href = TGA_pdf,
                    TGAPdf_target = TGA_pdf_blank,
                };

                return Json(response);
            
       
            return Json(null);
            //return PartialView();
        }

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {


            var recordAr = licence.CrMasRenterInformationArName;
            var recordEn = licence.CrMasRenterInformationEnName;
            var (operationAr, operationEn) = GetStatusTranslation(status);

            var (mainTask, subTask, system, currentUser) = await SetTrace(pageNumber);

            await _userLoginsService.SaveTracing(
                currentUser.CrMasUserInformationCode,
                recordAr,
                recordEn,
                operationAr,
                operationEn,
                mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode,
                mainTask.CrMasSysMainTasksArName,
                subTask.CrMasSysSubTasksArName,
                mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName,
                system.CrMasSysSystemCode,
                system.CrMasSysSystemArName,
                system.CrMasSysSystemEnName);
        }

        [HttpPost]
        public IActionResult DisplayToastError_NoUpdate(string messageText)
        {
            //نص الرسالة _localizer["AuthEmplpoyee_NoUpdate"] === messageText ; 
            if (messageText == null || messageText == "") messageText = "..";
            _toastNotification.AddErrorToastMessage(messageText, new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return Json(new { success = true });
        }


        public IActionResult DisplayToastSuccess_withIndex()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            return RedirectToAction("Index", "ReportClosedContract_Cas");
        }


    }
}
