using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.RenterServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class RenterNationalityController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterNationality _masRenterNationalities;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterNationalityController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterNationality;


        public RenterNationalityController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterNationality masRenterNationalities, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterNationalityController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterNationalities = masRenterNationalities;
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
            var renterNationalitiess = await _unitOfWork.CrMasSupRenterNationality
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterNationalitiesStatus == Status.Active);

            // If no active licenses, retrieve all licenses
            if (!renterNationalitiess.Any())
            {
                renterNationalitiess = await _unitOfWork.CrMasSupRenterNationality
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterNationalitiesStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            var nationality1_count = await _unitOfWork.CrMasRenterInformation.FindCountByColumnAsync<CrMasRenterInformation>(
            predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
            columnSelector: x => x.CrMasRenterInformationNationality  // تحديد العمود الذي نريد التجميع بناءً عليه
                                                                      //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
            );

            var all_Classifications2 = await _unitOfWork.CrMasSupCountryClassification.GetAllAsyncAsNoTrackingAsync();
            var all_Classifications = all_Classifications2.ToList();
            RenterNationalityVM VM = new RenterNationalityVM();
            VM.List_Nationality = renterNationalitiess;
            VM.Nationality_count_1 = nationality1_count;
            VM.crMasSupCountryClassificationSS = all_Classifications;
            return View(VM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterNationalitiesByStatus(string status, string search)
        {
            //sidebar Active
            var nationality1_count = await _unitOfWork.CrMasRenterInformation.FindCountByColumnAsync<CrMasRenterInformation>(
                predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
                columnSelector: x => x.CrMasRenterInformationNationality  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            var all_Classifications2 = await _unitOfWork.CrMasSupCountryClassification.GetAllAsyncAsNoTrackingAsync();
            var all_Classifications = all_Classifications2.ToList();


            if (!string.IsNullOrEmpty(status))
            {
                var RenterNationalitiessAll = await _unitOfWork.CrMasSupRenterNationality.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterNationalitiesStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterNationalitiesStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterNationalitiesStatus == Status.Hold);
                RenterNationalityVM VM = new RenterNationalityVM();
                VM.Nationality_count_1 = nationality1_count;
                VM.crMasSupCountryClassificationSS = all_Classifications;
                if (status == Status.All)
                {
                    var FilterAll = RenterNationalitiessAll.FindAll(x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterNationalitiesArName.Contains(search) ||
                                                                          x.CrMasSupRenterNationalitiesEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterNationalitiesCode.Contains(search)));
                    VM.List_Nationality = FilterAll;
                    return PartialView("_DataTableRenterNationality", VM);
                }
                var FilterByStatus = RenterNationalitiessAll.FindAll(x => x.CrMasSupRenterNationalitiesStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterNationalitiesArName.Contains(search) ||
                                                                           x.CrMasSupRenterNationalitiesEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterNationalitiesCode.Contains(search)));
                VM.List_Nationality = FilterByStatus;
                return PartialView("_DataTableRenterNationality", VM);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterNationality()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterNationality");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterNationality");
            }
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 1099999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterNationality");
            }
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active);

            // Set Title 
            RenterNationalityVM renterNationalitiesVM = new RenterNationalityVM();
            renterNationalitiesVM.crMasSupCountryClassificationSS = all_Classifications;
            renterNationalitiesVM.CrMasSupRenterNationalitiesCode = await GenerateLicenseCodeAsync();
            return View(renterNationalitiesVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterNationality(RenterNationalityVM renterNationalitiesVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active);
            renterNationalitiesVM.crMasSupCountryClassificationSS = all_Classifications;
            if (!ModelState.IsValid || renterNationalitiesVM == null)
            {
                return View("AddRenterNationality", renterNationalitiesVM);
            }
            try
            {
                // Map ViewModel to Entity
                var renterNationalitiesEntity = _mapper.Map<CrMasSupRenterNationality>(renterNationalitiesVM);

                renterNationalitiesEntity.CrMasSupRenterNationalitiesNaqlCode ??= 0;
                renterNationalitiesEntity.CrMasSupRenterNationalitiesNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masRenterNationalities.ExistsByDetailsAsync(renterNationalitiesEntity))
                {
                    await AddModelErrorsAsync(renterNationalitiesEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterNationality", renterNationalitiesVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 1099999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterNationality", renterNationalitiesVM);
                }
                // Generate and set the Driving License Code
                renterNationalitiesVM.CrMasSupRenterNationalitiesCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterNationalitiesEntity.CrMasSupRenterNationalitiesStatus = "A";
                renterNationalitiesEntity.CrMasSupRenterNationalitiesCounter = 0;
                await _unitOfWork.CrMasSupRenterNationality.AddAsync(renterNationalitiesEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterNationalitiesEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddRenterNationality", renterNationalitiesVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 1000000002 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterNationality");
            }
            var contract = await _unitOfWork.CrMasSupRenterNationality.FindAsync(x => x.CrMasSupRenterNationalitiesCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterNationality");
            }
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active || contract.CrMasSupRenterNationalitiesNaqlGcc == x.CrMasLessorCountryClassificationCode);

            var model = _mapper.Map<RenterNationalityVM>(contract);
            model.CrMasSupRenterNationalitiesNaqlCode ??= 0;
            model.CrMasSupRenterNationalitiesNaqlId ??= 0;
            model.crMasSupCountryClassificationSS = all_Classifications;
            //model.RentersHave_withType_Count = contract.CrCasRenterPrivateDriverInformations.Count + contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterNationalityVM renterNationalitiesVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active || renterNationalitiesVM.CrMasSupRenterNationalitiesNaqlGcc == x.CrMasLessorCountryClassificationCode);
            renterNationalitiesVM.crMasSupCountryClassificationSS = all_Classifications;
            if (user == null && renterNationalitiesVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterNationality");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterNationalitiesVM);
                }
                var renterNationalitiesEntity = _mapper.Map<CrMasSupRenterNationality>(renterNationalitiesVM);
                renterNationalitiesEntity.CrMasSupRenterNationalitiesNaqlCode ??= 0;
                renterNationalitiesEntity.CrMasSupRenterNationalitiesNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masRenterNationalities.ExistsByDetailsAsync(renterNationalitiesEntity))
                {
                    await AddModelErrorsAsync(renterNationalitiesEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterNationalitiesVM);
                }

                _unitOfWork.CrMasSupRenterNationality.Update(renterNationalitiesEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterNationalitiesEntity, Status.Update);
                return RedirectToAction("Index", "RenterNationality");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", renterNationalitiesVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupRenterNationality.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterNationalities.CheckIfCanDeleteIt(licence.CrMasSupRenterNationalitiesCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterNationalitiesStatus = status;
                _unitOfWork.CrMasSupRenterNationality.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupRenterNationality entity)
        {

            if (await _masRenterNationalities.ExistsByArabicNameAsync(entity.CrMasSupRenterNationalitiesArName, entity.CrMasSupRenterNationalitiesCode))
            {
                ModelState.AddModelError("CrMasSupRenterNationalitiesArName", _localizer["Existing"]);
            }

            if (await _masRenterNationalities.ExistsByEnglishNameAsync(entity.CrMasSupRenterNationalitiesEnName, entity.CrMasSupRenterNationalitiesCode))
            {
                ModelState.AddModelError("CrMasSupRenterNationalitiesEnName", _localizer["Existing"]);
            }

            if (await _masRenterNationalities.ExistsByNaqlCodeAsync((int)entity.CrMasSupRenterNationalitiesNaqlCode, entity.CrMasSupRenterNationalitiesCode))
            {
                ModelState.AddModelError("CrMasSupRenterNationalitiesNaqlCode", _localizer["Existing"]);
            }

            if (await _masRenterNationalities.ExistsByNaqlIdAsync((int)entity.CrMasSupRenterNationalitiesNaqlId, entity.CrMasSupRenterNationalitiesCode))
            {
                ModelState.AddModelError("CrMasSupRenterNationalitiesNaqlId", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterNationalitiess = await _unitOfWork.CrMasSupRenterNationality.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterNationalitiess != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterNationalitiesArName" && All_RenterNationalitiess.Any(x => x.CrMasSupRenterNationalitiesArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterNationalitiesArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterNationalitiesEnName" && All_RenterNationalitiess.Any(x => x.CrMasSupRenterNationalitiesEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterNationalitiesEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupRenterNationalitiesNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_RenterNationalitiess.Any(x => x.CrMasSupRenterNationalitiesNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterNationalitiesNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupRenterNationalitiesNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_RenterNationalitiess.Any(x => x.CrMasSupRenterNationalitiesNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterNationalitiesNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterNationality.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterNationalitiesCode) + 1).ToString() : "1000000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterNationality licence, string status)
        {


            var recordAr = licence.CrMasSupRenterNationalitiesArName;
            var recordEn = licence.CrMasSupRenterNationalitiesEnName;
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
            return RedirectToAction("Index", "RenterNationality");
        }


    }
}
