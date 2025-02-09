using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS.Users;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Bnan.Ui.ViewModels.MAS.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.MAS.Controllers.Employees
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class UsersForCompaniesController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserMainValidtion _userMainValidtion;
        private readonly IUserSubValidition _userSubValidition;
        private readonly IUserProcedureValidition _userProcedureValidition;
        private readonly IStringLocalizer<UsersForCompaniesController> _localizer;
        private readonly IToastNotification _toastNotification;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasCompanyUsers _masUser;
        private readonly string pageNumber = SubTasks.CrMasUserInformationFromMASToCAS;

        public UsersForCompaniesController(IUserService userService,
                              IAuthService authService,
                              IWebHostEnvironment webHostEnvironment,
                              UserManager<CrMasUserInformation> userManager,
                              IUnitOfWork unitOfWork, IUserLoginsService userLoginsService,
                              IMapper mapper, IUserMainValidtion userMainValidtion,
                              IUserSubValidition userSubValidition,
                              IStringLocalizer<UsersForCompaniesController> localizer,
                              IUserProcedureValidition userProcedureValidition, IToastNotification toastNotification, IBaseRepo baseRepo, IMasCompanyUsers masUser) : base(userManager, unitOfWork, mapper)
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
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            IQueryable<CrMasUserInformation> query = _unitOfWork.CrMasUserInformation.GetTableNoTracking().Include(x => x.CrMasUserInformationLessorNavigation)
                                                                .Where(x => x.CrMasUserInformationCode.StartsWith("CAS")); // جلب البيانات بدون تتبع
            var usersInfo = await query.Where(x => x.CrMasUserInformationStatus == Status.Active).ToListAsync();
            if (!usersInfo.Any())
            {
                usersInfo = await query.Where(x => x.CrMasUserInformationStatus == Status.Hold).ToListAsync();
                ViewBag.radio = "All";
            }
            else
            {
                ViewBag.radio = "A";
            }
            return View(usersInfo);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetUserByStatus(string status, string search)
        {
            // Get the current user (if needed for further filtering)
            var user = await _userManager.GetUserAsync(User);

            // Start building the query
            IQueryable<CrMasUserInformation> query = _unitOfWork.CrMasUserInformation.GetTableNoTracking().Include(x => x.CrMasUserInformationLessorNavigation).Where(x => x.CrMasUserInformationCode.StartsWith("CAS")); // Use strongly-typed Include
            // Filter by status if not "All"
            if (status != Status.All) query = query.Where(x => x.CrMasUserInformationStatus == status);
            else query = query.Where(x => x.CrMasUserInformationStatus != Status.Deleted);

            // Search in multiple fields if the search parameter is provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.CrMasUserInformationArName.Contains(search) ||
                    x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                    x.CrMasUserInformationCode.Contains(search) ||
                    x.CrMasUserInformationLessor.Contains(search) ||
                    x.CrMasUserInformationLessorNavigation.CrMasLessorInformationArShortName.Contains(search) ||
                    x.CrMasUserInformationLessorNavigation.CrMasLessorInformationEnShortName.ToLower().Contains(search));
            }

            // Execute the query and retrieve the results
            var result = await query.AsNoTracking().ToListAsync();

            // Return the results in a partial view
            return PartialView("_DataTableUsersCompanies", result);
        }
        [HttpGet]
        public async Task<IActionResult> AddUser()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "UsersForCompanies");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "UsersForCompanies");
            }
            var lessors = await _unitOfWork.CrMasLessorInformation.FindAllWithSelectAsNoTrackingAsync(x => x.CrMasLessorInformationStatus != Status.Deleted && x.CrMasLessorInformationCode != "0000",
                                                                                                        query => query.Select(c => new
                                                                                                        {
                                                                                                            c.CrMasLessorInformationCode,
                                                                                                            c.CrMasLessorInformationArLongName,
                                                                                                            c.CrMasLessorInformationEnLongName
                                                                                                        })
                                                                                                    );

            var lessorsListAr = lessors.Select(c => new SelectListItem { Value = c.CrMasLessorInformationCode.ToString().Trim(), Text = c.CrMasLessorInformationArLongName?.Trim() }).ToList();
            var lessorsListEn = lessors.Select(c => new SelectListItem { Value = c.CrMasLessorInformationCode.ToString().Trim(), Text = c.CrMasLessorInformationEnLongName?.Trim() }).ToList();

            ViewData["LessorsAr"] = lessorsListAr; // Pass the callingKeys to the view
            ViewData["LessorsEn"] = lessorsListEn; // Pass the callingKeys to the view

            var callingKeys = await _unitOfWork.CrMasSysCallingKeys.FindAllWithSelectAsNoTrackingAsync(x => x.CrMasSysCallingKeysStatus == Status.Active,
                    query => query.Select(c => new { c.CrMasSysCallingKeysCode, c.CrMasSysCallingKeysNo }));
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString().Trim(), Text = c.CrMasSysCallingKeysNo?.Trim() }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            CompanyUserVM registerViewModel = new CompanyUserVM();
            return View(registerViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(CompanyUserVM model,string WelcomeCardMessage)
        {

            var pageNumber = SubTasks.CrMasUserInformationFromMASToCAS;
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || model == null || user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "UsersForCompanies");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "UsersForCompanies");
            }
            try
            {
                var crMasUserInformation = _mapper.Map<CrMasUserInformation>(model);
                crMasUserInformation.CrMasUserInformationCode = $"CAS{model.CrMasUserInformationCode}";
                if (crMasUserInformation.CrMasUserInformationCode.Length > 10)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Index");
                }
                // Check if the entity already exists
                if (await _masUser.ExistsByUserCodeAsync(crMasUserInformation.CrMasUserInformationCode))
                {
                    await AddModelErrorsAsync(crMasUserInformation);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddUser", model);
                }

                var userCode = await _authService.AddUserCompanyForCas(crMasUserInformation);
                if (string.IsNullOrEmpty(userCode))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Index");
                }
                var newUser = await _userService.GetUserByUserNameAsync(userCode);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });
                await SaveTracingForUserChange(newUser, Status.Insert);
                if (WelcomeCardMessage != null) await WhatsAppServicesExtension.SendMediaAsync(newUser.CrMasUserInformationCallingKey + newUser.CrMasUserInformationMobileNo, " ", user.CrMasUserInformationLessor, WelcomeCardMessage, "WelcomeCard.png");
                //await SendMessageToWhatsup(newUser, user.CrMasUserInformationLessor);
                return RedirectToAction("Index", "UsersForCompanies");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!id.StartsWith("CAS"))
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "UsersForCompanies");
            }

            await SetPageTitleAsync(Status.Update, pageNumber);
            var userInfo = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == id, new[] { "CrMasUserInformationLessorNavigation" });
            if (userInfo == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "UsersForCompanies");
            }
            var crMasUserInformation = _mapper.Map<CompanyUserVM>(userInfo);
            crMasUserInformation.LessorArName = userInfo.CrMasUserInformationLessorNavigation?.CrMasLessorInformationArLongName;
            crMasUserInformation.LessorEnName = userInfo.CrMasUserInformationLessorNavigation?.CrMasLessorInformationEnLongName;
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString().Trim(), Text = c.CrMasSysCallingKeysNo?.Trim() }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            return View(crMasUserInformation);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CompanyUserVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && model == null && !model.CrMasUserInformationCode.StartsWith("CAS"))
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "UsersForCompanies");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", model);
                }
                var userInfo = _mapper.Map<CrMasUserInformation>(model);
                if (await _masUser.UpdateUser(userInfo) && await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                await SaveTracingForUserChange(userInfo, Status.Update);
                return RedirectToAction("Index", "UsersForCompanies");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", model);
            }
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
                await SaveTracingForUserChange(EditedUser, status);
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
            var employee = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == code);
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
                        await SaveTracingForUserChange(employee, Status.ResetPassword);
                        return Json(true);
                    }
                }
            }

            return Json(false);
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrMasUserInformation entity)
        {

            //if (await _masUser.ExistsByArabicNameAsync(entity.CrMasUserInformationArName, entity.CrMasUserInformationCode))
            //{
            //    ModelState.AddModelError("CrMasUserInformationArName", _localizer["Existing"]);
            //}

            //if (await _masUser.ExistsByEnglishNameAsync(entity.CrMasUserInformationEnName, entity.CrMasUserInformationCode))
            //{
            //    ModelState.AddModelError("CrMasUserInformationEnName", _localizer["Existing"]);
            //}

            if (await _masUser.ExistsByUserCodeAsync(entity.CrMasUserInformationCode))
            {
                ModelState.AddModelError("CrMasUserInformationCode", _localizer["Existing"]);
            }

            if (await _masUser.ExistsByUserIdAsync(entity.CrMasUserInformationId, entity.CrMasUserInformationCode))
            {
                ModelState.AddModelError("CrMasUserInformationId", _localizer["Existing"]);
            }
        }

        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_Users = await _unitOfWork.CrMasUserInformation.GetAllAsyncAsNoTrackingAsync();
            var errors = new List<ErrorResponse>();
            dataField = $"CAS{dataField}";
            if (!string.IsNullOrEmpty(dataField) && All_Users != null)
            {
                //if (existName == "CrMasUserInformationArName" && All_Users.Any(x => x.CrMasUserInformationArName == dataField))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasUserInformationArName", Message = _localizer["Existing"] });
                //}
                //else if (existName == "CrMasUserInformationEnName" && All_Users.Any(x => x.CrMasUserInformationEnName?.ToLower() == dataField.ToLower()))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasUserInformationEnName", Message = _localizer["Existing"] });
                //}
                if (existName == "CrMasUserInformationCode" && !string.IsNullOrEmpty(dataField) && All_Users.Any(x => x.CrMasUserInformationCode.ToLower() == dataField.ToLower()))
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
        private async Task<bool> SendMessageToWhatsup(CrMasUserInformation user, string lessorCode)
        {
            var message = GetMessageToSendWhatsup(user.CrMasUserInformationCode, user.CrMasUserInformationArName, user.CrMasUserInformationEnName);
            var result = await WhatsAppServicesExtension.SendMessageAsync($"{user.CrMasUserInformationCallingKey}{user.CrMasUserInformationMobileNo}", message, lessorCode);
            return true;
        }
        private string GetMessageToSendWhatsup(string userCode, string userArName, string userEnName)
        {
            var messageAr = string.Format("عزيزي {0}، تم إنشاء مستخدم برقم وكلمة مرور '{1}'. " +
                              "يرجى تسجيل الدخول وتغيير كلمة المرور.",
                              userArName, userCode);

            var messageEn = string.Format("Dear {0}, a user with the code and password '{1}' has been created. " +
                                         "Please log in and change your password.",
                                         userEnName, userCode);

            var fullMessage = messageAr + Environment.NewLine + messageEn;

            return fullMessage;
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
            return RedirectToAction("Index", "UsersForCompanies");
        }
        public IActionResult SuccessResetPassword()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });
            return RedirectToAction("Index", "UsersForCompanies");
        }
        public IActionResult Failed()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });
            return RedirectToAction("Index", "UsersForCompanies");
        }
    }
}
