using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.Identitiy;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Data;

namespace Bnan.Ui.Areas.MAS.Controllers.Employees
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]

    public class UsersController : BaseController
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
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly string pageNumber = SubTasks.CrMasUserInformationForMAS;
        public UsersController(IUserService userService,
                               IAuthService authService,
                               IWebHostEnvironment webHostEnvironment,
                               UserManager<CrMasUserInformation> userManager,
                               IUnitOfWork unitOfWork, IUserLoginsService userLoginsService,
                               IMapper mapper, IUserMainValidtion userMainValidtion,
                               IUserSubValidition userSubValidition,
                               IStringLocalizer<UsersController> localizer,
                               IUserProcedureValidition userProcedureValidition, IToastNotification toastNotification, IBaseRepo baseRepo, IMasUser masUser, IWebHostEnvironment hostingEnvironment) : base(userManager, unitOfWork, mapper)
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
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Users()
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
        public async Task<PartialViewResult> GetUserByStatus(string status, string search)
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
                    return PartialView("_DataTableUsers", FilterAll);
                }
                var FilterByStatus = usersInfo.FindAll(x => x.CrMasUserInformationStatus == status &&
                                                                         (x.CrMasUserInformationArName.Contains(search) ||
                                                                          x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                          x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationCode.Contains(search)));
                return PartialView("_DataTableUsers", FilterByStatus);
            }
            return PartialView();
        }

        public async Task<IActionResult> AddUser()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Users", "Users");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Users", "Users");
            }
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString().Trim(), Text = c.CrMasSysCallingKeysNo?.Trim() }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            RegisterViewModel registerViewModel = new RegisterViewModel();
            return View(registerViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(RegisterViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || model == null || user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Users", model);
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Users", "Users");
            }
            try
            {
                var crMasUserInformation = _mapper.Map<CrMasUserInformation>(model);
                // Check if the entity already exists
                if (await _masUser.ExistsByDetailsAsync(crMasUserInformation))
                {
                    await AddModelErrorsAsync(crMasUserInformation);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddUser", model);
                }

                if (!await _authService.RegisterAsync(crMasUserInformation))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Users", model);
                }
                //Add Role 
                var newUser = await _userService.GetUserByUserNameAsync(model.CrMasUserInformationCode);
                if (!await _authService.AddRoleAsync(newUser, "MAS"))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Users", model);
                };
                //Add Main Validitions
                //Add Sub Validitions
                //Add Procedures Validitions

                if (!await _userMainValidtion.AddMainValiditionsForEachUser(newUser.CrMasUserInformationCode, "1"))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Users", model);
                }

                if (!await _userSubValidition.AddSubValiditionsForEachUser(newUser.CrMasUserInformationCode, "1"))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Users", model);
                }

                if (!await _userProcedureValidition.AddProceduresValiditionsForEachUser(newUser.CrMasUserInformationCode, "1"))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Users", model);
                }

                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });

                await SaveTracingForUserChange(newUser, Status.Insert);
                return RedirectToAction("Users", "Users");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Users", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            await SetPageTitleAsync(Status.Update, pageNumber);
            var userInfo = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == id);
            if (userInfo == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Users", "Users");
            }
            var crMasUserInformation = _mapper.Map<RegisterViewModel>(userInfo);
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString().Trim(), Text = c.CrMasSysCallingKeysNo?.Trim() }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            return View(crMasUserInformation);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RegisterViewModel model)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && model == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterDrivingLicense");
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
                // Check if the entity already exists
                if (await _masUser.ExistsByDetailsAsync(userInfo))
                {
                    await AddModelErrorsAsync(userInfo);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", model);
                }
                if (await _masUser.UpdateUser(userInfo) && await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                await SaveTracingForUserChange(userInfo, Status.Update);
                return RedirectToAction("Users", "Users");
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


        //Error exist message when run post action to get what is the exist field << Help Up in Back End
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

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_Users = await _unitOfWork.CrMasUserInformation.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_Users != null)
            {
                if (existName == "CrMasUserInformationArName" && All_Users.Any(x => x.CrMasUserInformationArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasUserInformationArName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrMasUserInformationEnName" && All_Users.Any(x => x.CrMasUserInformationEnName?.ToLower() == dataField.ToLower()))
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

        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, "1106004");

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

            string foldername = $"{"images\\Bnan\\Users"}\\{user.CrMasUserInformationCode}";
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
                filePathSignture = await FileExtensions.SaveSigntureImage(_hostingEnvironment, UserSignatureFile, user.CrMasUserInformationCode, user.CrMasUserInformationSignature, foldername);
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
            user.CrMasUserInformationDefaultLanguage = model.CrMasUserInformationDefaultLanguage;
            await _userManager.UpdateAsync(user);
            await SaveTracingForUserChange(user, Status.Update);
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, "1106005");
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
                await SaveTracingForUserChange(user, Status.ChangePassword);
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
            return RedirectToAction("Users", "Users");
        }
    }
}