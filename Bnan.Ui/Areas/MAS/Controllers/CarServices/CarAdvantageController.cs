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
    public class CarAdvantageController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCarAdvantage _masCarAdvantage;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarAdvantageController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupCarAdvantage;


        public CarAdvantageController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarAdvantage masCarAdvantage, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarAdvantageController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCarAdvantage = masCarAdvantage;
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
            var CarAdvantages = await _unitOfWork.CrMasSupCarAdvantage
                .FindAllAsNoTrackingAsync(x => x.CrMasSupCarAdvantagesStatus == Status.Active);

            var advantages_Count = await _unitOfWork.CrCasRenterContractAdvantage.FindCountByColumnAsync<CrCasCarAdvantage>(
                predicate: null,
                columnSelector: x => x.CrCasRenterContractAdvantagesCode  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            // If no active licenses, retrieve all licenses
            if (!CarAdvantages.Any())
            {
                CarAdvantages = await _unitOfWork.CrMasSupCarAdvantage
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupCarAdvantagesStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CarAdvantageVM VM = new CarAdvantageVM();
            VM.crMasSupCarAdvantage = CarAdvantages;
            VM.advantages_count = advantages_Count;
            return View(VM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCarAdvantageByStatus(string status, string search)
        {

            //sidebar Active
            if (!string.IsNullOrEmpty(status))
            {
                var CarAdvantagesAll = await _unitOfWork.CrMasSupCarAdvantage.FindAllAsNoTrackingAsync(x => x.CrMasSupCarAdvantagesStatus == Status.Active ||
                                                                                                                            x.CrMasSupCarAdvantagesStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupCarAdvantagesStatus == Status.Hold);

                var advantages_Count = await _unitOfWork.CrCasRenterContractAdvantage.FindCountByColumnAsync<CrCasCarAdvantage>(
                    predicate: null,
                    columnSelector: x => x.CrCasRenterContractAdvantagesCode  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );

                CarAdvantageVM VM = new CarAdvantageVM();
                VM.advantages_count = advantages_Count;

                if (status == Status.All)
                {
                    var FilterAll = CarAdvantagesAll.FindAll(x => x.CrMasSupCarAdvantagesStatus != Status.Deleted &&
                                                                         (x.CrMasSupCarAdvantagesArName.Contains(search) ||
                                                                          x.CrMasSupCarAdvantagesEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupCarAdvantagesCode.Contains(search)));
                    VM.crMasSupCarAdvantage = FilterAll;
                    return PartialView("_DataTableCarAdvantage", VM);
                }
                var FilterByStatus = CarAdvantagesAll.FindAll(x => x.CrMasSupCarAdvantagesStatus == status &&
                                                                            (
                                                                           x.CrMasSupCarAdvantagesArName.Contains(search) ||
                                                                           x.CrMasSupCarAdvantagesEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupCarAdvantagesCode.Contains(search)));
                VM.crMasSupCarAdvantage = FilterByStatus;
                return PartialView("_DataTableCarAdvantage", VM);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCarAdvantage()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "CarAdvantage");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarAdvantage");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarAdvantage");
            }
            // Set Title 
            CarAdvantageVM CarAdvantageVM = new CarAdvantageVM();
            CarAdvantageVM.CrMasSupCarAdvantagesCode = await GenerateLicenseCodeAsync();
            return View(CarAdvantageVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarAdvantage(CarAdvantageVM CarAdvantageVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || CarAdvantageVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarAdvantage", CarAdvantageVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var CarAdvantageEntity = _mapper.Map<CrMasSupCarAdvantage>(CarAdvantageVM);

                // Check if the entity already exists
                if (await _masCarAdvantage.ExistsByDetailsAsync(CarAdvantageEntity))
                {
                    await AddModelErrorsAsync(CarAdvantageEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCarAdvantage", CarAdvantageVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCarAdvantage", CarAdvantageVM);
                }
                // Generate and set the Driving License Code
                CarAdvantageVM.CrMasSupCarAdvantagesCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                CarAdvantageEntity.CrMasSupCarAdvantagesStatus = "A";
                await _unitOfWork.CrMasSupCarAdvantage.AddAsync(CarAdvantageEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, CarAdvantageEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddCarAdvantage", CarAdvantageVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupCarAdvantage.FindAsync(x => x.CrMasSupCarAdvantagesCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "CarAdvantage");
            }
            var model = _mapper.Map<CarAdvantageVM>(contract);
            //model.RentersHave_withType_Count = contract.CrCasRenterPrivateDriverInformations.Count + contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CarAdvantageVM CarAdvantageVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && CarAdvantageVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "CarAdvantage");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", CarAdvantageVM);
                }
                var CarAdvantageEntity = _mapper.Map<CrMasSupCarAdvantage>(CarAdvantageVM);

                // Check if the entity already exists
                if (await _masCarAdvantage.ExistsByDetailsAsync(CarAdvantageEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(CarAdvantageEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", CarAdvantageVM);
                }

                _unitOfWork.CrMasSupCarAdvantage.Update(CarAdvantageEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, CarAdvantageEntity, Status.Update);
                return RedirectToAction("Index", "CarAdvantage");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", CarAdvantageVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupCarAdvantage.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCarAdvantage.CheckIfCanDeleteIt(licence.CrMasSupCarAdvantagesCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupCarAdvantagesStatus = status;
                _unitOfWork.CrMasSupCarAdvantage.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupCarAdvantage entity)
        {

            if (await _masCarAdvantage.ExistsByArabicNameAsync(entity.CrMasSupCarAdvantagesArName, entity.CrMasSupCarAdvantagesCode))
            {
                ModelState.AddModelError("CrMasSupCarAdvantageArName", _localizer["Existing"]);
            }

            if (await _masCarAdvantage.ExistsByEnglishNameAsync(entity.CrMasSupCarAdvantagesEnName, entity.CrMasSupCarAdvantagesCode))
            {
                ModelState.AddModelError("CrMasSupCarAdvantageEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_CarAdvantages = await _unitOfWork.CrMasSupCarAdvantage.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_CarAdvantages != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupCarAdvantageArName" && All_CarAdvantages.Any(x => x.CrMasSupCarAdvantagesArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarAdvantageArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupCarAdvantageEnName" && All_CarAdvantages.Any(x => x.CrMasSupCarAdvantagesEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupCarAdvantageEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupCarAdvantage.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupCarAdvantagesCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupCarAdvantage licence, string status)
        {


            var recordAr = licence.CrMasSupCarAdvantagesArName;
            var recordEn = licence.CrMasSupCarAdvantagesEnName;
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
            return RedirectToAction("Index", "CarAdvantage");
        }


    }
}
