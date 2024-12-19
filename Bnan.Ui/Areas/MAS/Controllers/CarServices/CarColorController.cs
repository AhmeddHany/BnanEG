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
namespace Bnan.Ui.Areas.MAS.Controllers.CarServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class CarColorController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarColor _masCarColor;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarColorController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarColor;


        public CarColorController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarColor masCarColor, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarColorController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarColor = masCarColor;
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
            // Retrieve active driving licenses
            var carColors = await _unitOfWork.CrMasSupCarColor
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarColorStatus == Status.Active);

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarColor>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationMainColor  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );


            // If no active licenses, retrieve all licenses
            if (!carColors.Any())
            {
                carColors = await _unitOfWork.CrMasSupCarColor
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarColorStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarColorVM vm = new CarColorVM();
            vm.crMasSupCarColor = carColors;
            vm.cars_count = Cars_Count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarColorByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CarColorsAll = await _unitOfWork.CrMasSupCarColor.FindAllAsNoTrackingAsync(x => x.CrMasSupCarColorStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarColorStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarColorStatus == Status.Hold);
                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarColor>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationMainColor  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                CarColorVM vm = new CarColorVM();
                vm.cars_count = Cars_Count;
                if (status == Status.All)
                {
                    var FilterAll = CarColorsAll.FindAll(x => x.CrMasSupCarColorStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarColorArName.Contains(search) ||
                                                                          x.CrMasSupCarColorEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarColorCode.Contains(search)));
                    vm.crMasSupCarColor = FilterAll;
                    return PartialView("_DataTableCarColor", vm);
                }
                var FilterByStatus = CarColorsAll.FindAll(x => x.CrMasSupCarColorStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarColorArName.Contains(search) ||
                                                                           x.CrMasSupCarColorEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarColorCode.Contains(search)));
                vm.crMasSupCarColor = FilterByStatus;
                return PartialView("_DataTableCarColor", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarColor()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarColor");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarColor");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarColor");
            }
            // Set Title 
            CarColorVM carColorVM = new CarColorVM();
            carColorVM.CrMasSupCarColorCode = await GenerateLicenseCodeAsync();
            return View(carColorVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarColor(CarColorVM carColorVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || carColorVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarColor", carColorVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var carColorEntity = _mapper.Map<CrMasSupCarColor>(carColorVM);

                // Check if the entity already exists
                if (await _masCarColor.ExistsByDetailsAsync(carColorEntity))
                {
                    await AddModelErrorsAsync(carColorEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarColor", carColorVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarColor", carColorVM);
                }
                // Generate and set the Driving License Code
                carColorVM.CrMasSupCarColorCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                carColorEntity.CrMasSupCarColorStatus = "A";
                carColorEntity.CrMasSupCarColorCounter = 0;
                await _unitOfWork.CrMasSupCarColor.AddAsync(carColorEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, carColorEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarColor", carColorVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarColor.FindAsync(x => x.CrMasSupCarColorCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarColor");
            }
            var model = _mapper.Map<CarColorVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarColorVM carColorVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && carColorVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarColor");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", carColorVM);
                }
                var carColorEntity = _mapper.Map<CrMasSupCarColor>(carColorVM);

                // Check if the entity already exists
                if (await _masCarColor.ExistsByDetailsAsync(carColorEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(carColorEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", carColorVM);
                }

                _unitOfWork.CrMasSupCarColor.Update(carColorEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, carColorEntity, Status.Update);
                return RedirectToAction("Index", "CarColor");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", carColorVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarColor.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarColor.CheckIfCanDeleteIt(licence.CrMasSupCarColorCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarColorStatus = status;
                _unitOfWork.CrMasSupCarColor.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarColor entity)
        {

            if (await _masCarColor.ExistsByArabicNameAsync(entity.CrMasSupCarColorArName, entity.CrMasSupCarColorCode))
            {
                ModelState.AddModelError("CrMasSupCarColorArName", _localizer["Existing"]);
            }

            if (await _masCarColor.ExistsByEnglishNameAsync(entity.CrMasSupCarColorEnName, entity.CrMasSupCarColorCode))
            {
                ModelState.AddModelError("CrMasSupCarColorEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarColors = await _unitOfWork.CrMasSupCarColor.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarColors != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarColorArName" && All_CarColors.Any(x => x.CrMasSupCarColorArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarColorArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarColorEnName" && All_CarColors.Any(x => x.CrMasSupCarColorEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarColorEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarColor.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarColorCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarColor licence, string status)
        {


            var recordAr = licence.CrMasSupCarColorArName;
            var recordEn = licence.CrMasSupCarColorEnName;
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
            return RedirectToAction("Index", "CarColor");
        }


    }
}
