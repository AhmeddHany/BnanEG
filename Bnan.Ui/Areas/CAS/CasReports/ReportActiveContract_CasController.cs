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
using System;
using System.Numerics;
namespace Bnan.Ui.Areas.CAS.Controllers.CasReports
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class ReportActiveContract_CasController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportActiveContract_CasController> _localizer;
        private readonly string pageNumber = SubTasks.Active_contracts_Report_Cas;


        public ReportActiveContract_CasController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportActiveContract_CasController> localizer) : base(userManager, unitOfWork, mapper)
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
                    predicate: x => x.CrCasRenterContractBasicStatus == Status.Active || x.CrCasRenterContractBasicStatus == Status.Expire,
                    selectProjection: query => query.Select(x => new Date_ReportActiveContract_CasVM
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
                && x.CrCasRenterContractBasicStatus == Status.Active
                && x.CrCasRenterContractBasicExpectedStartDate > start && x.CrCasRenterContractBasicExpectedStartDate <= end,
                selectProjection: query => query.Select(x => new ReportActiveContract_CasVM
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
                })
                , includes: new string[] { "CrCasRenterContractBasic1" }
                );
            //var allRenters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
            //    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
            //    predicate: x=>x.CrMasRenterInformationStatus != Status.Deleted,
            //    selectProjection: query => query.Select(x => new CrMasRenterInformation
            //    {
            //        CrMasRenterInformationId = x.CrMasRenterInformationId,
            //        CrMasRenterInformationArName = x.CrMasRenterInformationArName,
            //        CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
            //        CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
            //    })
            //    //,includes: new string[] { "CrMasRenterInformationNationalityNavigation", "CrMasRenterInformationProfessionNavigation" } 
            //    );

            var allStatus_contracts = await _unitOfWork.CrCasRenterContractAlert.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasRenterContractAlertLessor == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new Date_status_ContractVM
                {
                    CrCasRenterContractAlertNo = x.CrCasRenterContractAlertNo,
                    CrCasRenterContractAlertLessor = x.CrCasRenterContractAlertLessor,
                    CrCasRenterContractAlertBranch = x.CrCasRenterContractAlertBranch,
                    CrCasRenterContractAlertContractActiviteStatus = x.CrCasRenterContractAlertContractActiviteStatus,
                    CrCasRenterContractAlertContractStatus = x.CrCasRenterContractAlertContractStatus,
                })
                );
            var all_Invoices = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
            //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate:x=> x.CrCasAccountInvoiceLessorCode == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasAccountInvoiceReferenceContract,
                    nameAr = x.CrCasAccountInvoicePdfFile,
                    str4 = x.CrCasAccountInvoiceUserCode,
                })
                //,includes: new string[] { "" } 
                );

            //// If no active licenses, retrieve all licenses
            //if (!all_RenterBasicContract.Any())
            //{
            //    all_RenterBasicContract = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
            //       //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
            //       predicate: x => x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
            //       && x.CrCasRenterContractBasicStatus == Status.Expire
            //       && x.CrCasRenterContractBasicExpectedStartDate > start && x.CrCasRenterContractBasicExpectedStartDate <= end,
            //       selectProjection: query => query.Select(x => new ReportActiveContract_CasVM
            //{
            //    CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
            //        CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
            //        CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
            //        CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
            //        //CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
            //        CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
            //        CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
            //        CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
            //        CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
            //        branchAr = x.CrCasRenterContractBasic1.CrCasBranchInformationArShortName,
            //        branchEn = x.CrCasRenterContractBasic1.CrCasBranchInformationEnShortName,
            //        CrCasRenterContractBasicAmountPaidAdvance = x.CrCasRenterContractBasicAmountPaidAdvance,
            //        CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
            //        CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
            //        //CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
            //        //CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
            //        CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
            //        CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
            //    })
            //    , includes: new string[] { "CrCasRenterContractBasic1" }
            //       );
            //    ViewBag.radio = "All";
            //}
            //else ViewBag.radio = "A";
            ViewBag.radio = "All";


            listReportActiveContract_CasVM VM = new listReportActiveContract_CasVM();
            //VM.all_RentersMas = allRenters;
            VM.all_status = allStatus_contracts;
            VM.all_contractBasic = all_RenterBasicContract;
            VM.all_Invoices = all_Invoices;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");

            return View(VM);
        }

        [HttpGet]
        //[Route("/CAS/ReportActiveContract_Cas/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string status, string start, string end)
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
                    predicate: x => x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                    && (x.CrCasRenterContractBasicStatus ==Status.Active || x.CrCasRenterContractBasicStatus == Status.Expire)
                    && x.CrCasRenterContractBasicExpectedStartDate > start_Date && x.CrCasRenterContractBasicExpectedStartDate <= end_Date,
                    selectProjection: query => query.Select(x => new ReportActiveContract_CasVM
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
                    })
                , includes: new string[] { "CrCasRenterContractBasic1" }
                    );
                //var allRenters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                //    predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
                //    selectProjection: query => query.Select(x => new CrMasRenterInformation
                //    {
                //        CrMasRenterInformationId = x.CrMasRenterInformationId,
                //        CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                //        CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                //        CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
                //    })
                //    //,includes: new string[] { "CrMasRenterInformationNationalityNavigation", "CrMasRenterInformationProfessionNavigation" } 
                //    );

                var allStatus_contracts = await _unitOfWork.CrCasRenterContractAlert.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasRenterContractAlertLessor == user.CrMasUserInformationLessor,
                    selectProjection: query => query.Select(x => new Date_status_ContractVM
                    {
                        CrCasRenterContractAlertNo = x.CrCasRenterContractAlertNo,
                        CrCasRenterContractAlertLessor = x.CrCasRenterContractAlertLessor,
                        CrCasRenterContractAlertBranch = x.CrCasRenterContractAlertBranch,
                        CrCasRenterContractAlertContractActiviteStatus = x.CrCasRenterContractAlertContractActiviteStatus,
                        CrCasRenterContractAlertContractStatus = x.CrCasRenterContractAlertContractStatus,
                    })
                    );
                var all_Invoices = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasAccountInvoiceLessorCode == user.CrMasUserInformationLessor,
                    selectProjection: query => query.Select(x => new cas_list_String_4
                    {
                        id_key = x.CrCasAccountInvoiceReferenceContract,
                        nameAr = x.CrCasAccountInvoicePdfFile,
                        str4 = x.CrCasAccountInvoiceUserCode,
                    })
                    //,includes: new string[] { "" } 
                    );


                listReportActiveContract_CasVM VM = new listReportActiveContract_CasVM();
                //VM.all_RentersMas = allRenters;
                VM.all_status = allStatus_contracts;
                VM.all_Invoices = all_Invoices;
                if (status == "all")
                {
                    VM.all_contractBasic = AllContracts;
                    return PartialView("_DataTableReportActiveContract_Cas", VM);
                }
                else if(status == "today")
                {
                    var query_Alert1 = allStatus_contracts.Where(x => x.CrCasRenterContractAlertContractActiviteStatus == "2").ToList();
                    AllContracts = AllContracts.Where(x => query_Alert1.Any(y => y.CrCasRenterContractAlertNo == x.CrCasRenterContractBasicNo && x.CrCasRenterContractBasicStatus == "A")).OrderBy(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
                }
                else if (status == "tomorrow")
                {
                    var query_Alert2 = allStatus_contracts.Where(x => x.CrCasRenterContractAlertContractActiviteStatus == "1").ToList();
                    AllContracts = AllContracts.Where(x => query_Alert2.Any(y => y.CrCasRenterContractAlertNo == x.CrCasRenterContractBasicNo)).OrderBy(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
                }
                else if (status == "later")
                {
                    var query_Alert3 = allStatus_contracts.Where(x => x.CrCasRenterContractAlertContractActiviteStatus == "0").ToList();
                    AllContracts = AllContracts.Where(x => query_Alert3.Any(y => y.CrCasRenterContractAlertNo == x.CrCasRenterContractBasicNo)).OrderBy(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
                }
                else if (status == "ended")
                {
                    var query_Alert3 = allStatus_contracts.Where(x => x.CrCasRenterContractAlertContractActiviteStatus == "3").ToList();
                    AllContracts = AllContracts.Where(x => query_Alert3.Any(y => y.CrCasRenterContractAlertNo == x.CrCasRenterContractBasicNo)).OrderBy(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
                }
                VM.all_contractBasic = AllContracts;
                return PartialView("_DataTableReportActiveContract_Cas", VM);
            
            }
            listReportActiveContract_CasVM VM2 = new listReportActiveContract_CasVM();

            return PartialView("_DataTableReportActiveContract_Cas", VM2);
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
            return RedirectToAction("Index", "ReportActiveContract_Cas");
        }


    }
}
