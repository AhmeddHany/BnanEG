using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.Identitiy;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Bnan.Ui.Areas.Identity.Controllers
{

    [Area("Identity")]
    public class AccountController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IStringLocalizer<AccountController> _localizer;
        private readonly SignInManager<CrMasUserInformation> _signInManager;


        public AccountController(IAuthService authService, IUserService userService, UserManager<CrMasUserInformation> userManager, SignInManager<CrMasUserInformation> signInManager, IStringLocalizer<AccountController> localizer, IUnitOfWork unitOfWork, IMapper mapper) : base(userManager, unitOfWork, mapper)
        {
            _authService = authService;
            _userService = userService;
            _signInManager = signInManager;
            _localizer = localizer;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Log in", "Bnan", "", "", "", "");
            else await ViewData.SetPageTitleAsync("تسجيل الدخول", "بنان", "", "", "", "");
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var isAuth = _userService.CheckUserifAuth(User);
                if (isAuth)
                {
                    bool? userAuthBnan = user.CrMasUserInformationAuthorizationBnan;
                    bool? userAuthOwners = user.CrMasUserInformationAuthorizationOwner;
                    bool? userAuthAdmin = user.CrMasUserInformationAuthorizationAdmin;
                    bool? userAuthBranch = user.CrMasUserInformationAuthorizationBranch;
                    if (userAuthBnan == true)
                    {
                        return RedirectToAction("Index", "Home", new { area = "MAS" });
                    }

                    if (userAuthAdmin == true && userAuthBranch == true && userAuthOwners == true)
                    {
                        return RedirectToAction("Systems", "Account");
                    }
                    else if ((userAuthAdmin == true && userAuthBranch == true) || (userAuthAdmin == true && userAuthOwners == true) || (userAuthBranch == true && userAuthOwners == true))
                    {
                        return RedirectToAction("Systems", "Account");
                    }
                    else if (userAuthAdmin == true && userAuthBranch == false && userAuthOwners == false)
                    {
                        return RedirectToAction("Index", "Home", new { area = "CAS" });
                    }
                    else if (userAuthAdmin == false && userAuthBranch == true && userAuthOwners == false)
                    {
                        return RedirectToAction("Index", "Home", new { area = "BS" });
                    }
                    else
                    {
                        return RedirectToAction("Logout", "Account", new { area = "BS" });
                    }
                }
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (CultureInfo.CurrentUICulture.Name == "en-US")
                await ViewData.SetPageTitleAsync("Log in", "Bnan", "", "", "", "");
            else
                await ViewData.SetPageTitleAsync("تسجيل الدخول", "بنان", "", "", "", "");

            // التحقق من المدخلات قبل تنفيذ أي عمليات أخرى
            if (string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
            {
                if (string.IsNullOrWhiteSpace(model.UserName))
                    ModelState.AddModelError(nameof(model.UserName), _localizer["PleaseEnterUserName"]);

                if (string.IsNullOrWhiteSpace(model.Password))
                    ModelState.AddModelError(nameof(model.Password), _localizer["PleaseEnterPassword"]);

                return View(model);
            }

            // البحث عن المستخدم
            var user = await _unitOfWork.CrMasUserInformation.FindAsync(
                x => x.CrMasUserInformationCode == model.UserName,
                new[] { "CrMasUserInformationLessorNavigation", "CrMasUserBranchValidities.CrMasUserBranchValidity1" }
            );

            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Password), _localizer["InvalidCredentials"]);
                return View(model);
            }

            // التحقق من كلمة المرور
            if (!await _authService.CheckPassword(model.UserName, model.Password))
            {
                string errorMessage = _localizer["InvalidCredentials"];
                if (!string.IsNullOrEmpty(user.CrMasUserInformationRemindMe))
                {
                    errorMessage += $": {user.CrMasUserInformationRemindMe}";
                }
                ModelState.AddModelError("Hint", errorMessage);
                return View("Login", model);
            }

            // التحقق مما إذا كان الحساب محذوفًا
            if (user.CrMasUserInformationStatus == Status.Deleted)
            {
                ModelState.AddModelError("Hint", _localizer["AccountDelete"]);
                return View(model);
            }

            if (user.CrMasUserInformationLessorNavigation.CrMasLessorInformationStatus == Status.Deleted)
            {
                ModelState.AddModelError("Hint", _localizer["CompanyDeleted"]);
                return View(model);
            }

            // التحقق من صلاحيات الفروع
            if (user.CrMasUserInformationAuthorizationBranch.GetValueOrDefault() && !user.CrMasUserInformationAuthorizationAdmin.GetValueOrDefault() && !user.CrMasUserInformationAuthorizationOwner.GetValueOrDefault())
            {
                var branchValidities = user.CrMasUserBranchValidities.Where(x => x.CrMasUserBranchValidityBranchStatus == Status.Active);
                if (!branchValidities.Any())
                {
                    ModelState.AddModelError("Hint", _localizer["NoHaveBranches"]);
                    return View(model);
                }
                else if (branchValidities.Count() == 1 && branchValidities.First().CrMasUserBranchValidity1.CrCasBranchInformationStatus == Status.Deleted)
                {
                    ModelState.AddModelError("Hint", _localizer["HaveOneBranchDeleted"]);
                    return View(model);
                }
            }

            // تنفيذ تسجيل الدخول
            var result = await _authService.LoginAsync(model.UserName, model.Password);
            if (result.Succeeded)
            {
                // تحديد لغة الواجهة
                SetLanguage("~/", user.CrMasUserInformationDefaultLanguage.ToLower() == "en" ? "en-US" : "ar-EG");

                // تحديث بيانات المستخدم بعد تسجيل الدخول
                user.CrMasUserInformationOperationStatus = true;
                user.CrMasUserInformationLastActionDate = DateTime.UtcNow;
                user.CrMasUserInformationEntryLastDate = DateTime.UtcNow.Date;
                user.CrMasUserInformationEntryLastTime = DateTime.UtcNow.TimeOfDay;
                await _userService.SaveChanges(user);

                // تحديد الوجهة المناسبة بعد تسجيل الدخول
                string redirectUrl = string.Empty;

                if (user.CrMasUserInformationAuthorizationBnan == true)
                    redirectUrl = Url.Action("Index", "Home", new { area = "MAS" });
                else if (user.CrMasUserInformationAuthorizationAdmin == true &&
                         user.CrMasUserInformationAuthorizationBranch == true &&
                         user.CrMasUserInformationAuthorizationOwner == true)
                    redirectUrl = Url.Action("Systems", "Account");
                else if (user.CrMasUserInformationAuthorizationAdmin == true)
                    redirectUrl = Url.Action("Index", "Home", new { area = "CAS" });
                else if (user.CrMasUserInformationAuthorizationBranch == true)
                    redirectUrl = Url.Action("Index", "Home", new { area = "BS" });
                else
                {
                    ModelState.AddModelError("Stat", _localizer["AuthEmplpoyee"]);
                    return View(model);
                }

                return Redirect(redirectUrl);
            }

            if (result.IsLockedOut)
            {
                return View("AccountLocked");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Systems()
        {
            var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);

            // إعداد عنوان الصفحة بناءً على اللغة
            if (CultureInfo.CurrentUICulture.Name == "en-US")
                await ViewData.SetPageTitleAsync("Systems", "", "", "", "", user.CrMasUserInformationEnName);
            else
                await ViewData.SetPageTitleAsync("الأنظمة", "", "", "", "", user.CrMasUserInformationArName);

            // قائمة للصلاحيات المفعّلة
            var permissions = new List<string>();

            if (user.CrMasUserInformationAuthorizationOwner == true)
                permissions.Add("OWN");
            if (user.CrMasUserInformationAuthorizationAdmin == true)
                permissions.Add("CAS");
            if (user.CrMasUserInformationAuthorizationBranch == true)
                permissions.Add("BS");

            // إذا كان لديه صلاحية واحدة فقط
            if (permissions.Count == 1)
            {
                return RedirectToAction("Index", "Home", new { area = permissions.First() });
            }

            // إذا كان لديه أكثر من صلاحية، اعرض صفحة الأنظمة
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            //var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            user.CrMasUserInformationOperationStatus = false;
            user.CrMasUserInformationExitLastDate = DateTime.Now.Date;
            user.CrMasUserInformationExitLastTime = DateTime.Now.TimeOfDay;
            await _userService.SaveChanges(user);
            await _authService.SignOut();
            return RedirectToAction("Login", "Account");
        }


        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateLastActionDate()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(false);
            }
            user.CrMasUserInformationLastActionDate = DateTime.Now;
            await _unitOfWork.CompleteAsync();
            return Json(true);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new CrMasUserInformation
                {
                    CrMasUserInformationCode = model.Code,
                    CrMasUserInformationPassWord = model.Code,
                    CrMasUserInformationRemindMe = "البريد",
                    CrMasUserInformationLessor = "4009",
                    CrMasUserInformationDefaultBranch = "",
                    CrMasUserInformationDefaultLanguage = "AR",
                    CrMasUserInformationAuthorizationBnan = false,
                    CrMasUserInformationAuthorizationAdmin = true,
                    CrMasUserInformationAuthorizationBranch = true,
                    CrMasUserInformationAuthorizationOwner = true,
                    CrMasUserInformationAuthorizationFoolwUp = true,
                    CrMasUserInformationArName = "تميم",
                    CrMasUserInformationEnName = "Tamem",
                    CrMasUserInformationTasksArName = "مدير الفني لنظام",
                    CrMasUserInformationTasksEnName = "System Technical Manager",
                    CrMasUserInformationReservedBalance = 0,
                    CrMasUserInformationTotalBalance = 0,
                    CrMasUserInformationAvailableBalance = 0,
                    CrMasUserInformationCreditLimit = 0,
                    CrMasUserInformationMobileNo = "",
                    CrMasUserInformationEmail = "",
                    CrMasUserInformationChangePassWordLastDate = null,
                    CrMasUserInformationEntryLastDate = null,
                    UserName = model.Code,
                    Id = model.Code
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return RedirectToAction("Systems", "Account");
        }
        [Route("Identity/Account/Error")]
        [Route("Identity/Account/Error/{statusCode}")]
        [AllowAnonymous]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                ViewBag.OriginalPath = statusCodeResult?.OriginalPath;
                ViewBag.OriginalQueryString = statusCodeResult?.OriginalQueryString;
                ViewBag.StatusCode = statusCode.Value;

                if (statusCode.Value == 403)
                {
                    ViewBag.ErrorMessage = "Access Denied: You do not have permission to access this resource.";
                }
                else if (statusCode.Value == 404)
                {
                    ViewBag.ErrorMessage = "The resource you are looking for could not be found.";
                }
                else if (statusCode.Value == 500)
                {
                    ViewBag.ErrorMessage = "There was an internal server error. Please try again later.";
                }
                else
                {
                    ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
            }
            return View();
        }
        [HttpGet]
        public IActionResult SetLanguage(string returnUrl, string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );

            return LocalRedirect(returnUrl);
        }
    }
}