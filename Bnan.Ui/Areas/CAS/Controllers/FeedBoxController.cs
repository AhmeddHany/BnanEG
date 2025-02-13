using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class FeedBoxController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IStringLocalizer<FeedBoxController> _localizer;
        private readonly IToastNotification _toastNotification;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        //private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.FeedBoxCAS;

        public FeedBoxController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IUserLoginsService userLoginsService, IToastNotification toastNotification, IStringLocalizer<FeedBoxController> localizer, IAdminstritiveProcedures adminstritiveProcedures, IUserService userService, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _adminstritiveProcedures = adminstritiveProcedures;
            _userService = userService;
            _baseRepo = baseRepo;
        }

        public async Task<IActionResult> FeedBox()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await SetPageTitleAsync(string.Empty, pageNumber);
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            // Exclude the current user from the list
            var usersByLessor = await _userService.GetAllUsersByLessor(user.CrMasUserInformationLessor);
            var usersWithOutMangerAndCurrentUser = usersByLessor.Where(x =>!x.CrMasUserInformationCode.StartsWith("CAS") &&
                                                                            x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                                            x.CrMasUserInformationStatus == Status.Active &&
                                                                            x.CrMasUserInformationAuthorizationBranch == true);

            List<CrMasUserInformation> ListUsers = new List<CrMasUserInformation>();

            if (usersWithOutMangerAndCurrentUser != null)
            {
                foreach (var item in usersWithOutMangerAndCurrentUser)
                {
                    var proc = _unitOfWork.CrCasSysAdministrativeProcedure.Find(a => a.CrCasSysAdministrativeProceduresLessor == user.CrMasUserInformationLessor &&
                     a.CrCasSysAdministrativeProceduresCode == "303" && a.CrCasSysAdministrativeProceduresStatus == Status.Insert
                     && a.CrCasSysAdministrativeProceduresTargeted == item.CrMasUserInformationCode);
                    if (proc == null)
                    {
                        ListUsers.Add(item);
                    }
                }
            }
            return View(ListUsers);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeesBySearch(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            // Exclude the current user from the list
            var usersByLessor = await _userService.GetAllUsersByLessor(user.CrMasUserInformationLessor);
            var usersWithOutMangerAndCurrentUser = usersByLessor.Where(x => x.CrMasUserInformationCode.StartsWith("CAS") &&
                                                                            x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                                            x.CrMasUserInformationStatus == Status.Active &&
                                                                            x.CrMasUserInformationAuthorizationBranch == true&&
                                                                            (x.CrMasUserInformationArName.Contains(search) ||
                                                                            x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                            x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                            x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                            x.CrMasUserInformationCode.Contains(search)));

            List < CrMasUserInformation> ListUsers = new List<CrMasUserInformation>();

            if (usersWithOutMangerAndCurrentUser != null)
            {
                foreach (var item in usersWithOutMangerAndCurrentUser)
                {
                    var proc = _unitOfWork.CrCasSysAdministrativeProcedure.Find(a => a.CrCasSysAdministrativeProceduresLessor == user.CrMasUserInformationLessor &&
                     a.CrCasSysAdministrativeProceduresCode == "303" && a.CrCasSysAdministrativeProceduresStatus == Status.Insert
                     && a.CrCasSysAdministrativeProceduresTargeted == item.CrMasUserInformationCode);
                    if (proc == null)
                    {
                        ListUsers.Add(item);
                    }
                }
                return PartialView("_DataTableFeedBoxForUsers", ListUsers);
            }
            return PartialView();
        }
        //[HttpGet]
        //public async Task<IActionResult> Send(string id)
        //{
        //    //sidebar Active
        //    ViewBag.id = "#sidebarAcount";
        //    ViewBag.no = "0";
        //    //To Set Title;
        //    var titles = await setTitle("204", "2204001", "2");
        //    await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "اضافة", "Create", titles[3]);

        //    ViewBag.Date = DateTime.Now.Date.ToString("dd/MM/yyyy");
        //    var currentUser = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == id);
        //    if (currentUser == null)
        //    {
        //        _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
        //        return RedirectToAction("FeedBox");
        //    }

        //    return View(currentUser);
        //}
        [HttpPost]
        public async Task<IActionResult> Send(CrMasUserInformation Model, string FeedValue, string Reasons)
        {
            try
            {

            
            
            var userLogin = await _userManager.GetUserAsync(User);
            // التحقق من القيم المدخلة
            if (string.IsNullOrEmpty(FeedValue) || string.IsNullOrWhiteSpace(FeedValue))
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("FeedBox");
            }
            // محاولة تحويل FeedValue إلى decimal مع التحقق من نجاح التحويل
            if (!decimal.TryParse(FeedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal feedAmount))
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("FeedBox");
            }


            var result = await _adminstritiveProcedures.SaveAdminstritive(userLogin.CrMasUserInformationCode, "1", "303", "30", userLogin.CrMasUserInformationLessor, "100",
            Model.CrMasUserInformationCode, decimal.Parse(FeedValue, CultureInfo.InvariantCulture), null, null, null, null, null, null, null, "تحت الإجراء", "Under Proccessing", "I", Reasons);

            if (result)
            {
                await SaveTracingForChange(Status.Insert);
                _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            }
            else _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("FeedBox");
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("FeedBox");
            }
        }

        private async Task SaveTracingForChange(string status)
        {


            var recordAr = "تغذية صندوق";
            var recordEn = "Feed Box";
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
