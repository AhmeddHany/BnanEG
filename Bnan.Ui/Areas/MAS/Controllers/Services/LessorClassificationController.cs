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
namespace Bnan.Ui.Areas.MAS.Controllers.Services
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class LessorClassificationController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasLessorClassification _masLessorClassification;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<LessorClassificationController> _localizer;
        private readonly string pageNumber = SubTasks.CrCasLessorClassification;


        public LessorClassificationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasLessorClassification masLessorClassification, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<LessorClassificationController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masLessorClassification = masLessorClassification;
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
            var renterSectors = await _unitOfWork.CrCasLessorClassification
                .FindAllAsNoTrackingAsync(x => x.CrMasLessorClassificationStatus == Status.Active, new[] { "CrMasLessorInformations" });

            // If no active licenses, retrieve all licenses
            if (!renterSectors.Any())
            {
                renterSectors = await _unitOfWork.CrCasLessorClassification
                    .FindAllAsNoTrackingAsync(x => x.CrMasLessorClassificationStatus == Status.Hold,
                                              new[] { "CrMasLessorInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterSectors);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetLessorClassificationByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var LessorClassificationsAll = await _unitOfWork.CrCasLessorClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorClassificationStatus == Status.Active ||
                                                                                                                            x.CrMasLessorClassificationStatus == Status.Deleted ||
                                                                                                                            x.CrMasLessorClassificationStatus == Status.Hold, new[] { "CrMasLessorInformations" });

                if (status == Status.All)
                {
                    var FilterAll = LessorClassificationsAll.FindAll(x => x.CrMasLessorClassificationStatus != Status.Deleted &&
                                                                         (x.CrCasLessorClassificationArName.Contains(search) ||
                                                                          x.CrCasLessorClassificationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrCasLessorClassificationCode.Contains(search)));
                    return PartialView("_DataTableLessorClassification", FilterAll);
                }
                var FilterByStatus = LessorClassificationsAll.FindAll(x => x.CrMasLessorClassificationStatus == status &&
                                                                            (
                                                                           x.CrCasLessorClassificationArName.Contains(search) ||
                                                                           x.CrCasLessorClassificationEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrCasLessorClassificationCode.Contains(search)));
                return PartialView("_DataTableLessorClassification", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddLessorClassification()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "LessorClassification");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "LessorClassification");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "LessorClassification");
            }
            // Set Title 
            LessorClassificationVM renterSectorVM = new LessorClassificationVM();
            renterSectorVM.CrCasLessorClassificationCode = await GenerateLicenseCodeAsync();
            return View(renterSectorVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddLessorClassification(LessorClassificationVM renterSectorVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterSectorVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddLessorClassification", renterSectorVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterSectorEntity = _mapper.Map<CrCasLessorClassification>(renterSectorVM);

                // Check if the entity already exists
                if (await _masLessorClassification.ExistsByDetailsAsync(renterSectorEntity))
                {
                    await AddModelErrorsAsync(renterSectorEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddLessorClassification", renterSectorVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddLessorClassification", renterSectorVM);
                }
                // Generate and set the Driving License Code
                renterSectorVM.CrCasLessorClassificationCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterSectorEntity.CrMasLessorClassificationStatus = "A";
                await _unitOfWork.CrCasLessorClassification.AddAsync(renterSectorEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterSectorEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddLessorClassification", renterSectorVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 1 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "LessorClassification");
            }
            var contract = await _unitOfWork.CrCasLessorClassification.FindAsync(x => x.CrCasLessorClassificationCode == id, new[] { "CrMasLessorInformations" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "LessorClassification");
            }
            var model = _mapper.Map<LessorClassificationVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(LessorClassificationVM renterSectorVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterSectorVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "LessorClassification");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterSectorVM);
                }
                var renterSectorEntity = _mapper.Map<CrCasLessorClassification>(renterSectorVM);
                // Check if the entity already exists
                if (await _masLessorClassification.ExistsByDetailsAsync(renterSectorEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterSectorEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterSectorVM);
                }

                _unitOfWork.CrCasLessorClassification.Update(renterSectorEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterSectorEntity, Status.Update);
                return RedirectToAction("Index", "LessorClassification");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterSectorVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrCasLessorClassification.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masLessorClassification.CheckIfCanDeleteIt(licence.CrCasLessorClassificationCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasLessorClassificationStatus = status;
                _unitOfWork.CrCasLessorClassification.Update(licence);
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
        private async Task AddModelErrorsAsync(CrCasLessorClassification entity)
        {

            if (await _masLessorClassification.ExistsByArabicNameAsync(entity.CrCasLessorClassificationArName, entity.CrCasLessorClassificationCode))
            {
                ModelState.AddModelError("CrCasLessorClassificationArName", _localizer["Existing"]);
            }

            if (await _masLessorClassification.ExistsByEnglishNameAsync(entity.CrCasLessorClassificationEnName, entity.CrCasLessorClassificationCode))
            {
                ModelState.AddModelError("CrCasLessorClassificationEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_LessorClassifications = await _unitOfWork.CrCasLessorClassification.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_LessorClassifications != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrCasLessorClassificationArName" && All_LessorClassifications.Any(x => x.CrCasLessorClassificationArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasLessorClassificationArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasLessorClassificationEnName" && All_LessorClassifications.Any(x => x.CrCasLessorClassificationEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasLessorClassificationEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrCasLessorClassification.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrCasLessorClassificationCode) + 1).ToString() : "0";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrCasLessorClassification licence, string status)
        {


            var recordAr = licence.CrCasLessorClassificationArName;
            var recordEn = licence.CrCasLessorClassificationEnName;
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
            return RedirectToAction("Index", "LessorClassification");
        }


    }
}
