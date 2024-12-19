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
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Numerics;
using static Pipelines.Sockets.Unofficial.SocketConnection;

namespace Bnan.Ui.Areas.MAS.Controllers.Services
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class CountryClassificationController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCountryClassification _masCountryClassification;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CountryClassificationController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCountryClassification;


        public CountryClassificationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCountryClassification masCountryClassification, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CountryClassificationController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCountryClassification = masCountryClassification;
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
            var country_count = await _unitOfWork.CrMasSysCallingKeys.FindCountByColumnAsync<CrMasSysCallingKey>(
                predicate: x => x.CrMasSysCallingKeysStatus == Status.Active,
                columnSelector: x => x.CrMasSysCallingKeysClassificationCode  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            // Retrieve active driving licenses
            var countryClassifications = await _unitOfWork.CrMasSupCountryClassification
                .FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active);

            // If no active licenses, retrieve all licenses
            if (!countryClassifications.Any())
            {
                countryClassifications = await _unitOfWork.CrMasSupCountryClassification
                    .FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Hold
                                             );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CountryClassificationVM vm = new CountryClassificationVM();
            vm.Countries = countryClassifications;
            vm.Country_count = country_count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCountryClassificationByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CountryClassificationsAll = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active ||
                                                                                                                            x.CrMasLessorCountryClassificationStatus == Status.Deleted ||
                                                                                                                            x.CrMasLessorCountryClassificationStatus == Status.Hold);

                var country_count = await _unitOfWork.CrMasSysCallingKeys.FindCountByColumnAsync<CrMasSysCallingKey>(
                    predicate: x => x.CrMasSysCallingKeysStatus == Status.Active,
                    columnSelector: x => x.CrMasSysCallingKeysClassificationCode  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                CountryClassificationVM vm = new CountryClassificationVM();
                vm.Country_count = country_count;
                if (status == Status.All)
                {
                    var FilterAll = CountryClassificationsAll.FindAll(x => x.CrMasLessorCountryClassificationStatus != Status.Deleted &&
                                                                         (x.CrMasLessorCountryClassificationArName.Contains(search) ||
                                                                          x.CrMasLessorCountryClassificationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasLessorCountryClassificationCode.Contains(search)));
                    vm.Countries = FilterAll;
                    return PartialView("_DataTableCountryClassification", vm);
                }
                var FilterByStatus = CountryClassificationsAll.FindAll(x => x.CrMasLessorCountryClassificationStatus == status &&
                                                                            (
                                                                           x.CrMasLessorCountryClassificationArName.Contains(search) ||
                                                                           x.CrMasLessorCountryClassificationEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasLessorCountryClassificationCode.Contains(search)));
                vm.Countries = FilterByStatus;
                return PartialView("_DataTableCountryClassification", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCountryClassification()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CountryClassification");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CountryClassification");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CountryClassification");
            }
            // Set Title 
            CountryClassificationVM countryClassificationVM = new CountryClassificationVM();
            countryClassificationVM.CrMasLessorCountryClassificationCode = await GenerateLicenseCodeAsync();
            return View(countryClassificationVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCountryClassification(CountryClassificationVM countryClassificationVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || countryClassificationVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCountryClassification", countryClassificationVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var countryClassificationEntity = _mapper.Map<CrMasSupCountryClassification>(countryClassificationVM);

                // Check if the entity already exists
                if (await _masCountryClassification.ExistsByDetailsAsync(countryClassificationEntity))
                {
                    await AddModelErrorsAsync(countryClassificationEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCountryClassification", countryClassificationVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCountryClassification", countryClassificationVM);
                }
                // Generate and set the Driving License Code
                countryClassificationVM.CrMasLessorCountryClassificationCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                countryClassificationEntity.CrMasLessorCountryClassificationStatus = "A";
                await _unitOfWork.CrMasSupCountryClassification.AddAsync(countryClassificationEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, countryClassificationEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCountryClassification", countryClassificationVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasSupCountryClassification.FindAsync(x => x.CrMasLessorCountryClassificationCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CountryClassification");
            }
            var model = _mapper.Map<CountryClassificationVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CountryClassificationVM countryClassificationVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && countryClassificationVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CountryClassification");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", countryClassificationVM);
                }
                var countryClassificationEntity = _mapper.Map<CrMasSupCountryClassification>(countryClassificationVM);
                // Check if the entity already exists
                if (await _masCountryClassification.ExistsByDetailsAsync(countryClassificationEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(countryClassificationEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", countryClassificationVM);
                }

                _unitOfWork.CrMasSupCountryClassification.Update(countryClassificationEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, countryClassificationEntity, Status.Update);
                return RedirectToAction("Index", "CountryClassification");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", countryClassificationVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCountryClassification.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCountryClassification.CheckIfCanDeleteIt(licence.CrMasLessorCountryClassificationCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasLessorCountryClassificationStatus = status;
                _unitOfWork.CrMasSupCountryClassification.Update(licence);
                _unitOfWork.Complete();
                await SaveTracingForLicenseChange(user, licence, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrMasSupCountryClassification entity)
        {

            if (await _masCountryClassification.ExistsByArabicNameAsync(entity.CrMasLessorCountryClassificationArName, entity.CrMasLessorCountryClassificationCode))
            {
                ModelState.AddModelError("CrMasLessorCountryClassificationArName", _localizer["Existing"]);
            }

            if (await _masCountryClassification.ExistsByEnglishNameAsync(entity.CrMasLessorCountryClassificationEnName, entity.CrMasLessorCountryClassificationCode))
            {
                ModelState.AddModelError("CrMasLessorCountryClassificationEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CountryClassifications = await _unitOfWork.CrMasSupCountryClassification.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CountryClassifications != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasLessorCountryClassificationArName" && All_CountryClassifications.Any(x => x.CrMasLessorCountryClassificationArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasLessorCountryClassificationArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasLessorCountryClassificationEnName" && All_CountryClassifications.Any(x => x.CrMasLessorCountryClassificationEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasLessorCountryClassificationEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCountryClassification.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasLessorCountryClassificationCode) + 1).ToString() : "1";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCountryClassification licence, string status)
        {


            var recordAr = licence.CrMasLessorCountryClassificationArName;
            var recordEn = licence.CrMasLessorCountryClassificationEnName;
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
            return RedirectToAction("Index", "CountryClassification");
        }


    }
}
