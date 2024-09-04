using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.MAS;
using MessagePack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;
namespace Bnan.Ui.Areas.CAS.Controllers
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class ReportFClosedContractController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly ICompnayContract _compnayContract;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportFClosedContractController> _localizer;


        public ReportFClosedContractController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, ICompnayContract compnayContract,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportFClosedContractController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _compnayContract = compnayContract;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {

            //sidebar Active
            ViewBag.id = "#sidebarReport";
            ViewBag.no = "3";
            //ViewBag.StartDate = "2024-02-15";
            //ViewBag.EndDate = "2024-02-15";
            ViewBag.StartDate = "";
            ViewBag.EndDate = "";

            var (mainTask, subTask, system, currentUser) = await SetTrace("205", "2205004", "2");


            var titles = await setTitle("205", "2205004", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            //var lastDate = DateTime.Now;
            //var startDate = lastDate.Date.AddMonths(-1);

            var RenterContract_Basic_All = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicStatus == Status.Closed && x.CrCasRenterContractBasicLessor == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterContractBasic1" }).OrderByDescending(x => x.CrCasRenterContractBasicIssuedDate);

            ReportActiveContractVM reportActiveContractVM = new ReportActiveContractVM();
            //--------------------------------

            if (RenterContract_Basic_All.Count() > 1)
            {
                var lastDate = RenterContract_Basic_All.FirstOrDefault(x => x.CrCasRenterContractBasicIssuedDate != null)?.CrCasRenterContractBasicIssuedDate;
                //var startDate = RenterContract_Basic_All.LastOrDefault(x => x.CrCasRenterContractBasicIssuedDate != null)?.CrCasRenterContractBasicIssuedDate;
                if (lastDate != null)
                {
                    var startDate = lastDate.Value.AddMonths(-1);
                    var llast = lastDate.Value.Date.AddDays(1);
                    var RenterContract_Basic_All1 = RenterContract_Basic_All.Where(x => x.CrCasRenterContractBasicIssuedDate >= startDate.Date && x.CrCasRenterContractBasicIssuedDate < llast).ToList();

                    reportActiveContractVM.crCasRenterContractBasic = RenterContract_Basic_All1;
                    ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
                    ViewBag.EndDate = lastDate?.ToString("yyyy-MM-dd");

                    //var users = _unitOfWork.CrMasUserInformation.FindAll(x => x.CrMasUserInformationLessor == currentUser.CrMasUserInformationLessor).Select(x => (
                    //x.CrMasUserInformationCode,
                    //x.CrMasUserInformationArName,
                    //x.CrMasUserInformationEnName)).ToList();

                    var invoices = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == currentUser.CrMasUserInformationLessor && x.CrCasAccountInvoiceType == "309").Select(x => (
                    x.CrCasAccountInvoiceReferenceContract,
                    x.CrCasAccountInvoiceArPdfFile,
                    x.CrCasAccountInvoiceEnPdfFile,
                    x.CrCasAccountInvoiceNo)).ToList();


                    //reportActiveContractVM.All_users = users;
                    reportActiveContractVM.All_Invoices = invoices;

                }
            }



            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            return View("Index", reportActiveContractVM);



        }

        [HttpGet]
        public async Task<PartialViewResult> GetAllContractsByDate_status(string _max, string _mini)
        {
            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "3";

            var (mainTask, subTask, system, currentUser) = await SetTrace("205", "2205004", "2");

            if (!string.IsNullOrEmpty(_max) && !string.IsNullOrEmpty(_mini))
            {
                // "today"  "tomorrow"   "after_longTime" 

                _max = DateTime.Parse(_max).Date.AddDays(1).ToString("yyyy-MM-dd");


                var RenterContract_Basic_All1 = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicIssuedDate < DateTime.Parse(_max).Date && x.CrCasRenterContractBasicIssuedDate >= DateTime.Parse(_mini).Date && x.CrCasRenterContractBasicStatus == Status.Closed && x.CrCasRenterContractBasicLessor == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterContractBasic1", "CrCasRenterContractBasic4", "CrCasRenterContractBasic3", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation", "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasicNavigation", "CrCasRenterContractBasic5" }).OrderByDescending(x => x.CrCasRenterContractBasicIssuedDate).ToList();

                //var users = _unitOfWork.CrMasUserInformation.FindAll(x => x.CrMasUserInformationLessor == currentUser.CrMasUserInformationLessor).Select(x => (
                //x.CrMasUserInformationCode,
                //x.CrMasUserInformationArName,
                //x.CrMasUserInformationEnName)).ToList();

                var invoices = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == currentUser.CrMasUserInformationLessor && x.CrCasAccountInvoiceType == "309").Select(x => (
                x.CrCasAccountInvoiceReferenceContract,
                x.CrCasAccountInvoiceArPdfFile,
                x.CrCasAccountInvoiceEnPdfFile,
                x.CrCasAccountInvoiceNo)).ToList();

                ReportActiveContractVM reportActiveContractVM = new ReportActiveContractVM();
                reportActiveContractVM.crCasRenterContractBasic = RenterContract_Basic_All1;
                //reportActiveContractVM.All_users = users;
                reportActiveContractVM.All_Invoices = invoices;

                return PartialView("_DataTableReportFClosedContract", reportActiveContractVM);

            }
            return PartialView();
        }

        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarReport";
            ViewBag.no = "3";
            var titles = await setTitle("205", "2205004", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            var (mainTask, subTask, system, currentUser) = await SetTrace("205", "2205004", "2");

            var RenterContract_Basic_All = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicStatus == Status.Closed && x.CrCasRenterContractBasicLessor == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterContractBasic1", "CrCasRenterContractBasic4", "CrCasRenterContractBasic3", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation", "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasicNavigation", "CrCasRenterContractBasic5" }).ToList();

            if (RenterContract_Basic_All?.Count() < 1)
            {
                ViewBag.Data = "0";
                //_toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View();
            }
            else
            {
                ViewBag.Data = "1";
                return RedirectToAction("Index");
            }

        }



    }
}

