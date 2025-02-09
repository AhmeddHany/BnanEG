using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS.Employees;
using Bnan.Ui.ViewModels.Identitiy;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.CAS.Controllers.Employees
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]

    public class EmployeesController : BaseController
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
        private readonly IStringLocalizer<EmployeesController> _localizer;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasUser _masUser;

        private readonly string pageNumber = SubTasks.CrMasUserInformationForCAS;


        public EmployeesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, IUserService userService, IWebHostEnvironment webHostEnvironment, IUserLoginsService userLoginsService, IUserMainValidtion userMainValidtion, IUserSubValidition userSubValidition, IUserProcedureValidition userProcedureValidition, IUserBranchValidity userBranchValidity, IToastNotification toastNotification, IStringLocalizer<EmployeesController> localizer, IUserContractValididation userContractValididation, IAdminstritiveProcedures adminstritiveProcedures, IWebHostEnvironment hostingEnvironment, IBaseRepo baseRepo, IMasUser masUser) : base(userManager, unitOfWork, mapper)
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
            _hostingEnvironment = hostingEnvironment;
            _baseRepo = baseRepo;
            _masUser = masUser;
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
                                                  x.CrMasUserInformationStatus == Status.Active);

            if (!usersInfo.Any())
            {
                usersInfo = await _unitOfWork.CrMasUserInformation
                .FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && !x.CrMasUserInformationCode.StartsWith("CAS") &&
                                                  x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                  x.CrMasUserInformationStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(usersInfo);
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);
            var lessorCode = user.CrMasUserInformationLessor;
            if (!string.IsNullOrEmpty(status))
            {
                var employeesInfo = await _unitOfWork.CrMasUserInformation.FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && !x.CrMasUserInformationCode.StartsWith("CAS") &&
                                                                                                     x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                                                                    (x.CrMasUserInformationStatus == Status.Active ||
                                                                                                     x.CrMasUserInformationStatus == Status.Deleted ||
                                                                                                     x.CrMasUserInformationStatus == Status.Hold));

                if (status == Status.All)
                {
                    var FilterAll = employeesInfo.FindAll(x => x.CrMasUserInformationStatus != Status.Deleted &&
                                                                         (x.CrMasUserInformationArName.Contains(search) ||
                                                                          x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                          x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationCode.Contains(search)));
                    return PartialView("_DataTableEmployees", FilterAll);
                }
                var FilterByStatus = employeesInfo.FindAll(x => x.CrMasUserInformationStatus == status &&
                                                                         (x.CrMasUserInformationArName.Contains(search) ||
                                                                          x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                          x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationCode.Contains(search)));
                return PartialView("_DataTableEmployees", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddEmployee()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Employees", "Index");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Employees", "Index");
            }
            var callingKeys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(x => x.CrMasSysCallingKeysStatus == Status.Active,
                     query => query.Select(c => new { c.CrMasSysCallingKeysCode, c.CrMasSysCallingKeysNo }));
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString().Trim(), Text = c.CrMasSysCallingKeysNo?.Trim() }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view

            var branches = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(x => x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && x.CrCasBranchInformationStatus == Status.Active,
                query => query.Select(branch => new
                {
                    branch.CrCasBranchInformationCode,
                    branch.CrCasBranchInformationArName,
                    branch.CrCasBranchInformationEnName,
                }));

            EmployeesWithAuthrizationVM AddEmployeesWithAuthrization = new EmployeesWithAuthrizationVM
            {
                BranchesAuthrization = branches.Select(branch => new AuthrizationBranchesVM
                {
                    Code = branch.CrCasBranchInformationCode,
                    Name = CultureInfo.CurrentCulture.Name == "en-US" ? branch.CrCasBranchInformationEnName : branch.CrCasBranchInformationArName,
                    Authrization = false
                }).ToList()

            };
            AddEmployeesWithAuthrization.CrMasUserInformationLessor = user.CrMasUserInformationLessor;
            return View(AddEmployeesWithAuthrization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(EmployeesWithAuthrizationVM model)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            // التحقق من تسجيل دخول المستخدم
            if (userLogin == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Employees");
            }

            // التحقق من صحة البيانات المدخلة
            if (ModelState.IsValid)
            {
                // التحقق من صلاحية المستخدم لإضافة موظف جديد
                if (!await _baseRepo.CheckValidation(userLogin.CrMasUserInformationCode, pageNumber, Status.Insert))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index", "Employees");
                }

                // التحقق إذا كان الموظف موجود مسبقًا
                var user = await _userService.GetUserByUserNameAsync(model.CrMasUserInformationCode);
                if (user != null)
                {
                    ModelState.AddModelError("Exist", _localizer["EmployeeExist"]);
                    return View(model);
                }

                // تعيين معطيات الموظف الجديدة
                model.CrMasUserInformationLessor = userLogin.CrMasUserInformationLessor;
                var crMasUserInformation = _mapper.Map<CrMasUserInformation>(model);

                // التحقق إذا كان هذا الموظف موجود في قاعدة البيانات
                if (await _masUser.ExistsByDetailsAsync(crMasUserInformation))
                {
                    await AddModelErrorsAsync(crMasUserInformation);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddEmployee", model);
                }

                // إنشاء المستخدم
                var createUser = await _authService.RegisterForCasAsync(crMasUserInformation);

                // التحقق إذا كانت عملية التسجيل فشلت
                if (createUser == null)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index", "Employees");
                }

                // إضافة الأدوار (Roles) للمستخدم
                if (
                    !await _userMainValidtion.AddMainValiditionsForEachUser(createUser.CrMasUserInformationCode, "2") ||
                    !await _userSubValidition.AddSubValiditionsForEachUser(createUser.CrMasUserInformationCode, "2") ||
                    !await _userProcedureValidition.AddProceduresValiditionsForEachUser(createUser.CrMasUserInformationCode, "2") ||
                    !await _userContractValididation.AddContractValiditionsForEachUserInCas(createUser.CrMasUserInformationCode, null) ||
                    !await ChangeRoleAsync(createUser))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index", "Employees");
                }

                // معالجة صلاحيات الفروع (Branches)
                foreach (var item in model.BranchesAuthrization)
                {
                    // إذا كان للمستخدم صلاحية فرع
                    if ((bool)createUser.CrMasUserInformationAuthorizationBranch)
                    {
                        // إضافة الصلاحية إذا كانت مفعلة
                        if (item.Authrization)
                        {
                            await _userBranchValidity.AddUserBranchValidity(createUser.CrMasUserInformationCode, userLogin.CrMasUserInformationLessor, item.Code, Status.Active);
                        }
                        else
                        {
                            // إزالة الصلاحية إذا كانت غير مفعلة
                            await _userBranchValidity.AddUserBranchValidity(createUser.CrMasUserInformationCode, userLogin.CrMasUserInformationLessor, item.Code, Status.Deleted);
                        }
                    }
                    else
                    {
                        // إزالة الصلاحية إذا كان لا توجد صلاحية فرع للمستخدم
                        await _userBranchValidity.AddUserBranchValidity(createUser.CrMasUserInformationCode, userLogin.CrMasUserInformationLessor, item.Code, Status.Deleted);
                    }
                }

                // التحقق من نجاح حفظ البيانات
                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    // حفظ تتبع التغييرات للمستخدم
                    await SaveTracingForUserChange(createUser, Status.Insert, pageNumber);
                    return RedirectToAction("Index", "Employees");
                }


            }

            // إذا كانت البيانات المدخلة غير صالحة، أعد عرض النموذج مع الأخطاء
            var callingKeys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(x => x.CrMasSysCallingKeysStatus == Status.Active,
                     query => query.Select(c => new { c.CrMasSysCallingKeysCode, c.CrMasSysCallingKeysNo }));

            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString().Trim(), Text = c.CrMasSysCallingKeysNo?.Trim() }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // تمرير callingKeys إلى الـ View
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, pageNumber);
            var userUpdated = await _unitOfWork.CrMasUserInformation.FindAsync(x => !id.StartsWith("CAS") && x.CrMasUserInformationCode == id);
            if (userUpdated == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Employees");
            }
            if (userUpdated.CrMasUserInformationCode == userLogin.CrMasUserInformationCode)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Employees");
            }


            var crMasUserInformationVM = _mapper.Map<EmployeesWithAuthrizationVM>(userUpdated);
            if (crMasUserInformationVM.CrMasUserInformationCreditDaysLimit == null) crMasUserInformationVM.CrMasUserInformationCreditDaysLimit = 5;
            var branches = await _unitOfWork.CrMasUserBranchValidity.FindAllWithSelectAsNoTrackingAsync(x => x.CrMasUserBranchValidityId == crMasUserInformationVM.CrMasUserInformationCode &&
                                                                                                          x.CrMasUserBranchValidityLessor == crMasUserInformationVM.CrMasUserInformationLessor,

            query => query.Select(branch => new
            {
                branch.CrMasUserBranchValidityId,
                branch.CrMasUserBranchValidityBranch,
                branch.CrMasUserBranchValidityBranchStatus,
                branch.CrMasUserBranchValidity1,
                branch.CrMasUserBranchValidityBranchCashBalance,
                branch.CrMasUserBranchValidityBranchSalesPointBalance,
                branch.CrMasUserBranchValidityBranchTransferBalance,
            }), new[] { "CrMasUserBranchValidity1" });


            crMasUserInformationVM.BranchesAuthrization = branches
                     .Where(branch =>
                         branch.CrMasUserBranchValidity1.CrCasBranchInformationStatus == Status.Active || // الفرع نشط
                         branch.CrMasUserBranchValidityBranchCashBalance > 0 || // يحتوي على رصيد نقدي
                         branch.CrMasUserBranchValidityBranchSalesPointBalance > 0 || // يحتوي على نقاط مبيعات
                         branch.CrMasUserBranchValidityBranchTransferBalance > 0 || // يحتوي على رصيد تحويل
                         (branch.CrMasUserBranchValidityBranchStatus == Status.Active) // الموظف لديه صلاحيات عليه
                     )
                     .Select(branch => new AuthrizationBranchesVM
                     {
                         Code = branch.CrMasUserBranchValidityBranch,
                         Name = CultureInfo.CurrentCulture.Name == "en-US"
                             ? branch.CrMasUserBranchValidity1.CrCasBranchInformationEnName
                             : branch.CrMasUserBranchValidity1.CrCasBranchInformationArName,
                         Authrization = branch.CrMasUserBranchValidityBranchStatus == Status.Active, // صلاحية الفرع للموظف
                         IfCanChangeAuthrization = branch.CrMasUserBranchValidityBranchCashBalance == 0 &&
                                                   branch.CrMasUserBranchValidityBranchSalesPointBalance == 0 &&
                                                   branch.CrMasUserBranchValidityBranchTransferBalance == 0,
                         BranchActiveOrHold = branch.CrMasUserBranchValidity1.CrCasBranchInformationStatus == Status.Active
                     })
                     .ToList();



            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysNo, Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view

            return View(crMasUserInformationVM);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EmployeesWithAuthrizationVM model)
        {
            using (var transaction = await _unitOfWork.CrMasUserInformation.BeginTransactionAsync())
            {
                try
                {
                    var userLogin = await _userManager.GetUserAsync(User);
                    await SetPageTitleAsync(Status.Update, pageNumber);

                    var user = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == model.CrMasUserInformationCode);
                    if (user == null || user.CrMasUserInformationCode == userLogin.CrMasUserInformationCode)
                    {
                        _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return RedirectToAction("Index", "Employees");
                    }

                    var crMasUserInformation = _mapper.Map<CrMasUserInformation>(model);
                    if (crMasUserInformation.CrMasUserInformationAuthorizationBranch == true)
                    {
                        if (model.BranchesAuthrization?.Where(x => x.Authrization == true).Count() == 0) crMasUserInformation.CrMasUserInformationAuthorizationBranch = false;
                        else crMasUserInformation.CrMasUserInformationAuthorizationBranch = true;
                    }

                    var updatedUser = await _authService.UpdateForCasAsync(crMasUserInformation);

                    foreach (var branch in model.BranchesAuthrization)
                    {
                        if (updatedUser.CrMasUserInformationAuthorizationBranch == true)
                        {
                            if (branch.Authrization) await _userBranchValidity.UpdateUserBranchValidity(updatedUser.CrMasUserInformationCode, updatedUser.CrMasUserInformationLessor, branch.Code, Status.Active);
                            else await _userBranchValidity.UpdateUserBranchValidity(updatedUser.CrMasUserInformationCode, updatedUser.CrMasUserInformationLessor, branch.Code, Status.Deleted);
                        }
                        else
                        {
                            await _userBranchValidity.UpdateUserBranchValidity(updatedUser.CrMasUserInformationCode, updatedUser.CrMasUserInformationLessor, branch.Code, Status.Deleted);
                        }
                    }

                    if (await _unitOfWork.CompleteAsync() > 0 && await ChangeRoleAsync(updatedUser))
                    {
                        await transaction.CommitAsync();
                        _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        await SaveTracingForUserChange(updatedUser, Status.Update, pageNumber);
                        return RedirectToAction("Index", "Employees");
                    }

                    await transaction.RollbackAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                }
            }

            return RedirectToAction("Index", "Employees");
        }
        private async Task<bool> ChangeRoleAsync(CrMasUserInformation user)
        {
            // Define role mappings using a list of tuples, converting nullable booleans to non-nullable
            var roleMappings = new List<(bool PropertyValue, string Role)>
                    {
                        (user.CrMasUserInformationAuthorizationBranch ?? false, RolesStrings.BS),
                        (user.CrMasUserInformationAuthorizationOwner ?? false, RolesStrings.OWN),
                        (user.CrMasUserInformationAuthorizationAdmin ?? false, RolesStrings.CAS)
                    };

            // Iterate through role mappings and perform role operations
            foreach (var (propertyValue, role) in roleMappings)
            {
                var userHasRole = await _userManager.IsInRoleAsync(user, role);

                if (propertyValue && !userHasRole)
                {
                    if (!await _authService.AddRoleAsync(user, role)) return false;
                }
                else if (!propertyValue && userHasRole)
                {
                    if (!await _authService.RemoveRoleAsync(user, role)) return false;
                }
            }
            return true;
        }
        [HttpPost]
        public async Task<string> EditStatus(string status, string code)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var EditedUser = await _unitOfWork.CrMasUserInformation.GetByIdAsync(code);
            if (EditedUser == null) return "false";
            try
            {
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                EditedUser.CrMasUserInformationStatus = status;
                _unitOfWork.CrMasUserInformation.Update(EditedUser);
                await _unitOfWork.CompleteAsync();
                await SaveTracingForUserChange(EditedUser, status, pageNumber);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string code)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(false);


            var employee = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == code && x.CrMasUserInformationLessor == currentUser.CrMasUserInformationLessor);
            if (employee != null)
            {
                // Remove the current password
                var removePasswordResult = await _userManager.RemovePasswordAsync(employee);
                if (removePasswordResult.Succeeded)
                {
                    // Add the new password
                    var addPasswordResult = await _userManager.AddPasswordAsync(employee, code);
                    if (addPasswordResult.Succeeded)
                    {
                        //employee.CrMasUserInformationPassWord = code;
                        await _unitOfWork.CompleteAsync();
                        return Json(true);
                    }
                }
            }

            return Json(false);
        }

        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, SubTasks.MyAccountCAS);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }

            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            var crMasUserInformation = _mapper.Map<RegisterViewModel>(user);
            return View(crMasUserInformation);
        }

        [HttpPost]
        public async Task<IActionResult> MyAccount(RegisterViewModel model, string UserSignatureFile, IFormFile UserImgFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }


            string foldername = $"{"images\\Company"}\\{user.CrMasUserInformationLessor}\\{"Users"}\\{user.CrMasUserInformationCode}";

            string filePathImage;
            string filePathSignture;
            var oldPathImage = model.CrMasUserInformationPicture;
            var oldPathSignture = model.CrMasUserInformationSignature;
            if (oldPathImage == "~/images/common/user.jpg") oldPathImage = "";
            if (oldPathSignture == "~/images/common/DefualtUserSignature.png") oldPathSignture = "";
            if (UserImgFile != null)
            {
                string fileNameImg = "Image_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقت
                filePathImage = await UserImgFile.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png", user.CrMasUserInformationPicture);
            }
            else if (string.IsNullOrEmpty(oldPathImage))
            {
                filePathImage = "~/images/common/user.jpg";
            }
            else
            {
                filePathImage = user.CrMasUserInformationPicture;

            }

            if (!string.IsNullOrEmpty(UserSignatureFile))
            {
                string fileNameSignture = "Signture_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقتs
                filePathSignture = await _hostingEnvironment.SaveSigntureImage(UserSignatureFile, user.CrMasUserInformationCode, user.CrMasUserInformationSignature, foldername);
            }
            else if (string.IsNullOrEmpty(oldPathSignture))
            {
                filePathSignture = "~/images/common/DefualtUserSignature.png";
            }
            else
            {
                filePathSignture = user.CrMasUserInformationSignature;
            }
            user.CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey;
            user.CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo;
            user.CrMasUserInformationEmail = model.CrMasUserInformationEmail;
            user.CrMasUserInformationExitTimer = model.CrMasUserInformationExitTimer;
            user.CrMasUserInformationSignature = filePathSignture;
            user.CrMasUserInformationPicture = filePathImage;
            user.CrMasUserInformationRemindMe = model.CrMasUserInformationRemindMe;
            user.CrMasUserInformationDefaultLanguage = model.CrMasUserInformationDefaultLanguage;
            await _userManager.UpdateAsync(user);
            await SaveTracingForUserChange(user, Status.Update, SubTasks.MyAccountCAS);
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, SubTasks.ChangePasswordCAS);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            // تحقق من صحة النموذج
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // الحصول على المستخدم الحالي
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            await SetPageTitleAsync(Status.Update, SubTasks.ChangePasswordCAS);
            // تحقق من كلمة المرور الحالية
            if (!await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassInCorrect", _localizer["CurrentPassInCorrect"]);
                return View(model);
            }
            // منع استخدام نفس كلمة المرور الحالية
            if (model.NewPassword == model.CurrentPassword)
            {
                ModelState.AddModelError("CannotUseCurrentPassword", _localizer["CannotUserCurrentPassword"]);
                return View(model);
            }
            // تغيير كلمة المرور
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword?.Trim(), model.NewPassword?.Trim());
            if (result.Succeeded)
            {
                user.CrMasUserInformationChangePassWordLastDate = DateTime.Now.Date;
                _unitOfWork.Complete();
                await SaveTracingForUserChange(user, Status.ChangePassword, SubTasks.ChangePasswordCAS);
                _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }

            // التعامل مع الأخطاء أثناء تغيير كلمة المرور
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField, string lessorCode)
        {
            var All_Users = await _unitOfWork.CrMasUserInformation.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_Users != null)
            {
                if (existName == "CrMasUserInformationArName" && All_Users.Any(x => x.CrMasUserInformationArName == dataField && x.CrMasUserInformationLessor == lessorCode))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasUserInformationArName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrMasUserInformationEnName" && All_Users.Any(x => x.CrMasUserInformationEnName?.ToLower() == dataField.ToLower() && x.CrMasUserInformationLessor == lessorCode))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasUserInformationEnName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrMasUserInformationCode" && !string.IsNullOrEmpty(dataField) && All_Users.Any(x => x.CrMasUserInformationCode == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasUserInformationCode", Message = _localizer["Existing"] });
                }
                else if (existName == "CrMasUserInformationId" && !string.IsNullOrEmpty(dataField) && All_Users.Any(x => x.CrMasUserInformationId == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasUserInformationId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }
        private async Task AddModelErrorsAsync(CrMasUserInformation entity)
        {

            if (await _masUser.ExistsByArabicNameAsync(entity.CrMasUserInformationArName, entity.CrMasUserInformationCode, entity.CrMasUserInformationLessor))
            {
                ModelState.AddModelError("CrMasUserInformationArName", _localizer["Existing"]);
            }

            if (await _masUser.ExistsByEnglishNameAsync(entity.CrMasUserInformationEnName, entity.CrMasUserInformationCode, entity.CrMasUserInformationLessor))
            {
                ModelState.AddModelError("CrMasUserInformationEnName", _localizer["Existing"]);
            }

            if (await _masUser.ExistsByUserCodeAsync(entity.CrMasUserInformationCode))
            {
                ModelState.AddModelError("CrMasUserInformationCode", _localizer["Existing"]);
            }

            if (await _masUser.ExistsByUserIdAsync(entity.CrMasUserInformationId, entity.CrMasUserInformationCode))
            {
                ModelState.AddModelError("CrMasUserInformationId", _localizer["Existing"]);
            }
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

        private async Task<bool> SendMessageToWhatsup(CrMasUserInformation user, string lessorCode)
        {
            var lessorNames = await _unitOfWork.CrMasLessorInformation.FindAsync(x => x.CrMasLessorInformationCode == lessorCode);
            var message = GetMessageToSendWhatsup(user.CrMasUserInformationCode, lessorNames.CrMasLessorInformationArShortName, lessorNames.CrMasLessorInformationEnShortName,
                                                  user.CrMasUserInformationArName, user.CrMasUserInformationEnName);
            var result = await WhatsAppServicesExtension.SendMessageAsync($"{user.CrMasUserInformationCallingKey}{user.CrMasUserInformationMobileNo}", message, lessorCode);
            return true;
        }
        private string GetMessageToSendWhatsup(string userCode, string companyNameAr, string companyNameEn, string userArName, string userEnName)
        {
            var messageAr = string.Format(
                "عزيزي/عزيزتي {0}،\n\n" +
                "نود إعلامك بأنه تم إنشاء حساب جديد لك برقم المستخدم: {1} وكلمة المرور: '{2}'، والخاصة بشركة {3}.\n\n" +
                "يرجى تسجيل الدخول باستخدام هذه البيانات والعمل على تغيير كلمة المرور في أقرب وقت ممكن لضمان أمان حسابك.\n\n" +
                "مع أطيب التحيات،\n" +
                "شركة {3}",
                userArName, userCode, userCode, companyNameAr);

            var messageEn = string.Format(
                "Dear {0},\n\n" +
                "We are pleased to inform you that a new account has been created for you with the following details:\n" +
                "User Code: {1}\n" +
                "Password: '{2}'\n\n" +
                "This account is associated with {3}.\n\n" +
                "Please log in using these credentials and change your password promptly to ensure the security of your account.\n\n" +
                "Best regards,\n" +
                "{3}",
                userEnName, userCode, userCode, companyNameEn);

            var fullMessage = messageAr + Environment.NewLine + Environment.NewLine + messageEn;

            return fullMessage;
        }
        public IActionResult SuccessToast()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Employees");
        }
        public IActionResult Failed()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });
            return RedirectToAction("Index", "Employees");
        }
    }
}
