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
namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class LessorOwners_CASController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly ILessorOwners_CAS _LessorOwners_CAS;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<LessorOwners_CASController> _localizer;
        private readonly string pageNumber = SubTasks.Owners_CAS;


        public LessorOwners_CASController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, ILessorOwners_CAS lessorOwners_CAS, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<LessorOwners_CASController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _LessorOwners_CAS = lessorOwners_CAS;
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
            var owners = await _unitOfWork.CrCasOwners.FindAllAsync(x => x.CrCasOwnersLessorCode == user.CrMasUserInformationLessor && x.CrCasOwnersStatus == Status.Active);

            var all_CarsCount = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrCasCarInformation>(
               predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && (x.CrCasCarInformationStatus != Status.Deleted || x.CrCasCarInformationStatus != Status.ForSale) && x.CrCasCarInformationOwnerStatus != Status.Deleted,
               columnSelector: x => x.CrCasCarInformationOwner  // تحديد العمود الذي نريد التجميع بناءً عليه
               //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
               );

            // If no active licenses, retrieve all licenses
            if (owners.Count()==0)
            {
                owners = await _unitOfWork.CrCasOwners
                    .FindAllAsync(x => x.CrCasOwnersLessorCode == user.CrMasUserInformationLessor && x.CrCasOwnersStatus == Status.Hold
                                              );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            CASContractSourceVM vm = new CASContractSourceVM();
            vm.list_crCasOwner = owners?.ToList() ?? new List<CrCasOwner>();
            vm.all_CarsCount = all_CarsCount;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetLessorOwners_CASByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var LessorOwners_CASsAll = await _unitOfWork.CrCasOwners.FindAllAsNoTrackingAsync(x =>  x.CrCasOwnersLessorCode==user.CrMasUserInformationLessor && (x.CrCasOwnersStatus == Status.Active ||
                                                                                                                            x.CrCasOwnersStatus == Status.Deleted ||
                                                                                                                            x.CrCasOwnersStatus == Status.Hold));
                var all_CarsCount = await _unitOfWork.CrCasCarInformation.FindCountByColumnAsync<CrCasCarInformation>(
                   predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && (x.CrCasCarInformationStatus != Status.Deleted || x.CrCasCarInformationStatus != Status.ForSale) && x.CrCasCarInformationOwnerStatus != Status.Deleted,
                   columnSelector: x => x.CrCasCarInformationOwner  // تحديد العمود الذي نريد التجميع بناءً عليه
                                                                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                   );

                CASContractSourceVM vm = new CASContractSourceVM();
                if (status == Status.All)
                {
                    var FilterAll = LessorOwners_CASsAll.FindAll(x => x.CrCasOwnersStatus != Status.Deleted &&
                                                                         (x.CrCasOwnersArName.Contains(search) ||
                                                                          x.CrCasOwnersEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrCasOwnersCode.Contains(search)));
                    vm.list_crCasOwner = FilterAll;
                    vm.all_CarsCount = all_CarsCount;
                    return PartialView("_DataTableLessorOwners_CAS", vm);
                }
                var FilterByStatus = LessorOwners_CASsAll.FindAll(x => x.CrCasOwnersStatus == status &&
                                                                            (
                                                                           x.CrCasOwnersArName.Contains(search) ||
                                                                           x.CrCasOwnersEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrCasOwnersCode.Contains(search)));
                vm.list_crCasOwner = FilterByStatus;
                vm.all_CarsCount = all_CarsCount;
                return PartialView("_DataTableLessorOwners_CAS", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddLessorOwners_CAS()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "LessorOwners_CAS");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "LessorOwners_CAS");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            //// Check If code > 9 get error , because code is char(1)
            //if (Int64.Parse(await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor)) > 7004999999)
            //{
            //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            //    return RedirectToAction("Index", "LessorOwners_CAS");
            //}
            var all_keys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSysCallingKey
                {
                    CrMasSysCallingKeysNo = x.CrMasSysCallingKeysNo,
                })
                );
            // Set Title 
            CASContractSourceVM lessorMarketingVM = new CASContractSourceVM();
            //lessorMarketingVM.CrCasOwnersCode = await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor);
            lessorMarketingVM.CrCasOwnersLessorCode = user.CrMasUserInformationLessor;           
            lessorMarketingVM.keys= all_keys;
            return View(lessorMarketingVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddLessorOwners_CAS(CASContractSourceVM lessorMarketingVM)
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
                return View("AddLessorOwners_CAS", lessorMarketingVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var ownerEntity = _mapper.Map<CrCasOwner>(lessorMarketingVM);

                // Check if the entity already exists
                if (await _LessorOwners_CAS.ExistsByDetails_AddAsync(ownerEntity))
                {
                    await AddModelErrorsAsync(ownerEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddLessorOwners_CAS", lessorMarketingVM);
                }
                //// Check If code > 9 get error , because code is char(1)
                //if (Int64.Parse(await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor)) > 7004999999)
                //{
                //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return View("AddLessorOwners_CAS", lessorMarketingVM);
                //}
                //// Generate and set the Driving License Code
                //lessorMarketingVM.CrCasOwnersCode = await GenerateLicenseCodeAsync(user.CrMasUserInformationLessor);
                // Set status and add the record
                ownerEntity.CrCasOwnersStatus = "A";
                await _unitOfWork.CrCasOwners.AddAsync(ownerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, ownerEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddLessorOwners_CAS", lessorMarketingVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);

            var contract = await _unitOfWork.CrCasOwners.FindAsync(x => x.CrCasOwnersCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "LessorOwners_CAS");
            }
            var all_keys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSysCallingKey
                {
                    CrMasSysCallingKeysNo = x.CrMasSysCallingKeysNo.ToString().Trim(),
                })
                );
            var model = _mapper.Map<CASContractSourceVM>(contract);
            model.countForCars = await _unitOfWork.CrMasLessorInformation.CountAsync(x => x.CrMasLessorInformationGovernmentNo.Trim() == id.Trim() && x.CrMasLessorInformationStatus != Status.Deleted);
            model.keys = all_keys;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CASContractSourceVM lessorMarketingVM)
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
                return RedirectToAction("Index", "LessorOwners_CAS");
            }
            //string[] parts = lessorMarketingVM.CrCasOwnersMobile?.ToString().Split('-');
            //lessorMarketingVM.keys = all_keys;
            //if (parts.Length == 2)
            //{
            //    lessorMarketingVM.mob = parts[1];
            //    lessorMarketingVM.key2 = parts[0];
            //}
            try
            {
                var canEdit = await _LessorOwners_CAS.CheckIfCanEdit_It(lessorMarketingVM.CrCasOwnersCode);
                if(!canEdit)
                {               
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", lessorMarketingVM);
                }
                
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", lessorMarketingVM);
                }
                var ownerEntity = _mapper.Map<CrCasOwner>(lessorMarketingVM);

                // Check if the entity already exists
                if (await _LessorOwners_CAS.ExistsByDetailsAsync(ownerEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(ownerEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", lessorMarketingVM);
                }

                _unitOfWork.CrCasOwners.Update(ownerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, ownerEntity, Status.Update);
                return RedirectToAction("Index", "LessorOwners_CAS");
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

            var thisOwner = await _unitOfWork.CrCasOwners.FindAsync(x => x.CrCasOwnersCode == code && x.CrCasOwnersLessorCode == user.CrMasUserInformationLessor);
            var All_Owner = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationOwner.Trim() == code && x.CrCasCarInformationLessor == user.CrMasUserInformationLessor);
            if (thisOwner == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _LessorOwners_CAS.CheckIfCanDeleteIt(thisOwner.CrCasOwnersCode)) return "udelete"; }
                
                var canEdit = await _LessorOwners_CAS.CheckIfCanEdit_It(thisOwner.CrCasOwnersCode);
                if (!canEdit)
                {
                    //_toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdateStatus"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return "un_NoUpdateStatus";
                }
                if (status == Status.UnHold)
                {
                    foreach (var single in All_Owner)
                    {
                        single.CrCasCarInformationOwnerStatus = Status.Active;
                    }
                    _unitOfWork.CrCasCarInformation.UpdateRange(All_Owner);
                }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                thisOwner.CrCasOwnersStatus = status;
                _unitOfWork.CrCasOwners.Update(thisOwner);
                if(status == Status.Hold)
                {
                    foreach(var single in All_Owner)
                    {
                        single.CrCasCarInformationOwnerStatus = Status.Hold;
                    }
                    _unitOfWork.CrCasCarInformation.UpdateRange(All_Owner);
                }
                _unitOfWork.Complete();
                await SaveTracingForLicenseChange(user, thisOwner, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrCasOwner entity)
        {


            if (await _LessorOwners_CAS.ExistsByArabicNameAsync(entity.CrCasOwnersArName, entity.CrCasOwnersCode,entity.CrCasOwnersLessorCode))
            {
                ModelState.AddModelError("CrCasOwnersArName", _localizer["Existing"]);
            }

            if (await _LessorOwners_CAS.ExistsByEnglishNameAsync(entity.CrCasOwnersEnName, entity.CrCasOwnersCode, entity.CrCasOwnersLessorCode))
            {
                ModelState.AddModelError("CrCasOwnersEnName", _localizer["Existing"]);
            }
            //if (await _LessorOwners_CAS.ExistsByEmailAsync(entity.CrCasOwnersEmail, entity.CrCasOwnersCode))
            //{
            //    ModelState.AddModelError("CrCasOwnersEmail", _localizer["Existing"]);
            //}
            //if (await _LessorOwners_CAS.ExistsByMobileAsync(entity.CrCasOwnersMobile, entity.CrCasOwnersCode))
            //{
            //    ModelState.AddModelError("CrCasOwnersMobile", _localizer["Existing"]);
            //}
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var user = await _userManager.GetUserAsync(User);

            var All_LessorOwners_CASs = await _unitOfWork.CrCasOwners.FindAllAsync(x=>x.CrCasOwnersLessorCode == user.CrMasUserInformationLessor);
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_LessorOwners_CASs != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrCasOwnersCode" && All_LessorOwners_CASs.Any(x => x.CrCasOwnersCode == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasOwnersCode", Message = _localizer["Existing"] });
                }
                // Check for existing Arabic driving license
                if (existName == "CrCasOwnersArName" && All_LessorOwners_CASs.Any(x => x.CrCasOwnersArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasOwnersArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasOwnersEnName" && All_LessorOwners_CASs.Any(x => x.CrCasOwnersEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasOwnersEnName", Message = _localizer["Existing"] });
                }
                //// Check for existing email
                //else if (existName == "CrCasOwnerEmail" && All_LessorOwners_CASs.Any(x => x.CrCasOwnersEmail?.ToLower() == dataField.ToLower()))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrCasOwnersEmail", Message = _localizer["Existing"] });
                //}
                //// Check for existing mobile
                //else if (existName == "CrCasOwnerMobile" && All_LessorOwners_CASs.Any(x => x.CrCasOwnersMobile == dataField))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrCasOwnersMobile", Message = _localizer["Existing"] });
                //}
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync(string lessorCode)
        {
            var allLicenses = await _unitOfWork.CrCasOwners.FindAllAsync(x=>x.CrCasOwnersLessorCode == lessorCode);
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrCasOwnersCode) + 1).ToString() : "700400" + lessorCode.Substring(3) + "100";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrCasOwner thisOwner, string status)
        {


            var recordAr = thisOwner.CrCasOwnersArName;
            var recordEn = thisOwner.CrCasOwnersEnName;
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
            return RedirectToAction("Index", "LessorOwners_CAS");
        }


    }
}
