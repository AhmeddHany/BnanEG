using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Inferastructure.Repository.MAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.CAS.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.MasReports
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class ReportCarContractController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportCarContractController> _localizer;
        private readonly string pageNumber = SubTasks.MasReport5;


        public ReportCarContractController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportCarContractController> localizer) : base(userManager, unitOfWork, mapper)
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

            var all_CarContracts = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationConractCount > 0,
                //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
                selectProjection: query => query.Select(x => new CarContractVM
                {
                    CrCasCarInformationSerailNo = x.CrCasCarInformationSerailNo,
                    CrCasCarInformationLessor = x.CrCasCarInformationLessor,
                    CrCasCarInformationConcatenateArName = x.CrCasCarInformationConcatenateArName,
                    CrCasCarInformationConcatenateEnName = x.CrCasCarInformationConcatenateEnName,
                    CrCasCarInformationLastContractDate = x.CrCasCarInformationLastContractDate,
                    CrCasCarInformationConractCount = x.CrCasCarInformationConractCount,
                    CrCasCarInformationCurrentMeter = x.CrCasCarInformationCurrentMeter,
                    CrCasCarInformationConractDaysNo = x.CrCasCarInformationConractDaysNo,
                    CrCasCarInformationForSaleStatus = x.CrCasCarInformationForSaleStatus,
                    CrCasCarInformationMaintenanceStatus = x.CrCasCarInformationMaintenanceStatus,
                    CrCasCarInformationStatus = x.CrCasCarInformationStatus,
                })
                //, includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                );

            var allLessors = await _unitOfWork.CrMasLessorInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasLessorInformationStatus != Status.Deleted,
                selectProjection: query => query.Select(x => new CrMasLessorInformation
                {
                    CrMasLessorInformationCode = x.CrMasLessorInformationCode,
                    CrMasLessorInformationArShortName = x.CrMasLessorInformationArShortName,
                    CrMasLessorInformationEnShortName = x.CrMasLessorInformationEnShortName,
                })
                );

            if (all_CarContracts?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            ViewBag.radio = "A";


            listReportCarContractVM VM = new listReportCarContractVM();
            VM.all_lessors = allLessors;
            VM.all_CarContracts = all_CarContracts;

            return View(VM);
        }

        [HttpGet]
        //[Route("/MAS/ReportCarContract/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string id, string start, string end)
        {
            listReportCarContractVM VM = new listReportCarContractVM();

            //sidebar Car
            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start);
                var end_Date = DateTime.Parse(end).AddDays(1);
                VM.start_Date = start_Date.ToString("yyyy-MM-dd");
                VM.end_Date = end_Date.AddDays(-1).ToString("yyyy-MM-dd");

                await SetPageTitleAsync(Status.Update, pageNumber);
                var thisRenterBasicContract = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x => x.CrCasRenterContractBasicCarSerailNo == id
                   && x.CrCasRenterContractBasicExpectedStartDate > start_Date && x.CrCasRenterContractBasicExpectedStartDate <= end_Date
                   && x.CrCasRenterContractBasicStatus != Status.Extension
                    ,
                    selectProjection: query => query.Select(x => new ReportCarContractVM
                    {
                        CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                        CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                        CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                        CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                        CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                        CrCasRenterContractBasicCurrentReadingMeter = x.CrCasRenterContractBasicCurrentReadingMeter,
                        CrCasRenterContractBasicActualCurrentReadingMeter = x.CrCasRenterContractBasicActualCurrentReadingMeter,
                        CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                        CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                        CrCasRenterContractBasicActualDays = x.CrCasRenterContractBasicActualDays,
                        CrCasRenterContractBasicActualTotal = x.CrCasRenterContractBasicActualTotal,
                        CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                        CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                        CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
                        CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
                        CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                        CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                        CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                        CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                    })
                    , includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                    );
                List<CrMasRenterInformation> all_renters = new List<CrMasRenterInformation>();
                List<CrCasAccountInvoice> all_Invoices = new List<CrCasAccountInvoice>();
                sumitionofClass_up_VM summition = new sumitionofClass_up_VM();
                foreach (var single in thisRenterBasicContract)
                {
                    var ThisInvoice = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x => x.CrCasAccountInvoiceReferenceContract == single.CrCasRenterContractBasicNo,
                    selectProjection: query => query.Select(x => new CrCasAccountInvoice
                    {
                        CrCasAccountInvoiceReferenceContract = x.CrCasAccountInvoiceReferenceContract,
                        CrCasAccountInvoicePdfFile = x.CrCasAccountInvoicePdfFile,
                        CrCasAccountInvoiceUserCode = x.CrCasAccountInvoiceUserCode,
                    })
                    //,includes: new string[] { "" } 
                    );
                    if (ThisInvoice.Count > 0)
                    {
                        all_Invoices.Add(ThisInvoice.FirstOrDefault());
                    }
                    var ThisRenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                        //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                        predicate: x => x.CrMasRenterInformationId == single.CrCasRenterContractBasicRenterId,
                        selectProjection: query => query.Select(x => new CrMasRenterInformation
                        {
                            CrMasRenterInformationId = x.CrMasRenterInformationId,
                            CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                            CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                        })
                        //,includes: new string[] { "" } 
                        );
                    if (ThisRenterData.Count > 0)
                    {
                        all_renters.Add(ThisRenterData.FirstOrDefault());
                    }
                    if (single.CrCasRenterContractBasicStatus == Status.Closed)
                    {
                        summition.Days_Count += single.CrCasRenterContractBasicActualDays;
                        summition.km_Count += single.CrCasRenterContractBasicActualCurrentReadingMeter - single.CrCasRenterContractBasicCurrentReadingMeter;
                        summition.Contracts_Count += 1;
                        summition.contract_Values_Total += single.CrCasRenterContractBasicActualTotal;
                    }
                    else
                    {
                        summition.Days_Count += single.CrCasRenterContractBasicExpectedRentalDays;
                        //summition.km_Count += 0;
                        summition.Contracts_Count += 1;
                        summition.contract_Values_Total += single.CrCasRenterContractBasicExpectedTotal;
                    }
                }
                if (thisRenterBasicContract.Count > 0)
                {
                    thisRenterBasicContract = thisRenterBasicContract.OrderBy(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
                }
                VM.CrCasRenterContractBasicCarSerailNo = id;
                VM.all_contractBasic = thisRenterBasicContract;
                VM.all_Renters = all_renters;
                VM.all_Invoices = all_Invoices;
                VM.summition = summition;



                return PartialView("_EditpartDataTableReportCarContract", VM);
            }
            listReportCarContractVM VM2 = new listReportCarContractVM();

            return PartialView("_EditpartDataTableReportCarContract", VM2);
            //return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            listReportCarContractVM VM = new listReportCarContractVM();

            await SetPageTitleAsync(Status.Update, pageNumber);

            var listmaxDate = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasRenterContractBasicCarSerailNo == id && x.CrCasRenterContractBasicStatus != Status.Extension,
                    selectProjection: query => query.Select(x => new Date_ReportActiveContractVM
                    {
                        dates = x.CrCasRenterContractBasicExpectedStartDate,
                    }));

            if (listmaxDate?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var maxDate = listmaxDate.Max(x => x.dates)?.ToString("yyyy-MM-dd");
            //var minDate = listmaxDate.Min(x => x.dates)?.ToString("yyyy-MM-dd");

            var end = DateTime.Now.AddDays(1);
            var start = DateTime.Now.AddMonths(-1);
            if (maxDate != null)
            {
                end = DateTime.Parse(maxDate).Date.AddDays(1);
                //start = DateTime.Parse(minDate);
                start = DateTime.Parse(maxDate).Date.AddMonths(-1);

            }

            var thisRenterBasicContract = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasRenterContractBasicCarSerailNo == id
                && x.CrCasRenterContractBasicExpectedStartDate > start && x.CrCasRenterContractBasicExpectedStartDate <= end
                && x.CrCasRenterContractBasicStatus != Status.Extension
                ,
                selectProjection: query => query.Select(x => new ReportCarContractVM
                {
                    CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                    CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                    CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                    CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                    CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                    CrCasRenterContractBasicCurrentReadingMeter = x.CrCasRenterContractBasicCurrentReadingMeter,
                    CrCasRenterContractBasicActualCurrentReadingMeter = x.CrCasRenterContractBasicActualCurrentReadingMeter,
                    CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                    CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                    CrCasRenterContractBasicActualDays = x.CrCasRenterContractBasicActualDays,
                    CrCasRenterContractBasicActualTotal = x.CrCasRenterContractBasicActualTotal,
                    CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                    CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                    CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateArName,
                    CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationConcatenateEnName,
                    CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                    CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                    CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                    CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                })
                , includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                );
            List<CrMasRenterInformation> all_renters = new List<CrMasRenterInformation>();
            List<CrCasAccountInvoice> all_Invoices = new List<CrCasAccountInvoice>();
            sumitionofClass_up_VM summition = new sumitionofClass_up_VM();
            foreach (var single in thisRenterBasicContract)
            {
                var ThisInvoice = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountInvoiceReferenceContract == single.CrCasRenterContractBasicNo,
                selectProjection: query => query.Select(x => new CrCasAccountInvoice
                {
                    CrCasAccountInvoiceReferenceContract = x.CrCasAccountInvoiceReferenceContract,
                    CrCasAccountInvoicePdfFile = x.CrCasAccountInvoicePdfFile,
                    CrCasAccountInvoiceUserCode = x.CrCasAccountInvoiceUserCode,
                })
                //,includes: new string[] { "" } 
                );
                if (ThisInvoice.Count > 0)
                {
                    all_Invoices.Add(ThisInvoice.FirstOrDefault());
                }
                var ThisRenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x => x.CrMasRenterInformationId == single.CrCasRenterContractBasicRenterId,
                    selectProjection: query => query.Select(x => new CrMasRenterInformation
                    {
                        CrMasRenterInformationId = x.CrMasRenterInformationId,
                        CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                        CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                    })
                    //,includes: new string[] { "" } 
                    );
                if (ThisRenterData.Count > 0)
                {
                    all_renters.Add(ThisRenterData.FirstOrDefault());
                }
                if (single.CrCasRenterContractBasicStatus == Status.Closed)
                {
                    summition.Days_Count += single.CrCasRenterContractBasicActualDays;
                    summition.km_Count += single.CrCasRenterContractBasicActualCurrentReadingMeter - single.CrCasRenterContractBasicCurrentReadingMeter;
                    summition.Contracts_Count += 1;
                    summition.contract_Values_Total += single.CrCasRenterContractBasicActualTotal;
                }
                else
                {
                    summition.Days_Count += single.CrCasRenterContractBasicExpectedRentalDays;
                    //summition.km_Count += 0;
                    summition.Contracts_Count += 1;
                    summition.contract_Values_Total += single.CrCasRenterContractBasicExpectedTotal;
                }

            }


            if (all_renters.Count == 0 || thisRenterBasicContract.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ReportCarContract");
            }
            if (thisRenterBasicContract.Count > 0)
            {
                thisRenterBasicContract = thisRenterBasicContract.OrderBy(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
            }
            VM.CrCasRenterContractBasicCarSerailNo = id;
            VM.all_contractBasic = thisRenterBasicContract;
            VM.all_Renters = all_renters;
            VM.all_Invoices = all_Invoices;
            VM.summition = summition;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");
            return View(VM);
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
            return RedirectToAction("Index", "ReportCarContract");
        }


    }
}
