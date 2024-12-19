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
    public class CarRegistrationController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarRegistration _masCarRegistration;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarRegistrationController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarRegistration;

        public CarRegistrationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarRegistration masCarRegistration, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarRegistrationController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarRegistration = masCarRegistration;
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
            var carRegisterations = await _unitOfWork.CrMasSupCarRegistration
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarRegistrationStatus == Status.Active);

            //var Cars_Count = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
            //    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
            //    selectProjection: query => query.Select(x => new CrCasCarInformation
            //    {
            //        CrCasCarInformationSerailNo = x.CrCasCarInformationSerailNo,
            //        CrCasCarInformationRegistration = x.CrCasCarInformationRegistration
            //    })
            //    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
            //    );

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarRegistration>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationRegistration  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );


            // If no active licenses, retrieve all licenses
            if (!carRegisterations.Any())
            {
                carRegisterations = await _unitOfWork.CrMasSupCarRegistration
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarRegistrationStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarRegistrationVM vm = new CarRegistrationVM();
            vm.crMasSupCarRegistration = carRegisterations;
            vm.cars_count = Cars_Count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarRegistrationByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CarRegistrationsAll = await _unitOfWork.CrMasSupCarRegistration.FindAllAsNoTrackingAsync(x => x.CrMasSupCarRegistrationStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarRegistrationStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarRegistrationStatus == Status.Hold);
                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarRegistration>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationRegistration  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                CarRegistrationVM vm = new CarRegistrationVM();
                vm.cars_count = Cars_Count;
                if (status == Status.All)
                {
                    var FilterAll = CarRegistrationsAll.FindAll(x => x.CrMasSupCarRegistrationStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarRegistrationArName.Contains(search) ||
                                                                          x.CrMasSupCarRegistrationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarRegistrationCode.Contains(search)));
                    vm.crMasSupCarRegistration = FilterAll;
                    return PartialView("_DataTableCarRegistration", vm);
                }
                var FilterByStatus = CarRegistrationsAll.FindAll(x => x.CrMasSupCarRegistrationStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarRegistrationArName.Contains(search) ||
                                                                           x.CrMasSupCarRegistrationEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarRegistrationCode.Contains(search)));
                vm.crMasSupCarRegistration = FilterByStatus;
                return PartialView("_DataTableCarRegistration", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarRegistration()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarRegistration");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarRegistration");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarRegistration");
            }
            // Set Title 
            CarRegistrationVM carRegisterationVM = new CarRegistrationVM();
            carRegisterationVM.CrMasSupCarRegistrationCode = await GenerateLicenseCodeAsync();
            return View(carRegisterationVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarRegistration(CarRegistrationVM carRegisterationVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || carRegisterationVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarRegistration", carRegisterationVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var carRegisterationEntity = _mapper.Map<CrMasSupCarRegistration>(carRegisterationVM);

                carRegisterationEntity.CrMasSupCarRegistrationNaqlCode ??= 0;
                carRegisterationEntity.CrMasSupCarRegistrationNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masCarRegistration.ExistsByDetailsAsync(carRegisterationEntity))
                {
                    await AddModelErrorsAsync(carRegisterationEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarRegistration", carRegisterationVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarRegistration", carRegisterationVM);
                }
                // Generate and set the Driving License Code
                carRegisterationVM.CrMasSupCarRegistrationCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                carRegisterationEntity.CrMasSupCarRegistrationStatus = "A";
                await _unitOfWork.CrMasSupCarRegistration.AddAsync(carRegisterationEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, carRegisterationEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarRegistration", carRegisterationVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarRegistration.FindAsync(x => x.CrMasSupCarRegistrationCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarRegistration");
            }
            var model = _mapper.Map<CarRegistrationVM>(contract);
            model.CrMasSupCarRegistrationNaqlCode ??= 0;
            model.CrMasSupCarRegistrationNaqlId ??= 0;
            //model.RentersHave_withType_Count = contract.CrCasRenterPrivateDriverInformations.Count + contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarRegistrationVM carRegisterationVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && carRegisterationVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarRegistration");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", carRegisterationVM);
                }
                var carRegisterationEntity = _mapper.Map<CrMasSupCarRegistration>(carRegisterationVM);
                carRegisterationEntity.CrMasSupCarRegistrationNaqlCode ??= 0;
                carRegisterationEntity.CrMasSupCarRegistrationNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masCarRegistration.ExistsByDetailsAsync(carRegisterationEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(carRegisterationEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", carRegisterationVM);
                }

                _unitOfWork.CrMasSupCarRegistration.Update(carRegisterationEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, carRegisterationEntity, Status.Update);
                return RedirectToAction("Index", "CarRegistration");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", carRegisterationVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarRegistration.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarRegistration.CheckIfCanDeleteIt(licence.CrMasSupCarRegistrationCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarRegistrationStatus = status;
                _unitOfWork.CrMasSupCarRegistration.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarRegistration entity)
        {

            if (await _masCarRegistration.ExistsByArabicNameAsync(entity.CrMasSupCarRegistrationArName, entity.CrMasSupCarRegistrationCode))
            {
                ModelState.AddModelError("CrMasSupCarRegistrationArName", _localizer["Existing"]);
            }

            if (await _masCarRegistration.ExistsByEnglishNameAsync(entity.CrMasSupCarRegistrationEnName, entity.CrMasSupCarRegistrationCode))
            {
                ModelState.AddModelError("CrMasSupCarRegistrationEnName", _localizer["Existing"]);
            }

            if (await _masCarRegistration.ExistsByNaqlCodeAsync((int)entity.CrMasSupCarRegistrationNaqlCode, entity.CrMasSupCarRegistrationCode))
            {
                ModelState.AddModelError("CrMasSupCarRegistrationNaqlCode", _localizer["Existing"]);
            }

            if (await _masCarRegistration.ExistsByNaqlIdAsync((int)entity.CrMasSupCarRegistrationNaqlId, entity.CrMasSupCarRegistrationCode))
            {
                ModelState.AddModelError("CrMasSupCarRegistrationNaqlId", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarRegistrations = await _unitOfWork.CrMasSupCarRegistration.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarRegistrations != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarRegistrationArName" && All_CarRegistrations.Any(x => x.CrMasSupCarRegistrationArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarRegistrationArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarRegistrationEnName" && All_CarRegistrations.Any(x => x.CrMasSupCarRegistrationEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarRegistrationEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupCarRegistrationNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_CarRegistrations.Any(x => x.CrMasSupCarRegistrationNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarRegistrationNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupCarRegistrationNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_CarRegistrations.Any(x => x.CrMasSupCarRegistrationNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarRegistrationNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarRegistration.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarRegistrationCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarRegistration licence, string status)
        {


            var recordAr = licence.CrMasSupCarRegistrationArName;
            var recordEn = licence.CrMasSupCarRegistrationEnName;
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
            return RedirectToAction("Index", "CarRegistration");
        }


    }
}
