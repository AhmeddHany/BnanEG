using AutoMapper;
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
using System.Numerics;
namespace Bnan.Ui.Areas.CAS.Controllers.CasReports
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class ReportSaved_CanceledContract_CasController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportSaved_CanceledContract_CasController> _localizer;
        private readonly string pageNumber = SubTasks.SavedAndCanceled_contracts_Report_Cas;


        public ReportSaved_CanceledContract_CasController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportSaved_CanceledContract_CasController> localizer) : base(userManager, unitOfWork, mapper)
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
                    predicate: x => x.CrCasRenterContractBasicStatus == Status.Update || x.CrCasRenterContractBasicStatus == Status.Cancel,
                    selectProjection: query => query.Select(x => new Date_ReportSaved_CanceledContract_CasVM
                    {
                        dates = x.CrCasRenterContractBasicExpectedStartDate,
                    }));

            if (listmaxDate?.Count == 0 )
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
                predicate: x => (x.CrCasRenterContractBasicStatus == Status.Update || x.CrCasRenterContractBasicStatus == Status.Cancel)
                && x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                && x.CrCasRenterContractBasicExpectedStartDate > start && x.CrCasRenterContractBasicExpectedStartDate <= end,
                selectProjection: query => query.Select(x => new ReportSaved_CanceledContract_CasVM
                {
                    CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                    CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                    CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                    CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                    CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                    CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                    //CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                    CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                    CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                    CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
                    CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
                    CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                    CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                    CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                    CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                    CrCasRenterContractBasicAmountPaidAdvance = x.CrCasRenterContractBasicAmountPaidAdvance,
                    branchAr = x.CrCasRenterContractBasic1.CrCasBranchInformationArShortName,
                    branchEn = x.CrCasRenterContractBasic1.CrCasBranchInformationEnShortName,
                })
                , includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic1" }
                );
            var allRenters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
                selectProjection: query => query.Select(x => new CrMasRenterInformation
                {
                    CrMasRenterInformationId = x.CrMasRenterInformationId,
                    CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                    CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                    CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
                })
                //,includes: new string[] { "CrMasRenterInformationNationalityNavigation", "CrMasRenterInformationProfessionNavigation" } 
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

            listReportSaved_CanceledContract_CasVM VM = new listReportSaved_CanceledContract_CasVM();
            VM.all_RentersMas = allRenters;
            VM.all_contractBasic = all_RenterBasicContract;
            VM.all_Invoices = all_Invoices;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");

            return View(VM);
        }

        [HttpGet]
        [Route("/CAS/ReportSaved_CanceledContract_Cas/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string status,string start , string end)
        {
            var user = await _userManager.GetUserAsync(User);

            //sidebar Active
            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start);
                var end_Date = DateTime.Parse(end).AddDays(1);
                
                var AllContracts = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x => (x.CrCasRenterContractBasicStatus == Status.Update || x.CrCasRenterContractBasicStatus == Status.Cancel)
                    && x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                    && x.CrCasRenterContractBasicExpectedStartDate > start_Date && x.CrCasRenterContractBasicExpectedStartDate <= end_Date
                    ,
                    selectProjection: query => query.Select(x => new ReportSaved_CanceledContract_CasVM
                    {
                        CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                        CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                        CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                        CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                        CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                        CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                        //CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                        CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                        CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                        CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
                        CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
                        CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                        CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                        CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                        CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                        CrCasRenterContractBasicAmountPaidAdvance = x.CrCasRenterContractBasicAmountPaidAdvance,
                        branchAr = x.CrCasRenterContractBasic1.CrCasBranchInformationArShortName,
                        branchEn = x.CrCasRenterContractBasic1.CrCasBranchInformationEnShortName,
                    })
                , includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasic1" }
                    );
                var allRenters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
                    selectProjection: query => query.Select(x => new CrMasRenterInformation
                    {
                        CrMasRenterInformationId = x.CrMasRenterInformationId,
                        CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                        CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                        CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
                    })
                    //,includes: new string[] { "CrMasRenterInformationNationalityNavigation", "CrMasRenterInformationProfessionNavigation" } 
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

                listReportSaved_CanceledContract_CasVM VM = new listReportSaved_CanceledContract_CasVM();
                VM.all_RentersMas = allRenters;
                VM.all_Invoices = all_Invoices;

                if (status == Status.All)
                {
                    var FilterAll = AllContracts;
                    VM.all_contractBasic = FilterAll;
                    return PartialView("_DataTableReportSaved_CanceledContract_Cas", VM);
                }
                var FilterByStatus = AllContracts.FindAll(x => x.CrCasRenterContractBasicStatus == status);

                VM.all_contractBasic = FilterByStatus;
                return PartialView("_DataTableReportSaved_CanceledContract_Cas", VM);
            }
            listReportSaved_CanceledContract_CasVM VM2 = new listReportSaved_CanceledContract_CasVM();

            return PartialView("_DataTableReportSaved_CanceledContract_Cas", VM2);
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
            return RedirectToAction("Index", "ReportSaved_CanceledContract_Cas");
        }


    }
}
