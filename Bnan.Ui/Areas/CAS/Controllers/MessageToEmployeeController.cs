using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using Bnan.Ui.ViewModels.BS;
using System.Diagnostics.Contracts;
using System.Numerics;



namespace Bnan.Ui.Areas.CAS.Controllers
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class MessageToEmployeeController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<MessageToEmployeeController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;


        public MessageToEmployeeController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService,  IAdminstritiveProcedures AdminstritiveProcedures,
        IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<MessageToEmployeeController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _adminstritiveProcedures = AdminstritiveProcedures;
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {

            //sidebar Active
            ViewBag.id = "#sidebarServices";
            ViewBag.no = "5";

            var (mainTask, subTask, system, currentUser) = await SetTrace("207", "2207006", "2");

           
            var titles = await setTitle("207", "2207006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var MessageToRentersAll = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] {  "CrCasRenterLessorNavigation.CrMasRenterPost" , "CrCasRenterLessorNavigation" }).ToList();
            
            var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x=>x.CrMasSysEvaluationsClassification=="1").ToList();
            ViewData["Rates"] = rates;



            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
           subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
           subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            MessageToRentersVM messageToRentersVM = new MessageToRentersVM();
            messageToRentersVM.CrCasRenterLessor = MessageToRentersAll;

            return View(messageToRentersVM);
        }


        [HttpPost]
        public async Task<IActionResult> SendMessageToRenters(List<string> values, string textarea_Chat , string Image)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            //////////////////


            //// Save Adminstrive Procedures
            //await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "307", "30", currentUser.CrMasUserInformationLessor, "100",
            //    Serial_pay, Total_Money_Decimal, Total_Money_Decimal, null, null, null, null, null, null, "تسديد مستحقات القيمة المضافة", "Payment Dues of Tax values", "I", reasons);


            foreach (var item in values)
            {
                var exist = _unitOfWork.CrMasRenterInformation.FindAll(x => item.Trim() == x.CrMasRenterInformationId ).FirstOrDefault();
                if (exist != null)
                {
                    // send with what's Up
                    var mobileNo = exist.CrMasRenterInformationMobile;
                    var Key_mobile = exist.CrMasRenterInformationCountreyKey;
                }
            }


            //////////////////

            //await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تم التسديد", "Payment Done", mainTask.CrMasSysMainTasksCode,
            //subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            //subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            return Json(new { code = 1 });
            //return View();

        }


        [HttpPost]
        public async Task<IActionResult> SendImageToRenters(List<string> values, string Image)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "5";
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204006", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("204", "2204006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            //////////////////


            //// Save Adminstrive Procedures
            //await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "307", "30", currentUser.CrMasUserInformationLessor, "100",
            //    Serial_pay, Total_Money_Decimal, Total_Money_Decimal, null, null, null, null, null, null, "تسديد مستحقات القيمة المضافة", "Payment Dues of Tax values", "I", reasons);


            foreach (var item in values)
            {
                var exist = _unitOfWork.CrMasRenterInformation.FindAll(x => item.Trim() == x.CrMasRenterInformationId).FirstOrDefault();
                if (exist != null)
                {
                    // send with what's Up
                    var mobileNo = exist.CrMasRenterInformationMobile;
                    var Key_mobile = exist.CrMasRenterInformationCountreyKey;
                }
            }


            //////////////////

            //await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تم التسديد", "Payment Done", mainTask.CrMasSysMainTasksCode,
            //subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            //subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            
            return Json(new { code = 1 });
            //return View();

        }


        [HttpGet]
        public async Task<PartialViewResult> GetMessageToRentersByStatusAsync(string status)
        {

            //sidebar Active
            ViewBag.id = "#sidebarServices";
            ViewBag.no = "5";

            var (mainTask, subTask, system, currentUser) = await SetTrace("207", "2207006", "2");

            if (!string.IsNullOrEmpty(status))
            {
                var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();

                ViewData["Rates"] = rates;

                if (status == Status.All)
                {
                    var MessageToRentersbyStatusAll = _unitOfWork.CrCasRenterLessor.FindAll(l => (l.CrCasRenterLessorStatus == Status.Active || l.CrCasRenterLessorStatus == Status.Rented) && l.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] {"CrCasRenterLessorNavigation.CrMasRenterPost" }).ToList();
                    return PartialView("_DataTableBasic", MessageToRentersbyStatusAll);
                }
                var MessageToRentersbyStatus = _unitOfWork.CrCasRenterLessor.FindAll(l => l.CrCasRenterLessorStatus == status && l.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] {"CrCasRenterLessorNavigation.CrMasRenterPost" }).ToList();
                return PartialView("_DataTableBasic", MessageToRentersbyStatus);
            }
            return PartialView();
        }




        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarServices";
            ViewBag.no = "5";
            var titles = await setTitle("205", "2207006", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            var (mainTask, subTask, system, currentUser) = await SetTrace("207", "2207006", "2");

            var MessageToRentersAll = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost" }).ToList();


            if (MessageToRentersAll?.Count() < 1)
            {
                ViewBag.Data = "5";
                return View();
            }
            else
            {
                ViewBag.Data = "1";
                return RedirectToAction("Index");
            }

        }

        public IActionResult SuccessToast()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "MessageToRenters");
        }
    }
}
