using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Linq;

namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class RenterBalancesController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IFinancialTransactionOfRenter _FinancialTransactionOfRenter;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterBalancesController> _localizer;


        public RenterBalancesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter FinancialTransactionOfRenter,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterBalancesController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _FinancialTransactionOfRenter = FinancialTransactionOfRenter;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "2";
            var (mainTask, subTask, system, currentUser) = await SetTrace("203", "2203003", "2");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("203", "2203003", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var FinancialTransactionOfRenterAll = _unitOfWork.CrCasAccountReceipt.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasAccountReceiptLessorCode && (x.CrCasAccountReceiptType == "301" || x.CrCasAccountReceiptType == "302") , new[] { "CrCasAccountReceiptRenter" }).OrderByDescending(x => x.CrCasAccountReceiptDate).ToList();
            var AllRenterLessor = _unitOfWork.CrCasRenterLessor.FindAll(x => currentUser.CrMasUserInformationLessor == x.CrCasRenterLessorCode && x.CrCasRenterLessorAvailableBalance != 0 && x.CrCasRenterLessorStatus != "R",new []{ "CrCasRenterLessorNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" }).ToList();


            if (AllRenterLessor?.Count() < 1)
            {
                return RedirectToAction("FailedMessageReport_NoData");
            }

            var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();
            ViewData["Rates"] = rates;

            FinancialTransactionOfRenterAll = FinancialTransactionOfRenterAll.Where(x=> AllRenterLessor.Any(y=>y.CrCasRenterLessorCode==x.CrCasAccountReceiptLessorCode && y.CrCasRenterLessorId == x.CrCasAccountReceiptRenterId )).ToList();
            List<CrCasAccountReceipt>? FinancialTransactionOfRente_Filtered = new List<CrCasAccountReceipt>();

            List<List<string>>? All_Counts = new List<List<string>>();

            foreach (var FT_Renter1 in FinancialTransactionOfRenterAll)
            {
                decimal? Total_Creditor = 0;
                decimal? Total_Debtor = 0;
                var x = FinancialTransactionOfRente_Filtered.Find(x => x.CrCasAccountReceiptRenterId == FT_Renter1.CrCasAccountReceiptRenterId);
                if (x == null)
                {
                    var counter = 0;
                    foreach (var FT_Renter_2 in FinancialTransactionOfRenterAll)
                    {
                        if (FT_Renter1.CrCasAccountReceiptRenterId == FT_Renter_2.CrCasAccountReceiptRenterId && FT_Renter1.CrCasAccountReceiptLessorCode == FT_Renter_2.CrCasAccountReceiptLessorCode)
                        {
                            //Total_Creditor = FT_Renter_2.CrCasRenterContractBasicExpectedTotal + Total_Creditor;
                            //Total_Debtor = FT_Renter_2.CrCasRenterContractBasicExpectedTotal + Total_Debtor;
                            Total_Creditor = 0;
                            Total_Debtor = 0;
                            counter = counter + 1;
                        }

                    }
                    All_Counts.Add(new List<string> { FT_Renter1.CrCasAccountReceiptRenterId, counter.ToString(), Total_Creditor?.ToString("N2", CultureInfo.InvariantCulture), Total_Debtor?.ToString("N2", CultureInfo.InvariantCulture) });
                    FinancialTransactionOfRente_Filtered.Add(FT_Renter1);
                }
            }


            FinancialTransactionOfRenterVM FT_RenterVM = new FinancialTransactionOfRenterVM();
            FT_RenterVM.crCasAccountReceipt = FinancialTransactionOfRenterAll;
            FT_RenterVM.crCasRenterLessor = AllRenterLessor;
            FT_RenterVM.All_Counts = All_Counts;
            FT_RenterVM.FinancialTransactionOfRente_Filtered = FinancialTransactionOfRente_Filtered;

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            return View(FT_RenterVM);
        }


        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "2";
            var titles = await setTitle("205", "2203003", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            var (mainTask, subTask, system, currentUser) = await SetTrace("203", "2203003", "2");

            ViewBag.Data = "0";
            //_toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return View();


        }


    }
}

