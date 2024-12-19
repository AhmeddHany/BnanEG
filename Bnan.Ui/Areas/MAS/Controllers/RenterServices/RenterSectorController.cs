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
    public class RenterSectorController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterSector _masRenterSector;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterSectorController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterSector;


        public RenterSectorController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterSector masRenterSector, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterSectorController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterSector = masRenterSector;
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
            var renterSectors = await _unitOfWork.CrMasSupRenterSector
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterSectorStatus == Status.Active, new[] { "CrMasRenterInformations" });

            // If no active licenses, retrieve all licenses
            if (!renterSectors.Any())
            {
                renterSectors = await _unitOfWork.CrMasSupRenterSector
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterSectorStatus == Status.Hold,
                                              new[] { "CrMasRenterInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterSectors);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterSectorByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterSectorsAll = await _unitOfWork.CrMasSupRenterSector.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterSectorStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterSectorStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterSectorStatus == Status.Hold, new[] { "CrMasRenterInformations" });

                if (status == Status.All)
                {
                    var FilterAll = RenterSectorsAll.FindAll(x => x.CrMasSupRenterSectorStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterSectorArName.Contains(search) ||
                                                                          x.CrMasSupRenterSectorEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterSectorCode.Contains(search)));
                    return PartialView("_DataTableRenterSector", FilterAll);
                }
                var FilterByStatus = RenterSectorsAll.FindAll(x => x.CrMasSupRenterSectorStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterSectorArName.Contains(search) ||
                                                                           x.CrMasSupRenterSectorEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterSectorCode.Contains(search)));
                return PartialView("_DataTableRenterSector", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterSector()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "RenterSector");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterSector");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterSector");
            }
            // Set Title 
            RenterSectorVM renterSectorVM = new RenterSectorVM();
            renterSectorVM.CrMasSupRenterSectorCode = await GenerateLicenseCodeAsync();
            return View(renterSectorVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterSector(RenterSectorVM renterSectorVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterSectorVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterSector", renterSectorVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterSectorEntity = _mapper.Map<CrMasSupRenterSector>(renterSectorVM);

                // Check if the entity already exists
                if (await _masRenterSector.ExistsByDetailsAsync(renterSectorEntity))
                {
                    await AddModelErrorsAsync(renterSectorEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterSector", renterSectorVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterSector", renterSectorVM);
                }
                // Generate and set the Driving License Code
                renterSectorVM.CrMasSupRenterSectorCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterSectorEntity.CrMasSupRenterSectorStatus = "A";
                await _unitOfWork.CrMasSupRenterSector.AddAsync(renterSectorEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterSectorEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterSector", renterSectorVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasSupRenterSector.FindAsync(x => x.CrMasSupRenterSectorCode == id, new[] { "CrMasRenterInformations" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterSector");
            }
            var model = _mapper.Map<RenterSectorVM>(contract);
            model.RentersHave_withType_Count = contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterSectorVM renterSectorVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterSectorVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterSector");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterSectorVM);
                }
                var renterSectorEntity = _mapper.Map<CrMasSupRenterSector>(renterSectorVM);
                // Check if the entity already exists
                if (await _masRenterSector.ExistsByDetailsAsync(renterSectorEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterSectorEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterSectorVM);
                }

                _unitOfWork.CrMasSupRenterSector.Update(renterSectorEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterSectorEntity, Status.Update);
                return RedirectToAction("Index", "RenterSector");
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

            var licence = await _unitOfWork.CrMasSupRenterSector.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterSector.CheckIfCanDeleteIt(licence.CrMasSupRenterSectorCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterSectorStatus = status;
                _unitOfWork.CrMasSupRenterSector.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupRenterSector entity)
        {

            if (await _masRenterSector.ExistsByArabicNameAsync(entity.CrMasSupRenterSectorArName, entity.CrMasSupRenterSectorCode))
            {
                ModelState.AddModelError("CrMasSupRenterSectorArName", _localizer["Existing"]);
            }

            if (await _masRenterSector.ExistsByEnglishNameAsync(entity.CrMasSupRenterSectorEnName, entity.CrMasSupRenterSectorCode))
            {
                ModelState.AddModelError("CrMasSupRenterSectorEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterSectors = await _unitOfWork.CrMasSupRenterSector.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterSectors != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterSectorArName" && All_RenterSectors.Any(x => x.CrMasSupRenterSectorArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterSectorArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterSectorEnName" && All_RenterSectors.Any(x => x.CrMasSupRenterSectorEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterSectorEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterSector.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterSectorCode) + 1).ToString() : "0";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterSector licence, string status)
        {


            var recordAr = licence.CrMasSupRenterSectorArName;
            var recordEn = licence.CrMasSupRenterSectorEnName;
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
            return RedirectToAction("Index", "RenterSector");
        }


    }
}
