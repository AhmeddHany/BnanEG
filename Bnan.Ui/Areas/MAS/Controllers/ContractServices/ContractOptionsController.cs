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
    public class ContractOptionsController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasContractOptions _masContractOptions;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ContractOptionsController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupContractOption;


        public ContractOptionsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasContractOptions masContractOptions, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ContractOptionsController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masContractOptions = masContractOptions;
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
            var contractOptionss = await _unitOfWork.CrMasSupContractOption
                .FindAllAsNoTrackingAsync(x => x.CrMasSupContractOptionsStatus == Status.Active);

            // If no active licenses, retrieve all licenses
            if (!contractOptionss.Any())
            {
                contractOptionss = await _unitOfWork.CrMasSupContractOption
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupContractOptionsStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(contractOptionss);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetContractOptionsByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var ContractOptionssAll = await _unitOfWork.CrMasSupContractOption.FindAllAsNoTrackingAsync(x => x.CrMasSupContractOptionsStatus == Status.Active ||
                                                                                                                            x.CrMasSupContractOptionsStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupContractOptionsStatus == Status.Hold);

                if (status == Status.All)
                {
                    var FilterAll = ContractOptionssAll.FindAll(x => x.CrMasSupContractOptionsStatus != Status.Deleted &&
                                                                         (x.CrMasSupContractOptionsArName.Contains(search) ||
                                                                          x.CrMasSupContractOptionsEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractOptionsCode.Contains(search)));
                    return PartialView("_DataTableContractOptions", FilterAll);
                }
                var FilterByStatus = ContractOptionssAll.FindAll(x => x.CrMasSupContractOptionsStatus == status &&
                                                                            (
                                                                           x.CrMasSupContractOptionsArName.Contains(search) ||
                                                                           x.CrMasSupContractOptionsEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupContractOptionsCode.Contains(search)));
                return PartialView("_DataTableContractOptions", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddContractOptions()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractOptions");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "ContractOptions");
            }
            // Check If code > 9 get error , because code is char(1)
            if (await GeneratCountCodeAsync() > 99999998)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "ContractOptions");
            }
            // Set Title 
            ContractOptionsVM contractOptionsVM = new ContractOptionsVM();
            //contractOptionsVM.CrMasSupContractOptionsCode = await GenerateLicenseCodeAsync();
            return View(contractOptionsVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddContractOptions(ContractOptionsVM contractOptionsVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || contractOptionsVM == null)
            {
                return View("AddContractOptions", contractOptionsVM);
            }
            try
            {
                // Map ViewModel to Entity
                var contractOptionsEntity = _mapper.Map<CrMasSupContractOption>(contractOptionsVM);

                contractOptionsEntity.CrMasSupContractOptionsNaqlCode ??= 0;
                //contractOptionsEntity.CrMasSupContractOptionsNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masContractOptions.ExistsByDetails_Add_Async(contractOptionsEntity))
                {
                    await AddModelErrorsAsync(contractOptionsEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddContractOptions", contractOptionsVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (await GeneratCountCodeAsync() > 99999998)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddContractOptions", contractOptionsVM);
                }
                //// Generate and set the Driving License Code
                //contractOptionsVM.CrMasSupContractOptionsCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                contractOptionsEntity.CrMasSupContractOptionsStatus = "A";
                await _unitOfWork.CrMasSupContractOption.AddAsync(contractOptionsEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, contractOptionsEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddContractOptions", contractOptionsVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasSupContractOption.FindAsync(x => x.CrMasSupContractOptionsCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractOptions");
            }
            var model = _mapper.Map<ContractOptionsVM>(contract);
            model.CrMasSupContractOptionsNaqlCode ??= 0;
            //model.CrMasSupContractOptionsNaqlId ??= 0;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ContractOptionsVM contractOptionsVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && contractOptionsVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractOptions");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", contractOptionsVM);
                }
                var contractOptionsEntity = _mapper.Map<CrMasSupContractOption>(contractOptionsVM);
                contractOptionsEntity.CrMasSupContractOptionsNaqlCode ??= 0;
                //contractOptionsEntity.CrMasSupContractOptionsNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masContractOptions.ExistsByDetailsAsync(contractOptionsEntity))
                {
                    await AddModelErrorsAsync(contractOptionsEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", contractOptionsVM);
                }

                _unitOfWork.CrMasSupContractOption.Update(contractOptionsEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, contractOptionsEntity, Status.Update);
                return RedirectToAction("Index", "ContractOptions");
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

            var licence = await _unitOfWork.CrMasSupContractOption.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupContractOptionsStatus = status;
                _unitOfWork.CrMasSupContractOption.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupContractOption entity)
        {

            var existedCode = await _masContractOptions.ExistsByCodeAsync(entity.CrMasSupContractOptionsCode);
            if (existedCode== "error_Codestart51")
            {
                ModelState.AddModelError("CrMasSupContractOptionsCode", _localizer["error_Codestart51"]);
            }
            else if(existedCode== "Existing")
            {
                ModelState.AddModelError("CrMasSupContractOptionsCode", _localizer["Existing"]);
            }

            if (await _masContractOptions.ExistsByArabicNameAsync(entity.CrMasSupContractOptionsArName, entity.CrMasSupContractOptionsCode))
            {
                ModelState.AddModelError("CrMasSupContractOptionsArName", _localizer["Existing"]);
            }

            if (await _masContractOptions.ExistsByEnglishNameAsync(entity.CrMasSupContractOptionsEnName, entity.CrMasSupContractOptionsCode))
            {
                ModelState.AddModelError("CrMasSupContractOptionsEnName", _localizer["Existing"]);
            }

            if (await _masContractOptions.ExistsByNaqlCodeAsync((int)entity.CrMasSupContractOptionsNaqlCode, entity.CrMasSupContractOptionsCode))
            {
                ModelState.AddModelError("CrMasSupContractOptionsNaqlCode", _localizer["Existing"]);
            }

            //if (await _masContractOptions.ExistsByNaqlIdAsync((int)entity.CrMasSupContractOptionsNaqlId, entity.CrMasSupContractOptionsCode))
            //{
            //    ModelState.AddModelError("CrMasSupContractOptionsNaqlId", _localizer["Existing"]);
            //}
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_ContractOptionss = await _unitOfWork.CrMasSupContractOption.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_ContractOptionss != null)
            {
                errors.Add(new ErrorResponse { Field = existName, Message = " " });
                // Check for existing rental system ID
                if (existName == "CrMasSupContractOptionsCode")
                {
                    //if (Int64.TryParse(dataField, out var code) == false)
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsCode", Message = _localizer["error_Codestart51"] });
                    //}
                    //else if (dataField.ToString().Substring(0, 2) != "51")
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsCode", Message = _localizer["error_Codestart51"] });
                    //}
                    //else if (dataField.ToString().Length != 10)
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsCode", Message = _localizer["error_Codestart51"] });
                    //}
                    if(Int64.TryParse(dataField, out var id) && id != 0 && All_ContractOptionss.Any(x => x.CrMasSupContractOptionsCode == id.ToString()))
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsCode", Message = _localizer["Existing"] });
                    }
                }
                // Check for existing Arabic driving license
                else if(existName == "CrMasSupContractOptionsArName" && All_ContractOptionss.Any(x => x.CrMasSupContractOptionsArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupContractOptionsEnName" && All_ContractOptionss.Any(x => x.CrMasSupContractOptionsEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupContractOptionsNaqlCode" && Int64.TryParse(dataField, out var code) && code != 0 && All_ContractOptionss.Any(x => x.CrMasSupContractOptionsNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsNaqlCode", Message = _localizer["Existing"] });
                }
                //// Check for existing rental system ID
                //else if (existName == "CrMasSupContractOptionsNaqlId" && Int64.TryParse(dataField, out var id) && id != 0 && All_ContractOptionss.Any(x => x.CrMasSupContractOptionsNaqlId == id))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSupContractOptionsNaqlId", Message = _localizer["Existing"] });
                //}
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupContractOption.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupContractOptionsCode) + 1).ToString() : "5100000001";
        }
        private async Task<int> GeneratCountCodeAsync()
        {
            var Count = await _unitOfWork.CrMasSupContractOption.CountAsync();
            return Count;
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupContractOption licence, string status)
        {


            var recordAr = licence.CrMasSupContractOptionsArName;
            var recordEn = licence.CrMasSupContractOptionsEnName;
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
            return RedirectToAction("Index", "ContractOptions");
        }


    }
}
