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
    public class AccountSalesPoint_CASController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IAccountSalesPoint_CAS _AccountSalesPoint_CAS;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<AccountSalesPoint_CASController> _localizer;
        private readonly string pageNumber = SubTasks.ServicesCas_SalesPoints;


        public AccountSalesPoint_CASController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IAccountSalesPoint_CAS AccountSalesPoint_CAS, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<AccountSalesPoint_CASController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _AccountSalesPoint_CAS = AccountSalesPoint_CAS;
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
            var SalesPoints = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsync(x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus == Status.Active && x.CrCasAccountSalesPointBankStatus == Status.Active && x.CrCasAccountSalesPointBranchStatus == Status.Active && x.CrCasAccountSalesPointBank != "00");
            //var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsync(x => x.CrMasSupAccountBankStatus != Status.Deleted);
            var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.GetAllAsync();

            var all_branchesNames = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasBranchInformationStatus == Status.Active && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasBranchInformationCode,
                    nameAr = x.CrCasBranchInformationArShortName,
                    nameEn = x.CrCasBranchInformationEnShortName,
                }));
            var all_AccountsNames = await _unitOfWork.CrCasAccountBank.FindAllAsync(x => x.CrCasAccountBankStatus == Status.Active && x.CrCasAccountBankLessor == user.CrMasUserInformationLessor);


            //var all_SalesPointsCount = await _unitOfWork.CrCasAccountSalesPoint.FindCountByColumnAsync<CrCasAccountSalesPoint>(
            //   predicate: x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted,
            //   columnSelector: x => x.CrCasAccountSalesPointAccountBank  // تحديد العمود الذي نريد التجميع بناءً عليه
            //   //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
            //   ) ;

            // If no active licenses, retrieve all licenses
            if (SalesPoints.Count()==0)
            {
                SalesPoints = await _unitOfWork.CrCasAccountSalesPoint
                    .FindAllAsync(x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus == Status.Hold && x.CrCasAccountSalesPointBankStatus == Status.Active && x.CrCasAccountSalesPointBranchStatus == Status.Active && x.CrCasAccountSalesPointBank != "00"
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CAS_AccountSalesPointVM vm = new CAS_AccountSalesPointVM();
            vm.all_branchesNames = all_branchesNames;
            vm.all_AccountsNames = all_AccountsNames?.ToList() ?? new List<CrCasAccountBank>();
            vm.all_BanksName = all_BanksName?.ToList() ?? new List<CrMasSupAccountBank>();
            vm.list_CrCasAccountSalesPoint = SalesPoints?.ToList() ?? new List<CrCasAccountSalesPoint>();
            //vm.all_SalesPointsCount = all_SalesPointsCount;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAccountSalesPoint_CASByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var AccountSalesPoint_CASsAll = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsNoTrackingAsync(x =>  x.CrCasAccountSalesPointLessor==user.CrMasUserInformationLessor && x.CrCasAccountSalesPointBank != "00" && x.CrCasAccountSalesPointBankStatus == Status.Active && x.CrCasAccountSalesPointBranchStatus == Status.Active && (x.CrCasAccountSalesPointStatus == Status.Active ||
                                                                                                                            x.CrCasAccountSalesPointStatus == Status.Deleted ||
                                                                                                                            x.CrCasAccountSalesPointStatus == Status.Hold));
                
                //var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsync(x => x.CrMasSupAccountBankStatus != Status.Deleted);
                var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.GetAllAsync();
                //var all_SalesPointsCount = await _unitOfWork.CrCasAccountSalesPoint.FindCountByColumnAsync<CrCasAccountSalesPoint>(
                //   predicate: x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted,
                //   columnSelector: x => x.CrCasAccountSalesPointAccountBank  // تحديد العمود الذي نريد التجميع بناءً عليه
                //   //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                //   ) ;
                var all_branchesNames = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x =>  x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor,
                    selectProjection: query => query.Select(x => new cas_list_String_4
                    {
                        id_key = x.CrCasBranchInformationCode,
                        nameAr = x.CrCasBranchInformationArShortName,
                        nameEn = x.CrCasBranchInformationEnShortName,
                    }));
                var all_AccountsNames = await _unitOfWork.CrCasAccountBank.FindAllAsync(x => x.CrCasAccountBankStatus == Status.Active && x.CrCasAccountBankLessor == user.CrMasUserInformationLessor);


                CAS_AccountSalesPointVM vm = new CAS_AccountSalesPointVM();
                vm.all_branchesNames = all_branchesNames;
                vm.all_AccountsNames = all_AccountsNames?.ToList() ?? new List<CrCasAccountBank>();
                if (status == Status.All)
                {
                    var FilterAll = AccountSalesPoint_CASsAll.FindAll(x => x.CrCasAccountSalesPointStatus != Status.Deleted &&
                                                                         (x.CrCasAccountSalesPointArName.Contains(search) ||
                                                                          x.CrCasAccountSalesPointEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrCasAccountSalesPointCode.Contains(search)));
                    vm.list_CrCasAccountSalesPoint = FilterAll;
                    //vm.all_SalesPointsCount = all_SalesPointsCount;
                    vm.all_BanksName = all_BanksName?.ToList() ?? new List<CrMasSupAccountBank>();
                    return PartialView("_DataTableAccountSalesPoint_CAS", vm);
                }
                var FilterByStatus = AccountSalesPoint_CASsAll.FindAll(x => x.CrCasAccountSalesPointStatus == status &&
                                                                            (
                                                                           x.CrCasAccountSalesPointArName.Contains(search) ||
                                                                           x.CrCasAccountSalesPointEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrCasAccountSalesPointCode.Contains(search)));
                vm.list_CrCasAccountSalesPoint = FilterByStatus;
                //vm.all_SalesPointsCount = all_SalesPointsCount;
                vm.all_BanksName = all_BanksName?.ToList() ?? new List<CrMasSupAccountBank>();
                return PartialView("_DataTableAccountSalesPoint_CAS", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddAccountSalesPoint_CAS()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "AccountSalesPoint_CAS");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "AccountSalesPoint_CAS");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            //// Check If code > 9 get error , because code is char(1)
            //if (Int64.Parse(await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor)) > 7004999999)
            //{
            //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            //    return RedirectToAction("Index", "AccountSalesPoint_CAS");
            //}
            var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsNoTrackingAsync(x=>x.CrMasSupAccountBankStatus==Status.Active && x.CrMasSupAccountBankCode!="00");
            
            var all_branchesNames = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasBranchInformationStatus == Status.Active && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasBranchInformationCode,
                    nameAr = x.CrCasBranchInformationArShortName,
                    nameEn = x.CrCasBranchInformationEnShortName,
                }));
            var all_AccountsNames = await _unitOfWork.CrCasAccountBank.FindAllAsync(x => x.CrCasAccountBankStatus == Status.Active && x.CrCasAccountBankLessor == user.CrMasUserInformationLessor);


            var all_BanksNames = await _unitOfWork.CrMasSupAccountBanks.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasSupAccountBankStatus == Status.Active,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrMasSupAccountBankCode,
                    nameAr = x.CrMasSupAccountBankArName,
                    nameEn = x.CrMasSupAccountBankEnName,
                }));
            // Set Title 
            CAS_AccountSalesPointVM Acc_SalesPointVM = new CAS_AccountSalesPointVM();
            //Acc_SalesPointVM.CrCasAccountSalesPointCode = await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor);
            Acc_SalesPointVM.CrCasAccountSalesPointLessor = user.CrMasUserInformationLessor;
            Acc_SalesPointVM.all_BanksName = all_BanksName;
            Acc_SalesPointVM.all_AccountsNames = all_AccountsNames?.ToList() ?? new List<CrCasAccountBank>();
            Acc_SalesPointVM.all_branchesNames = all_branchesNames;
            Acc_SalesPointVM.all_BanksNames = all_BanksNames;
            return View(Acc_SalesPointVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddAccountSalesPoint_CAS(CAS_AccountSalesPointVM Acc_SalesPointVM)
        {

            var all_BanksName = await _unitOfWork.CrMasSupAccountBanks.FindAllAsNoTrackingAsync(x => x.CrMasSupAccountBankStatus == Status.Active && x.CrMasSupAccountBankCode != "00");

            Acc_SalesPointVM.all_BanksName = all_BanksName;

            var user = await _userManager.GetUserAsync(User);


            var all_branchesNames = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasBranchInformationStatus == Status.Active && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasBranchInformationCode,
                    nameAr = x.CrCasBranchInformationArShortName,
                    nameEn = x.CrCasBranchInformationEnShortName,
                }));
            var all_AccountsNames = await _unitOfWork.CrCasAccountBank.FindAllAsync( x => x.CrCasAccountBankStatus == Status.Active && x.CrCasAccountBankLessor == user.CrMasUserInformationLessor);

            var all_BanksNames = await _unitOfWork.CrMasSupAccountBanks.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasSupAccountBankStatus == Status.Active,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrMasSupAccountBankCode,
                    nameAr = x.CrMasSupAccountBankArName,
                    nameEn = x.CrMasSupAccountBankEnName,
                }));


            Acc_SalesPointVM.all_branchesNames = all_branchesNames;
            Acc_SalesPointVM.all_AccountsNames = all_AccountsNames?.ToList()?? new List<CrCasAccountBank>();
            Acc_SalesPointVM.all_BanksNames = all_BanksNames;

            if (!ModelState.IsValid || Acc_SalesPointVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountSalesPoint_CAS", Acc_SalesPointVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);

                var NewSerial = await Generate_serial_Async(user?.CrMasUserInformationLessor, Acc_SalesPointVM?.CrCasAccountSalesPointBrn, Acc_SalesPointVM?.CrCasAccountSalesPointBank);
                var NewCode = (user?.CrMasUserInformationLessor ?? "") + (Acc_SalesPointVM?.CrCasAccountSalesPointBrn ?? "") + (Acc_SalesPointVM?.CrCasAccountSalesPointBank ?? "") + NewSerial;
                
                Acc_SalesPointVM.CrCasAccountSalesPointSerial = NewSerial;
                Acc_SalesPointVM.CrCasAccountSalesPointCode = NewCode;
                Acc_SalesPointVM.CrCasAccountSalesPointLessor = user?.CrMasUserInformationLessor ?? " ";

                // Map ViewModel to Entity
                var SalesPointEntity = _mapper.Map<CrCasAccountSalesPoint>(Acc_SalesPointVM);

                // Check if the entity already exists
                if (await _AccountSalesPoint_CAS.ExistsByDetails_AddAsync(SalesPointEntity))
                {
                    await AddModelErrorsAsync(SalesPointEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddAccountSalesPoint_CAS", Acc_SalesPointVM);
                }
                //// Check If code > 9 get error , because code is char(1)
                //if (Int64.Parse(await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor)) > 7004999999)
                //{
                //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return View("AddAccountSalesPoint_CAS", Acc_SalesPointVM);
                //}
                //// Generate and set the Driving License Code
                //Acc_SalesPointVM.CrCasAccountSalesPointCode = await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor);
                // Set status and add the record
                SalesPointEntity.CrCasAccountSalesPointStatus = "A";
                SalesPointEntity.CrCasAccountSalesPointBankStatus = "A";
                SalesPointEntity.CrCasAccountSalesPointBranchStatus = "A";
                SalesPointEntity.CrCasAccountSalesPointTotalBalance = 0;
                SalesPointEntity.CrCasAccountSalesPointTotalAvailable = 0;
                SalesPointEntity.CrCasAccountSalesPointTotalReserved = 0;
                await _unitOfWork.CrCasAccountSalesPoint.AddAsync(SalesPointEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, SalesPointEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddAccountSalesPoint_CAS", Acc_SalesPointVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointCode == id );
            if (contract == null || user==null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "AccountSalesPoint_CAS");
            }
            var this_AccountData = await _unitOfWork.CrCasAccountBank.FindAsync(x => x.CrCasAccountBankCode == contract.CrCasAccountSalesPointAccountBank);
            var This_BankName = await _unitOfWork.CrMasSupAccountBanks.FindAsync(x => x.CrMasSupAccountBankCode == contract.CrCasAccountSalesPointBank);
            var all_branchesNames = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasBranchInformationStatus == Status.Active && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && x.CrCasBranchInformationCode== contract.CrCasAccountSalesPointBrn,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasBranchInformationCode,
                    nameAr = x.CrCasBranchInformationArShortName,
                    nameEn = x.CrCasBranchInformationEnShortName,
                }));
            var model = _mapper.Map<CAS_AccountSalesPointVM>(contract);
            model.This_BankName = This_BankName;
            model.this_AccountData = this_AccountData;
            model.all_branchesNames = all_branchesNames;
            //model.countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountSalesPoint== contract.CrCasAccountSalesPointCode);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CAS_AccountSalesPointVM Acc_SalesPointVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && Acc_SalesPointVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "AccountSalesPoint_CAS");
            }

            try
            {
                var This_BankName = await _unitOfWork.CrMasSupAccountBanks.FindAsync(x=>x.CrMasSupAccountBankCode==Acc_SalesPointVM.CrCasAccountSalesPointBank);
                Acc_SalesPointVM.This_BankName = This_BankName;
                //Acc_SalesPointVM.countForSales = await _unitOfWork.CrCasAccountSalesPoint.CountAsync(x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor && x.CrCasAccountSalesPointStatus != Status.Deleted && x.CrCasAccountSalesPointAccountSalesPoint == Acc_SalesPointVM.CrCasAccountSalesPointCode);


                //var canEdit = await _AccountSalesPoint_CAS.CheckIfCanEdit_It(Acc_SalesPointVM.CrCasAccountSalesPointCode,user.CrMasUserInformationLessor);
                //if(!canEdit)
                //{               
                //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return View("Edit", Acc_SalesPointVM);
                //}
                
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", Acc_SalesPointVM);
                }
                var SalesPointEntity = _mapper.Map<CrCasAccountSalesPoint>(Acc_SalesPointVM);

                // Check if the entity already exists
                if (await _AccountSalesPoint_CAS.ExistsByDetailsAsync(SalesPointEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(SalesPointEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", Acc_SalesPointVM);
                }

                _unitOfWork.CrCasAccountSalesPoint.Update(SalesPointEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, SalesPointEntity, Status.Update);
                return RedirectToAction("Index", "AccountSalesPoint_CAS");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", Acc_SalesPointVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var thisSalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointCode == code && x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor);
            if (thisSalesPoint == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (thisSalesPoint.CrCasAccountSalesPointTotalBalance>0) return "udelete"; }
                
                //var canEdit = await _AccountSalesPoint_CAS.CheckIfCanEdit_It(thisSalesPoint.CrCasAccountSalesPointCode, user.CrMasUserInformationLessor);
                //if (!canEdit)
                //{
                //    //_toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdateStatus"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return "un_NoUpdateStatus";
                //}
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                thisSalesPoint.CrCasAccountSalesPointStatus = status;
                _unitOfWork.CrCasAccountSalesPoint.Update(thisSalesPoint);
                _unitOfWork.Complete();
                await SaveTracingForLicenseChange(user, thisSalesPoint, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrCasAccountSalesPoint entity)
        {


            if (await _AccountSalesPoint_CAS.ExistsByArabicNameAsync(entity.CrCasAccountSalesPointArName, "N", entity.CrCasAccountSalesPointLessor))
            {
                ModelState.AddModelError("CrCasAccountSalesPointArName", _localizer["Existing"]);
            }

            if (await _AccountSalesPoint_CAS.ExistsByEnglishNameAsync(entity.CrCasAccountSalesPointEnName, "N", entity.CrCasAccountSalesPointLessor))
            {
                ModelState.AddModelError("CrCasAccountSalesPointEnName", _localizer["Existing"]);
                
            }
            if (await _AccountSalesPoint_CAS.ExistsBySalesPointIbanAsync(entity.CrCasAccountSalesPointNo, "N"))
            {
                ModelState.AddModelError("CrCasAccountSalesPointNo", _localizer["Existing"]);              
            }
            //if (await _AccountSalesPoint_CAS.ExistsByEmailAsync(entity.CrCasAccountSalesPointEmail, entity.CrCasAccountSalesPointCode))
            //{
            //    ModelState.AddModelError("CrCasAccountSalesPointEmail", _localizer["Existing"]);
            //}
            //if (await _AccountSalesPoint_CAS.ExistsByMobileAsync(entity.CrCasAccountSalesPointMobile, entity.CrCasAccountSalesPointCode))
            //{
            //    ModelState.AddModelError("CrCasAccountSalesPointMobile", _localizer["Existing"]);
            //}
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var user = await _userManager.GetUserAsync(User);

            var All_AccountSalesPoint_CASs = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsync(x => x.CrCasAccountSalesPointLessor == user.CrMasUserInformationLessor);

            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_AccountSalesPoint_CASs != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrCasAccountSalesPointArName" && All_AccountSalesPoint_CASs.Any(x => x.CrCasAccountSalesPointArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasAccountSalesPointArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasAccountSalesPointEnName" && All_AccountSalesPoint_CASs.Any(x => x.CrCasAccountSalesPointEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasAccountSalesPointEnName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasAccountSalesPointEnName" && All_AccountSalesPoint_CASs.Any(x => x.CrCasAccountSalesPointEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasAccountSalesPointEnName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasAccountSalesPointNo")
                {
                    var All_AccountSalesPoint_CAS_ibans = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsync(x => x.CrCasAccountSalesPointNo.ToLower() == dataField.ToLower());
                    if (All_AccountSalesPoint_CAS_ibans.Any(x => x.CrCasAccountSalesPointNo?.ToLower() == dataField.ToLower()))
                    {
                        errors.Add(new ErrorResponse { Field = "CrCasAccountSalesPointNo", Message = _localizer["Existing"] });
                    }
                } 
                
            }

            return Json(new { errors });
        }
        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> Get_Bank_Data(string bankAccount)
        {
            var this_AccountData = await _unitOfWork.CrCasAccountBank.FindAsync(x => x.CrCasAccountBankCode == bankAccount);
            var bankNo = this_AccountData?.CrCasAccountBankNo ?? "0";
            var This_BankName = await _unitOfWork.CrMasSupAccountBanks.FindAsync(x => x.CrMasSupAccountBankCode == bankNo);

            return Json(new {
                bank_Iban = this_AccountData?.CrCasAccountBankIban ?? " ",
                bank_No = bankNo ?? " ",
                bank_Ar = This_BankName?.CrMasSupAccountBankArName ?? " ",
                bank_En = This_BankName?.CrMasSupAccountBankEnName ?? " ",
            });

        }

        //Helper Methods 
        private async Task<string> Generate_serial_Async(string lessorCode,string branchNo,string BankNo)
        {
            var allLicenses = await _unitOfWork.CrCasAccountSalesPoint.FindAllAsync(x=>x.CrCasAccountSalesPointLessor == lessorCode && x.CrCasAccountSalesPointBrn== branchNo && x.CrCasAccountSalesPointBank == BankNo);
            if(allLicenses.Count()>1) allLicenses = allLicenses.OrderBy(x => x.CrCasAccountSalesPointSerial);
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrCasAccountSalesPointSerial) + 1).ToString("00") : "01";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrCasAccountSalesPoint thisSalesPoint, string status)
        {


            var recordAr = thisSalesPoint.CrCasAccountSalesPointArName;
            var recordEn = thisSalesPoint.CrCasAccountSalesPointEnName;
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
            return RedirectToAction("Index", "AccountSalesPoint_CAS");
        }


    }
}
