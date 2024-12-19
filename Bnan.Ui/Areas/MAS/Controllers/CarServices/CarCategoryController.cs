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
    public class CarCategoryController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarCategory _masCarCategory;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarCategoryController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarCategory;


        public CarCategoryController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarCategory masCarCategory, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarCategoryController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarCategory = masCarCategory;
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
            var carCategorys = await _unitOfWork.CrMasSupCarCategory
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarCategoryStatus == Status.Active);

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarCategory>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationCategory  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );


            // If no active licenses, retrieve all licenses
            if (!carCategorys.Any())
            {
                carCategorys = await _unitOfWork.CrMasSupCarCategory
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarCategoryStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarCategoryVM vm = new CarCategoryVM();
            vm.crMasSupCarCategory = carCategorys;
            vm.cars_count = Cars_Count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarCategoryByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CarCategorysAll = await _unitOfWork.CrMasSupCarCategory.FindAllAsNoTrackingAsync(x => x.CrMasSupCarCategoryStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarCategoryStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarCategoryStatus == Status.Hold);
                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarCategory>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationCategory  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                CarCategoryVM vm = new CarCategoryVM();
                vm.cars_count = Cars_Count;
                if (status == Status.All)
                {
                    var FilterAll = CarCategorysAll.FindAll(x => x.CrMasSupCarCategoryStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarCategoryArName.Contains(search) ||
                                                                          x.CrMasSupCarCategoryEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarCategoryCode.Contains(search)));
                    vm.crMasSupCarCategory = FilterAll;
                    return PartialView("_DataTableCarCategory", vm);
                }
                var FilterByStatus = CarCategorysAll.FindAll(x => x.CrMasSupCarCategoryStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarCategoryArName.Contains(search) ||
                                                                           x.CrMasSupCarCategoryEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarCategoryCode.Contains(search)));
                vm.crMasSupCarCategory = FilterByStatus;
                return PartialView("_DataTableCarCategory", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarCategory()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarCategory");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarCategory");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 3499999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarCategory");
            }
            // Set Title 
            CarCategoryVM carCategoryVM = new CarCategoryVM();
            carCategoryVM.CrMasSupCarCategoryCode = await GenerateLicenseCodeAsync();
            return View(carCategoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarCategory(CarCategoryVM carCategoryVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || carCategoryVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarCategory", carCategoryVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var carCategoryEntity = _mapper.Map<CrMasSupCarCategory>(carCategoryVM);

                // Check if the entity already exists
                if (await _masCarCategory.ExistsByDetailsAsync(carCategoryEntity))
                {
                    await AddModelErrorsAsync(carCategoryEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarCategory", carCategoryVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 3499999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarCategory", carCategoryVM);
                }
                // Generate and set the Driving License Code
                carCategoryVM.CrMasSupCarCategoryCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                carCategoryEntity.CrMasSupCarCategoryStatus = "A";
                await _unitOfWork.CrMasSupCarCategory.AddAsync(carCategoryEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, carCategoryEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarCategory", carCategoryVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarCategory.FindAsync(x => x.CrMasSupCarCategoryCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarCategory");
            }
            var model = _mapper.Map<CarCategoryVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarCategoryVM carCategoryVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && carCategoryVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarCategory");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", carCategoryVM);
                }
                var carCategoryEntity = _mapper.Map<CrMasSupCarCategory>(carCategoryVM);

                // Check if the entity already exists
                if (await _masCarCategory.ExistsByDetailsAsync(carCategoryEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(carCategoryEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", carCategoryVM);
                }

                _unitOfWork.CrMasSupCarCategory.Update(carCategoryEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, carCategoryEntity, Status.Update);
                return RedirectToAction("Index", "CarCategory");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", carCategoryVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarCategory.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarCategory.CheckIfCanDeleteIt(licence.CrMasSupCarCategoryCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarCategoryStatus = status;
                _unitOfWork.CrMasSupCarCategory.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarCategory entity)
        {

            if (await _masCarCategory.ExistsByArabicNameAsync(entity.CrMasSupCarCategoryArName, entity.CrMasSupCarCategoryCode))
            {
                ModelState.AddModelError("CrMasSupCarCategoryArName", _localizer["Existing"]);
            }

            if (await _masCarCategory.ExistsByEnglishNameAsync(entity.CrMasSupCarCategoryEnName, entity.CrMasSupCarCategoryCode))
            {
                ModelState.AddModelError("CrMasSupCarCategoryEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarCategorys = await _unitOfWork.CrMasSupCarCategory.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarCategorys != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarCategoryArName" && All_CarCategorys.Any(x => x.CrMasSupCarCategoryArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarCategoryArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarCategoryEnName" && All_CarCategorys.Any(x => x.CrMasSupCarCategoryEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarCategoryEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarCategory.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarCategoryCode) + 1).ToString() : "3400000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarCategory licence, string status)
        {


            var recordAr = licence.CrMasSupCarCategoryArName;
            var recordEn = licence.CrMasSupCarCategoryEnName;
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
            return RedirectToAction("Index", "CarCategory");
        }


    }
}
