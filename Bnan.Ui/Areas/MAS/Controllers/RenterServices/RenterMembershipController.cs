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
    public class RenterMembershipController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterMembership _masRenterMembership;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterMembershipController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterMembership;


        public RenterMembershipController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterMembership masRenterMembership, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterMembershipController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterMembership = masRenterMembership;
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
            var renterMemberships = await _unitOfWork.CrMasSupRenterMembership
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterMembershipStatus == Status.Active, new[] { "CrCasRenterLessors" });

            // If no active licenses, retrieve all licenses
            if (!renterMemberships.Any())
            {
                renterMemberships = await _unitOfWork.CrMasSupRenterMembership
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterMembershipStatus == Status.Hold, new[] { "CrCasRenterLessors" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterMemberships);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterMembershipByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterMembershipsAll = await _unitOfWork.CrMasSupRenterMembership.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterMembershipStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterMembershipStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterMembershipStatus == Status.Hold, new[] { "CrCasRenterLessors" });

                if (status == Status.All)
                {
                    var FilterAll = RenterMembershipsAll.FindAll(x => x.CrMasSupRenterMembershipStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterMembershipArName.Contains(search) ||
                                                                          x.CrMasSupRenterMembershipEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterMembershipCode.Contains(search)));
                    return PartialView("_DataTableRenterMembership", FilterAll);
                }
                var FilterByStatus = RenterMembershipsAll.FindAll(x => x.CrMasSupRenterMembershipStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterMembershipArName.Contains(search) ||
                                                                           x.CrMasSupRenterMembershipEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterMembershipCode.Contains(search)));
                return PartialView("_DataTableRenterMembership", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterMembership()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "RenterMembership");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterMembership");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 1699999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterMembership");
            }
            // Set Title 
            RenterMembershipVM renterMembershipVM = new RenterMembershipVM();
            renterMembershipVM.CrMasSupRenterMembershipCode = await GenerateLicenseCodeAsync();
            return View(renterMembershipVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterMembership(RenterMembershipVM renterMembershipVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterMembershipVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterMembership", renterMembershipVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterMembershipEntity = _mapper.Map<CrMasSupRenterMembership>(renterMembershipVM);

                // Check if the entity already exists
                if (await _masRenterMembership.ExistsByDetailsAsync(renterMembershipEntity))
                {
                    await AddModelErrorsAsync(renterMembershipEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterMembership", renterMembershipVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 1699999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterMembership", renterMembershipVM);
                }
                // Generate and set the Driving License Code
                renterMembershipVM.CrMasSupRenterMembershipCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterMembershipEntity.CrMasSupRenterMembershipStatus = "A";
                await _unitOfWork.CrMasSupRenterMembership.AddAsync(renterMembershipEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterMembershipEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterMembership", renterMembershipVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupRenterMembership.FindAsync(x => x.CrMasSupRenterMembershipCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterMembership");
            }
            var model = _mapper.Map<RenterMembershipVM>(contract);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterMembershipVM renterMembershipVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterMembershipVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterMembership");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterMembershipVM);
                }
                var renterMembershipEntity = _mapper.Map<CrMasSupRenterMembership>(renterMembershipVM);

                // Check if the entity already exists
                if (await _masRenterMembership.ExistsByDetailsAsync(renterMembershipEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterMembershipEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterMembershipVM);
                }

                _unitOfWork.CrMasSupRenterMembership.Update(renterMembershipEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterMembershipEntity, Status.Update);
                return RedirectToAction("Index", "RenterMembership");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterMembershipVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupRenterMembership.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterMembership.CheckIfCanDeleteIt(licence.CrMasSupRenterMembershipCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterMembershipStatus = status;
                _unitOfWork.CrMasSupRenterMembership.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupRenterMembership entity)
        {

            if (await _masRenterMembership.ExistsByArabicNameAsync(entity.CrMasSupRenterMembershipArName, entity.CrMasSupRenterMembershipCode))
            {
                ModelState.AddModelError("CrMasSupRenterMembershipArName", _localizer["Existing"]);
            }

            if (await _masRenterMembership.ExistsByEnglishNameAsync(entity.CrMasSupRenterMembershipEnName, entity.CrMasSupRenterMembershipCode))
            {
                ModelState.AddModelError("CrMasSupRenterMembershipEnName", _localizer["Existing"]);
            }

        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterMemberships = await _unitOfWork.CrMasSupRenterMembership.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterMemberships != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterMembershipArName" && All_RenterMemberships.Any(x => x.CrMasSupRenterMembershipArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterMembershipArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterMembershipEnName" && All_RenterMemberships.Any(x => x.CrMasSupRenterMembershipEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterMembershipEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterMembership.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterMembershipCode) + 1).ToString() : "1600000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterMembership licence, string status)
        {


            var recordAr = licence.CrMasSupRenterMembershipArName;
            var recordEn = licence.CrMasSupRenterMembershipEnName;
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
            return RedirectToAction("Index", "RenterMembership");
        }


    }
}
