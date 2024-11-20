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
namespace Bnan.Ui.Areas.MAS.Controllers
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

        public CarBrandController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarBrand masCarBrand, IBaseRepo BaseRepo,IMasBase masBase,
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

            var pageNumber = SubTasks.CrMasSupCarBrand;
            // Set page titles
            await SetPageTitleAsync(string.Empty, pageNumber);

            // Retrieve active driving licenses
            var renterDrivingLicenses = await _unitOfWork.CrMasSupCarBrand
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active );

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarBrand>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationBrand  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );


            // If no active licenses, retrieve all licenses
            if (!renterDrivingLicenses.Any())
            {
                renterDrivingLicenses = await _unitOfWork.CrMasSupCarBrand
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarBrandVM vm = new CarBrandVM();
            vm.crMasSupCarBrand = renterDrivingLicenses;
            vm.cars_count = Cars_Count;
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
                                                                                                                            x.CrMasSupCarBrandStatus == Status.Hold );
                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarBrand>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationBrand  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                CarBrandVM vm = new CarBrandVM();
                vm.cars_count = Cars_Count;
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
            var pageNumber = SubTasks.CrMasSupCarBrand;
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
            if (int.Parse(await GenerateLicenseCodeAsync()) > 3999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarBrand");
            }
            // Set Title 
            CarBrandVM renterDrivingLicenseVM = new CarBrandVM();
            renterDrivingLicenseVM.CrMasSupCarBrandCode = await GenerateLicenseCodeAsync();
            return View(renterDrivingLicenseVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarBrand(CarBrandVM renterDrivingLicenseVM)
        {
            var pageNumber = SubTasks.CrMasSupCarBrand;
            
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterDrivingLicenseVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarBrand", renterDrivingLicenseVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupCarBrand>(renterDrivingLicenseVM);

                // Check if the entity already exists
                if (await _masCarBrand.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    await AddModelErrorsAsync(renterDrivingLicenseEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarBrand", renterDrivingLicenseVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (int.Parse(await GenerateLicenseCodeAsync()) > 3999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarBrand", renterDrivingLicenseVM);
                }
                // Generate and set the Driving License Code
                renterDrivingLicenseVM.CrMasSupCarBrandCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterDrivingLicenseEntity.CrMasSupCarBrandStatus = "A";
                await _unitOfWork.CrMasSupCarBrand.AddAsync(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterDrivingLicenseEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarBrand", renterDrivingLicenseVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var pageNumber = SubTasks.CrMasSupCarBrand;
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
        public async Task<IActionResult> Edit(CarBrandVM renterDrivingLicenseVM)
        {
            var pageNumber = SubTasks.CrMasSupCarBrand;
            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterDrivingLicenseVM == null)
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
                    return View("Edit", renterDrivingLicenseVM);
                }
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupCarBrand>(renterDrivingLicenseVM);

                // Check if the entity already exists
                if (await _masCarBrand.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterDrivingLicenseEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterDrivingLicenseVM);
                }

                _unitOfWork.CrMasSupCarBrand.Update(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterDrivingLicenseEntity, Status.Update);
                return RedirectToAction("Index", "CarBrand");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterDrivingLicenseVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {
            var pageNumber = SubTasks.CrMasSupCarBrand;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarBrand.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {
                
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if(status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
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
            var pageNumber = SubTasks.CrMasSupCarBrand;

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
