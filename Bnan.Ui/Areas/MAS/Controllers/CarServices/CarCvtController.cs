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
    public class CarCvtController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarCvt _masCarCvt;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarCvtController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarCvt;


        public CarCvtController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarCvt masCarCvt, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarCvtController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarCvt = masCarCvt;
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
            var CarCvt = await _unitOfWork.CrMasSupCarCvt
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarCvtStatus == Status.Active);

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarCvt>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationCvt  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
            );

            // If no active licenses, retrieve all licenses
            if (!CarCvt.Any())
            {
                CarCvt = await _unitOfWork.CrMasSupCarCvt
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarCvtStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarCvtVM vm = new CarCvtVM();
            vm.crMasSupCarCvt = CarCvt;
            vm.cars_count = Cars_Count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarCvtByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CarCvtAll = await _unitOfWork.CrMasSupCarCvt.FindAllAsNoTrackingAsync(x => x.CrMasSupCarCvtStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarCvtStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarCvtStatus == Status.Hold);
                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarCvt>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationCvt  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
                CarCvtVM vm = new CarCvtVM();
                vm.cars_count = Cars_Count;
                if (status == Status.All)
                {
                    var FilterAll = CarCvtAll.FindAll(x => x.CrMasSupCarCvtStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarCvtArName.Contains(search) ||
                                                                          x.CrMasSupCarCvtEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarCvtCode.Contains(search)));
                    vm.crMasSupCarCvt = FilterAll;
                    return PartialView("_DataTableCarCvt", vm);
                }
                var FilterByStatus = CarCvtAll.FindAll(x => x.CrMasSupCarCvtStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarCvtArName.Contains(search) ||
                                                                           x.CrMasSupCarCvtEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarCvtCode.Contains(search)));
                vm.crMasSupCarCvt = FilterByStatus;
                return PartialView("_DataTableCarCvt", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarCvt()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarCvt");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarCvt");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarCvt");
            }
            // Set Title 
            CarCvtVM CarCvtVM = new CarCvtVM();
            CarCvtVM.CrMasSupCarCvtCode = await GenerateLicenseCodeAsync();
            return View(CarCvtVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarCvt(CarCvtVM CarCvtVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || CarCvtVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarCvt", CarCvtVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var CarCvtEntity = _mapper.Map<CrMasSupCarCvt>(CarCvtVM);

                // Check if the entity already exists
                if (await _masCarCvt.ExistsByDetailsAsync(CarCvtEntity))
                {
                    await AddModelErrorsAsync(CarCvtEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarCvt", CarCvtVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarCvt", CarCvtVM);
                }
                // Generate and set the Driving License Code
                CarCvtVM.CrMasSupCarCvtCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                CarCvtEntity.CrMasSupCarCvtStatus = "A";
                await _unitOfWork.CrMasSupCarCvt.AddAsync(CarCvtEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, CarCvtEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarCvt", CarCvtVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarCvt.FindAsync(x => x.CrMasSupCarCvtCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarCvt");
            }
            var model = _mapper.Map<CarCvtVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarCvtVM CarCvtVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && CarCvtVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarCvt");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", CarCvtVM);
                }
                var CarCvtEntity = _mapper.Map<CrMasSupCarCvt>(CarCvtVM);

                // Check if the entity already exists
                if (await _masCarCvt.ExistsByDetailsAsync(CarCvtEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(CarCvtEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", CarCvtVM);
                }

                _unitOfWork.CrMasSupCarCvt.Update(CarCvtEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, CarCvtEntity, Status.Update);
                return RedirectToAction("Index", "CarCvt");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", CarCvtVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarCvt.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarCvt.CheckIfCanDeleteIt(licence.CrMasSupCarCvtCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarCvtStatus = status;
                _unitOfWork.CrMasSupCarCvt.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarCvt entity)
        {

            if (await _masCarCvt.ExistsByArabicNameAsync(entity.CrMasSupCarCvtArName, entity.CrMasSupCarCvtCode))
            {
                ModelState.AddModelError("CrMasSupCarCvtArName", _localizer["Existing"]);
            }

            if (await _masCarCvt.ExistsByEnglishNameAsync(entity.CrMasSupCarCvtEnName, entity.CrMasSupCarCvtCode))
            {
                ModelState.AddModelError("CrMasSupCarCvtEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarCvt = await _unitOfWork.CrMasSupCarCvt.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarCvt != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarCvtArName" && All_CarCvt.Any(x => x.CrMasSupCarCvtArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarCvtArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarCvtEnName" && All_CarCvt.Any(x => x.CrMasSupCarCvtEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarCvtEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarCvt.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarCvtCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarCvt licence, string status)
        {


            var recordAr = licence.CrMasSupCarCvtArName;
            var recordEn = licence.CrMasSupCarCvtEnName;
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
            return RedirectToAction("Index", "CarCvt");
        }


    }
}
