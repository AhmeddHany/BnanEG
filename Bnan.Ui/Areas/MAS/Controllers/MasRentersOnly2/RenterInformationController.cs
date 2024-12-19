using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Inferastructure.Repository.MAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.MasRentersOnly2
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class RenterInformationController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterInformationController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasRenterInformation;


        public RenterInformationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterInformationController> localizer) : base(userManager, unitOfWork, mapper)
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


            var allRenters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrMasRenterInformationStatus == Status.Active,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasRenterInformation
                {
                    CrMasRenterInformationId = x.CrMasRenterInformationId,
                    CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                    CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                    CrMasRenterInformationCountreyKey = x.CrMasRenterInformationCountreyKey,
                    CrMasRenterInformationMobile = x.CrMasRenterInformationMobile,
                    CrMasRenterInformationEmail = x.CrMasRenterInformationEmail,
                    CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
                    CrMasRenterInformationProfession = x.CrMasRenterInformationProfession,
                    CrMasRenterInformationNationality = x.CrMasRenterInformationNationality,
                })
                //,includes: new string[] { "CrMasRenterInformationNationalityNavigation", "CrMasRenterInformationProfessionNavigation" } 
                );

            var allPosts = await _unitOfWork.CrMasRenterPost.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasRenterPost
                {
                    CrMasRenterPostCode = x.CrMasRenterPostCode,
                    CrMasRenterPostArShortConcatenate = x.CrMasRenterPostArShortConcatenate,
                    CrMasRenterPostEnShortConcatenate = x.CrMasRenterPostEnShortConcatenate,
                    CrMasRenterPostStatus = x.CrMasRenterPostStatus,
                })
                );
            var allNationalities = await _unitOfWork.CrMasSupRenterNationality.FindAllWithSelectAsNoTrackingAsync(
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSupRenterNationality
                {
                    CrMasSupRenterNationalitiesCode = x.CrMasSupRenterNationalitiesCode,
                    CrMasSupRenterNationalitiesArName = x.CrMasSupRenterNationalitiesArName,
                    CrMasSupRenterNationalitiesEnName = x.CrMasSupRenterNationalitiesEnName,
                })
                );
            var allProfessions = await _unitOfWork.CrMasSupRenterProfession.FindAllWithSelectAsNoTrackingAsync(
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSupRenterProfession
                {
                    CrMasSupRenterProfessionsCode = x.CrMasSupRenterProfessionsCode,
                    CrMasSupRenterProfessionsArName = x.CrMasSupRenterProfessionsArName,
                    CrMasSupRenterProfessionsEnName = x.CrMasSupRenterProfessionsEnName,
                })
                );
            // If no active licenses, retrieve all licenses
            if (!allRenters.Any())
            {
                allRenters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: x => x.CrMasRenterInformationStatus == Status.Hold,
                    selectProjection: query => query.Select(x => new CrMasRenterInformation
                    {
                        CrMasRenterInformationId = x.CrMasRenterInformationId,
                        CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                        CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                        CrMasRenterInformationCountreyKey = x.CrMasRenterInformationCountreyKey,
                        CrMasRenterInformationMobile = x.CrMasRenterInformationMobile,
                        CrMasRenterInformationEmail = x.CrMasRenterInformationEmail,
                        CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
                    })
                    //, includes: new string[] { "CrMasRenterInformationNationalityNavigation", "CrMasRenterInformationProfessionNavigation" }
                    );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            if (allPosts.Count == 0 || allRenters.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            RenterInformationVM VM = new RenterInformationVM();
            VM.all_Renters = allRenters;
            VM.all_post = allPosts;
            VM.all_Professions = allProfessions;
            VM.all_Nationalities = allNationalities;

            return View(VM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            RenterInformationVM VM = new RenterInformationVM();

            await SetPageTitleAsync(Status.Update, pageNumber);
            var ThisRenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasRenterInformationId == id,
                selectProjection: query => query.Select(x => new RenterInformationVM
                {
                    CrMasRenterInformationId = x.CrMasRenterInformationId,
                    CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                    CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                    CrMasRenterInformationCountreyKey = x.CrMasRenterInformationCountreyKey,
                    CrMasRenterInformationMobile = x.CrMasRenterInformationMobile,
                    CrMasRenterInformationEmail = x.CrMasRenterInformationEmail,
                    CrMasRenterInformationStatus = x.CrMasRenterInformationStatus,
                    CrMasRenterInformationIban = x.CrMasRenterInformationIban,
                    CrMasRenterInformationExpiryDrivingLicenseDate = x.CrMasRenterInformationExpiryDrivingLicenseDate,
                    CrMasRenterInformationExpiryIdDate = x.CrMasRenterInformationExpiryIdDate,
                    CrMasRenterInformationBirthDate = x.CrMasRenterInformationBirthDate,
                    CrMasRenterInformationRenterIdImage = x.CrMasRenterInformationRenterIdImage,
                    CrMasRenterInformationRenterLicenseImage = x.CrMasRenterInformationRenterLicenseImage,
                    CrMasRenterInformationSignature = x.CrMasRenterInformationSignature,
                    CrMasRenterInformationDrivingLicenseNo = x.CrMasRenterInformationDrivingLicenseNo,
                    ProffessionArName = x.CrMasRenterInformationProfessionNavigation.CrMasSupRenterProfessionsArName,
                    ProffessionEnName = x.CrMasRenterInformationProfessionNavigation.CrMasSupRenterProfessionsEnName,
                    NationalityArName = x.CrMasRenterInformationNationalityNavigation.CrMasSupRenterNationalitiesArName,
                    NationalityEnName = x.CrMasRenterInformationNationalityNavigation.CrMasSupRenterNationalitiesEnName,
                    BankArName = x.CrMasRenterInformationBankNavigation.CrMasSupAccountBankArName,
                    BankEnName = x.CrMasRenterInformationBankNavigation.CrMasSupAccountBankEnName,
                    DrivingLicesnseArName = x.CrMasRenterInformationDrivingLicenseTypeNavigation.CrMasSupRenterDrivingLicenseArName,
                    DrivingLicesnseEnName = x.CrMasRenterInformationDrivingLicenseTypeNavigation.CrMasSupRenterDrivingLicenseEnName,
                    WorkPlaceArName = x.CrMasRenterInformationEmployerNavigation.CrMasSupRenterEmployerArName,
                    WorkPlaceEnName = x.CrMasRenterInformationEmployerNavigation.CrMasSupRenterEmployerEnName,
                    GenderArName = x.CrMasRenterInformationGenderNavigation.CrMasSupRenterGenderArName,
                    GenderEnName = x.CrMasRenterInformationGenderNavigation.CrMasSupRenterGenderEnName,
                    IDtypeArName = x.CrMasRenterInformationIdtypeNavigation.CrMasSupRenterIdtypeArName,
                    IDtypeEnName = x.CrMasRenterInformationIdtypeNavigation.CrMasSupRenterIdtypeEnName,
                    addressArName = x.CrMasRenterPost.CrMasRenterPostArShortConcatenate,
                    addressEnName = x.CrMasRenterPost.CrMasRenterPostEnShortConcatenate,
                })
                , includes: new string[] { "CrMasRenterInformationNationalityNavigation",
                    "CrMasRenterInformationProfessionNavigation", "CrMasRenterInformationBankNavigation",
                    "CrMasRenterInformationDrivingLicenseTypeNavigation",
                    "CrMasRenterInformationEmployerNavigation","CrMasRenterInformationGenderNavigation","CrMasRenterInformationIdtypeNavigation","CrMasRenterPost" }
                );
            if (ThisRenterData.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterInformation");
            }
            VM = ThisRenterData?.FirstOrDefault();
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
            return RedirectToAction("Index", "RenterInformation");
        }


    }
}
