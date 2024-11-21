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
namespace Bnan.Ui.Areas.MAS.Controllers
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class AccountReferenceController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasAccountReference _masAccountReference;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<AccountReferenceController> _localizer;

        public AccountReferenceController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasAccountReference masAccountReference, IBaseRepo BaseRepo,IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<AccountReferenceController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masAccountReference = masAccountReference;
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

            var pageNumber = SubTasks.CrMasSupAccountReference;
            // Set page titles
            await SetPageTitleAsync(string.Empty, pageNumber);

            // Retrieve active driving licenses
            var renterDrivingLicenses = await _unitOfWork.CrMasSupAccountReference
                .FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active );

            var AccountReferences_count = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrMasSupAccountReference>(
                //predicate: x => x.status != Status.Deleted,
                columnSelector: x => x.CrCasAccountReceiptReferenceType  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );


            // If no active licenses, retrieve all licenses
            if (!renterDrivingLicenses.Any())
            {
                renterDrivingLicenses = await _unitOfWork.CrMasSupAccountReference
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            MasAccountReferenceVM vm = new MasAccountReferenceVM();
            vm.crMasSupAccountReference = renterDrivingLicenses;
            vm.AccountReferences_count = AccountReferences_count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAccountReferenceByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var AccountReferencesAll = await _unitOfWork.CrMasSupAccountReference.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active ||
                                                                                                                            x.CrMasSupAccountPaymentMethodStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupAccountPaymentMethodStatus == Status.Hold );
                var AccountReferences_count = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrMasSupAccountReference>(
                    //predicate: x => x.Status != Status.Deleted,
                    columnSelector: x => x.CrCasAccountReceiptReferenceType  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                MasAccountReferenceVM vm = new MasAccountReferenceVM();
                vm.AccountReferences_count = AccountReferences_count;
                if (status == Status.All)
                {
                    var FilterAll = AccountReferencesAll.FindAll(x => x.CrMasSupAccountPaymentMethodStatus != Status.Deleted &&
                                                                         (x.CrMasSupAccountReceiptReferenceArName.Contains(search) ||
                                                                          x.CrMasSupAccountReceiptReferenceEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupAccountReceiptReferenceCode.Contains(search)));
                    vm.crMasSupAccountReference = FilterAll;
                    return PartialView("_DataTableAccountReference", vm);
                }
                var FilterByStatus = AccountReferencesAll.FindAll(x => x.CrMasSupAccountPaymentMethodStatus == status &&
                                                                            (
                                                                           x.CrMasSupAccountReceiptReferenceArName.Contains(search) ||
                                                                           x.CrMasSupAccountReceiptReferenceEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupAccountReceiptReferenceCode.Contains(search)));
                vm.crMasSupAccountReference = FilterByStatus;
                return PartialView("_DataTableAccountReference", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddAccountReference()
        {
            var pageNumber = SubTasks.CrMasSupAccountReference;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "AccountReference");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "AccountReference");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (int.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "AccountReference");
            }
            // Set Title 
            MasAccountReferenceVM renterDrivingLicenseVM = new MasAccountReferenceVM();
            renterDrivingLicenseVM.CrMasSupAccountReceiptReferenceCode = await GenerateLicenseCodeAsync();
            return View(renterDrivingLicenseVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddAccountReference(MasAccountReferenceVM renterDrivingLicenseVM)
        {
            var pageNumber = SubTasks.CrMasSupAccountReference;
            
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterDrivingLicenseVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountReference", renterDrivingLicenseVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupAccountReference>(renterDrivingLicenseVM);

                // Check if the entity already exists
                if (await _masAccountReference.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    await AddModelErrorsAsync(renterDrivingLicenseEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddAccountReference", renterDrivingLicenseVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (int.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddAccountReference", renterDrivingLicenseVM);
                }
                // Generate and set the Driving License Code
                renterDrivingLicenseVM.CrMasSupAccountReceiptReferenceCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterDrivingLicenseEntity.CrMasSupAccountPaymentMethodStatus = "A";
                await _unitOfWork.CrMasSupAccountReference.AddAsync(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterDrivingLicenseEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountReference", renterDrivingLicenseVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var pageNumber = SubTasks.CrMasSupAccountReference;
            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupAccountReference.FindAsync(x => x.CrMasSupAccountReceiptReferenceCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "AccountReference");
            }
            var model = _mapper.Map<MasAccountReferenceVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(MasAccountReferenceVM renterDrivingLicenseVM)
        {
            var pageNumber = SubTasks.CrMasSupAccountReference;
            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterDrivingLicenseVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "AccountReference");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterDrivingLicenseVM);
                }
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupAccountReference>(renterDrivingLicenseVM);

                // Check if the entity already exists
                if (await _masAccountReference.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterDrivingLicenseEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterDrivingLicenseVM);
                }

                _unitOfWork.CrMasSupAccountReference.Update(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterDrivingLicenseEntity, Status.Update);
                return RedirectToAction("Index", "AccountReference");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterDrivingLicenseVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {
            var pageNumber = SubTasks.CrMasSupAccountReference;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupAccountReference.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {
                
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if(status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupAccountPaymentMethodStatus = status;
                _unitOfWork.CrMasSupAccountReference.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupAccountReference entity)
        {

            if (await _masAccountReference.ExistsByArabicNameAsync(entity.CrMasSupAccountReceiptReferenceArName, entity.CrMasSupAccountReceiptReferenceCode))
            {
                ModelState.AddModelError("CrMasSupAccountReceiptReferenceArName", _localizer["Existing"]);
            }

            if (await _masAccountReference.ExistsByEnglishNameAsync(entity.CrMasSupAccountReceiptReferenceEnName, entity.CrMasSupAccountReceiptReferenceCode))
            {
                ModelState.AddModelError("CrMasSupAccountReceiptReferenceEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_AccountReferences = await _unitOfWork.CrMasSupAccountReference.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_AccountReferences != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupAccountReceiptReferenceArName" && All_AccountReferences.Any(x => x.CrMasSupAccountReceiptReferenceArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountReceiptReferenceArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupAccountReceiptReferenceEnName" && All_AccountReferences.Any(x => x.CrMasSupAccountReceiptReferenceEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountReceiptReferenceEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupAccountReference.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupAccountReceiptReferenceCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupAccountReference licence, string status)
        {
            var pageNumber = SubTasks.CrMasSupAccountReference;

            var recordAr = licence.CrMasSupAccountReceiptReferenceArName;
            var recordEn = licence.CrMasSupAccountReceiptReferenceEnName;
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
            return RedirectToAction("Index", "AccountReference");
        }


    }
}
