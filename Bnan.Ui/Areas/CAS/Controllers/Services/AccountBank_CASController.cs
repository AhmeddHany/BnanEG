using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;

using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Numerics;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Bnan.Ui.Areas.CAS.Controllers.Services
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class AccountBank_CASController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IAccountBank_CAS _AccountBank_CAS;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<AccountBank_CASController> _localizer;
        private readonly string pageNumber = SubTasks.ServicesCas_Banks;


        public AccountBank_CASController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IAccountBank_CAS AccountBank_CAS, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<AccountBank_CASController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _AccountBank_CAS = AccountBank_CAS;
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
            var Banks = await _unitOfWork.CrCasAccountBank.FindAllAsync(x => x.CrCasAccountBankLessor == user.CrMasUserInformationLessor && x.CrCasAccountBankStatus == Status.Active && x.CrCasAccountBankNo != "00");
            //var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsync(x => x.CrMasSupAccountBankStatus != Status.Deleted);
            var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.GetAllAsync();


            var all_SalesPointsCount = await _unitOfWork.CrCasAccountSalesPoint.FindCountByColumnAsync<CrCasAccountSalesPoint>(
               predicate: x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted,
               columnSelector: x => x.CrCasAccountSalesPointAccountBank  // تحديد العمود الذي نريد التجميع بناءً عليه
               //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
               ) ;

            // If no active licenses, retrieve all licenses
            if (Banks.Count()==0)
            {
                Banks = await _unitOfWork.CrCasAccountBank
                    .FindAllAsync(x => x.CrCasAccountBankLessor == user.CrMasUserInformationLessor && x.CrCasAccountBankStatus == Status.Hold && x.CrCasAccountBankNo != "00"
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CAS_AccountBankVM vm = new CAS_AccountBankVM();
            vm.all_BanksName = all_BanksName?.ToList() ?? new List<CrMasSupAccountBank>();
            vm.list_CrCasAccountBank = Banks?.ToList() ?? new List<CrCasAccountBank>();
            vm.all_SalesPointsCount = all_SalesPointsCount;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAccountBank_CASByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var AccountBank_CASsAll = await _unitOfWork.CrCasAccountBank.FindAllAsNoTrackingAsync(x =>  x.CrCasAccountBankLessor==user.CrMasUserInformationLessor && x.CrCasAccountBankNo != "00" &&(x.CrCasAccountBankStatus == Status.Active ||
                                                                                                                            x.CrCasAccountBankStatus == Status.Deleted ||
                                                                                                                            x.CrCasAccountBankStatus == Status.Hold));
                
                //var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsync(x => x.CrMasSupAccountBankStatus != Status.Deleted);
                var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.GetAllAsync();
                var all_SalesPointsCount = await _unitOfWork.CrCasAccountSalesPoint.FindCountByColumnAsync<CrCasAccountSalesPoint>(
                   predicate: x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted,
                   columnSelector: x => x.CrCasAccountSalesPointAccountBank  // تحديد العمود الذي نريد التجميع بناءً عليه
                   //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                   );

                CAS_AccountBankVM vm = new CAS_AccountBankVM();
                if (status == Status.All)
                {
                    var FilterAll = AccountBank_CASsAll.FindAll(x => x.CrCasAccountBankStatus != Status.Deleted &&
                                                                         (x.CrCasAccountBankArName.Contains(search) ||
                                                                          x.CrCasAccountBankEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrCasAccountBankCode.Contains(search)));
                    vm.list_CrCasAccountBank = FilterAll;
                    vm.all_SalesPointsCount = all_SalesPointsCount;
                    vm.all_BanksName = all_BanksName?.ToList() ?? new List<CrMasSupAccountBank>();
                    return PartialView("_DataTableAccountBank_CAS", vm);
                }
                var FilterByStatus = AccountBank_CASsAll.FindAll(x => x.CrCasAccountBankStatus == status &&
                                                                            (
                                                                           x.CrCasAccountBankArName.Contains(search) ||
                                                                           x.CrCasAccountBankEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrCasAccountBankCode.Contains(search)));
                vm.list_CrCasAccountBank = FilterByStatus;
                vm.all_SalesPointsCount = all_SalesPointsCount;
                vm.all_BanksName = all_BanksName?.ToList() ?? new List<CrMasSupAccountBank>();
                return PartialView("_DataTableAccountBank_CAS", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddAccountBank_CAS()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "AccountBank_CAS");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "AccountBank_CAS");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            //// Check If code > 9 get error , because code is char(1)
            //if (Int64.Parse(await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor)) > 7004999999)
            //{
            //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            //    return RedirectToAction("Index", "AccountBank_CAS");
            //}
            var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsNoTrackingAsync(x=>x.CrMasSupAccountBankStatus==Status.Active && x.CrMasSupAccountBankCode!="00");

            // Set Title 
            CAS_AccountBankVM Acc_BankVM = new CAS_AccountBankVM();
            //Acc_BankVM.CrCasAccountBankCode = await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor);
            Acc_BankVM.CrCasAccountBankLessor = user.CrMasUserInformationLessor;           
            Acc_BankVM.all_BanksName = all_BanksName;
            return View(Acc_BankVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddAccountBank_CAS(CAS_AccountBankVM Acc_BankVM)
        {

            var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountBankStatus == Status.Active && x.CrMasSupAccountBankCode != "00");

            Acc_BankVM.all_BanksName = all_BanksName;

            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || Acc_BankVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountBank_CAS", Acc_BankVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);

                var NewSerial = await Generate_serial_Async(user?.CrMasUserInformationLessor, Acc_BankVM?.CrCasAccountBankNo);
                var NewCode = (user?.CrMasUserInformationLessor ?? "") + (Acc_BankVM?.CrCasAccountBankNo??"") + NewSerial;
                
                Acc_BankVM.CrCasAccountBankSerail = NewSerial;
                Acc_BankVM.CrCasAccountBankCode = NewCode;
                Acc_BankVM.CrCasAccountBankLessor = user?.CrMasUserInformationLessor ?? " ";

                // Map ViewModel to Entity
                var ownerEntity = _mapper.Map<CrCasAccountBank>(Acc_BankVM);

                // Check if the entity already exists
                if (await _AccountBank_CAS.ExistsByDetails_AddAsync(ownerEntity))
                {
                    await AddModelErrorsAsync(ownerEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddAccountBank_CAS", Acc_BankVM);
                }
                //// Check If code > 9 get error , because code is char(1)
                //if (Int64.Parse(await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor)) > 7004999999)
                //{
                //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return View("AddAccountBank_CAS", Acc_BankVM);
                //}
                //// Generate and set the Driving License Code
                //Acc_BankVM.CrCasAccountBankCode = await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor);
                // Set status and add the record
                ownerEntity.CrCasAccountBankStatus = "A";
                await _unitOfWork.CrCasAccountBank.AddAsync(ownerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, ownerEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountBank_CAS", Acc_BankVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrCasAccountBank.FindAsync(x => x.CrCasAccountBankCode == id);
            if (contract == null || user==null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "AccountBank_CAS");
            }
            var This_BankName = await _unitOfWork.CrMasSupAccountBanks.FindAsync(x => x.CrMasSupAccountBankCode == contract.CrCasAccountBankNo);

            var model = _mapper.Map<CAS_AccountBankVM>(contract);
            model.This_BankName = This_BankName;
            model.countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountBank== contract.CrCasAccountBankCode && x.CrCasAccountSalesPointTotalBalance > 0);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CAS_AccountBankVM Acc_BankVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && Acc_BankVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "AccountBank_CAS");
            }

            try
            {
                var This_BankName = await _unitOfWork.CrMasSupAccountBanks.FindAsync(x=>x.CrMasSupAccountBankCode==Acc_BankVM.CrCasAccountBankNo);
                Acc_BankVM.This_BankName = This_BankName;
                Acc_BankVM.countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountBank == Acc_BankVM.CrCasAccountBankCode && x.CrCasAccountSalesPointTotalBalance > 0);


                //var canEdit = await _AccountBank_CAS.CheckIfCanEdit_It(Acc_BankVM.CrCasAccountBankCode,user.CrMasUserInformationLessor);
                //if(!canEdit)
                //{               
                //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return View("Edit", Acc_BankVM);
                //}
                
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", Acc_BankVM);
                }
                var ownerEntity = _mapper.Map<CrCasAccountBank>(Acc_BankVM);

                // Check if the entity already exists
                if (await _AccountBank_CAS.ExistsByDetailsAsync(ownerEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(ownerEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", Acc_BankVM);
                }

                _unitOfWork.CrCasAccountBank.Update(ownerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, ownerEntity, Status.Update);
                return RedirectToAction("Index", "AccountBank_CAS");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", Acc_BankVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var thisBank = await _unitOfWork.CrCasAccountBank.FindAsync(x => x.CrCasAccountBankCode == code && x.CrCasAccountBankLessor == user.CrMasUserInformationLessor);
            var All_Banks = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsync(x => x.CrCasAccountSalesPointAccountBank.Trim() == code && x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor);

            if (thisBank == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _AccountBank_CAS.CheckIfCanDeleteIt(thisBank.CrCasAccountBankCode, user.CrMasUserInformationLessor)) return "udelete"; }
                
                //var canEdit = await _AccountBank_CAS.CheckIfCanEdit_It(thisBank.CrCasAccountBankCode, user.CrMasUserInformationLessor);
                //if (!canEdit)
                //{
                //    //_toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdateStatus"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return "un_NoUpdateStatus";
                //}
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                thisBank.CrCasAccountBankStatus = status;
                _unitOfWork.CrCasAccountBank.Update(thisBank);

                foreach (var single in All_Banks)
                {
                    single.CrCasAccountSalesPointBankStatus = status;
                }
                _unitOfWork.CrCasAccountSalesPoint.UpdateRange(All_Banks);

                _unitOfWork.Complete();
                await SaveTracingForLicenseChange(user, thisBank, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrCasAccountBank entity)
        {


            if (await _AccountBank_CAS.ExistsByArabicNameAsync(entity.CrCasAccountBankArName, "N", entity.CrCasAccountBankLessor))
            {
                ModelState.AddModelError("CrCasAccountBankArName", _localizer["Existing"]);
            }

            if (await _AccountBank_CAS.ExistsByEnglishNameAsync(entity.CrCasAccountBankEnName, "N", entity.CrCasAccountBankLessor))
            {
                ModelState.AddModelError("CrCasAccountBankEnName", _localizer["Existing"]);
                
            }
            if (await _AccountBank_CAS.ExistsByBankIbanAsync(entity.CrCasAccountBankIban, "N"))
            {
                ModelState.AddModelError("CrCasAccountBankIban", _localizer["Existing"]);              
            }
            //if (await _AccountBank_CAS.ExistsByEmailAsync(entity.CrCasAccountBankEmail, entity.CrCasAccountBankCode))
            //{
            //    ModelState.AddModelError("CrCasAccountBankEmail", _localizer["Existing"]);
            //}
            //if (await _AccountBank_CAS.ExistsByMobileAsync(entity.CrCasAccountBankMobile, entity.CrCasAccountBankCode))
            //{
            //    ModelState.AddModelError("CrCasAccountBankMobile", _localizer["Existing"]);
            //}
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var user = await _userManager.GetUserAsync(User);

            var All_AccountBank_CASs = await _unitOfWork.CrCasAccountBank.FindAllAsync(x => x.CrCasAccountBankLessor == user.CrMasUserInformationLessor);

            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_AccountBank_CASs != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrCasAccountBankArName" && All_AccountBank_CASs.Any(x => x.CrCasAccountBankArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasAccountBankArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasAccountBankEnName" && All_AccountBank_CASs.Any(x => x.CrCasAccountBankEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasAccountBankEnName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasAccountBankEnName" && All_AccountBank_CASs.Any(x => x.CrCasAccountBankEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasAccountBankEnName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasAccountBankIban")
                {
                    var All_AccountBank_CAS_ibans = await _unitOfWork.CrCasAccountBank.FindAllAsync(x => x.CrCasAccountBankIban.ToLower() == dataField.ToLower());
                    if (All_AccountBank_CAS_ibans.Any(x => x.CrCasAccountBankIban?.ToLower() == dataField.ToLower()))
                    {
                        errors.Add(new ErrorResponse { Field = "CrCasAccountBankIban", Message = _localizer["Existing"] });
                    }
                } 
                
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> Generate_serial_Async(string lessorCode,string BankNo)
        {
            var allLicenses = await _unitOfWork.CrCasAccountBank.FindAllAsync(x=>x.CrCasAccountBankLessor == lessorCode && x.CrCasAccountBankNo == BankNo);
            if(allLicenses.Count()>1) allLicenses = allLicenses.OrderBy(x => x.CrCasAccountBankSerail);
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrCasAccountBankSerail) + 1).ToString("00") : "01";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrCasAccountBank thisBank, string status)
        {


            var recordAr = thisBank.CrCasAccountBankArName;
            var recordEn = thisBank.CrCasAccountBankEnName;
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
            return RedirectToAction("Index", "AccountBank_CAS");
        }


    }
}
