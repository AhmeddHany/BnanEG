using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.CAS.Controllers.Employees
{

    public class EmployeesContractValiditionsController : BaseController
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
        private readonly IUserContractValididation _userContractValididation;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<EmployeesContractValiditionsController> _localizer;


        public EmployeesContractValiditionsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, IUserService userService, IWebHostEnvironment webHostEnvironment, IUserLoginsService userLoginsService, IUserMainValidtion userMainValidtion, IUserSubValidition userSubValidition, IUserProcedureValidition userProcedureValidition, IUserBranchValidity userBranchValidity, IToastNotification toastNotification, IStringLocalizer<EmployeesContractValiditionsController> localizer, IUserContractValididation userContractValididation, IAdminstritiveProcedures adminstritiveProcedures, IWebHostEnvironment hostingEnvironment) : base(userManager, unitOfWork, mapper)
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
            _userContractValididation = userContractValididation;
            _adminstritiveProcedures = adminstritiveProcedures;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsers";
            ViewBag.no = "2";
            // Set Title
            var titles = await setTitle("206", "2206003", "2");
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
            return View(userWithOutManger.Where(x => x.CrMasUserInformationCode != userLessor.CrMasUserInformationCode && x.CrMasUserInformationStatus == Status.Active && x.CrMasUserInformationAuthorizationBranch == true).ToList());
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsers";
            ViewBag.no = "2";

            // Set Title
            var titles = await setTitle("206", "2206003", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);
            var user = await _userService.GetUserByUserNameAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View();
            }
            var contractValidtion = _unitOfWork.CrMasUserContractValidity.Find(x => x.CrMasUserContractValidityUserId == user.Id);
            if (contractValidtion == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View();
            }

            var model = _mapper.Map<ContractValiditionsVM>(contractValidtion);
            model.CrMasUserContractValidityUser = user;
            model.CrMasSysProcedure = _unitOfWork.CrMasSysProcedure.FindAll(x => x.CrMasSysProceduresStatus == Status.Active && (x.CrMasSysProceduresClassification == "10" || x.CrMasSysProceduresClassification == "11" || x.CrMasSysProceduresClassification == "12" || x.CrMasSysProceduresClassification == "13")).ToList();
            model.CrCasLessorMechanism = _unitOfWork.CrCasLessorMechanism.FindAll(x => x.CrCasLessorMechanismCode == user.CrMasUserInformationLessor && (x.CrCasLessorMechanismProceduresClassification == "10" || x.CrCasLessorMechanismProceduresClassification == "11" || x.CrCasLessorMechanismProceduresClassification == "12" || x.CrCasLessorMechanismProceduresClassification == "13")).ToList();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ContractValiditionsVM contractValiditionsVM)
        {
            var user = await _userService.GetUserByUserNameAsync(contractValiditionsVM.CrMasUserContractValidityUserId);

            var contractValidition = _unitOfWork.CrMasUserContractValidity.Find(x => x.CrMasUserContractValidityUserId == contractValiditionsVM.CrMasUserContractValidityUserId);
            if (contractValidition == null) _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            var model = _mapper.Map<CrMasUserContractValidity>(contractValiditionsVM);
            if (await _userContractValididation.EditContractValiditionsForEmployee(model)) _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            // Save Adminstrive Procedures
            await _adminstritiveProcedures.SaveAdminstritive(user.CrMasUserInformationCode, "1", "234", "20", user.CrMasUserInformationLessor, "100",
                user.CrMasUserInformationCode, null, null, null, null, null, null, null, null, "تحديث صلاحيات العقد", "edit contract Validity", "U", null);
            return RedirectToAction("EmployeeContractValiditions");
        }

    }
}
