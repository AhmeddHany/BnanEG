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
namespace Bnan.Ui.Areas.MAS.Controllers.PostServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class CountriesController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCountries _masCountries;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CountriesController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSysCallingKeys;


        public CountriesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCountries masCountries, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CountriesController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCountries = masCountries;
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
            var countrys = await _unitOfWork.CrMasSysCallingKeys
                .FindAllAsNoTrackingAsync(x => x.CrMasSysCallingKeysStatus == Status.Active);

            var country_count = await _unitOfWork.CrMasRenterInformation.FindCountByColumnAsync<CrMasSysCallingKey>(
                predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
                columnSelector: x => x.CrMasRenterInformationCountreyKey  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            // If no active licenses, retrieve all licenses
            if (!countrys.Any())
            {
                countrys = await _unitOfWork.CrMasSysCallingKeys
                    .FindAllAsNoTrackingAsync(x => x.CrMasSysCallingKeysStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            var all_Classifications2 = await _unitOfWork.CrMasSupCountryClassification.GetAllAsyncAsNoTrackingAsync();
            var all_Classifications = all_Classifications2.ToList();
            CountriesVM vm = new CountriesVM();
            vm.Countries = countrys;
            vm.Country_count = country_count;
            vm.crMasSupCountryClassificationSS = all_Classifications;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetCountriesByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var CountriessAll = await _unitOfWork.CrMasSysCallingKeys.FindAllAsNoTrackingAsync(x => x.CrMasSysCallingKeysStatus == Status.Active ||
                                                                                                                            x.CrMasSysCallingKeysStatus == Status.Deleted ||
                                                                                                                            x.CrMasSysCallingKeysStatus == Status.Hold);

                var country_count = await _unitOfWork.CrMasRenterInformation.FindCountByColumnAsync<CrMasRenterInformation>(
                    predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
                    columnSelector: x => x.CrMasRenterInformationCountreyKey  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                var all_Classifications2 = await _unitOfWork.CrMasSupCountryClassification.GetAllAsyncAsNoTrackingAsync();
                var all_Classifications = all_Classifications2.ToList();
                CountriesVM vm = new CountriesVM();
                vm.Country_count = country_count;
                vm.crMasSupCountryClassificationSS = all_Classifications;
                if (status == Status.All)
                {
                    var FilterAll = CountriessAll.FindAll(x => x.CrMasSysCallingKeysStatus != Status.Deleted &&
                                                                         (x.CrMasSysCallingKeysArName.Contains(search) ||
                                                                          x.CrMasSysCallingKeysEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSysCallingKeysCode.Contains(search)));
                    vm.Countries = FilterAll;
                    return PartialView("_DataTableCountries", vm);
                }
                var FilterByStatus = CountriessAll.FindAll(x => x.CrMasSysCallingKeysStatus == status &&
                                                                            (
                                                                           x.CrMasSysCallingKeysArName.Contains(search) ||
                                                                           x.CrMasSysCallingKeysEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSysCallingKeysCode.Contains(search)));
                vm.Countries = FilterByStatus;
                return PartialView("_DataTableCountries", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddCountries()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Countries");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Countries");
            }
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Countries");
            }
            // Set Title 
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active);

            CountriesVM countryVM = new CountriesVM();
            countryVM.CrMasSysCallingKeysCode = await GenerateLicenseCodeAsync();
            countryVM.crMasSupCountryClassificationSS = all_Classifications;
            return View(countryVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCountries(CountriesVM countryVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active);
            countryVM.crMasSupCountryClassificationSS = all_Classifications;
            if (!ModelState.IsValid || countryVM == null)
            {
                return View("AddCountries", countryVM);
            }
            try
            {
                // Map ViewModel to Entity
                var countryEntity = _mapper.Map<CrMasSysCallingKey>(countryVM);

                countryEntity.CrMasSysCallingKeysNaqlCode ??= 0;
                countryEntity.CrMasSysCallingKeysNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masCountries.ExistsByDetailsAsync(countryEntity))
                {
                    await AddModelErrorsAsync(countryEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddCountries", countryVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddCountries", countryVM);
                }
                // Generate and set the Driving License Code
                countryVM.CrMasSysCallingKeysCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                countryEntity.CrMasSysCallingKeysStatus = "A";
                await _unitOfWork.CrMasSysCallingKeys.AddAsync(countryEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, countryEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddCountries", countryVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSysCallingKeys.FindAsync(x => x.CrMasSysCallingKeysCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Countries");
            }
            var model = _mapper.Map<CountriesVM>(contract);
            model.CrMasSysCallingKeysNaqlCode ??= 0;
            model.CrMasSysCallingKeysNaqlId ??= 0;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CountriesVM countryVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && countryVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Countries");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", countryVM);
                }
                var countryEntity = _mapper.Map<CrMasSysCallingKey>(countryVM);
                countryEntity.CrMasSysCallingKeysNaqlCode ??= 0;
                countryEntity.CrMasSysCallingKeysNaqlId ??= 0;
                countryEntity.CrMasSysCallingKeysArName = "أأ";
                countryEntity.CrMasSysCallingKeysEnName = "AA";
                // Check if the entity already exists
                if (await _masCountries.ExistsByDetailsAsync(countryEntity))
                {
                    await AddModelErrorsAsync(countryEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", countryVM);
                }
                var contract = await _unitOfWork.CrMasSysCallingKeys.FindAsync(x => x.CrMasSysCallingKeysCode == countryVM.CrMasSysCallingKeysCode);
                if (contract == null) { return View("Edit", countryVM); }
                contract.CrMasSysCallingKeysNaqlCode = countryVM.CrMasSysCallingKeysNaqlCode;
                contract.CrMasSysCallingKeysNaqlId = countryVM.CrMasSysCallingKeysNaqlId;
                contract.CrMasSysCallingKeysReasons = countryVM.CrMasSysCallingKeysReasons;
                _unitOfWork.CrMasSysCallingKeys.Update(contract);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, contract, Status.Update);
                return RedirectToAction("Index", "Countries");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", countryVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSysCallingKeys.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masCountries.CheckIfCanDeleteIt(licence.CrMasSysCallingKeysNo)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSysCallingKeysStatus = status;
                _unitOfWork.CrMasSysCallingKeys.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSysCallingKey entity)
        {

            if (await _masCountries.ExistsByArabicNameAsync(entity.CrMasSysCallingKeysArName, entity.CrMasSysCallingKeysCode))
            {
                ModelState.AddModelError("CrMasSysCallingKeysArName", _localizer["Existing"]);
            }

            if (await _masCountries.ExistsByEnglishNameAsync(entity.CrMasSysCallingKeysEnName, entity.CrMasSysCallingKeysCode))
            {
                ModelState.AddModelError("CrMasSysCallingKeysEnName", _localizer["Existing"]);
            }

            if (await _masCountries.ExistsByNaqlCodeAsync((int)entity.CrMasSysCallingKeysNaqlCode, entity.CrMasSysCallingKeysCode))
            {
                ModelState.AddModelError("CrMasSysCallingKeysNaqlCode", _localizer["Existing"]);
            }

            if (await _masCountries.ExistsByNaqlIdAsync((int)entity.CrMasSysCallingKeysNaqlId, entity.CrMasSysCallingKeysCode))
            {
                ModelState.AddModelError("CrMasSysCallingKeysNaqlId", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_Countriess = await _unitOfWork.CrMasSysCallingKeys.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_Countriess != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSysCallingKeysArName" && All_Countriess.Any(x => x.CrMasSysCallingKeysArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysCallingKeysArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSysCallingKeysEnName" && All_Countriess.Any(x => x.CrMasSysCallingKeysEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysCallingKeysEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSysCallingKeysNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_Countriess.Any(x => x.CrMasSysCallingKeysNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysCallingKeysNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSysCallingKeysNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_Countriess.Any(x => x.CrMasSysCallingKeysNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysCallingKeysNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSysCallingKeys.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSysCallingKeysCode) + 1).ToString() : "100";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSysCallingKey licence, string status)
        {


            var recordAr = licence.CrMasSysCallingKeysArName;
            var recordEn = licence.CrMasSysCallingKeysEnName;
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
            return RedirectToAction("Index", "Countries");
        }


    }
}
