using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Inferastructure.Repository.CAS;
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
using Bnan.Ui.ViewModels.CAS.Renters;

namespace Bnan.Ui.Areas.CAS.Controllers.Renters
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class RenterContractController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterContractController> _localizer;
        private readonly string pageNumber = SubTasks.RentersCas_RentersContracts;


        public RenterContractController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService,  IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterContractController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
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


            var allRenters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasRenterInformation
                {
                    CrMasRenterInformationId = x.CrMasRenterInformationId,
                    CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                    CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                    CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
                })
                //,includes: new string[] { "CrMasRenterInformationNationalityNavigation", "CrMasRenterInformationProfessionNavigation" } 
                );

            var allCasRenterLessor = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
                predicate: x=>x.CrCasRenterLessorCode== user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus!=Status.Deleted,
                selectProjection: query => query.Select(x => new CrCasRenterLessor
                {
                    CrCasRenterLessorId = x.CrCasRenterLessorId,
                    CrCasRenterLessorDateFirstInteraction = x.CrCasRenterLessorDateFirstInteraction,
                    CrCasRenterLessorDateLastContractual = x.CrCasRenterLessorDateLastContractual,
                    CrCasRenterLessorContractCount = x.CrCasRenterLessorContractCount,
                    CrCasRenterLessorContractDays = x.CrCasRenterLessorContractDays,
                    CrCasRenterLessorContractKm = x.CrCasRenterLessorContractKm,
                    CrCasRenterLessorStatus = x.CrCasRenterLessorStatus,
                })
                );
            var allCasRenterIds = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
                selectProjection: query => query.Select(x => new CrCasRenterLessor
                {
                    CrCasRenterLessorId = x.CrCasRenterLessorId,
                })
                );
            allCasRenterIds = allCasRenterIds?.DistinctBy(x => x.CrCasRenterLessorId)?.ToList();
            // If no active licenses, retrieve all licenses
            if (!allRenters.Any())
            {
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            if (allCasRenterIds?.Count == 0 || allRenters.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            RenterContractVM VM = new RenterContractVM();
            VM.all_Renters = allRenters;
            VM.allCasRenterLessor = allCasRenterLessor;
            VM.allCasRenterIds = allCasRenterIds;


            return View(VM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            listRenterContractVM VM = new listRenterContractVM();

            await SetPageTitleAsync(Status.Update, pageNumber);
            var thisRenterBasicContract = await _unitOfWork.CrCasRenterContractBasic.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasRenterContractBasicRenterId == id && x.CrCasRenterContractBasicLessor == user.CrMasUserInformationLessor
                && (x.CrCasRenterContractBasicStatus == Status.Active || x.CrCasRenterContractBasicStatus == Status.Closed || x.CrCasRenterContractBasicStatus == Status.Expire)
                ,
                selectProjection: query => query.Select(x => new RenterContractVM
                {
                    CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                    CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                    CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                    CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                    CrCasRenterContractBasicBranch = x.CrCasRenterContractBasicBranch,
                    CrCasRenterContractBasicCarSerailNo = x.CrCasRenterContractBasicCarSerailNo,
                    CrCasRenterContractBasicCurrentReadingMeter = x.CrCasRenterContractBasicCurrentReadingMeter,
                    CrCasRenterContractBasicActualCurrentReadingMeter = x.CrCasRenterContractBasicActualCurrentReadingMeter,
                    CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                    CrCasRenterContractBasicActualCloseDateTime = x.CrCasRenterContractBasicActualCloseDateTime,
                    CrCasRenterContractBasicActualDays = x.CrCasRenterContractBasicActualDays,
                    CrCasRenterContractBasicActualTotal = x.CrCasRenterContractBasicActualTotal,
                    CrCasRenterContractBasicPdfFile = x.CrCasRenterContractBasicPdfFile,
                    CrCasRenterContractBasicPdfTga = x.CrCasRenterContractBasicPdfTga,
                    CarArName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationPlateArNo,
                    CarEnName = x.CrCasRenterContractBasicCarSerailNoNavigation.CrCasCarInformationPlateEnNo,
                    CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                    CrCasRenterContractBasicExpectedTotal = x.CrCasRenterContractBasicExpectedTotal,
                    CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                    CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                })
                , includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                ); ;
            var ThisRenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasRenterInformationId == id,
                selectProjection: query => query.Select(x => new CrMasRenterInformation
                {
                    CrMasRenterInformationId = x.CrMasRenterInformationId,
                    CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                    CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                })
                //,includes: new string[] { "" } 
                );

            var all_Branches = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: null,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasBranchInformationCode,
                    nameAr = x.CrCasBranchInformationArShortName,
                    nameEn = x.CrCasBranchInformationEnShortName,
                })
                );
            var all_Invoices = await _unitOfWork.CrCasAccountInvoice.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountInvoiceLessorCode == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasAccountInvoiceReferenceContract,
                    nameAr = x.CrCasAccountInvoicePdfFile,
                    str4 = x.CrCasAccountInvoiceUserCode,
                })
                //,includes: new string[] { "" } 
                );


            if (ThisRenterData.Count == 0 || thisRenterBasicContract.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterContract");
            }
            VM.CrMasRenterInformationId = id;
            VM.all_Branches = all_Branches;
            VM.all_Invoices = all_Invoices;
            VM.all_contractBasic = thisRenterBasicContract;
            VM.thisRenterData = ThisRenterData?.FirstOrDefault();

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
            return RedirectToAction("Index", "RenterContract");
        }


    }
}
