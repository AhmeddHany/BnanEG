using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
//using Bnan.Inferastructure.Repository.CAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public class ReportEmployeeContract_casController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportEmployeeContract_casController> _localizer;
        private readonly string pageNumber = SubTasks.Employers_contracts_Report_Cas;


        public ReportEmployeeContract_casController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportEmployeeContract_casController> localizer) : base(userManager, unitOfWork, mapper)
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

            var all_users_hasContracts = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x =>
                   x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                   && x.CrCasRenterContractBasicUserInsert != null
                   && x.CrCasRenterContractBasicStatus != Status.Extension
                    ,
                    selectProjection: query => query.Select(x => new cas_list_String_2
                    {
                        id_key = x.CrCasRenterContractBasicUserInsert,
                        str2 = x.CrCasRenterContractBasicNo,
                    })
                    //, includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                    );

            all_users_hasContracts = all_users_hasContracts.DistinctBy(x => x.id_key).ToList();

            var all_EmployeeContracts = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x =>
                   x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor
                   && (x.CrCasRenterContractStatisticsUserClose != null || x.CrCasRenterContractStatisticsUserOpen != null)
                    ,
                    selectProjection: query => query.Select(x => new EmployeeContract_CAS_VM
                    {
                        CrCasRenterContractStatisticsNo = x.CrCasRenterContractStatisticsNo,
                        CrCasRenterContractStatisticsUserOpen = x.CrCasRenterContractStatisticsUserOpen,
                        CrCasRenterContractStatisticsUserClose = x.CrCasRenterContractStatisticsUserClose,
                        CrCasRenterContractStatisticsDate = x.CrCasRenterContractStatisticsDate,
                        CrCasRenterContractStatisicsDays = x.CrCasRenterContractStatisicsDays,
                        CrCasRenterContractStatisticsExpensesValue = x.CrCasRenterContractStatisticsExpensesValue,
                        CrCasRenterContractStatisticsCompensationValue = x.CrCasRenterContractStatisticsCompensationValue,
                        CrCasRenterContractStatisticsRentValue = x.CrCasRenterContractStatisticsRentValue,

                    })
                    //, includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                    );


            var all_UserData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasUserInformationLessor == user.CrMasUserInformationLessor && x.CrMasUserInformationStatus != Status.Deleted,
                selectProjection: query => query.Select(x => new UserInfo_EmployeeContract_CAS_VM
                {
                    CrMasUserInformationCode = x.CrMasUserInformationCode,
                    CrMasUserInformationArName = x.CrMasUserInformationArName,
                    CrMasUserInformationEnName = x.CrMasUserInformationEnName,
                    CrMasUserInformationOperationStatus = x.CrMasUserInformationOperationStatus,
                    CrMasUserInformationStatus = x.CrMasUserInformationStatus,
                    CrMasUserInformationPicture = x.CrMasUserInformationPicture,
                })
                //,includes: new string[] { "" } 
                );

            all_UserData = all_UserData.Where(x => all_users_hasContracts.Any(y => y.id_key == x.CrMasUserInformationCode)).ToList();

            foreach (var single in all_UserData)
            {
                var list_Contracts = all_EmployeeContracts?.Where(y => y.CrCasRenterContractStatisticsUserOpen?.Trim() == single.CrMasUserInformationCode)?.ToList();
                var closed_Contracts = all_EmployeeContracts?.Where(y => y.CrCasRenterContractStatisticsUserClose?.Trim() == single.CrMasUserInformationCode)?.ToList();
                if (list_Contracts != null)
                {
                    single.LastDate = list_Contracts.Max(k => k.CrCasRenterContractStatisticsDate)?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "";
                    single.RentValue = ((list_Contracts.Sum(c => c.CrCasRenterContractStatisticsRentValue) ?? 0.0m) + (list_Contracts.Sum(c => c.CrCasRenterContractStatisticsCompensationValue) ?? 0.0m) - (list_Contracts.Sum(c => c.CrCasRenterContractStatisticsExpensesValue) ?? 0.0m)).ToString("N2", CultureInfo.InvariantCulture) ?? "0.00";
                    single.open = list_Contracts.Count();
                    single.DaysCount = list_Contracts.Sum(c => c.CrCasRenterContractStatisicsDays) ?? 0;
                }
                if (closed_Contracts != null)
                {
                    single.close = closed_Contracts.Count();
                }
            }

            if (all_EmployeeContracts?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            ViewBag.radio = "A";


            listReportEmployeeContract_CAS_VM VM = new listReportEmployeeContract_CAS_VM();
            VM.all_EmployeeContracts = all_EmployeeContracts;
            VM.all_UserData = all_UserData;
            return View(VM);
        }

        [HttpGet]
        //[Route("/CAS/ReportEmployeeContract/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string id, string start, string end)
        {
            var user = await _userManager.GetUserAsync(User);
            listReportEmployeeContract_CAS_VM VM = new listReportEmployeeContract_CAS_VM();

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
                    predicate: x => x.CrCasRenterContractBasicUserInsert == id
                    && x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                   && x.CrCasRenterContractBasicExpectedStartDate > start_Date && x.CrCasRenterContractBasicExpectedStartDate <= end_Date
                   && x.CrCasRenterContractBasicStatus != Status.Extension
                    ,
                    selectProjection: query => query.Select(x => new ReportEmployeeContract_CAS_VM
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
                        CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                        CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                        CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                        CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                        CrCasRenterContractBasicUserInsert = x.CrCasRenterContractBasicUserInsert,
                    })
                    //, includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                    );


                List<CrMasRenterInformation> all_renters = new List<CrMasRenterInformation>();
                List<CrCasAccountInvoice> all_Invoices = new List<CrCasAccountInvoice>();
                sumitionofClass_Employee_up_VM summition = new sumitionofClass_Employee_up_VM();

                foreach (var single in thisRenterBasicContract)
                {
                    var ThisInvoice = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x => x.CrCasAccountInvoiceReferenceContract == single.CrCasRenterContractBasicNo
                    && x.CrCasAccountInvoiceLessorCode == user.CrMasUserInformationLessor
                    ,
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
                VM.UserInsert = id;
                VM.all_contractBasic = thisRenterBasicContract;
                VM.all_Renters = all_renters;
                VM.all_Invoices = all_Invoices;
                VM.summition = summition;



                return PartialView("_EditpartDataTableReportEmployeeContract_cas", VM);
            }
            listReportEmployeeContract_CAS_VM VM2 = new listReportEmployeeContract_CAS_VM();

            return PartialView("_EditpartDataTableReportEmployeeContract_cas", VM2);
            //return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            listReportEmployeeContract_CAS_VM VM = new listReportEmployeeContract_CAS_VM();

            await SetPageTitleAsync(Status.Update, pageNumber);

            var listmaxDate = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasRenterContractBasicUserInsert == id && x.CrCasRenterContractBasicStatus != Status.Extension
                    && x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                    ,
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
                predicate: x => x.CrCasRenterContractBasicUserInsert == id
                && x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                && x.CrCasRenterContractBasicExpectedStartDate > start && x.CrCasRenterContractBasicExpectedStartDate <= end
                && x.CrCasRenterContractBasicStatus != Status.Extension
                ,
                selectProjection: query => query.Select(x => new ReportEmployeeContract_CAS_VM
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
                    CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                    CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                    CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                    CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                })
                //, includes: new string[] { }
                );

            var ThisUserData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasUserInformationCode == id.Trim(),
                selectProjection: query => query.Select(x => new UserInfo_EmployeeContract_CAS_VM
                {
                    CrMasUserInformationCode = x.CrMasUserInformationCode,
                    CrMasUserInformationArName = x.CrMasUserInformationArName,
                    CrMasUserInformationEnName = x.CrMasUserInformationEnName,
                    CrMasUserInformationOperationStatus = x.CrMasUserInformationOperationStatus,
                    CrMasUserInformationStatus = x.CrMasUserInformationStatus,
                    CrMasUserInformationPicture = x.CrMasUserInformationPicture,
                })
                //,includes: new string[] { "" } 
                );
            List<CrMasRenterInformation> all_renters = new List<CrMasRenterInformation>();
            List<CrCasAccountInvoice> all_Invoices = new List<CrCasAccountInvoice>();
            sumitionofClass_Employee_up_VM summition = new sumitionofClass_Employee_up_VM();
            foreach (var single in thisRenterBasicContract)
            {
                var ThisInvoice = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountInvoiceReferenceContract == single.CrCasRenterContractBasicNo
                && x.CrCasAccountInvoiceLessorCode == user.CrMasUserInformationLessor
                ,
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
                return RedirectToAction("Index", "ReportEmployeeContract_cas");
            }
            if (thisRenterBasicContract.Count > 0)
            {
                thisRenterBasicContract = thisRenterBasicContract.OrderBy(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
            }
            VM.UserInsert = id;
            VM.all_contractBasic = thisRenterBasicContract;
            VM.all_Renters = all_renters;
            VM.all_Invoices = all_Invoices;
            VM.summition = summition;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");
            VM.ThisUserData = ThisUserData;
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
            return RedirectToAction("Index", "ReportEmployeeContract_cas");
        }


    }
}
