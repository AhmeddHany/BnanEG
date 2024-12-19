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
    public class CarModelController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarModel _masCarModel;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarModelController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarModel;


        public CarModelController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarModel masCarModel, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarModelController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarModel = masCarModel;
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
            var allBrands_actived = await _unitOfWork.CrMasSupCarBrand
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active);
            var allBrands_Holded = await _unitOfWork.CrMasSupCarBrand
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Hold);
            //var allBrands_Deleted = await _unitOfWork.CrMasSupCarBrand
            //    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Deleted);

            // Retrieve active Car Models
            var allModels2 = await _unitOfWork.CrMasSupCarModel.GetAllAsyncAsNoTrackingAsync();
            List<CrMasSupCarModel>? allModels = new List<CrMasSupCarModel>();
            allModels = allModels2.Where(x => x.CrMasSupCarModelStatus == Status.Active && allBrands_actived.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim())).ToList();

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarModel>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationModel  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );



            // If no active licenses, retrieve all licenses
            if (!allModels.Any())
            {
                //allModels = allModels2.ToList();
                allModels = allModels2.Where(x => allBrands_Holded.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim()) || x.CrMasSupCarModelStatus == Status.Hold && allBrands_actived.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim())
                              ).ToList();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarModelVM vm = new CarModelVM();
            vm.crMasSupCarModel = allModels;
            vm.cars_count = Cars_Count;
            vm.All_Brands = allBrands_actived;

            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarModelByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                //var CarModelsAll = await _unitOfWork.CrMasSupCarModel.FindAllAsNoTrackingAsync(x => x.CrMasSupCarModelStatus == Status.Active ||
                //                                                                                                            x.CrMasSupCarModelStatus == Status.Deleted ||
                //                                                                                                            x.CrMasSupCarModelStatus == Status.Hold );

                var allModels2 = await _unitOfWork.CrMasSupCarModel.GetAllAsyncAsNoTrackingAsync();

                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarModel>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationModel  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );

                var allBrands_actived = await _unitOfWork.CrMasSupCarBrand
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active);
                var allBrands_Holded = await _unitOfWork.CrMasSupCarBrand
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Hold);
                var allBrands_Deleted = await _unitOfWork.CrMasSupCarBrand
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Deleted);

                CarModelVM vm = new CarModelVM();
                vm.cars_count = Cars_Count;
                vm.All_Brands = allBrands_actived;

                if (status == Status.All)
                {
                    var FilterAll = allModels2.Where(x => (x.CrMasSupCarModelStatus != Status.Deleted || allBrands_Deleted.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim())) &&
                                                                         (x.CrMasSupCarModelArName.Contains(search) ||
                                                                          x.CrMasSupCarModelEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarModelCode.Contains(search))).ToList();
                    vm.crMasSupCarModel = FilterAll;
                    return PartialView("_DataTableCarModel", vm);
                }
                if (status == Status.Active)
                {
                    var FilterByStatus3 = allModels2.Where(x => x.CrMasSupCarModelStatus == status && allBrands_actived.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim()) &&
                                                                                (
                                                                               x.CrMasSupCarModelArName.Contains(search) ||
                                                                               x.CrMasSupCarModelEnName.ToLower().Contains(search.ToLower()) ||
                                                                               x.CrMasSupCarModelCode.Contains(search))).ToList();
                    vm.crMasSupCarModel = FilterByStatus3;
                    return PartialView("_DataTableCarModel", vm);
                }
                if (status == Status.Hold)
                {
                    var FilterByStatus = allModels2.Where(x => (allBrands_Holded.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim()) || x.CrMasSupCarModelStatus == status && allBrands_actived.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim())) &&
                                                                                (
                                                                               x.CrMasSupCarModelArName.Contains(search) ||
                                                                               x.CrMasSupCarModelEnName.ToLower().Contains(search.ToLower()) ||
                                                                               x.CrMasSupCarModelCode.Contains(search))).ToList();
                    vm.crMasSupCarModel = FilterByStatus;
                    return PartialView("_DataTableCarModel", vm);
                }
                if (status == Status.Deleted)
                {
                    var FilterByStatus2 = allModels2.Where(x => (allBrands_Deleted.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim()) || x.CrMasSupCarModelStatus == status && allBrands_actived.Any(y => y.CrMasSupCarBrandCode.Trim() == x.CrMasSupCarModelBrand.Trim())) &&
                                                                                (
                                                                               x.CrMasSupCarModelArName.Contains(search) ||
                                                                               x.CrMasSupCarModelEnName.ToLower().Contains(search.ToLower()) ||
                                                                               x.CrMasSupCarModelCode.Contains(search))).ToList();
                    vm.crMasSupCarModel = FilterByStatus2;
                    return PartialView("_DataTableCarModel", vm);
                }

            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarModel()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarModel");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarModel");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 3199999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarModel");
            }
            var allBrands = await _unitOfWork.CrMasSupCarBrand
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active);
            // Set Title 
            CarModelVM vm = new CarModelVM();
            vm.All_Brands = allBrands;
            vm.CrMasSupCarModelCode = await GenerateLicenseCodeAsync();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarModel(CarModelVM carModelVM)
        {


            var user = await _userManager.GetUserAsync(User);
            var allBrands = await _unitOfWork.CrMasSupCarBrand
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active);
            carModelVM.All_Brands = allBrands;

            if (!ModelState.IsValid || carModelVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarModel", carModelVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var carModelEntity = _mapper.Map<CrMasSupCarModel>(carModelVM);

                // Check if the entity already exists
                if (await _masCarModel.ExistsByDetailsAsync(carModelEntity))
                {
                    await AddModelErrorsAsync(carModelEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarModel", carModelVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 3199999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarModel", carModelVM);
                }
                // Generate and set the Driving License Code
                carModelVM.CrMasSupCarModelCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                carModelEntity.CrMasSupCarModelStatus = "A";
                carModelEntity.CrMasSupCarModelGroup = "31";
                carModelEntity.CrMasSupCarModelCounter = 0;
                var singelBrand = await _unitOfWork.CrMasSupCarBrand.FindAsync(x => x.CrMasSupCarBrandCode == carModelEntity.CrMasSupCarModelBrand);
                if (singelBrand == null) { singelBrand.CrMasSupCarBrandArName = " "; singelBrand.CrMasSupCarBrandEnName = " "; }
                carModelEntity.CrMasSupCarModelArConcatenateName = singelBrand?.CrMasSupCarBrandArName + " - " + carModelEntity.CrMasSupCarModelArName;
                carModelEntity.CrMasSupCarModelConcatenateEnName = singelBrand?.CrMasSupCarBrandEnName + " - " + carModelEntity.CrMasSupCarModelEnName;
                await _unitOfWork.CrMasSupCarModel.AddAsync(carModelEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, carModelEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarModel", carModelVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarModel.FindAsync(x => x.CrMasSupCarModelCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarModel");
            }
            var model = _mapper.Map<CarModelVM>(contract);
            var allBrands_activated = await _unitOfWork.CrMasSupCarBrand.FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active);
            model.All_Brands = allBrands_activated;
            if (!allBrands_activated.Any(x => x.CrMasSupCarBrandCode.Trim() == contract.CrMasSupCarModelBrand.Trim()))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarModel");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarModelVM carModelVM)
        {

            var user = await _userManager.GetUserAsync(User);
            var allBrands = await _unitOfWork.CrMasSupCarBrand
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarBrandStatus == Status.Active);
            carModelVM.All_Brands = allBrands;

            if (user == null && carModelVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarModel");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", carModelVM);
                }
                var carModelEntity = _mapper.Map<CrMasSupCarModel>(carModelVM);

                // Check if the entity already exists
                if (await _masCarModel.ExistsByDetailsAsync(carModelEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(carModelEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", carModelVM);
                }
                carModelEntity.CrMasSupCarModelGroup = "31";
                var singelBrand = await _unitOfWork.CrMasSupCarBrand.FindAsync(x => x.CrMasSupCarBrandCode == carModelEntity.CrMasSupCarModelBrand);
                if (singelBrand == null) { singelBrand.CrMasSupCarBrandArName = " "; singelBrand.CrMasSupCarBrandEnName = " "; }
                carModelEntity.CrMasSupCarModelArConcatenateName = singelBrand?.CrMasSupCarBrandArName + " - " + carModelEntity.CrMasSupCarModelArName;
                carModelEntity.CrMasSupCarModelConcatenateEnName = singelBrand?.CrMasSupCarBrandEnName + " - " + carModelEntity.CrMasSupCarModelEnName;
                _unitOfWork.CrMasSupCarModel.Update(carModelEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, carModelEntity, Status.Update);
                return RedirectToAction("Index", "CarModel");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", carModelVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarModel.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarModel.CheckIfCanDeleteIt(licence.CrMasSupCarModelCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarModelStatus = status;
                _unitOfWork.CrMasSupCarModel.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarModel entity)
        {

            if (await _masCarModel.ExistsByArabicNameAsync(entity.CrMasSupCarModelArName, entity.CrMasSupCarModelCode, entity?.CrMasSupCarModelBrand))
            {
                ModelState.AddModelError("CrMasSupCarModelArName", _localizer["Existing"]);
            }

            if (await _masCarModel.ExistsByEnglishNameAsync(entity.CrMasSupCarModelEnName, entity.CrMasSupCarModelCode, entity?.CrMasSupCarModelBrand))
            {
                ModelState.AddModelError("CrMasSupCarModelEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField, string brandCode)
        {
            var All_CarModels = await _unitOfWork.CrMasSupCarModel.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarModels != null)
            {
                // Check for existing Arabic 
                if (existName == "CrMasSupCarModelArName" && All_CarModels.Any(x => x.CrMasSupCarModelArName == dataField && x.CrMasSupCarModelBrand == brandCode))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarModelArName", Message = _localizer["Existing"] });
                }
                // Check for existing English 
                else if (existName == "CrMasSupCarModelEnName" && All_CarModels.Any(x => x.CrMasSupCarModelEnName?.ToLower() == dataField.ToLower() && x.CrMasSupCarModelBrand == brandCode))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarModelEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarModel.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarModelCode) + 1).ToString() : "3100000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarModel licence, string status)
        {


            var recordAr = licence.CrMasSupCarModelArName;
            var recordEn = licence.CrMasSupCarModelEnName;
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
            return RedirectToAction("Index", "CarModel");
        }


    }
}
