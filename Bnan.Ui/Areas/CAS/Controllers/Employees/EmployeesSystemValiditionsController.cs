using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS.Employees;
using Bnan.Ui.ViewModels.MAS.UserValiditySystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.CAS.Controllers.Employees
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class EmployeesSystemValiditionsController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly IUserMainValidtion _userMainValidtion;
        private readonly IUserSubValidition _userSubValidition;
        private readonly IUserProcedureValidition _userProcedureValidition;
        private readonly IUserBranchValidity _userBranchValidity;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<EmployeesSystemValiditionsController> _localizer;
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.CrMasUserSystemValiditionsCAS;


        public EmployeesSystemValiditionsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, IUserService userService, IWebHostEnvironment webHostEnvironment, IUserLoginsService userLoginsService, IUserMainValidtion userMainValidtion, IUserSubValidition userSubValidition, IUserProcedureValidition userProcedureValidition, IUserBranchValidity userBranchValidity, IToastNotification toastNotification, IStringLocalizer<EmployeesSystemValiditionsController> localizer, IAdminstritiveProcedures adminstritiveProcedures, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _authService = authService;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _userLoginsService = userLoginsService;
            _userMainValidtion = userMainValidtion;
            _userSubValidition = userSubValidition;
            _userProcedureValidition = userProcedureValidition;
            _userBranchValidity = userBranchValidity;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _adminstritiveProcedures = adminstritiveProcedures;
            _baseRepo = baseRepo;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lessorCode = user.CrMasUserInformationLessor;

            await SetPageTitleAsync(string.Empty, pageNumber);
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            var usersInfo = await _unitOfWork.CrMasUserInformation
                .FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && !x.CrMasUserInformationCode.StartsWith("CAS") &&
                                                  x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                  x.CrMasUserInformationStatus != Status.Deleted);

            return View(usersInfo);
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeesBySearch(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            var lessorCode = user.CrMasUserInformationLessor;

            var employeesInfo = await _unitOfWork.CrMasUserInformation.FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && !x.CrMasUserInformationCode.StartsWith("CAS") &&
                                                                                                 x.CrMasUserInformationCode != user.CrMasUserInformationCode && x.CrMasUserInformationStatus != Status.Deleted &&
                                                                                                 (x.CrMasUserInformationArName.Contains(search) ||
                                                                                                  x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                                                  x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                                                  x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                                                  x.CrMasUserInformationCode.Contains(search)));




            return PartialView("_DataTableEmployeesValidities", employeesInfo);

        }
        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "EmployeesSystemValiditions");
            }
            var EditedUser = await _unitOfWork.CrMasUserInformation.FindAsync(x => !id.StartsWith("CAS") && x.CrMasUserInformationCode == id &&
                                                                                   x.CrMasUserInformationLessor == user.CrMasUserInformationLessor  );
            if (EditedUser == null || user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "EmployeesSystemValiditions");
            }
            var subTasksHaveStatusDorW = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksStatus == Status.Deleted || x.CrMasSysSubTasksStatus == Status.Wating);
            var subTaskIdsWithStatusDorW = subTasksHaveStatusDorW.Select(x => x.CrMasSysSubTasksCode).ToHashSet();

            var mainValidition = await _unitOfWork.CrMasUserMainValidations.FindAllAsNoTrackingAsync(x => x.CrMasUserMainValidationUser == id);
            var subValition = await _unitOfWork.CrMasUserSubValidations.FindAllAsNoTrackingAsync(x => x.CrMasUserSubValidationUser == id);
            var procedureValidition = await _unitOfWork.CrMasUserProceduresValidations.FindAllAsNoTrackingAsync(x => x.CrMasUserProceduresValidationCode == id);
            var procedureTasks = await _unitOfWork.CrMasSysProceduresTasks.GetAllAsyncAsNoTrackingAsync();

            EmployeesValiditesVM viewModel = new EmployeesValiditesVM
            {
                MainTasks = (List<CrMasSysMainTask>)_unitOfWork.CrMasSysMainTasks.FindAll(x => x.CrMasSysMainTasksSystem == "2"),
                MainValidation = mainValidition,

                CrMasUserInformationCode = EditedUser.CrMasUserInformationCode,
                CrMasUserInformationArName = EditedUser.CrMasUserInformationArName,
                CrMasUserInformationEnName = EditedUser.CrMasUserInformationEnName,

                SubTasks = (List<CrMasSysSubTask>)_unitOfWork.CrMasSysSubTasks.FindAll(x => x.CrMasSysSubTasksSystemCode == "2"),
                SubValidation = subValition.Where(x => !subTaskIdsWithStatusDorW.Contains(x.CrMasUserSubValidationSubTasks)).ToList(),
                ProceduresTasks = (List<CrMasSysProceduresTask>)procedureTasks,
                ProceduresValidation = procedureValidition,
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] CheckBoxModels model)
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
                    var mainTask = _unitOfWork.CrMasUserMainValidations.Find(x => x.CrMasUserMainValidationUser == model.UserId && x.CrMasUserMainValidationMainTasks == checkboxMain.id);
                    if (mainTask != null) mainTask.CrMasUserMainValidationAuthorization = checkboxMain.value;
                }
                foreach (var checkboxSub in model.CheckboxesSubTask)
                {
                    var mainTask = _unitOfWork.CrMasUserMainValidations.Find(x => x.CrMasUserMainValidationUser == model.UserId && x.CrMasUserMainValidationMainTasks == checkboxSub.mainTaskId);
                    var subTask = _unitOfWork.CrMasUserSubValidations.Find(x => x.CrMasUserSubValidationUser == model.UserId && x.CrMasUserSubValidationMain == checkboxSub.mainTaskId && x.CrMasUserSubValidationSubTasks == checkboxSub.subTaskId);
                    if (mainTask.CrMasUserMainValidationAuthorization == true)
                    {
                        if (subTask != null) subTask.CrMasUserSubValidationAuthorization = checkboxSub.value;
                    }
                    else
                    {
                        subTask.CrMasUserSubValidationAuthorization = false;
                    }

                }
                foreach (var checkboxProcedure in model.CheckboxesProcedureTask)
                {
                    var mainTask = _unitOfWork.CrMasUserMainValidations.Find(x => x.CrMasUserMainValidationUser == model.UserId && x.CrMasUserMainValidationMainTasks == checkboxProcedure.mainTaskId);
                    var subTask = _unitOfWork.CrMasUserSubValidations.Find(x => x.CrMasUserSubValidationUser == model.UserId && x.CrMasUserSubValidationMain == checkboxProcedure.mainTaskId && x.CrMasUserSubValidationSubTasks == checkboxProcedure.subTaskId);
                    var procedureTask = _unitOfWork.CrMasUserProceduresValidations.Find(x => x.CrMasUserProceduresValidationCode == model.UserId && x.CrMasUserProceduresValidationMainTask == checkboxProcedure.mainTaskId && x.CrMasUserProceduresValidationSubTasks == checkboxProcedure.subTaskId);

                    if (mainTask.CrMasUserMainValidationAuthorization == true && subTask.CrMasUserSubValidationAuthorization == true)
                    {
                        if (procedureTask != null && checkboxProcedure != null)
                        {
                            if (checkboxProcedure.procedureName.ToLower() == "insert" || checkboxProcedure.procedureName == "Car show for sale") procedureTask.CrMasUserProceduresValidationInsertAuthorization = checkboxProcedure.value;
                            if (checkboxProcedure.procedureName.ToLower() == "update" || checkboxProcedure.procedureName == "Cancel Offer To Sell") procedureTask.CrMasUserProceduresValidationUpDateAuthorization = checkboxProcedure.value;
                            if (checkboxProcedure.procedureName.ToLower() == "hold" || checkboxProcedure.procedureName == "Sale Execution") procedureTask.CrMasUserProceduresValidationHoldAuthorization = checkboxProcedure.value;
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
                }
                //Save Adminstrive Procedures
                await _unitOfWork.CompleteAsync();
                await SaveTracingForUserChange(user, Status.Update);
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
            return RedirectToAction("Index", "EmployeesSystemValiditions");
        }
        public IActionResult DisplayToastError_ForEditValidtions()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            return RedirectToAction("Index", "EmployeesSystemValiditions");
        }
    }
}
