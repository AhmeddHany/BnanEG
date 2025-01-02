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
    public class RenterDrivingLicenseController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterDrivingLicense _masRenterDrivingLicense;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterDrivingLicenseController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterDrivingLicense;


        public RenterDrivingLicenseController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterDrivingLicense masRenterDrivingLicense, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterDrivingLicenseController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterDrivingLicense = masRenterDrivingLicense;
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
            var renterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active, new[] { "CrMasRenterInformations" });

            // If no active licenses, retrieve all licenses
            if (!renterDrivingLicenses.Any())
            {
                renterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Hold,
                                              new[] { "CrMasRenterInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterDrivingLicenses);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterDrivingLicenseByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterDrivingLicensesAll = await _unitOfWork.CrMasSupRenterDrivingLicense.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterDrivingLicenseStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterDrivingLicenseStatus == Status.Hold, new[] { "CrMasRenterInformations" });

                if (status == Status.All)
                {
                    var FilterAll = RenterDrivingLicensesAll.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterDrivingLicenseArName.Contains(search) ||
                                                                          x.CrMasSupRenterDrivingLicenseEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterDrivingLicenseCode.Contains(search)));
                    return PartialView("_DataTableRenterDrivingLicense", FilterAll);
                }
                var FilterByStatus = RenterDrivingLicensesAll.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterDrivingLicenseArName.Contains(search) ||
                                                                           x.CrMasSupRenterDrivingLicenseEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterDrivingLicenseCode.Contains(search)));
                return PartialView("_DataTableRenterDrivingLicense", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterDrivingLicense()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            // Set Title 
            RenterDrivingLicenseVM renterDrivingLicenseVM = new RenterDrivingLicenseVM();
            renterDrivingLicenseVM.CrMasSupRenterDrivingLicenseCode = await GenerateLicenseCodeAsync();
            return View(renterDrivingLicenseVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterDrivingLicense(RenterDrivingLicenseVM renterDrivingLicenseVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || renterDrivingLicenseVM == null)
            {
                return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
            }
            try
            {
                // Map ViewModel to Entity
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupRenterDrivingLicense>(renterDrivingLicenseVM);

                renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlCode ??= 0;
                renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masRenterDrivingLicense.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    await AddModelErrorsAsync(renterDrivingLicenseEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
                }
                // Generate and set the Driving License Code
                renterDrivingLicenseVM.CrMasSupRenterDrivingLicenseCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseStatus = "A";
                await _unitOfWork.CrMasSupRenterDrivingLicense.AddAsync(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterDrivingLicenseEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 2 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            var contract = await _unitOfWork.CrMasSupRenterDrivingLicense.FindAsync(x => x.CrMasSupRenterDrivingLicenseCode == id, new[] { "CrMasRenterInformations" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            var model = _mapper.Map<RenterDrivingLicenseVM>(contract);
            model.CrMasSupRenterDrivingLicenseNaqlCode ??= 0;
            model.CrMasSupRenterDrivingLicenseNaqlId ??= 0;
            model.RentersHave_withType_Count = contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterDrivingLicenseVM renterDrivingLicenseVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && renterDrivingLicenseVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterDrivingLicenseVM);
                }
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupRenterDrivingLicense>(renterDrivingLicenseVM);
                renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlCode ??= 0;
                renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masRenterDrivingLicense.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    await AddModelErrorsAsync(renterDrivingLicenseEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterDrivingLicenseVM);
                }

                _unitOfWork.CrMasSupRenterDrivingLicense.Update(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterDrivingLicenseEntity, Status.Update);
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", renterDrivingLicenseVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupRenterDrivingLicense.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterDrivingLicense.CheckIfCanDeleteIt(licence.CrMasSupRenterDrivingLicenseCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterDrivingLicenseStatus = status;
                _unitOfWork.CrMasSupRenterDrivingLicense.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupRenterDrivingLicense entity)
        {

            if (await _masRenterDrivingLicense.ExistsByArabicNameAsync(entity.CrMasSupRenterDrivingLicenseArName, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseArName", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByEnglishNameAsync(entity.CrMasSupRenterDrivingLicenseEnName, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseEnName", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByNaqlCodeAsync((int)entity.CrMasSupRenterDrivingLicenseNaqlCode, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseNaqlCode", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByNaqlIdAsync((int)entity.CrMasSupRenterDrivingLicenseNaqlId, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseNaqlId", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterDrivingLicenses != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterDrivingLicenseArName" && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterDrivingLicenseEnName" && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupRenterDrivingLicenseNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupRenterDrivingLicenseNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterDrivingLicenseCode) + 1).ToString() : "1";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterDrivingLicense licence, string status)
        {


            var recordAr = licence.CrMasSupRenterDrivingLicenseArName;
            var recordEn = licence.CrMasSupRenterDrivingLicenseEnName;
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
            return RedirectToAction("Index", "RenterDrivingLicense");
        }


    }
}
