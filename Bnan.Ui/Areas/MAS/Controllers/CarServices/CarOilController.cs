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
    public class CarOilController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarOil _masCarOil;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarOilController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarOil;


        public CarOilController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarOil masCarOil, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarOilController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarOil = masCarOil;
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
            var carOils = await _unitOfWork.CrMasSupCarOil
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarOilStatus == Status.Active, new[] { "CrCasCarInformations" });

            // If no active licenses, retrieve all licenses
            if (!carOils.Any())
            {
                carOils = await _unitOfWork.CrMasSupCarOil
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarOilStatus == Status.Hold,
                                              new[] { "CrCasCarInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(carOils);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarOilByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CarOilsAll = await _unitOfWork.CrMasSupCarOil.FindAllAsNoTrackingAsync(x => x.CrMasSupCarOilStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarOilStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarOilStatus == Status.Hold, new[] { "CrCasCarInformations" });

                if (status == Status.All)
                {
                    var FilterAll = CarOilsAll.FindAll(x => x.CrMasSupCarOilStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarOilArName.Contains(search) ||
                                                                          x.CrMasSupCarOilEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarOilCode.Contains(search)));
                    return PartialView("_DataTableCarOil", FilterAll);
                }
                var FilterByStatus = CarOilsAll.FindAll(x => x.CrMasSupCarOilStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarOilArName.Contains(search) ||
                                                                           x.CrMasSupCarOilEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarOilCode.Contains(search)));
                return PartialView("_DataTableCarOil", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarOil()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarOil");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarOil");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarOil");
            }
            // Set Title 
            CarOilVM carOilVM = new CarOilVM();
            carOilVM.CrMasSupCarOilCode = await GenerateLicenseCodeAsync();
            return View(carOilVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarOil(CarOilVM carOilVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || carOilVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarOil", carOilVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var carOilEntity = _mapper.Map<CrMasSupCarOil>(carOilVM);

                carOilEntity.CrMasSupCarOilNaqlCode ??= 0;
                carOilEntity.CrMasSupCarOilNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masCarOil.ExistsByDetailsAsync(carOilEntity))
                {
                    await AddModelErrorsAsync(carOilEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarOil", carOilVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarOil", carOilVM);
                }
                // Generate and set the Driving License Code
                carOilVM.CrMasSupCarOilCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                carOilEntity.CrMasSupCarOilStatus = "A";
                await _unitOfWork.CrMasSupCarOil.AddAsync(carOilEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, carOilEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarOil", carOilVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarOil.FindAsync(x => x.CrMasSupCarOilCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarOil");
            }
            var model = _mapper.Map<CarOilVM>(contract);
            model.CrMasSupCarOilNaqlCode ??= 0;
            model.CrMasSupCarOilNaqlId ??= 0;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarOilVM carOilVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && carOilVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarOil");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", carOilVM);
                }
                var carOilEntity = _mapper.Map<CrMasSupCarOil>(carOilVM);
                carOilEntity.CrMasSupCarOilNaqlCode ??= 0;
                carOilEntity.CrMasSupCarOilNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masCarOil.ExistsByDetailsAsync(carOilEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(carOilEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", carOilVM);
                }

                _unitOfWork.CrMasSupCarOil.Update(carOilEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, carOilEntity, Status.Update);
                return RedirectToAction("Index", "CarOil");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", carOilVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarOil.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarOil.CheckIfCanDeleteIt(licence.CrMasSupCarOilCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarOilStatus = status;
                _unitOfWork.CrMasSupCarOil.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarOil entity)
        {

            if (await _masCarOil.ExistsByArabicNameAsync(entity.CrMasSupCarOilArName, entity.CrMasSupCarOilCode))
            {
                ModelState.AddModelError("CrMasSupCarOilArName", _localizer["Existing"]);
            }

            if (await _masCarOil.ExistsByEnglishNameAsync(entity.CrMasSupCarOilEnName, entity.CrMasSupCarOilCode))
            {
                ModelState.AddModelError("CrMasSupCarOilEnName", _localizer["Existing"]);
            }

            if (await _masCarOil.ExistsByNaqlCodeAsync((int)entity.CrMasSupCarOilNaqlCode, entity.CrMasSupCarOilCode))
            {
                ModelState.AddModelError("CrMasSupCarOilNaqlCode", _localizer["Existing"]);
            }

            if (await _masCarOil.ExistsByNaqlIdAsync((int)entity.CrMasSupCarOilNaqlId, entity.CrMasSupCarOilCode))
            {
                ModelState.AddModelError("CrMasSupCarOilNaqlId", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarOils = await _unitOfWork.CrMasSupCarOil.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarOils != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarOilArName" && All_CarOils.Any(x => x.CrMasSupCarOilArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarOilArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarOilEnName" && All_CarOils.Any(x => x.CrMasSupCarOilEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarOilEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupCarOilNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_CarOils.Any(x => x.CrMasSupCarOilNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarOilNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupCarOilNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_CarOils.Any(x => x.CrMasSupCarOilNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarOilNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarOil.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarOilCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarOil licence, string status)
        {


            var recordAr = licence.CrMasSupCarOilArName;
            var recordEn = licence.CrMasSupCarOilEnName;
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
            return RedirectToAction("Index", "CarOil");
        }


    }
}
