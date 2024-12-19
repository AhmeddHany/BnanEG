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
    public class CarBrandController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarBrand _masCarBrand;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarBrandController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarBrand;


        public CarBrandController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarBrand masCarBrand, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarBrandController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarBrand = masCarBrand;
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
            var carBrands = await _unitOfWork.CrMasSupCarBrand
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active);

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarBrand>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationBrand  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            var models_Count = await _unitOfWork.CrMasSupCarModel.FindCountByColumnAsync<CrMasSupCarBrand>(
                predicate: x => x.CrMasSupCarModelStatus != Status.Deleted,
                columnSelector: x => x.CrMasSupCarModelBrand  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            // If no active licenses, retrieve all licenses
            if (!carBrands.Any())
            {
                carBrands = await _unitOfWork.CrMasSupCarBrand
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarBrandVM vm = new CarBrandVM();
            vm.crMasSupCarBrand = carBrands;
            vm.cars_count = Cars_Count;
            vm.models_count = models_Count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarBrandByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CarBrandsAll = await _unitOfWork.CrMasSupCarBrand.FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarBrandStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarBrandStatus == Status.Hold);
                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarBrand>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationBrand  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                var models_Count = await _unitOfWork.CrMasSupCarModel.FindCountByColumnAsync<CrMasSupCarBrand>(
                    predicate: x => x.CrMasSupCarModelStatus != Status.Deleted,
                    columnSelector: x => x.CrMasSupCarModelBrand  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                CarBrandVM vm = new CarBrandVM();
                vm.cars_count = Cars_Count;
                vm.models_count = models_Count;
                if (status == Status.All)
                {
                    var FilterAll = CarBrandsAll.FindAll(x => x.CrMasSupCarBrandStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarBrandArName.Contains(search) ||
                                                                          x.CrMasSupCarBrandEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarBrandCode.Contains(search)));
                    vm.crMasSupCarBrand = FilterAll;
                    return PartialView("_DataTableCarBrand", vm);
                }
                var FilterByStatus = CarBrandsAll.FindAll(x => x.CrMasSupCarBrandStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarBrandArName.Contains(search) ||
                                                                           x.CrMasSupCarBrandEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarBrandCode.Contains(search)));
                vm.crMasSupCarBrand = FilterByStatus;
                return PartialView("_DataTableCarBrand", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarBrand()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarBrand");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarBrand");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 3999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarBrand");
            }
            // Set Title 
            CarBrandVM carBrandVM = new CarBrandVM();
            carBrandVM.CrMasSupCarBrandCode = await GenerateLicenseCodeAsync();
            return View(carBrandVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarBrand(CarBrandVM carBrandVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || carBrandVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarBrand", carBrandVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var carBrandEntity = _mapper.Map<CrMasSupCarBrand>(carBrandVM);

                // Check if the entity already exists
                if (await _masCarBrand.ExistsByDetailsAsync(carBrandEntity))
                {
                    await AddModelErrorsAsync(carBrandEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarBrand", carBrandVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 3999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarBrand", carBrandVM);
                }
                // Generate and set the Driving License Code
                carBrandVM.CrMasSupCarBrandCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                carBrandEntity.CrMasSupCarBrandStatus = "A";
                await _unitOfWork.CrMasSupCarBrand.AddAsync(carBrandEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, carBrandEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarBrand", carBrandVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarBrand.FindAsync(x => x.CrMasSupCarBrandCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarBrand");
            }
            var model = _mapper.Map<CarBrandVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarBrandVM carBrandVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && carBrandVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarBrand");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", carBrandVM);
                }
                var carBrandEntity = _mapper.Map<CrMasSupCarBrand>(carBrandVM);

                // Check if the entity already exists
                if (await _masCarBrand.ExistsByDetailsAsync(carBrandEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(carBrandEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", carBrandVM);
                }

                _unitOfWork.CrMasSupCarBrand.Update(carBrandEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, carBrandEntity, Status.Update);
                return RedirectToAction("Index", "CarBrand");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", carBrandVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarBrand.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarBrand.CheckIfCanDeleteIt(licence.CrMasSupCarBrandCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarBrandStatus = status;
                _unitOfWork.CrMasSupCarBrand.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarBrand entity)
        {

            if (await _masCarBrand.ExistsByArabicNameAsync(entity.CrMasSupCarBrandArName, entity.CrMasSupCarBrandCode))
            {
                ModelState.AddModelError("CrMasSupCarBrandArName", _localizer["Existing"]);
            }

            if (await _masCarBrand.ExistsByEnglishNameAsync(entity.CrMasSupCarBrandEnName, entity.CrMasSupCarBrandCode))
            {
                ModelState.AddModelError("CrMasSupCarBrandEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarBrands = await _unitOfWork.CrMasSupCarBrand.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarBrands != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarBrandArName" && All_CarBrands.Any(x => x.CrMasSupCarBrandArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarBrandArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarBrandEnName" && All_CarBrands.Any(x => x.CrMasSupCarBrandEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarBrandEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarBrand.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarBrandCode) + 1).ToString() : "3001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarBrand licence, string status)
        {


            var recordAr = licence.CrMasSupCarBrandArName;
            var recordEn = licence.CrMasSupCarBrandEnName;
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
            return RedirectToAction("Index", "CarBrand");
        }


    }
}
