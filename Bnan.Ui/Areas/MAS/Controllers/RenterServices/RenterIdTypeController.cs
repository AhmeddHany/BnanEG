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
    public class RenterIdtypeController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterIdtype _masRenterIdtype;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterIdtypeController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterIdtype;


        public RenterIdtypeController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterIdtype masRenterIdtype, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterIdtypeController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterIdtype = masRenterIdtype;
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
            var renterIdtypes = await _unitOfWork.CrMasSupRenterIdtype
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterIdtypeStatus == Status.Active, new[] { "CrMasRenterInformations" });

            // If no active licenses, retrieve all licenses
            if (!renterIdtypes.Any())
            {
                renterIdtypes = await _unitOfWork.CrMasSupRenterIdtype
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterIdtypeStatus == Status.Hold,
                                              new[] { "CrMasRenterInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterIdtypes);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterIdtypeByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterIdtypesAll = await _unitOfWork.CrMasSupRenterIdtype.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterIdtypeStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterIdtypeStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterIdtypeStatus == Status.Hold, new[] { "CrMasRenterInformations" });

                if (status == Status.All)
                {
                    var FilterAll = RenterIdtypesAll.FindAll(x => x.CrMasSupRenterIdtypeStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterIdtypeArName.Contains(search) ||
                                                                          x.CrMasSupRenterIdtypeEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterIdtypeCode.Contains(search)));
                    return PartialView("_DataTableRenterIdtype", FilterAll);
                }
                var FilterByStatus = RenterIdtypesAll.FindAll(x => x.CrMasSupRenterIdtypeStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterIdtypeArName.Contains(search) ||
                                                                           x.CrMasSupRenterIdtypeEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterIdtypeCode.Contains(search)));
                return PartialView("_DataTableRenterIdtype", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterIdtype()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "RenterIdtype");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterIdtype");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterIdtype");
            }
            // Set Title 
            RenterIdtypeVM renterIdtypeVM = new RenterIdtypeVM();
            renterIdtypeVM.CrMasSupRenterIdtypeCode = await GenerateLicenseCodeAsync();
            return View(renterIdtypeVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterIdtype(RenterIdtypeVM renterIdtypeVM)
        {

            if (renterIdtypeVM.CrMasSupRenterIdtypeNaqlCode == null) renterIdtypeVM.CrMasSupRenterIdtypeNaqlCode = 0;
            if (renterIdtypeVM.CrMasSupRenterIdtypeNaqlId == null) renterIdtypeVM.CrMasSupRenterIdtypeNaqlId = 0;
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterIdtypeVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterIdtype", renterIdtypeVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterIdtypeEntity = _mapper.Map<CrMasSupRenterIdtype>(renterIdtypeVM);

                // Check if the entity already exists
                if (await _masRenterIdtype.ExistsByDetailsAsync(renterIdtypeEntity))
                {
                    await AddModelErrorsAsync(renterIdtypeEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterIdtype", renterIdtypeVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 9)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterIdtype", renterIdtypeVM);
                }
                // Generate and set the Driving License Code
                renterIdtypeVM.CrMasSupRenterIdtypeCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterIdtypeEntity.CrMasSupRenterIdtypeStatus = "A";
                await _unitOfWork.CrMasSupRenterIdtype.AddAsync(renterIdtypeEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterIdtypeEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterIdtype", renterIdtypeVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            // if value with code less than 0 Deleted
            if (long.Parse(id) < 0 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterIdtype");
            }
            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasSupRenterIdtype.FindAsync(x => x.CrMasSupRenterIdtypeCode == id, new[] { "CrMasRenterInformations" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterIdtype");
            }
            if (contract.CrMasSupRenterIdtypeNaqlCode == null) contract.CrMasSupRenterIdtypeNaqlCode = 0;
            if (contract.CrMasSupRenterIdtypeNaqlId == null) contract.CrMasSupRenterIdtypeNaqlId = 0;
            var model = _mapper.Map<RenterIdtypeVM>(contract);
            model.RentersHave_withType_Count = contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterIdtypeVM renterIdtypeVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterIdtypeVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterIdtype");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterIdtypeVM);
                }
                var renterIdtypeEntity = _mapper.Map<CrMasSupRenterIdtype>(renterIdtypeVM);

                if (renterIdtypeVM.CrMasSupRenterIdtypeNaqlCode == null) renterIdtypeEntity.CrMasSupRenterIdtypeNaqlCode = 0;
                if (renterIdtypeVM.CrMasSupRenterIdtypeNaqlId == null) renterIdtypeEntity.CrMasSupRenterIdtypeNaqlId = 0;
                // Check if the entity already exists
                if (await _masRenterIdtype.ExistsByDetailsAsync(renterIdtypeEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterIdtypeEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterIdtypeVM);
                }

                _unitOfWork.CrMasSupRenterIdtype.Update(renterIdtypeEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterIdtypeEntity, Status.Update);
                return RedirectToAction("Index", "RenterIdtype");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterIdtypeVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupRenterIdtype.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterIdtype.CheckIfCanDeleteIt(licence.CrMasSupRenterIdtypeCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterIdtypeStatus = status;
                _unitOfWork.CrMasSupRenterIdtype.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupRenterIdtype entity)
        {

            if (await _masRenterIdtype.ExistsByArabicNameAsync(entity.CrMasSupRenterIdtypeArName, entity.CrMasSupRenterIdtypeCode))
            {
                ModelState.AddModelError("CrMasSupRenterIdtypeArName", _localizer["Existing"]);
            }

            if (await _masRenterIdtype.ExistsByEnglishNameAsync(entity.CrMasSupRenterIdtypeEnName, entity.CrMasSupRenterIdtypeCode))
            {
                ModelState.AddModelError("CrMasSupRenterIdtypeEnName", _localizer["Existing"]);
            }

            if (await _masRenterIdtype.ExistsByNaqlCodeAsync((int)entity.CrMasSupRenterIdtypeNaqlCode, entity.CrMasSupRenterIdtypeCode))
            {
                ModelState.AddModelError("CrMasSupRenterIdtypeNaqlCode", _localizer["Existing"]);
            }

            if (await _masRenterIdtype.ExistsByNaqlIdAsync((int)entity.CrMasSupRenterIdtypeNaqlId, entity.CrMasSupRenterIdtypeCode))
            {
                ModelState.AddModelError("CrMasSupRenterIdtypeNaqlId", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterIdtypes = await _unitOfWork.CrMasSupRenterIdtype.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterIdtypes != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterIdtypeArName" && All_RenterIdtypes.Any(x => x.CrMasSupRenterIdtypeArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterIdtypeArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterIdtypeEnName" && All_RenterIdtypes.Any(x => x.CrMasSupRenterIdtypeEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterIdtypeEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupRenterIdtypeNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_RenterIdtypes.Any(x => x.CrMasSupRenterIdtypeNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterIdtypeNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupRenterIdtypeNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_RenterIdtypes.Any(x => x.CrMasSupRenterIdtypeNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterIdtypeNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterIdtype.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterIdtypeCode) + 1).ToString() : "0";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterIdtype licence, string status)
        {


            var recordAr = licence.CrMasSupRenterIdtypeArName;
            var recordEn = licence.CrMasSupRenterIdtypeEnName;
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
            return RedirectToAction("Index", "RenterIdtype");
        }


    }
}
