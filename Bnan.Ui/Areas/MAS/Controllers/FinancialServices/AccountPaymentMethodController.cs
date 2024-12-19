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
namespace Bnan.Ui.Areas.MAS.Controllers.FinancialServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class AccountPaymentMethodController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasAccountPaymentMethod _masAccountPaymentMethod;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<AccountPaymentMethodController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupAccountPaymentMethod;


        public AccountPaymentMethodController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasAccountPaymentMethod masAccountPaymentMethod, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<AccountPaymentMethodController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masAccountPaymentMethod = masAccountPaymentMethod;
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
            var paymenMethods = await _unitOfWork.CrMasSupAccountPaymentMethod
                .FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active, new[] { "CrCasAccountReceipts" });

            // If no active licenses, retrieve all licenses
            if (!paymenMethods.Any())
            {
                paymenMethods = await _unitOfWork.CrMasSupAccountPaymentMethod
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Hold,
                                              new[] { "CrCasAccountReceipts" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            var all_Classifications2 = await _unitOfWork.CrMasSupCountryClassification.GetAllAsyncAsNoTrackingAsync();
            var all_Classifications = all_Classifications2.ToList();
            AccountPaymentMethodVM Vm = new AccountPaymentMethodVM();
            Vm.crMasSupAccountPaymentMethod = paymenMethods;
            Vm.crMasSupCountryClassificationSS = all_Classifications;
            return View(Vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAccountPaymentMethodByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var AccountPaymentMethodsAll = await _unitOfWork.CrMasSupAccountPaymentMethod.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountPaymentMethodStatus == Status.Active ||
                                                                                                                            x.CrMasSupAccountPaymentMethodStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupAccountPaymentMethodStatus == Status.Hold, new[] { "CrCasAccountReceipts" });


                var all_Classifications2 = await _unitOfWork.CrMasSupCountryClassification.GetAllAsyncAsNoTrackingAsync();
                var all_Classifications = all_Classifications2.ToList();
                AccountPaymentMethodVM Vm = new AccountPaymentMethodVM();
                Vm.crMasSupCountryClassificationSS = all_Classifications;

                if (status == Status.All)
                {
                    var FilterAll = AccountPaymentMethodsAll.FindAll(x => x.CrMasSupAccountPaymentMethodStatus != Status.Deleted &&
                                                                         (x.CrMasSupAccountPaymentMethodArName.Contains(search) ||
                                                                          x.CrMasSupAccountPaymentMethodEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupAccountPaymentMethodCode.Contains(search)));
                    Vm.crMasSupAccountPaymentMethod = FilterAll;
                    return PartialView("_DataTableAccountPaymentMethod", Vm);
                }
                var FilterByStatus = AccountPaymentMethodsAll.FindAll(x => x.CrMasSupAccountPaymentMethodStatus == status &&
                                                                            (
                                                                           x.CrMasSupAccountPaymentMethodArName.Contains(search) ||
                                                                           x.CrMasSupAccountPaymentMethodEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupAccountPaymentMethodCode.Contains(search)));
                Vm.crMasSupAccountPaymentMethod = FilterByStatus;
                return PartialView("_DataTableAccountPaymentMethod", Vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddAccountPaymentMethod()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "AccountPaymentMethod");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "AccountPaymentMethod");
            }
            // Check If code > 9 get error , because code is char(1)
            if (await GeneratCountCodeAsync() > 98)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "AccountPaymentMethod");
            }
            // Set Title 
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active);

            AccountPaymentMethodVM paymenMethodVM = new AccountPaymentMethodVM();
            paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
            return View(paymenMethodVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddAccountPaymentMethod(AccountPaymentMethodVM paymenMethodVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active);

            if (!ModelState.IsValid || paymenMethodVM == null)
            {
                paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
                return View("AddAccountPaymentMethod", paymenMethodVM);
            }
            try
            {
                // Map ViewModel to Entity
                var paymenMethodEntity = _mapper.Map<CrMasSupAccountPaymentMethod>(paymenMethodVM);

                paymenMethodEntity.CrMasSupAccountPaymentMethodNaqlCode ??= 0;
                paymenMethodEntity.CrMasSupAccountPaymentMethodNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masAccountPaymentMethod.ExistsByDetailsAsync(paymenMethodEntity, false))
                {
                    await AddModelErrorsAsync(paymenMethodEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
                    return View("AddAccountPaymentMethod", paymenMethodVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (await GeneratCountCodeAsync() > 98)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
                    return View("AddAccountPaymentMethod", paymenMethodVM);
                }
                // Set status and add the record
                paymenMethodEntity.CrMasSupAccountPaymentMethodStatus = "A";
                await _unitOfWork.CrMasSupAccountPaymentMethod.AddAsync(paymenMethodEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, paymenMethodEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
                return View("AddAccountPaymentMethod", paymenMethodVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupAccountPaymentMethod.FindAsync(x => x.CrMasSupAccountPaymentMethodCode == id, new[] { "CrCasAccountReceipts" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "AccountPaymentMethod");
            }
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active || contract.CrMasSupAccountPaymentMethodClassification == x.CrMasLessorCountryClassificationCode);

            var model = _mapper.Map<AccountPaymentMethodVM>(contract);
            model.CrMasSupAccountPaymentMethodNaqlCode ??= 0;
            model.CrMasSupAccountPaymentMethodNaqlId ??= 0;
            model.crMasSupCountryClassificationSS = all_Classifications;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(AccountPaymentMethodVM paymenMethodVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            var all_Classifications = await _unitOfWork.CrMasSupCountryClassification.FindAllAsNoTrackingAsync(x => x.CrMasLessorCountryClassificationStatus == Status.Active || paymenMethodVM.CrMasSupAccountPaymentMethodClassification == x.CrMasLessorCountryClassificationCode);

            if (user == null && paymenMethodVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "AccountPaymentMethod");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
                    return View("Edit", paymenMethodVM);
                }
                var paymenMethodEntity = _mapper.Map<CrMasSupAccountPaymentMethod>(paymenMethodVM);
                paymenMethodEntity.CrMasSupAccountPaymentMethodNaqlCode ??= 0;
                paymenMethodEntity.CrMasSupAccountPaymentMethodNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masAccountPaymentMethod.ExistsByDetailsAsync(paymenMethodEntity, true))
                {
                    await AddModelErrorsAsync(paymenMethodEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
                    return View("Edit", paymenMethodVM);
                }

                _unitOfWork.CrMasSupAccountPaymentMethod.Update(paymenMethodEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, paymenMethodEntity, Status.Update);
                return RedirectToAction("Index", "AccountPaymentMethod");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                paymenMethodVM.crMasSupCountryClassificationSS = all_Classifications;
                return View("Edit", paymenMethodVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupAccountPaymentMethod.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masAccountPaymentMethod.CheckIfCanDeleteIt(licence.CrMasSupAccountPaymentMethodCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupAccountPaymentMethodStatus = status;
                _unitOfWork.CrMasSupAccountPaymentMethod.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupAccountPaymentMethod entity)
        {
            if (await _masAccountPaymentMethod.ExistsByPKCodeAsync(entity.CrMasSupAccountPaymentMethodCode))
            {
                ModelState.AddModelError("CrMasSupAccountPaymentMethodCode", _localizer["Existing"]);
            }
            if (await _masAccountPaymentMethod.CheckClassificationAsync(entity.CrMasSupAccountPaymentMethodClassification))
            {
                ModelState.AddModelError("CrMasSupAccountPaymentMethodClassification", _localizer["Erro_entry"]);
            }
            if (await _masAccountPaymentMethod.ExistsByArabicNameAsync(entity.CrMasSupAccountPaymentMethodArName, entity.CrMasSupAccountPaymentMethodCode))
            {
                ModelState.AddModelError("CrMasSupAccountPaymentMethodArName", _localizer["Existing"]);
            }

            if (await _masAccountPaymentMethod.ExistsByEnglishNameAsync(entity.CrMasSupAccountPaymentMethodEnName, entity.CrMasSupAccountPaymentMethodCode))
            {
                ModelState.AddModelError("CrMasSupAccountPaymentMethodEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_AccountPaymentMethods = await _unitOfWork.CrMasSupAccountPaymentMethod.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_AccountPaymentMethods != null)
            {
                // Check for existing Id Code PK
                if (existName == "CrMasSupAccountPaymentMethodCode" && await _masAccountPaymentMethod.ExistsByPKCodeAsync(dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountPaymentMethodCode", Message = _localizer["Existing"] });
                }
                // Check for existing Arabic driving license
                else if (existName == "CrMasSupAccountPaymentMethodArName" && All_AccountPaymentMethods.Any(x => x.CrMasSupAccountPaymentMethodArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountPaymentMethodArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupAccountPaymentMethodEnName" && All_AccountPaymentMethods.Any(x => x.CrMasSupAccountPaymentMethodEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountPaymentMethodEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupAccountPaymentMethodNaqlCode" && long.TryParse(dataField, out var code) && code != 0 && All_AccountPaymentMethods.Any(x => x.CrMasSupAccountPaymentMethodNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountPaymentMethodNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupAccountPaymentMethodNaqlId" && long.TryParse(dataField, out var id) && id != 0 && All_AccountPaymentMethods.Any(x => x.CrMasSupAccountPaymentMethodNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountPaymentMethodNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupAccountPaymentMethod.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupAccountPaymentMethodCode) + 1).ToString() : "10";
        }
        private async Task<int> GeneratCountCodeAsync()
        {
            var Count = await _unitOfWork.CrMasSupAccountPaymentMethod.CountAsync();
            return Count;
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupAccountPaymentMethod licence, string status)
        {


            var recordAr = licence.CrMasSupAccountPaymentMethodArName;
            var recordEn = licence.CrMasSupAccountPaymentMethodEnName;
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
            return RedirectToAction("Index", "AccountPaymentMethod");
        }


    }
}
