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
namespace Bnan.Ui.Areas.MAS.Controllers.RenterServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class RenterGenderController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterGender _masRenterGender;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterGenderController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterGender;


        public RenterGenderController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterGender masRenterGender, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterGenderController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterGender = masRenterGender;
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
            var renterGenders = await _unitOfWork.CrMasSupRenterGender
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterGenderStatus == Status.Active, new[] { "CrCasRenterLessors" });

            // If no active licenses, retrieve all licenses
            if (!renterGenders.Any())
            {
                renterGenders = await _unitOfWork.CrMasSupRenterGender
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterGenderStatus == Status.Hold, new[] { "CrCasRenterLessors" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterGenders);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterGenderByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterGendersAll = await _unitOfWork.CrMasSupRenterGender.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterGenderStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterGenderStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterGenderStatus == Status.Hold, new[] { "CrCasRenterLessors" });

                if (status == Status.All)
                {
                    var FilterAll = RenterGendersAll.FindAll(x => x.CrMasSupRenterGenderStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterGenderArName.Contains(search) ||
                                                                          x.CrMasSupRenterGenderEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterGenderCode.Contains(search)));
                    return PartialView("_DataTableRenterGender", FilterAll);
                }
                var FilterByStatus = RenterGendersAll.FindAll(x => x.CrMasSupRenterGenderStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterGenderArName.Contains(search) ||
                                                                           x.CrMasSupRenterGenderEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterGenderCode.Contains(search)));
                return PartialView("_DataTableRenterGender", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterGender()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "RenterGender");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterGender");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 1199999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterGender");
            }
            // Set Title 
            RenterGenderVM renterGenderVM = new RenterGenderVM();
            renterGenderVM.CrMasSupRenterGenderCode = await GenerateLicenseCodeAsync();
            return View(renterGenderVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterGender(RenterGenderVM renterGenderVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterGenderVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterGender", renterGenderVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterGenderEntity = _mapper.Map<CrMasSupRenterGender>(renterGenderVM);

                // Check if the entity already exists
                if (await _masRenterGender.ExistsByDetailsAsync(renterGenderEntity))
                {
                    await AddModelErrorsAsync(renterGenderEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterGender", renterGenderVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 1199999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterGender", renterGenderVM);
                }
                // Generate and set the Driving License Code
                renterGenderVM.CrMasSupRenterGenderCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterGenderEntity.CrMasSupRenterGenderStatus = "A";
                await _unitOfWork.CrMasSupRenterGender.AddAsync(renterGenderEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterGenderEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterGender", renterGenderVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 1100000002 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterGender");
            }

            var contract = await _unitOfWork.CrMasSupRenterGender.FindAsync(x => x.CrMasSupRenterGenderCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterGender");
            }
            var model = _mapper.Map<RenterGenderVM>(contract);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterGenderVM renterGenderVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterGenderVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterGender");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterGenderVM);
                }
                var renterGenderEntity = _mapper.Map<CrMasSupRenterGender>(renterGenderVM);

                // Check if the entity already exists
                if (await _masRenterGender.ExistsByDetailsAsync(renterGenderEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterGenderEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterGenderVM);
                }

                _unitOfWork.CrMasSupRenterGender.Update(renterGenderEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterGenderEntity, Status.Update);
                return RedirectToAction("Index", "RenterGender");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterGenderVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupRenterGender.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterGender.CheckIfCanDeleteIt(licence.CrMasSupRenterGenderCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterGenderStatus = status;
                _unitOfWork.CrMasSupRenterGender.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupRenterGender entity)
        {

            if (await _masRenterGender.ExistsByArabicNameAsync(entity.CrMasSupRenterGenderArName, entity.CrMasSupRenterGenderCode))
            {
                ModelState.AddModelError("CrMasSupRenterGenderArName", _localizer["Existing"]);
            }

            if (await _masRenterGender.ExistsByEnglishNameAsync(entity.CrMasSupRenterGenderEnName, entity.CrMasSupRenterGenderCode))
            {
                ModelState.AddModelError("CrMasSupRenterGenderEnName", _localizer["Existing"]);
            }

        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterGenders = await _unitOfWork.CrMasSupRenterGender.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterGenders != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterGenderArName" && All_RenterGenders.Any(x => x.CrMasSupRenterGenderArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterGenderArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterGenderEnName" && All_RenterGenders.Any(x => x.CrMasSupRenterGenderEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterGenderEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterGender.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterGenderCode) + 1).ToString() : "1100000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterGender licence, string status)
        {


            var recordAr = licence.CrMasSupRenterGenderArName;
            var recordEn = licence.CrMasSupRenterGenderEnName;
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
            return RedirectToAction("Index", "RenterGender");
        }


    }
}
