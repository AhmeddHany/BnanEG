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
namespace Bnan.Ui.Areas.MAS.Controllers.Companies
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class LessorMarketingController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasLessorMarketing _masLessorMarketing;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<LessorMarketingController> _localizer;
        private readonly string pageNumber = SubTasks.lessor_Marketing;


        public LessorMarketingController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasLessorMarketing masLessorMarketing, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<LessorMarketingController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masLessorMarketing = masLessorMarketing;
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
            var banks = await _unitOfWork.CrMasSupContractSource
                .FindAllAsNoTrackingAsync(x => x.CrMasSupContractSourceStatus == Status.Active);


            // If no active licenses, retrieve all licenses
            if (!banks.Any())
            {
                banks = await _unitOfWork.CrMasSupContractSource
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupContractSourceStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            MASContractSourceVM vm = new MASContractSourceVM();
            vm.crMasSupContractSource = banks;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetLessorMarketingByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var LessorMarketingsAll = await _unitOfWork.CrMasSupContractSource.FindAllAsNoTrackingAsync(x => x.CrMasSupContractSourceStatus == Status.Active ||
                                                                                                                            x.CrMasSupContractSourceStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupContractSourceStatus == Status.Hold);
                MASContractSourceVM vm = new MASContractSourceVM();
                if (status == Status.All)
                {
                    var FilterAll = LessorMarketingsAll.FindAll(x => x.CrMasSupContractSourceStatus != Status.Deleted &&
                                                                         (x.CrMasSupContractSourceArName.Contains(search) ||
                                                                          x.CrMasSupContractSourceEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractSourceCode.Contains(search)));
                    vm.crMasSupContractSource = FilterAll;
                    return PartialView("_DataTableLessorMarketing", vm);
                }
                var FilterByStatus = LessorMarketingsAll.FindAll(x => x.CrMasSupContractSourceStatus == status &&
                                                                            (
                                                                           x.CrMasSupContractSourceArName.Contains(search) ||
                                                                           x.CrMasSupContractSourceEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupContractSourceCode.Contains(search)));
                vm.crMasSupContractSource = FilterByStatus;
                return PartialView("_DataTableLessorMarketing", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddLessorMarketing()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "LessorMarketing");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "LessorMarketing");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "LessorMarketing");
            }
            var all_keys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSysCallingKey
                {
                    CrMasSysCallingKeysNo = x.CrMasSysCallingKeysNo,
                })
                );
            // Set Title 
            MASContractSourceVM lessorMarketingVM = new MASContractSourceVM();
            lessorMarketingVM.CrMasSupContractSourceCode = await GenerateLicenseCodeAsync();
            lessorMarketingVM.keys = all_keys;
            return View(lessorMarketingVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddLessorMarketing(MASContractSourceVM lessorMarketingVM)
        {

            var all_keys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSysCallingKey
                {
                    CrMasSysCallingKeysNo = x.CrMasSysCallingKeysNo,
                })
                );
            lessorMarketingVM.keys = all_keys;

            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || lessorMarketingVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddLessorMarketing", lessorMarketingVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var bankEntity = _mapper.Map<CrMasSupContractSource>(lessorMarketingVM);

                // Check if the entity already exists
                if (await _masLessorMarketing.ExistsByDetailsAsync(bankEntity))
                {
                    await AddModelErrorsAsync(bankEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddLessorMarketing", lessorMarketingVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddLessorMarketing", lessorMarketingVM);
                }
                // Generate and set the Driving License Code
                lessorMarketingVM.CrMasSupContractSourceCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                bankEntity.CrMasSupContractSourceStatus = "A";
                await _unitOfWork.CrMasSupContractSource.AddAsync(bankEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, bankEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddLessorMarketing", lessorMarketingVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupContractSource.FindAsync(x => x.CrMasSupContractSourceCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "LessorMarketing");
            }
            var all_keys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSysCallingKey
                {
                    CrMasSysCallingKeysNo = x.CrMasSysCallingKeysNo.Trim(),
                })
                );
            var model = _mapper.Map<MASContractSourceVM>(contract);
            string[] parts = contract.CrMasSupContractSourceMobile?.ToString().Split('-');
            model.keys = all_keys;
            if (parts.Length == 2)
            {
                model.mob = parts[1];
                model.key2 = parts[0];
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(MASContractSourceVM lessorMarketingVM)
        {
            var all_keys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSysCallingKey
                {
                    CrMasSysCallingKeysNo = x.CrMasSysCallingKeysNo,
                })
                );
            lessorMarketingVM.keys = all_keys;

            var user = await _userManager.GetUserAsync(User);
            if (user == null && lessorMarketingVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "LessorMarketing");
            }
            string[] parts = lessorMarketingVM.CrMasSupContractSourceMobile?.ToString().Split('-');
            lessorMarketingVM.keys = all_keys;
            if (parts.Length == 2)
            {
                lessorMarketingVM.mob = parts[1];
                lessorMarketingVM.key2 = parts[0];
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", lessorMarketingVM);
                }
                var bankEntity = _mapper.Map<CrMasSupContractSource>(lessorMarketingVM);

                // Check if the entity already exists
                if (await _masLessorMarketing.ExistsByDetailsAsync(bankEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(bankEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", lessorMarketingVM);
                }

                _unitOfWork.CrMasSupContractSource.Update(bankEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, bankEntity, Status.Update);
                return RedirectToAction("Index", "LessorMarketing");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", lessorMarketingVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupContractSource.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                //if (status == Status.Deleted) { if (!await _masLessorMarketing.CheckIfCanDeleteIt(licence.CrMasSupContractSourceCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupContractSourceStatus = status;
                _unitOfWork.CrMasSupContractSource.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupContractSource entity)
        {

            if (await _masLessorMarketing.ExistsByArabicNameAsync(entity.CrMasSupContractSourceArName, entity.CrMasSupContractSourceCode))
            {
                ModelState.AddModelError("CrMasSupContractSourceArName", _localizer["Existing"]);
            }

            if (await _masLessorMarketing.ExistsByEnglishNameAsync(entity.CrMasSupContractSourceEnName, entity.CrMasSupContractSourceCode))
            {
                ModelState.AddModelError("CrMasSupContractSourceEnName", _localizer["Existing"]);
            }
            //if (await _masLessorMarketing.ExistsByEmailAsync(entity.CrMasSupContractSourceEmail, entity.CrMasSupContractSourceCode))
            //{
            //    ModelState.AddModelError("CrMasSupContractSourceEmail", _localizer["Existing"]);
            //}
            if (await _masLessorMarketing.ExistsByMobileAsync(entity.CrMasSupContractSourceMobile, entity.CrMasSupContractSourceCode))
            {
                ModelState.AddModelError("CrMasSupContractSourceMobile", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_LessorMarketings = await _unitOfWork.CrMasSupContractSource.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_LessorMarketings != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupContractSourceArName" && All_LessorMarketings.Any(x => x.CrMasSupContractSourceArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractSourceArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupContractSourceEnName" && All_LessorMarketings.Any(x => x.CrMasSupContractSourceEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractSourceEnName", Message = _localizer["Existing"] });
                }
                //// Check for existing email
                //else if (existName == "CrMasSupContractSourceEmail" && All_LessorMarketings.Any(x => x.CrMasSupContractSourceEmail?.ToLower() == dataField.ToLower()))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSupContractSourceEmail", Message = _localizer["Existing"] });
                //}
                // Check for existing mobile
                else if (existName == "CrMasSupContractSourceMobile" && All_LessorMarketings.Any(x => x.CrMasSupContractSourceMobile == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractSourceMobile", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupContractSource.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupContractSourceCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupContractSource licence, string status)
        {


            var recordAr = licence.CrMasSupContractSourceArName;
            var recordEn = licence.CrMasSupContractSourceEnName;
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
            return RedirectToAction("Index", "LessorMarketing");
        }


    }
}
