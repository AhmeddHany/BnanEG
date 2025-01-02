using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.Identitiy;
using Bnan.Ui.ViewModels.MAS.UserValiditySystem;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.CAS.Controllers.Employees
{
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


        public EmployeesSystemValiditionsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, IUserService userService, IWebHostEnvironment webHostEnvironment, IUserLoginsService userLoginsService, IUserMainValidtion userMainValidtion, IUserSubValidition userSubValidition, IUserProcedureValidition userProcedureValidition, IUserBranchValidity userBranchValidity, IToastNotification toastNotification, IStringLocalizer<EmployeesSystemValiditionsController> localizer, IAdminstritiveProcedures adminstritiveProcedures) : base(userManager, unitOfWork, mapper)
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
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsers";
            ViewBag.no = "1";
            // Set Title
            var titles = await setTitle("206", "2206002", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);


            var user = User; // Get the current User object
            var userLessor = await _userService.GetUserLessor(user);

            if (userLessor == null)
            {
                return RedirectToAction("Login", "Account");
            }
            // Exclude the current user from the list
            var usersByLessor = await _userService.GetAllUsersByLessor(userLessor.CrMasUserInformationLessor);
            var userWithOutManger = usersByLessor.Where(x => x.CrMasUserInformationCode != "CAS" + userLessor.CrMasUserInformationLessor);
            return View(userWithOutManger.Where(x => x.CrMasUserInformationCode != userLessor.CrMasUserInformationCode && x.CrMasUserInformationStatus == Status.Active && x.CrMasUserInformationAuthorizationAdmin == true).ToList());
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsers";
            ViewBag.no = "1";

            // Set Title
            var titles = await setTitle("206", "2206002", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);
            var user = await _userService.GetUserByUserNameAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Users");
            }
            var mainValidition = _unitOfWork.CrMasUserMainValidations.FindAll(x => x.CrMasUserMainValidationUser == id);
            var subValition = _unitOfWork.CrMasUserSubValidations.FindAll(x => x.CrMasUserSubValidationUser == id);
            var procedureValidition = _unitOfWork.CrMasUserProceduresValidations.FindAll(x => x.CrMasUserProceduresValidationCode == id);
            var procedureTasks = _unitOfWork.CrMasSysProceduresTasks.GetAll();

            RegisterViewModel viewModel = new RegisterViewModel
            {
                CrMasSysMainTasks = (List<CrMasSysMainTask>)_unitOfWork.CrMasSysMainTasks.FindAll(x => x.CrMasSysMainTasksStatus == Status.Active),
                CrMasUserMainValidations = (List<CrMasUserMainValidation>)mainValidition,

                CrMasUserInformationCode = user.CrMasUserInformationCode,
                CrMasUserInformationArName = user.CrMasUserInformationArName,
                CrMasUserInformationEnName = user.CrMasUserInformationEnName,

                CrMasSysSubTasks = (List<CrMasSysSubTask>)_unitOfWork.CrMasSysSubTasks.FindAll(x => x.CrMasSysSubTasksStatus == Status.Active),
                CrMasUserSubValidations = (List<CrMasUserSubValidation>)subValition,
                CrMasSysProceduresTasks = (List<CrMasSysProceduresTask>)procedureTasks,
                ProceduresValidations = (List<CrMasUserProceduresValidation>)procedureValidition,

            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] CheckBoxModels model)
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
            var user = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == model.UserId);

            //Save Adminstrive Procedures
            await _adminstritiveProcedures.SaveAdminstritive(user.CrMasUserInformationCode, "1", "234", "20", user.CrMasUserInformationLessor, "100",
           user.CrMasUserInformationCode, null, null, null, null, null, null, null, null, "تحديث صلاحيات العقد", "edit contract Validity", "U", null);
            await _unitOfWork.CompleteAsync();
            return Json(new { success = true });
        }
    }
}
