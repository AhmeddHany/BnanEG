using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.Identitiy;
using Bnan.Ui.ViewModels.MAS.UserValiditySystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Data;

namespace Bnan.Ui.Areas.MAS.Controllers.Employees
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]

    public class UsersSystemValiditionsController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserMainValidtion _userMainValidtion;
        private readonly IUserSubValidition _userSubValidition;
        private readonly IUserProcedureValidition _userProcedureValidition;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly IToastNotification _toastNotification;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasUser _masUser;
        private readonly string pageNumber = SubTasks.CrMasUserSystemValiditionsMAS;
        public UsersSystemValiditionsController(IUserService userService,
                               IAuthService authService,
                               IWebHostEnvironment webHostEnvironment,
                               UserManager<CrMasUserInformation> userManager,
                               IUnitOfWork unitOfWork, IUserLoginsService userLoginsService,
                               IMapper mapper, IUserMainValidtion userMainValidtion,
                               IUserSubValidition userSubValidition,
                               IStringLocalizer<UsersController> localizer,
                               IUserProcedureValidition userProcedureValidition, IToastNotification toastNotification, IBaseRepo baseRepo, IMasUser masUser) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _authService = authService;
            _webHostEnvironment = webHostEnvironment;
            _userLoginsService = userLoginsService;
            _userMainValidtion = userMainValidtion;
            _userSubValidition = userSubValidition;
            _userProcedureValidition = userProcedureValidition;
            _localizer = localizer;
            _toastNotification = toastNotification;
            _baseRepo = baseRepo;
            _masUser = masUser;
        }
        [HttpGet]
        public async Task<IActionResult> SystemValiditions()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);


            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            // Retrieve active driving licenses
            var usersInfo = await _unitOfWork.CrMasUserInformation
                .FindAllAsNoTrackingAsync(x => x.CrMasUserInformationCode != Status.MASUserCode &&
                                              x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                              x.CrMasUserInformationStatus == Status.Active &&
                                              x.CrMasUserInformationLessor == Status.MASLessorCode);

            // If no active licenses, retrieve all licenses
            if (!usersInfo.Any())
            {
                usersInfo = await _unitOfWork.CrMasUserInformation
                    .FindAllAsNoTrackingAsync(x => x.CrMasUserInformationCode != Status.MASUserCode &&
                                              x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                              x.CrMasUserInformationStatus == Status.Hold
                                              && x.CrMasUserInformationLessor == Status.MASLessorCode);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(usersInfo);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetUserSystemValiditionsByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(status))
            {
                var usersInfo = await _unitOfWork.CrMasUserInformation.FindAllAsNoTrackingAsync(x => x.CrMasUserInformationCode != Status.MASUserCode &&
                                                                                                     x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                                                                     x.CrMasUserInformationLessor == Status.MASLessorCode &&
                                                                                                    (x.CrMasUserInformationStatus == Status.Active ||
                                                                                                     x.CrMasUserInformationStatus == Status.Deleted ||
                                                                                                     x.CrMasUserInformationStatus == Status.Hold));

                if (status == Status.All)
                {
                    var FilterAll = usersInfo.FindAll(x => x.CrMasUserInformationStatus != Status.Deleted &&
                                                                         (x.CrMasUserInformationArName.Contains(search) ||
                                                                          x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                          x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationCode.Contains(search)));
                    return PartialView("_DataTableSystemValiditions", FilterAll);
                }
                var FilterByStatus = usersInfo.FindAll(x => x.CrMasUserInformationStatus == status &&
                                                                         (x.CrMasUserInformationArName.Contains(search) ||
                                                                          x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                          x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationCode.Contains(search)));
                return PartialView("_DataTableSystemValiditions", FilterByStatus);
            }
            return PartialView();
        }
        [HttpGet]
        public async Task<IActionResult> EditSystemValiditions(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            var EditedUser = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == id);
            if (EditedUser == null || user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Users", "Users");
            }
            var subTasksHaveStatusDorW = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksStatus == Status.Deleted || x.CrMasSysSubTasksStatus == Status.Wating);
            var subTaskIdsWithStatusDorW = subTasksHaveStatusDorW.Select(x => x.CrMasSysSubTasksCode).ToHashSet();

            var mainValidition = await _unitOfWork.CrMasUserMainValidations.FindAllAsNoTrackingAsync(x => x.CrMasUserMainValidationUser == id);
            var subValition = await _unitOfWork.CrMasUserSubValidations.FindAllAsNoTrackingAsync(x => x.CrMasUserSubValidationUser == id);
            var procedureValidition = await _unitOfWork.CrMasUserProceduresValidations.FindAllAsNoTrackingAsync(x => x.CrMasUserProceduresValidationCode == id);
            var procedureTasks = await _unitOfWork.CrMasSysProceduresTasks.GetAllAsyncAsNoTrackingAsync();
            RegisterViewModel viewModel = new RegisterViewModel
            {
                CrMasSysMainTasks = (List<CrMasSysMainTask>)_unitOfWork.CrMasSysMainTasks.FindAll(x => x.CrMasSysMainTasksSystem == "1"),
                CrMasUserMainValidations = mainValidition,

                CrMasUserInformationCode = EditedUser.CrMasUserInformationCode,
                CrMasUserInformationArName = EditedUser.CrMasUserInformationArName,
                CrMasUserInformationEnName = EditedUser.CrMasUserInformationEnName,

                CrMasSysSubTasks = (List<CrMasSysSubTask>)_unitOfWork.CrMasSysSubTasks.FindAll(x => x.CrMasSysSubTasksSystemCode == "1"),
                CrMasUserSubValidations = subValition.Where(x => !subTaskIdsWithStatusDorW.Contains(x.CrMasUserSubValidationSubTasks)).ToList(),
                CrMasSysProceduresTasks = (List<CrMasSysProceduresTask>)procedureTasks,
                ProceduresValidations = procedureValidition,
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditSystemValiditions([FromBody] CheckBoxModels model)
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return Json(new { success = false });
            }
            try
            {
                foreach (var checkboxMain in model.CheckboxesMainTask)
                {
                    var mainTask = await _unitOfWork.CrMasUserMainValidations.FindAsync(x => x.CrMasUserMainValidationUser == model.UserId && x.CrMasUserMainValidationMainTasks == checkboxMain.id);
                    if (mainTask != null) mainTask.CrMasUserMainValidationAuthorization = checkboxMain.value;
                    _unitOfWork.CrMasUserMainValidations.Update(mainTask);

                }
                foreach (var checkboxSub in model.CheckboxesSubTask)
                {
                    var mainTask = await _unitOfWork.CrMasUserMainValidations.FindAsync(x => x.CrMasUserMainValidationUser == model.UserId && x.CrMasUserMainValidationMainTasks == checkboxSub.mainTaskId);
                    var subTask = await _unitOfWork.CrMasUserSubValidations.FindAsync(x => x.CrMasUserSubValidationUser == model.UserId && x.CrMasUserSubValidationMain == checkboxSub.mainTaskId && x.CrMasUserSubValidationSubTasks == checkboxSub.subTaskId);
                    if (mainTask.CrMasUserMainValidationAuthorization == true)
                    {
                        if (subTask != null)
                        {
                            if (subTask.CrMasUserSubValidationAuthorization == false && checkboxSub.value == true)
                            {
                                // SaveTracing
                                var (mainTask1, subTask1, system, currentUser) = await SetTrace("105", "1105002", "1");
                                var RecordAr = $"{_unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == model.UserId).CrMasUserInformationArName} - {_unitOfWork.CrMasSysSubTasks.Find(x => x.CrMasSysSubTasksCode == subTask.CrMasUserSubValidationSubTasks).CrMasSysSubTasksArName.ToString()}";
                                var RecordEn = $"{_unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == model.UserId).CrMasUserInformationEnName} - {_unitOfWork.CrMasSysSubTasks.Find(x => x.CrMasSysSubTasksCode == subTask.CrMasUserSubValidationSubTasks).CrMasSysSubTasksEnName.ToString()}";
                                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "إضافة دور", "Add role", mainTask1.CrMasSysMainTasksCode,
                                subTask1.CrMasSysSubTasksCode, mainTask1.CrMasSysMainTasksArName, subTask1.CrMasSysSubTasksArName, mainTask1.CrMasSysMainTasksEnName,
                                subTask1.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                            }

                            subTask.CrMasUserSubValidationAuthorization = checkboxSub.value;
                        }
                    }
                    else
                    {
                        if (subTask.CrMasUserSubValidationAuthorization == true)
                        {
                            // SaveTracing
                            var (mainTask1, subTask1, system, currentUser) = await SetTrace("105", "1105002", "1");
                            var RecordAr = $"{_unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == model.UserId).CrMasUserInformationArName} - {_unitOfWork.CrMasSysSubTasks.Find(x => x.CrMasSysSubTasksCode == subTask.CrMasUserSubValidationSubTasks).CrMasSysSubTasksArName.ToString()}";
                            var RecordEn = $"{_unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == model.UserId).CrMasUserInformationEnName} - {_unitOfWork.CrMasSysSubTasks.Find(x => x.CrMasSysSubTasksCode == subTask.CrMasUserSubValidationSubTasks).CrMasSysSubTasksEnName.ToString()}";
                            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "حذف دور", "Remove role", mainTask1.CrMasSysMainTasksCode,
                            subTask1.CrMasSysSubTasksCode, mainTask1.CrMasSysMainTasksArName, subTask1.CrMasSysSubTasksArName, mainTask1.CrMasSysMainTasksEnName,
                            subTask1.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                        }

                        subTask.CrMasUserSubValidationAuthorization = false;
                    }
                    _unitOfWork.CrMasUserSubValidations.Update(subTask);
                }
                foreach (var checkboxProcedure in model.CheckboxesProcedureTask)
                {
                    var mainTask = await _unitOfWork.CrMasUserMainValidations.FindAsync(x => x.CrMasUserMainValidationUser == model.UserId && x.CrMasUserMainValidationMainTasks == checkboxProcedure.mainTaskId);
                    var subTask = await _unitOfWork.CrMasUserSubValidations.FindAsync(x => x.CrMasUserSubValidationUser == model.UserId && x.CrMasUserSubValidationMain == checkboxProcedure.mainTaskId && x.CrMasUserSubValidationSubTasks == checkboxProcedure.subTaskId);
                    var procedureTask = await _unitOfWork.CrMasUserProceduresValidations.FindAsync(x => x.CrMasUserProceduresValidationCode == model.UserId && x.CrMasUserProceduresValidationMainTask == checkboxProcedure.mainTaskId && x.CrMasUserProceduresValidationSubTasks == checkboxProcedure.subTaskId);

                    if (mainTask.CrMasUserMainValidationAuthorization == true && subTask.CrMasUserSubValidationAuthorization == true)
                    {
                        if (procedureTask != null && checkboxProcedure != null)
                        {
                            if (checkboxProcedure.procedureName.ToLower() == "insert") procedureTask.CrMasUserProceduresValidationInsertAuthorization = checkboxProcedure.value;
                            if (checkboxProcedure.procedureName.ToLower() == "update") procedureTask.CrMasUserProceduresValidationUpDateAuthorization = checkboxProcedure.value;
                            if (checkboxProcedure.procedureName.ToLower() == "hold") procedureTask.CrMasUserProceduresValidationHoldAuthorization = checkboxProcedure.value;
                            if (checkboxProcedure.procedureName.ToLower() == "unhold") procedureTask.CrMasUserProceduresValidationUnHoldAuthorization = checkboxProcedure.value;
                            if (checkboxProcedure.procedureName.ToLower() == "delete") procedureTask.CrMasUserProceduresValidationDeleteAuthorization = checkboxProcedure.value;
                            if (checkboxProcedure.procedureName.ToLower() == "undelete") procedureTask.CrMasUserProceduresValidationUnDeleteAuthorization = checkboxProcedure.value;


                        }
                    }
                    else
                    {
                        procedureTask.CrMasUserProceduresValidationInsertAuthorization = false;
                        procedureTask.CrMasUserProceduresValidationUpDateAuthorization = false;
                        procedureTask.CrMasUserProceduresValidationHoldAuthorization = false;
                        procedureTask.CrMasUserProceduresValidationUnHoldAuthorization = false;
                        procedureTask.CrMasUserProceduresValidationDeleteAuthorization = false;
                        procedureTask.CrMasUserProceduresValidationUnDeleteAuthorization = false;
                    }
                    _unitOfWork.CrMasUserProceduresValidations.Update(procedureTask);

                }
                await _unitOfWork.CompleteAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }


        }
        private async Task SaveTracingForUserChange(CrMasUserInformation userCreated, string status)
        {


            var recordAr = $"{userCreated.CrMasUserInformationArName} - {userCreated.CrMasUserInformationTasksArName}";
            var recordEn = $"{userCreated.CrMasUserInformationEnName} - {userCreated.CrMasUserInformationTasksEnName}";
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
        public IActionResult DisplayToastSuccess_ForEditValidtions()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            return RedirectToAction("SystemValiditions", "UsersSystemValiditions");
        }
        public IActionResult DisplayToastError_ForEditValidtions()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            return RedirectToAction("SystemValiditions", "UsersSystemValiditions");
        }

    }
}