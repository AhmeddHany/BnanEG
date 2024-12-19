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
    public class RenterProfessionController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterProfession _masRenterProfession;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterProfessionController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterProfession;


        public RenterProfessionController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterProfession masRenterProfession, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterProfessionController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterProfession = masRenterProfession;
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
            var renterProfessions = await _unitOfWork.CrMasSupRenterProfession
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterProfessionsStatus == Status.Active, new[] { "CrMasRenterInformations" });

            // If no active licenses, retrieve all licenses
            if (!renterProfessions.Any())
            {
                renterProfessions = await _unitOfWork.CrMasSupRenterProfession
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterProfessionsStatus == Status.Hold,
                                              new[] { "CrMasRenterInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterProfessions);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterProfessionByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterProfessionsAll = await _unitOfWork.CrMasSupRenterProfession.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterProfessionsStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterProfessionsStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterProfessionsStatus == Status.Hold, new[] { "CrMasRenterInformations" });

                if (status == Status.All)
                {
                    var FilterAll = RenterProfessionsAll.FindAll(x => x.CrMasSupRenterProfessionsStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterProfessionsArName.Contains(search) ||
                                                                          x.CrMasSupRenterProfessionsEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterProfessionsCode.Contains(search)));
                    return PartialView("_DataTableRenterProfession", FilterAll);
                }
                var FilterByStatus = RenterProfessionsAll.FindAll(x => x.CrMasSupRenterProfessionsStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterProfessionsArName.Contains(search) ||
                                                                           x.CrMasSupRenterProfessionsEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterProfessionsCode.Contains(search)));
                return PartialView("_DataTableRenterProfession", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterProfession()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "RenterProfession");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterProfession");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 1499999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterProfession");
            }
            // Set Title 
            RenterProfessionVM renterProfessionVM = new RenterProfessionVM();
            renterProfessionVM.CrMasSupRenterProfessionsCode = await GenerateLicenseCodeAsync();
            return View(renterProfessionVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterProfession(RenterProfessionVM renterProfessionVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterProfessionVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterProfession", renterProfessionVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterProfessionEntity = _mapper.Map<CrMasSupRenterProfession>(renterProfessionVM);

                // Check if the entity already exists
                if (await _masRenterProfession.ExistsByDetailsAsync(renterProfessionEntity))
                {
                    await AddModelErrorsAsync(renterProfessionEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterProfession", renterProfessionVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 1499999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterProfession", renterProfessionVM);
                }
                // Generate and set the Driving License Code
                renterProfessionVM.CrMasSupRenterProfessionsCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterProfessionEntity.CrMasSupRenterProfessionsStatus = "A";
                await _unitOfWork.CrMasSupRenterProfession.AddAsync(renterProfessionEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterProfessionEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterProfession", renterProfessionVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 1400000002 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterProfession");
            }
            var contract = await _unitOfWork.CrMasSupRenterProfession.FindAsync(x => x.CrMasSupRenterProfessionsCode == id, new[] { "CrMasRenterInformations" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterProfession");
            }
            var model = _mapper.Map<RenterProfessionVM>(contract);
            model.RentersHave_withType_Count = contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterProfessionVM renterProfessionVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterProfessionVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterProfession");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterProfessionVM);
                }
                var renterProfessionEntity = _mapper.Map<CrMasSupRenterProfession>(renterProfessionVM);

                // Check if the entity already exists
                if (await _masRenterProfession.ExistsByDetailsAsync(renterProfessionEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterProfessionEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterProfessionVM);
                }

                _unitOfWork.CrMasSupRenterProfession.Update(renterProfessionEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterProfessionEntity, Status.Update);
                return RedirectToAction("Index", "RenterProfession");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterProfessionVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupRenterProfession.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterProfession.CheckIfCanDeleteIt(licence.CrMasSupRenterProfessionsCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterProfessionsStatus = status;
                _unitOfWork.CrMasSupRenterProfession.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupRenterProfession entity)
        {

            if (await _masRenterProfession.ExistsByArabicNameAsync(entity.CrMasSupRenterProfessionsArName, entity.CrMasSupRenterProfessionsCode))
            {
                ModelState.AddModelError("CrMasSupRenterProfessionsArName", _localizer["Existing"]);
            }

            if (await _masRenterProfession.ExistsByEnglishNameAsync(entity.CrMasSupRenterProfessionsEnName, entity.CrMasSupRenterProfessionsCode))
            {
                ModelState.AddModelError("CrMasSupRenterProfessionsEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterProfessions = await _unitOfWork.CrMasSupRenterProfession.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterProfessions != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterProfessionsArName" && All_RenterProfessions.Any(x => x.CrMasSupRenterProfessionsArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterProfessionsArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterProfessionsEnName" && All_RenterProfessions.Any(x => x.CrMasSupRenterProfessionsEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterProfessionsEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterProfession.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterProfessionsCode) + 1).ToString() : "1400000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterProfession licence, string status)
        {


            var recordAr = licence.CrMasSupRenterProfessionsArName;
            var recordEn = licence.CrMasSupRenterProfessionsEnName;
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
            return RedirectToAction("Index", "RenterProfession");
        }


    }
}
