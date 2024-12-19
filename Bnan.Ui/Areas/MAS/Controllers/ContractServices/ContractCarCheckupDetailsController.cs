using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NToastNotify;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.ContractServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class ContractCarCheckupDetailsController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasContractCarCheckupDetails _masContractCarCheckupDetails;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ContractCarCheckupDetailsController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupContractCarCheckupDetail;


        public ContractCarCheckupDetailsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasContractCarCheckupDetails masContractCarCheckupDetails, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ContractCarCheckupDetailsController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masContractCarCheckupDetails = masContractCarCheckupDetails;
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

            var allCheckups = await _unitOfWork.CrMasSupContractCarCheckup.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: null,
                selectProjection: query => query.Select(x => new CrMasSupContractCarCheckup
                {
                    CrMasSupContractCarCheckupCode = x.CrMasSupContractCarCheckupCode,
                    CrMasSupContractCarCheckupArName = x.CrMasSupContractCarCheckupArName,
                    CrMasSupContractCarCheckupEnName = x.CrMasSupContractCarCheckupEnName,
                    CrMasSupContractCarCheckupStatus = x.CrMasSupContractCarCheckupStatus
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var Checkup_activated = allCheckups.Where(x => x.CrMasSupContractCarCheckupStatus == Status.Active).ToList();
            var checkupDetails2 = await _unitOfWork.CrMasSupContractCarCheckupDetail.GetAllAsyncAsNoTrackingAsync();
            List<CrMasSupContractCarCheckupDetail>? checkupDetails = new List<CrMasSupContractCarCheckupDetail>();
            checkupDetails = checkupDetails2.Where(x => x.CrMasSupContractCarCheckupDetailsStatus == Status.Active && allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Active)).ToList();


            // If no active licenses, retrieve all licenses
            if (!checkupDetails.Any())
            {
                checkupDetails = checkupDetails2.Where(x => allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Hold) || x.CrMasSupContractCarCheckupDetailsStatus == Status.Hold && allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Active)).ToList();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            ContractCarCheckupDetailsVM VM = new ContractCarCheckupDetailsVM();
            VM.crMasSupContractCarCheckupDetails = checkupDetails;
            VM.crMasSupContractCarCheckup = allCheckups;
            VM.Checkup_activated = Checkup_activated;
            return View(VM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetContractCarCheckupDetailsByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var all_CheckupDetails_2 = await _unitOfWork.CrMasSupContractCarCheckupDetail.GetAllAsyncAsNoTrackingAsync();
                var allCheckups = await _unitOfWork.CrMasSupContractCarCheckup.FindAllWithSelectAsNoTrackingAsync(
                    //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                    predicate: null,
                    selectProjection: query => query.Select(x => new CrMasSupContractCarCheckup
                    {
                        CrMasSupContractCarCheckupCode = x.CrMasSupContractCarCheckupCode,
                        CrMasSupContractCarCheckupArName = x.CrMasSupContractCarCheckupArName,
                        CrMasSupContractCarCheckupEnName = x.CrMasSupContractCarCheckupEnName,
                        CrMasSupContractCarCheckupStatus = x.CrMasSupContractCarCheckupStatus
                    })
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                var Checkup_activated = allCheckups.Where(x => x.CrMasSupContractCarCheckupStatus == Status.Active).ToList();
                ContractCarCheckupDetailsVM VM = new ContractCarCheckupDetailsVM();
                VM.crMasSupContractCarCheckup = allCheckups;
                VM.Checkup_activated = Checkup_activated;

                if (status == Status.All)
                {
                    var FilterAll = all_CheckupDetails_2.Where(x => (x.CrMasSupContractCarCheckupDetailsStatus != Status.Deleted || allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Deleted)) &&
                                                                         (x.CrMasSupContractCarCheckupDetailsArName.Contains(search) ||
                                                                          x.CrMasSupContractCarCheckupDetailsEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractCarCheckupDetailsCode.Contains(search))).ToList();
                    VM.crMasSupContractCarCheckupDetails = FilterAll;
                    return PartialView("_DataTableContractCarCheckupDetails", VM);
                }
                if (status == Status.Active)
                {
                    var FilterByStatus3 = all_CheckupDetails_2.Where(x => x.CrMasSupContractCarCheckupDetailsStatus == status && allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Active) &&
                                                                         (x.CrMasSupContractCarCheckupDetailsArName.Contains(search) ||
                                                                          x.CrMasSupContractCarCheckupDetailsEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractCarCheckupDetailsCode.Contains(search))).ToList();
                    VM.crMasSupContractCarCheckupDetails = FilterByStatus3;
                    return PartialView("_DataTableContractCarCheckupDetails", VM);
                }
                if (status == Status.Hold)
                {
                    var FilterByStatus = all_CheckupDetails_2.Where(x => (allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Hold) || x.CrMasSupContractCarCheckupDetailsStatus == status && allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Active)) &&
                                                                         (x.CrMasSupContractCarCheckupDetailsArName.Contains(search) ||
                                                                          x.CrMasSupContractCarCheckupDetailsEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractCarCheckupDetailsCode.Contains(search))).ToList();
                    VM.crMasSupContractCarCheckupDetails = FilterByStatus;
                    return PartialView("_DataTableContractCarCheckupDetails", VM);
                }
                if (status == Status.Deleted)
                {
                    var FilterByStatus2 = all_CheckupDetails_2.Where(x => (allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Deleted) || x.CrMasSupContractCarCheckupDetailsStatus == status && allCheckups.Any(y => y.CrMasSupContractCarCheckupCode.Trim() == x.CrMasSupContractCarCheckupDetailsCode.Trim() && y.CrMasSupContractCarCheckupStatus == Status.Active)) &&
                                                                         (x.CrMasSupContractCarCheckupDetailsArName.Contains(search) ||
                                                                          x.CrMasSupContractCarCheckupDetailsEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractCarCheckupDetailsCode.Contains(search))).ToList();
                    VM.crMasSupContractCarCheckupDetails = FilterByStatus2;
                    return PartialView("_DataTableContractCarCheckupDetails", VM);
                }

            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddContractCarCheckupDetails()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractCarCheckupDetails");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "ContractCarCheckupDetails");
            }
            //// Check If code > 9 get error , because code is char(1)
            //if (await CountofThisEntityAsync() > 99*99)
            //{
            //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            //    return RedirectToAction("Index", "ContractCarCheckupDetails");
            //}

            var allCheckups_activated = await _unitOfWork.CrMasSupContractCarCheckup.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSupContractCarCheckupStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSupContractCarCheckup
                {
                    CrMasSupContractCarCheckupCode = x.CrMasSupContractCarCheckupCode,
                    CrMasSupContractCarCheckupArName = x.CrMasSupContractCarCheckupArName,
                    CrMasSupContractCarCheckupEnName = x.CrMasSupContractCarCheckupEnName
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            // Set Title 
            ContractCarCheckupDetailsVM contractOptionsVM = new ContractCarCheckupDetailsVM();
            contractOptionsVM.crMasSupContractCarCheckup = allCheckups_activated;
            //contractOptionsVM.CrMasSupContractCarCheckupDetailsCode = await GenerateLicenseCodeAsync();
            return View(contractOptionsVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddContractCarCheckupDetails(ContractCarCheckupDetailsVM contractOptionsVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            var allCheckups_activated = await _unitOfWork.CrMasSupContractCarCheckup.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSupContractCarCheckupStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSupContractCarCheckup
                {
                    CrMasSupContractCarCheckupCode = x.CrMasSupContractCarCheckupCode,
                    CrMasSupContractCarCheckupArName = x.CrMasSupContractCarCheckupArName,
                    CrMasSupContractCarCheckupEnName = x.CrMasSupContractCarCheckupEnName
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            //// Set Title 
            //if (!ModelState.IsValid || contractOptionsVM == null)
            //{
            //    contractOptionsVM.crMasSupContractCarCheckup = allCheckups_activated;
            //    return View("AddContractCarCheckupDetails", contractOptionsVM);
            //}
            try
            {
                // Map ViewModel to Entity
                var contractOptionsEntity = _mapper.Map<CrMasSupContractCarCheckupDetail>(contractOptionsVM);

                // Check if the entity already exists
                if (await _masContractCarCheckupDetails.ExistsByDetails_Add_Async(contractOptionsEntity))
                {
                    await AddModelErrorsAsync(contractOptionsEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    contractOptionsVM.crMasSupContractCarCheckup = allCheckups_activated;
                    return View("AddContractCarCheckupDetails", contractOptionsVM);
                }
                //// Check If code > 9 get error , because code is char(1)
                //if (await CountofThisEntityAsync() > 99 * 99)
                //{
                //    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                //    return View("AddContractCarCheckupDetails", contractOptionsVM);
                //}


                // Set status and add the record
                contractOptionsEntity.CrMasSupContractCarCheckupDetailsStatus = "A";
                await _unitOfWork.CrMasSupContractCarCheckupDetail.AddAsync(contractOptionsEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, contractOptionsEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                contractOptionsVM.crMasSupContractCarCheckup = allCheckups_activated;
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddContractCarCheckupDetails", contractOptionsVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id, string no)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasSupContractCarCheckupDetail.FindAsync(x => x.CrMasSupContractCarCheckupDetailsCode == id && x.CrMasSupContractCarCheckupDetailsNo == no);
            var Checkup = await _unitOfWork.CrMasSupContractCarCheckup.FindAsync(x => x.CrMasSupContractCarCheckupCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractCarCheckupDetails");
            }
            var model = _mapper.Map<ContractCarCheckupDetailsVM>(contract);
            model.singleCheckup = Checkup;
            var allCheckups_Activated = await _unitOfWork.CrMasSupContractCarCheckup.FindAllAsNoTrackingAsync(x => x.CrMasSupContractCarCheckupStatus == Status.Active);
            if (!allCheckups_Activated.Any(x => x.CrMasSupContractCarCheckupCode.Trim() == contract.CrMasSupContractCarCheckupDetailsCode.Trim()))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "ContractCarCheckupDetails");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ContractCarCheckupDetailsVM contractOptionsVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && contractOptionsVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractCarCheckupDetails");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", contractOptionsVM);
                }
                var contractOptionsEntity = _mapper.Map<CrMasSupContractCarCheckupDetail>(contractOptionsVM);
                var thisnewEntity = await _unitOfWork.CrMasSupContractCarCheckupDetail.FindAsync(x => x.CrMasSupContractCarCheckupDetailsCode == contractOptionsVM.CrMasSupContractCarCheckupDetailsCode && x.CrMasSupContractCarCheckupDetailsNo == contractOptionsVM.CrMasSupContractCarCheckupDetailsNo);
                if (thisnewEntity == null)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", contractOptionsVM);
                }
                thisnewEntity.CrMasSupContractCarCheckupDetailsReasons = contractOptionsVM.CrMasSupContractCarCheckupDetailsReasons;

                _unitOfWork.CrMasSupContractCarCheckupDetail.Update(thisnewEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, thisnewEntity, Status.Update);
                return RedirectToAction("Index", "ContractCarCheckupDetails");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", contractOptionsVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status, string no)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupContractCarCheckupDetail.FindAsync(x => x.CrMasSupContractCarCheckupDetailsCode == code && x.CrMasSupContractCarCheckupDetailsNo == no);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupContractCarCheckupDetailsStatus = status;
                _unitOfWork.CrMasSupContractCarCheckupDetail.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupContractCarCheckupDetail entity)
        {

            var existedCode = await _masContractCarCheckupDetails.ExistsByCodeAsync(entity.CrMasSupContractCarCheckupDetailsCode, entity.CrMasSupContractCarCheckupDetailsNo);
            if (existedCode == "error_Codestart9")
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupDetailsNo", _localizer["error_Codestart9"]);
            }
            else if (existedCode == "Existing")
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupDetailsNo", _localizer["Existing"]);
            }

            if (await _masContractCarCheckupDetails.ExistsByArabicNameAsync(entity.CrMasSupContractCarCheckupDetailsArName, entity.CrMasSupContractCarCheckupDetailsCode, entity.CrMasSupContractCarCheckupDetailsNo))
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupDetailsArName", _localizer["Existing"]);
            }

            if (await _masContractCarCheckupDetails.ExistsByEnglishNameAsync(entity.CrMasSupContractCarCheckupDetailsEnName, entity.CrMasSupContractCarCheckupDetailsCode, entity.CrMasSupContractCarCheckupDetailsNo))
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupDetailsEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField, string code1, string No)
        {
            var All_ContractCarCheckupDetailss = await _unitOfWork.CrMasSupContractCarCheckupDetail.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_ContractCarCheckupDetailss != null)
            {
                errors.Add(new ErrorResponse { Field = existName, Message = " " });

                // Check for existing rental system ID
                if (existName == "CrMasSupContractCarCheckupDetailsNo")
                {
                    if (long.TryParse(No, out var nno) == false || code1.ToString().Length > 2 || No.ToString().Length > 2)
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupDetailsNo", Message = _localizer["error_Codestart9"] });
                    }
                    var thisobj = await _unitOfWork.CrMasSupContractCarCheckupDetail.FindAsync(x => x.CrMasSupContractCarCheckupDetailsCode == code1 && x.CrMasSupContractCarCheckupDetailsNo == No);
                    if (thisobj != null)
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupDetailsNo", Message = _localizer["Existing"] });
                    }
                }
                // Check for existing rental system ID
                if (existName == "CheckOption_Select")
                {
                    if (long.TryParse(dataField, out var code2) == false || dataField.ToString().Length != 2)
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupDetailsCode", Message = _localizer["error_Codestart2"] });
                    }
                    var thisobj = await _unitOfWork.CrMasSupContractCarCheckupDetail.FindAsync(x => x.CrMasSupContractCarCheckupDetailsCode == code1 && x.CrMasSupContractCarCheckupDetailsNo == No);
                    if (thisobj != null)
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupDetailsNo", Message = _localizer["Existing"] });
                    }
                    else
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupDetailsNo", Message = " " });
                    }
                }
                // Check for existing Arabic driving license
                else if (existName == "CrMasSupContractCarCheckupDetailsArName" && All_ContractCarCheckupDetailss.Any(x => x.CrMasSupContractCarCheckupDetailsArName == dataField && x.CrMasSupContractCarCheckupDetailsCode == code1.Trim() && x.CrMasSupContractCarCheckupDetailsNo != No.Trim()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupDetailsArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupContractCarCheckupDetailsEnName" && All_ContractCarCheckupDetailss.Any(x => x.CrMasSupContractCarCheckupDetailsEnName?.ToLower() == dataField.ToLower() && x.CrMasSupContractCarCheckupDetailsCode == code1.Trim() && x.CrMasSupContractCarCheckupDetailsNo != No.Trim()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupDetailsEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupContractCarCheckupDetail.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupContractCarCheckupDetailsCode) + 1).ToString() : "10";
        }

        private async Task<int> CountofThisEntityAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupContractCarCheckupDetail.GetAllAsyncAsNoTrackingAsync();
            return allLicenses.Count();
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupContractCarCheckupDetail licence, string status)
        {


            var recordAr = licence.CrMasSupContractCarCheckupDetailsArName;
            var recordEn = licence.CrMasSupContractCarCheckupDetailsEnName;
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
            return RedirectToAction("Index", "ContractCarCheckupDetails");
        }


    }
}
