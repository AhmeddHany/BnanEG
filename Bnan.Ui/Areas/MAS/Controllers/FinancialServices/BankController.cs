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
    public class BankController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasAccountBank _masAccountBank;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<BankController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupAccountBank;


        public BankController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasAccountBank masAccountBank, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<BankController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masAccountBank = masAccountBank;
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
            var banks = await _unitOfWork.CrMasSupAccountBanks
                .FindAllAsNoTrackingAsync(x => x.CrMasSupAccountBankStatus == Status.Active);

            var Banks_count = await _unitOfWork.CrCasAccountBank.FindCountByColumnAsync<CrMasSupAccountBank>(
                predicate: x => x.CrCasAccountBankStatus != Status.Deleted,
                columnSelector: x => x.CrCasAccountBankNo  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );


            // If no active licenses, retrieve all licenses
            if (!banks.Any())
            {
                banks = await _unitOfWork.CrMasSupAccountBanks
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupAccountBankStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            MasAccountBankVM vm = new MasAccountBankVM();
            vm.crMasSupAccountBank = banks;
            vm.Banks_count = Banks_count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAccountBankByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var AccountBanksAll = await _unitOfWork.CrMasSupAccountBanks.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountBankStatus == Status.Active ||
                                                                                                                            x.CrMasSupAccountBankStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupAccountBankStatus == Status.Hold);
                var Banks_count = await _unitOfWork.CrCasAccountBank.FindCountByColumnAsync<CrMasSupAccountBank>(
                    predicate: x => x.CrCasAccountBankStatus != Status.Deleted,
                    columnSelector: x => x.CrCasAccountBankNo  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                MasAccountBankVM vm = new MasAccountBankVM();
                vm.Banks_count = Banks_count;
                if (status == Status.All)
                {
                    var FilterAll = AccountBanksAll.FindAll(x => x.CrMasSupAccountBankStatus != Status.Deleted &&
                                                                         (x.CrMasSupAccountBankArName.Contains(search) ||
                                                                          x.CrMasSupAccountBankEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupAccountBankCode.Contains(search)));
                    vm.crMasSupAccountBank = FilterAll;
                    return PartialView("_DataTableAccountBank", vm);
                }
                var FilterByStatus = AccountBanksAll.FindAll(x => x.CrMasSupAccountBankStatus == status &&
                                                                            (
                                                                           x.CrMasSupAccountBankArName.Contains(search) ||
                                                                           x.CrMasSupAccountBankEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupAccountBankCode.Contains(search)));
                vm.crMasSupAccountBank = FilterByStatus;
                return PartialView("_DataTableAccountBank", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddAccountBank()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "Bank");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Bank");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Bank");
            }
            // Set Title 
            MasAccountBankVM bankVM = new MasAccountBankVM();
            bankVM.CrMasSupAccountBankCode = await GenerateLicenseCodeAsync();
            return View(bankVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddAccountBank(MasAccountBankVM bankVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || bankVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountBank", bankVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var bankEntity = _mapper.Map<CrMasSupAccountBank>(bankVM);

                // Check if the entity already exists
                if (await _masAccountBank.ExistsByDetailsAsync(bankEntity))
                {
                    await AddModelErrorsAsync(bankEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddAccountBank", bankVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddAccountBank", bankVM);
                }
                // Generate and set the Driving License Code
                bankVM.CrMasSupAccountBankCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                bankEntity.CrMasSupAccountBankStatus = "A";
                await _unitOfWork.CrMasSupAccountBanks.AddAsync(bankEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, bankEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountBank", bankVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrMasSupAccountBanks.FindAsync(x => x.CrMasSupAccountBankCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Bank");
            }
            var model = _mapper.Map<MasAccountBankVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(MasAccountBankVM bankVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && bankVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "Bank");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", bankVM);
                }
                var bankEntity = _mapper.Map<CrMasSupAccountBank>(bankVM);

                // Check if the entity already exists
                if (await _masAccountBank.ExistsByDetailsAsync(bankEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(bankEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", bankVM);
                }

                _unitOfWork.CrMasSupAccountBanks.Update(bankEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, bankEntity, Status.Update);
                return RedirectToAction("Index", "Bank");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", bankVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupAccountBanks.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masAccountBank.CheckIfCanDeleteIt(licence.CrMasSupAccountBankCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupAccountBankStatus = status;
                _unitOfWork.CrMasSupAccountBanks.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupAccountBank entity)
        {

            if (await _masAccountBank.ExistsByArabicNameAsync(entity.CrMasSupAccountBankArName, entity.CrMasSupAccountBankCode))
            {
                ModelState.AddModelError("CrMasSupAccountBankArName", _localizer["Existing"]);
            }

            if (await _masAccountBank.ExistsByEnglishNameAsync(entity.CrMasSupAccountBankEnName, entity.CrMasSupAccountBankCode))
            {
                ModelState.AddModelError("CrMasSupAccountBankEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_AccountBanks = await _unitOfWork.CrMasSupAccountBanks.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_AccountBanks != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupAccountBankArName" && All_AccountBanks.Any(x => x.CrMasSupAccountBankArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountBankArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupAccountBankEnName" && All_AccountBanks.Any(x => x.CrMasSupAccountBankEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupAccountBankEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupAccountBanks.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupAccountBankCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupAccountBank licence, string status)
        {


            var recordAr = licence.CrMasSupAccountBankArName;
            var recordEn = licence.CrMasSupAccountBankEnName;
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
            return RedirectToAction("Index", "Bank");
        }


    }
}
