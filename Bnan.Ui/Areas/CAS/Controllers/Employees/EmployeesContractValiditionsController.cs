using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.Areas.CAS.Controllers.Employees
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
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
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.CrMasUserContractValiditionsCAS;


        public EmployeesContractValiditionsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, IUserService userService, IWebHostEnvironment webHostEnvironment, IUserLoginsService userLoginsService, IUserMainValidtion userMainValidtion, IUserSubValidition userSubValidition, IUserProcedureValidition userProcedureValidition, IUserBranchValidity userBranchValidity, IToastNotification toastNotification, IStringLocalizer<EmployeesContractValiditionsController> localizer, IUserContractValididation userContractValididation, IAdminstritiveProcedures adminstritiveProcedures, IWebHostEnvironment hostingEnvironment, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
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




            return PartialView("_DataTableEmployeesContractValidities", employeesInfo);

        }




        [HttpGet]
        public async Task<IActionResult> Edit(string id)
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
                                                                                   x.CrMasUserInformationLessor == user.CrMasUserInformationLessor);
            if (EditedUser == null || user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "EmployeesSystemValiditions");
            }
            var contractValidtion = await _unitOfWork.CrMasUserContractValidity.FindAsync(x => x.CrMasUserContractValidityUserId == EditedUser.CrMasUserInformationCode);
            if (contractValidtion == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "EmployeesContractValiditions");
            }

            var model = _mapper.Map<ContractValiditionsVM>(contractValidtion);
            model.CrMasSysProcedure = await _unitOfWork.CrMasSysProcedure.FindAllAsNoTrackingAsync(x => (x.CrMasSysProceduresStatus == Status.Active || x.CrMasSysProceduresStatus == "1") &&
                                                                                                        (x.CrMasSysProceduresClassification == "10" ||
                                                                                                         x.CrMasSysProceduresClassification == "11" ||
                                                                                                         x.CrMasSysProceduresClassification == "12" ||
                                                                                                         x.CrMasSysProceduresClassification == "13"));

            model.CrCasLessorMechanism = await _unitOfWork.CrCasLessorMechanism.FindAllAsNoTrackingAsync(x => x.CrCasLessorMechanismCode == user.CrMasUserInformationLessor &&
                                                                                                             (x.CrCasLessorMechanismProceduresClassification == "10" ||
                                                                                                              x.CrCasLessorMechanismProceduresClassification == "11" ||
                                                                                                              x.CrCasLessorMechanismProceduresClassification == "12" ||
                                                                                                              x.CrCasLessorMechanismProceduresClassification == "13"));
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ContractValiditionsVM contractValiditionsVM)
        {
            // التحقق فقط من الحقول التي تحتوي على Range
            var validationResults = new List<ValidationResult>();

            // إنشاء السياق للتحقق من الحقول الفردية
            var contextDiscountRate = new ValidationContext(contractValiditionsVM) { MemberName = nameof(contractValiditionsVM.CrMasUserContractValidityDiscountRate) };
            var contextKm = new ValidationContext(contractValiditionsVM) { MemberName = nameof(contractValiditionsVM.CrMasUserContractValidityKm) };
            var contextHour = new ValidationContext(contractValiditionsVM) { MemberName = nameof(contractValiditionsVM.CrMasUserContractValidityHour) };

            bool isDiscountRateValid = Validator.TryValidateProperty(contractValiditionsVM.CrMasUserContractValidityDiscountRate, contextDiscountRate, validationResults);
            bool isKmValid = Validator.TryValidateProperty(contractValiditionsVM.CrMasUserContractValidityKm, contextKm, validationResults);
            bool isHourValid = Validator.TryValidateProperty(contractValiditionsVM.CrMasUserContractValidityHour, contextHour, validationResults);

            // إذا كانت أي من الحقول غير صالحة
            if (!isDiscountRateValid || !isKmValid || !isHourValid)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "EmployeesContractValiditions");
            }

            // معالجة باقي البيانات بعد التحقق من الحقول
            var userLogin = await _userManager.GetUserAsync(User);
            var userEdited = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == contractValiditionsVM.CrMasUserContractValidityUserId);
            var contractValidition = await _unitOfWork.CrMasUserContractValidity.FindAsync(x => x.CrMasUserContractValidityUserId == contractValiditionsVM.CrMasUserContractValidityUserId);

            if (userLogin == null || contractValidition == null || userEdited == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "EmployeesContractValiditions");
            }

            if (userLogin.CrMasUserInformationCode == userEdited.CrMasUserInformationCode)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "EmployeesContractValiditions");
            }

            var model = _mapper.Map<CrMasUserContractValidity>(contractValiditionsVM);
            var updateModel = await _userContractValididation.EditContractValiditionsForEmployee(model);
            if (updateModel && await _unitOfWork.CompleteAsync() > 0)
            {
                _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                // Save Adminstrive Procedures
                await SaveTracingForUserChange(userEdited, Status.Update, pageNumber);
                return RedirectToAction("Index", "EmployeesContractValiditions");
            }

            return RedirectToAction("Index", "EmployeesContractValiditions");
        }
        private async Task SaveTracingForUserChange(CrMasUserInformation userCreated, string status, string pageNumber)
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
    }
}
