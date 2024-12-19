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
using NToastNotify;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.ContractServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class ContractCarCheckupController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasContractCarCheckup _masContractCarCheckup;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ContractCarCheckupController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupContractCarCheckup;


        public ContractCarCheckupController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasContractCarCheckup masContractCarCheckup, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ContractCarCheckupController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masContractCarCheckup = masContractCarCheckup;
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
            var contractOptionss = await _unitOfWork.CrMasSupContractCarCheckup
                .FindAllAsNoTrackingAsync(x => x.CrMasSupContractCarCheckupStatus == Status.Active);

            // If no active licenses, retrieve all licenses
            if (!contractOptionss.Any())
            {
                contractOptionss = await _unitOfWork.CrMasSupContractCarCheckup
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupContractCarCheckupStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(contractOptionss);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetContractCarCheckupByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var ContractCarCheckupsAll = await _unitOfWork.CrMasSupContractCarCheckup.FindAllAsNoTrackingAsync(x => x.CrMasSupContractCarCheckupStatus == Status.Active ||
                                                                                                                            x.CrMasSupContractCarCheckupStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupContractCarCheckupStatus == Status.Hold);

                if (status == Status.All)
                {
                    var FilterAll = ContractCarCheckupsAll.FindAll(x => x.CrMasSupContractCarCheckupStatus != Status.Deleted &&
                                                                         (x.CrMasSupContractCarCheckupArName.Contains(search) ||
                                                                          x.CrMasSupContractCarCheckupEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractCarCheckupCode.Contains(search)));
                    return PartialView("_DataTableContractCarCheckup", FilterAll);
                }
                var FilterByStatus = ContractCarCheckupsAll.FindAll(x => x.CrMasSupContractCarCheckupStatus == status &&
                                                                            (
                                                                           x.CrMasSupContractCarCheckupArName.Contains(search) ||
                                                                           x.CrMasSupContractCarCheckupEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupContractCarCheckupCode.Contains(search)));
                return PartialView("_DataTableContractCarCheckup", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddContractCarCheckup()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractCarCheckup");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "ContractCarCheckup");
            }
            // Check If code > 9 get error , because code is char(1)
            if (await GeneratCountCodeAsync() > 89)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "ContractCarCheckup");
            }
            // Set Title 
            ContractCarCheckupVM contractOptionsVM = new ContractCarCheckupVM();
            //contractOptionsVM.CrMasSupContractCarCheckupCode = await GenerateLicenseCodeAsync();
            return View(contractOptionsVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddContractCarCheckup(ContractCarCheckupVM contractOptionsVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || contractOptionsVM == null)
            {
                return View("AddContractCarCheckup", contractOptionsVM);
            }
            try
            {
                // Map ViewModel to Entity
                var contractOptionsEntity = _mapper.Map<CrMasSupContractCarCheckup>(contractOptionsVM);

                //contractOptionsEntity.CrMasSupContractCarCheckupNaqlCode ??= 0;
                //contractOptionsEntity.CrMasSupContractCarCheckupNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masContractCarCheckup.ExistsByDetails_Add_Async(contractOptionsEntity))
                {
                    await AddModelErrorsAsync(contractOptionsEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddContractCarCheckup", contractOptionsVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (await GeneratCountCodeAsync() > 89)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddContractCarCheckup", contractOptionsVM);
                }
                //// Generate and set the Driving License Code
                //contractOptionsVM.CrMasSupContractCarCheckupCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                contractOptionsEntity.CrMasSupContractCarCheckupStatus = "A";
                await _unitOfWork.CrMasSupContractCarCheckup.AddAsync(contractOptionsEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, contractOptionsEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddContractCarCheckup", contractOptionsVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasSupContractCarCheckup.FindAsync(x => x.CrMasSupContractCarCheckupCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractCarCheckup");
            }
            var model = _mapper.Map<ContractCarCheckupVM>(contract);
            //model.CrMasSupContractCarCheckupNaqlCode ??= 0;
            //model.CrMasSupContractCarCheckupNaqlId ??= 0;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ContractCarCheckupVM contractOptionsVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && contractOptionsVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractCarCheckup");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", contractOptionsVM);
                }
                var contractOptionsEntity = _mapper.Map<CrMasSupContractCarCheckup>(contractOptionsVM);
                //contractOptionsEntity.CrMasSupContractCarCheckupNaqlCode ??= 0;
                //contractOptionsEntity.CrMasSupContractCarCheckupNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masContractCarCheckup.ExistsByDetailsAsync(contractOptionsEntity))
                {
                    await AddModelErrorsAsync(contractOptionsEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", contractOptionsVM);
                }

                _unitOfWork.CrMasSupContractCarCheckup.Update(contractOptionsEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, contractOptionsEntity, Status.Update);
                return RedirectToAction("Index", "ContractCarCheckup");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", contractOptionsVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupContractCarCheckup.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupContractCarCheckupStatus = status;
                _unitOfWork.CrMasSupContractCarCheckup.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupContractCarCheckup entity)
        {

            var existedCode = await _masContractCarCheckup.ExistsByCodeAsync(entity.CrMasSupContractCarCheckupCode);
            if (existedCode == "error_Codestart2")
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupCode", _localizer["error_Codestart2"]);
            }
            else if (existedCode == "Existing")
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupCode", _localizer["Existing"]);
            }

            if (await _masContractCarCheckup.ExistsByArabicNameAsync(entity.CrMasSupContractCarCheckupArName, entity.CrMasSupContractCarCheckupCode))
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupArName", _localizer["Existing"]);
            }

            if (await _masContractCarCheckup.ExistsByEnglishNameAsync(entity.CrMasSupContractCarCheckupEnName, entity.CrMasSupContractCarCheckupCode))
            {
                ModelState.AddModelError("CrMasSupContractCarCheckupEnName", _localizer["Existing"]);
            }

            //if (await _masContractCarCheckup.ExistsByNaqlCodeAsync((int)entity.CrMasSupContractCarCheckupNaqlCode, entity.CrMasSupContractCarCheckupCode))
            //{
            //    ModelState.AddModelError("CrMasSupContractCarCheckupNaqlCode", _localizer["Existing"]);
            //}

            //if (await _masContractCarCheckup.ExistsByNaqlIdAsync((int)entity.CrMasSupContractCarCheckupNaqlId, entity.CrMasSupContractCarCheckupCode))
            //{
            //    ModelState.AddModelError("CrMasSupContractCarCheckupNaqlId", _localizer["Existing"]);
            //}
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_ContractCarCheckups = await _unitOfWork.CrMasSupContractCarCheckup.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_ContractCarCheckups != null)
            {
                errors.Add(new ErrorResponse { Field = existName, Message = " " });
                // Check for existing rental system ID
                if (existName == "CrMasSupContractCarCheckupCode")
                {
                    //if (Int64.TryParse(dataField, out var code) == false)
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupCode", Message = _localizer["error_Codestart2"] });
                    //}
                    //else if (dataField.ToString().Length != 2)
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupCode", Message = _localizer["error_Codestart2"] });
                    //}
                    if (long.TryParse(dataField, out var id) && id != 0 && All_ContractCarCheckups.Any(x => x.CrMasSupContractCarCheckupCode == id.ToString()))
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupCode", Message = _localizer["Existing"] });
                    }
                }
                // Check for existing Arabic driving license
                else if (existName == "CrMasSupContractCarCheckupArName" && All_ContractCarCheckups.Any(x => x.CrMasSupContractCarCheckupArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupContractCarCheckupEnName" && All_ContractCarCheckups.Any(x => x.CrMasSupContractCarCheckupEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupEnName", Message = _localizer["Existing"] });
                }
                //// Check for existing rental system number
                //else if (existName == "CrMasSupContractCarCheckupNaqlCode" && Int64.TryParse(dataField, out var code) && code != 0 && All_ContractCarCheckups.Any(x => x.CrMasSupContractCarCheckupNaqlCode == code))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupNaqlCode", Message = _localizer["Existing"] });
                //}
                //// Check for existing rental system ID
                //else if (existName == "CrMasSupContractCarCheckupNaqlId" && Int64.TryParse(dataField, out var id) && id != 0 && All_ContractCarCheckups.Any(x => x.CrMasSupContractCarCheckupNaqlId == id))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSupContractCarCheckupNaqlId", Message = _localizer["Existing"] });
                //}
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupContractCarCheckup.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupContractCarCheckupCode) + 1).ToString() : "10";
        }
        private async Task<int> GeneratCountCodeAsync()
        {
            var Count = await _unitOfWork.CrMasSupContractCarCheckup.CountAsync();
            return Count;
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupContractCarCheckup licence, string status)
        {


            var recordAr = licence.CrMasSupContractCarCheckupArName;
            var recordEn = licence.CrMasSupContractCarCheckupEnName;
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
            return RedirectToAction("Index", "ContractCarCheckup");
        }


    }
}
