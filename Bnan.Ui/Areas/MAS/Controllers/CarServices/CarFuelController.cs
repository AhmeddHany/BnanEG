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
    public class CarFuelController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarFuel _masCarFuel;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarFuelController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarFuel;


        public CarFuelController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarFuel masCarFuel, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarFuelController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarFuel = masCarFuel;
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
            var carFuels = await _unitOfWork.CrMasSupCarFuel
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarFuelStatus == Status.Active);

            //var Cars_Count = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
            //    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
            //    selectProjection: query => query.Select(x => new CrCasCarInformation
            //    {
            //        CrCasCarInformationSerailNo = x.CrCasCarInformationSerailNo,
            //        CrCasCarInformationFuel = x.CrCasCarInformationFuel
            //    })
            //    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
            //    );

            var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarFuel>(
                predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                columnSelector: x => x.CrCasCarInformationFuel  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );


            // If no active licenses, retrieve all licenses
            if (!carFuels.Any())
            {
                carFuels = await _unitOfWork.CrMasSupCarFuel
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarFuelStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarFuelVM vm = new CarFuelVM();
            vm.crMasSupCarFuel = carFuels;
            vm.cars_count = Cars_Count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarFuelByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CarFuelsAll = await _unitOfWork.CrMasSupCarFuel.FindAllAsNoTrackingAsync(x => x.CrMasSupCarFuelStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarFuelStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarFuelStatus == Status.Hold);
                var Cars_Count = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrMasSupCarFuel>(
                    predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrCasCarInformationFuel  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                CarFuelVM vm = new CarFuelVM();
                vm.cars_count = Cars_Count;
                if (status == Status.All)
                {
                    var FilterAll = CarFuelsAll.FindAll(x => x.CrMasSupCarFuelStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarFuelArName.Contains(search) ||
                                                                          x.CrMasSupCarFuelEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarFuelCode.Contains(search)));
                    vm.crMasSupCarFuel = FilterAll;
                    return PartialView("_DataTableCarFuel", vm);
                }
                var FilterByStatus = CarFuelsAll.FindAll(x => x.CrMasSupCarFuelStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarFuelArName.Contains(search) ||
                                                                           x.CrMasSupCarFuelEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarFuelCode.Contains(search)));
                vm.crMasSupCarFuel = FilterByStatus;
                return PartialView("_DataTableCarFuel", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarFuel()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarFuel");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarFuel");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarFuel");
            }
            // Set Title 
            CarFuelVM carFuelVM = new CarFuelVM();
            carFuelVM.CrMasSupCarFuelCode = await GenerateLicenseCodeAsync();
            return View(carFuelVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarFuel(CarFuelVM carFuelVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || carFuelVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarFuel", carFuelVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var carFuelEntity = _mapper.Map<CrMasSupCarFuel>(carFuelVM);

                carFuelEntity.CrMasSupCarFuelNaqlCode ??= 0;
                carFuelEntity.CrMasSupCarFuelNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masCarFuel.ExistsByDetailsAsync(carFuelEntity))
                {
                    await AddModelErrorsAsync(carFuelEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarFuel", carFuelVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarFuel", carFuelVM);
                }
                // Generate and set the Driving License Code
                carFuelVM.CrMasSupCarFuelCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                carFuelEntity.CrMasSupCarFuelStatus = "A";
                await _unitOfWork.CrMasSupCarFuel.AddAsync(carFuelEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, carFuelEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarFuel", carFuelVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarFuel.FindAsync(x => x.CrMasSupCarFuelCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarFuel");
            }
            var model = _mapper.Map<CarFuelVM>(contract);
            model.CrMasSupCarFuelNaqlCode ??= 0;
            model.CrMasSupCarFuelNaqlId ??= 0;
            //model.RentersHave_withType_Count = contract.CrCasRenterPrivateDriverInformations.Count + contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarFuelVM carFuelVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && carFuelVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarFuel");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", carFuelVM);
                }
                var carFuelEntity = _mapper.Map<CrMasSupCarFuel>(carFuelVM);
                carFuelEntity.CrMasSupCarFuelNaqlCode ??= 0;
                carFuelEntity.CrMasSupCarFuelNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masCarFuel.ExistsByDetailsAsync(carFuelEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(carFuelEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", carFuelVM);
                }

                _unitOfWork.CrMasSupCarFuel.Update(carFuelEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, carFuelEntity, Status.Update);
                return RedirectToAction("Index", "CarFuel");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", carFuelVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarFuel.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarFuel.CheckIfCanDeleteIt(licence.CrMasSupCarFuelCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarFuelStatus = status;
                _unitOfWork.CrMasSupCarFuel.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarFuel entity)
        {

            if (await _masCarFuel.ExistsByArabicNameAsync(entity.CrMasSupCarFuelArName, entity.CrMasSupCarFuelCode))
            {
                ModelState.AddModelError("CrMasSupCarFuelArName", _localizer["Existing"]);
            }

            if (await _masCarFuel.ExistsByEnglishNameAsync(entity.CrMasSupCarFuelEnName, entity.CrMasSupCarFuelCode))
            {
                ModelState.AddModelError("CrMasSupCarFuelEnName", _localizer["Existing"]);
            }

            if (await _masCarFuel.ExistsByNaqlCodeAsync((int)entity.CrMasSupCarFuelNaqlCode, entity.CrMasSupCarFuelCode))
            {
                ModelState.AddModelError("CrMasSupCarFuelNaqlCode", _localizer["Existing"]);
            }

            if (await _masCarFuel.ExistsByNaqlIdAsync((int)entity.CrMasSupCarFuelNaqlId, entity.CrMasSupCarFuelCode))
            {
                ModelState.AddModelError("CrMasSupCarFuelNaqlId", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarFuels = await _unitOfWork.CrMasSupCarFuel.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarFuels != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarFuelArName" && All_CarFuels.Any(x => x.CrMasSupCarFuelArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarFuelArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarFuelEnName" && All_CarFuels.Any(x => x.CrMasSupCarFuelEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarFuelEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupCarFuelNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_CarFuels.Any(x => x.CrMasSupCarFuelNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarFuelNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupCarFuelNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_CarFuels.Any(x => x.CrMasSupCarFuelNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarFuelNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarFuel.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarFuelCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarFuel licence, string status)
        {


            var recordAr = licence.CrMasSupCarFuelArName;
            var recordEn = licence.CrMasSupCarFuelEnName;
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
            return RedirectToAction("Index", "CarFuel");
        }


    }
}
