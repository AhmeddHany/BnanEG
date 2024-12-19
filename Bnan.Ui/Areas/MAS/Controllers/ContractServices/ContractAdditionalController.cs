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
    public class ContractAdditionalController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasContractAdditional _masContractAdditional;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ContractAdditionalController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupContractAdditional;


        public ContractAdditionalController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasContractAdditional masContractAdditional, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ContractAdditionalController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masContractAdditional = masContractAdditional;
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
            var contractAdditionals = await _unitOfWork.CrMasSupContractAdditional
                .FindAllAsNoTrackingAsync(x => x.CrMasSupContractAdditionalStatus == Status.Active);

            // If no active licenses, retrieve all licenses
            if (!contractAdditionals.Any())
            {
                contractAdditionals = await _unitOfWork.CrMasSupContractAdditional
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupContractAdditionalStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(contractAdditionals);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetContractAdditionalByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var ContractAdditionalsAll = await _unitOfWork.CrMasSupContractAdditional.FindAllAsNoTrackingAsync(x => x.CrMasSupContractAdditionalStatus == Status.Active ||
                                                                                                                            x.CrMasSupContractAdditionalStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupContractAdditionalStatus == Status.Hold);

                if (status == Status.All)
                {
                    var FilterAll = ContractAdditionalsAll.FindAll(x => x.CrMasSupContractAdditionalStatus != Status.Deleted &&
                                                                         (x.CrMasSupContractAdditionalArName.Contains(search) ||
                                                                          x.CrMasSupContractAdditionalEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupContractAdditionalCode.Contains(search)));
                    return PartialView("_DataTableContractAdditional", FilterAll);
                }
                var FilterByStatus = ContractAdditionalsAll.FindAll(x => x.CrMasSupContractAdditionalStatus == status &&
                                                                            (
                                                                           x.CrMasSupContractAdditionalArName.Contains(search) ||
                                                                           x.CrMasSupContractAdditionalEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupContractAdditionalCode.Contains(search)));
                return PartialView("_DataTableContractAdditional", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddContractAdditional()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractAdditional");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "ContractAdditional");
            }
            // Check If code > 9 get error , because code is char(1)
            if (await GeneratCountCodeAsync() > 99999998)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "ContractAdditional");
            }
            // Set Title 
            ContractAdditionalVM contractAdditionalVM = new ContractAdditionalVM();
            //contractAdditionalVM.CrMasSupContractAdditionalCode = await GenerateLicenseCodeAsync();
            return View(contractAdditionalVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddContractAdditional(ContractAdditionalVM contractAdditionalVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || contractAdditionalVM == null)
            {
                return View("AddContractAdditional", contractAdditionalVM);
            }
            try
            {
                // Map ViewModel to Entity
                var contractAdditionalEntity = _mapper.Map<CrMasSupContractAdditional>(contractAdditionalVM);

                contractAdditionalEntity.CrMasSupContractAdditionalNaqlCode ??= 0;
                //contractAdditionalEntity.CrMasSupContractAdditionalNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masContractAdditional.ExistsByDetails_Add_Async(contractAdditionalEntity))
                {
                    await AddModelErrorsAsync(contractAdditionalEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddContractAdditional", contractAdditionalVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (await GeneratCountCodeAsync() > 99999998)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddContractAdditional", contractAdditionalVM);
                }
                //// Generate and set the Driving License Code
                //contractAdditionalVM.CrMasSupContractAdditionalCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                contractAdditionalEntity.CrMasSupContractAdditionalStatus = "A";
                await _unitOfWork.CrMasSupContractAdditional.AddAsync(contractAdditionalEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, contractAdditionalEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddContractAdditional", contractAdditionalVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasSupContractAdditional.FindAsync(x => x.CrMasSupContractAdditionalCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractAdditional");
            }
            var model = _mapper.Map<ContractAdditionalVM>(contract);
            model.CrMasSupContractAdditionalNaqlCode ??= 0;
            //model.CrMasSupContractAdditionalNaqlId ??= 0;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ContractAdditionalVM contractAdditionalVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && contractAdditionalVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ContractAdditional");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", contractAdditionalVM);
                }
                var contractAdditionalEntity = _mapper.Map<CrMasSupContractAdditional>(contractAdditionalVM);
                contractAdditionalEntity.CrMasSupContractAdditionalNaqlCode ??= 0;
                //contractAdditionalEntity.CrMasSupContractAdditionalNaqlId ??= 0;

                // Check if the entity already exists
                if (await _masContractAdditional.ExistsByDetailsAsync(contractAdditionalEntity))
                {
                    await AddModelErrorsAsync(contractAdditionalEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", contractAdditionalVM);
                }

                _unitOfWork.CrMasSupContractAdditional.Update(contractAdditionalEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, contractAdditionalEntity, Status.Update);
                return RedirectToAction("Index", "ContractAdditional");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", contractAdditionalVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupContractAdditional.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupContractAdditionalStatus = status;
                _unitOfWork.CrMasSupContractAdditional.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupContractAdditional entity)
        {

            var existedCode = await _masContractAdditional.ExistsByCodeAsync(entity.CrMasSupContractAdditionalCode);
            if (existedCode== "error_Codestart50")
            {
                ModelState.AddModelError("CrMasSupContractAdditionalCode", _localizer["error_Codestart50"]);
            }
            else if(existedCode== "Existing")
            {
                ModelState.AddModelError("CrMasSupContractAdditionalCode", _localizer["Existing"]);
            }

            if (await _masContractAdditional.ExistsByArabicNameAsync(entity.CrMasSupContractAdditionalArName, entity.CrMasSupContractAdditionalCode))
            {
                ModelState.AddModelError("CrMasSupContractAdditionalArName", _localizer["Existing"]);
            }

            if (await _masContractAdditional.ExistsByEnglishNameAsync(entity.CrMasSupContractAdditionalEnName, entity.CrMasSupContractAdditionalCode))
            {
                ModelState.AddModelError("CrMasSupContractAdditionalEnName", _localizer["Existing"]);
            }

            if (await _masContractAdditional.ExistsByNaqlCodeAsync((int)entity.CrMasSupContractAdditionalNaqlCode, entity.CrMasSupContractAdditionalCode))
            {
                ModelState.AddModelError("CrMasSupContractAdditionalNaqlCode", _localizer["Existing"]);
            }

            //if (await _masContractAdditional.ExistsByNaqlIdAsync((int)entity.CrMasSupContractAdditionalNaqlId, entity.CrMasSupContractAdditionalCode))
            //{
            //    ModelState.AddModelError("CrMasSupContractAdditionalNaqlId", _localizer["Existing"]);
            //}
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_ContractAdditionals = await _unitOfWork.CrMasSupContractAdditional.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_ContractAdditionals != null)
            {
                errors.Add(new ErrorResponse { Field = existName, Message = " " });
                // Check for existing rental system ID
                if (existName == "CrMasSupContractAdditionalCode")
                {
                    //if (Int64.TryParse(dataField, out var code) == false)
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalCode", Message = _localizer["error_Codestart50"] });
                    //}
                    //else if (dataField.ToString().Substring(0, 2) != "50")
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalCode", Message = _localizer["error_Codestart50"] });
                    //}
                    //else if (dataField.ToString().Length != 10)
                    //{
                    //    errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalCode", Message = _localizer["error_Codestart50"] });
                    //}
                    if(Int64.TryParse(dataField, out var id) && id != 0 && All_ContractAdditionals.Any(x => x.CrMasSupContractAdditionalCode == id.ToString()))
                    {
                        errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalCode", Message = _localizer["Existing"] });
                    }
                }
                // Check for existing Arabic driving license
                else if(existName == "CrMasSupContractAdditionalArName" && All_ContractAdditionals.Any(x => x.CrMasSupContractAdditionalArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupContractAdditionalEnName" && All_ContractAdditionals.Any(x => x.CrMasSupContractAdditionalEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupContractAdditionalNaqlCode" && Int64.TryParse(dataField, out var code) && code != 0 && All_ContractAdditionals.Any(x => x.CrMasSupContractAdditionalNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalNaqlCode", Message = _localizer["Existing"] });
                }
                //// Check for existing rental system ID
                //else if (existName == "CrMasSupContractAdditionalNaqlId" && Int64.TryParse(dataField, out var id) && id != 0 && All_ContractAdditionals.Any(x => x.CrMasSupContractAdditionalNaqlId == id))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSupContractAdditionalNaqlId", Message = _localizer["Existing"] });
                //}
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupContractAdditional.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupContractAdditionalCode) + 1).ToString() : "5000000001";
        }
        private async Task<int> GeneratCountCodeAsync()
        {
            var Count = await _unitOfWork.CrMasSupContractAdditional.CountAsync();
            return Count;
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupContractAdditional licence, string status)
        {


            var recordAr = licence.CrMasSupContractAdditionalArName;
            var recordEn = licence.CrMasSupContractAdditionalEnName;
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
            return RedirectToAction("Index", "ContractAdditional");
        }


    }
}
