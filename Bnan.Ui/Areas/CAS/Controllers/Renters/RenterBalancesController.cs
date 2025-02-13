using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
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
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterBalancesController> _localizer;
        private readonly string pageNumber = SubTasks.RentersCas_RentersDebits;
        private readonly IWebHostEnvironment _env;



        public RenterBalancesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IWebHostEnvironment env,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterBalancesController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            // Set page titles
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var FinancialTransactionOfRenterAll = _unitOfWork.CrCasAccountReceipt.FindAll(x => user.CrMasUserInformationLessor == x.CrCasAccountReceiptLessorCode && (x.CrCasAccountReceiptType == "301" || x.CrCasAccountReceiptType == "302"), new[] { "CrCasAccountReceiptRenter" });
            var AllRenterLessor = _unitOfWork.CrCasRenterLessor.FindAll(x => user.CrMasUserInformationLessor == x.CrCasRenterLessorCode && x.CrCasRenterLessorAvailableBalance != 0 && x.CrCasRenterLessorStatus != "R", new[] { "CrCasRenterLessorNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });


            //var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();

            FinancialTransactionOfRenterAll = FinancialTransactionOfRenterAll.Where(x => AllRenterLessor.Any(y => y.CrCasRenterLessorCode == x.CrCasAccountReceiptLessorCode && y.CrCasRenterLessorId == x.CrCasAccountReceiptRenterId));
            List<CrCasAccountReceipt>? FinancialTransactionOfRente_Filtered = new List<CrCasAccountReceipt>();

            //List<List<string>>? All_Counts = new List<List<string>>();

            //foreach (var FT_Renter1 in FinancialTransactionOfRenterAll)
            //{
            //    decimal? Total_Creditor = 0;
            //    decimal? Total_Debtor = 0;
            //    var x = FinancialTransactionOfRente_Filtered.Find(x => x.CrCasAccountReceiptRenterId == FT_Renter1.CrCasAccountReceiptRenterId);
            //    if (x == null)
            //    {
            //        var counter = 0;
            //        foreach (var FT_Renter_2 in FinancialTransactionOfRenterAll)
            //        {
            //            if (FT_Renter1.CrCasAccountReceiptRenterId == FT_Renter_2.CrCasAccountReceiptRenterId && FT_Renter1.CrCasAccountReceiptLessorCode == FT_Renter_2.CrCasAccountReceiptLessorCode)
            //            {
            //                //Total_Creditor = FT_Renter_2.CrCasRenterContractBasicExpectedTotal + Total_Creditor;
            //                //Total_Debtor = FT_Renter_2.CrCasRenterContractBasicExpectedTotal + Total_Debtor;
            //                Total_Creditor = 0;
            //                Total_Debtor = 0;
            //                counter = counter + 1;
            //            }

            //        }
            //        All_Counts.Add(new List<string> { FT_Renter1.CrCasAccountReceiptRenterId, counter.ToString(), Total_Creditor?.ToString("N2", CultureInfo.InvariantCulture), Total_Debtor?.ToString("N2", CultureInfo.InvariantCulture) });
            //        FinancialTransactionOfRente_Filtered.Add(FT_Renter1);
            //    }
            //}
            FinancialTransactionOfRente_Filtered = FinancialTransactionOfRenterAll.DistinctBy(x=> new { x.CrCasAccountReceiptRenterId, x.CrCasAccountReceiptLessorCode }).ToList();
            //FinancialTransactionOfRente_Filtered.OrderByDescending(x=>x.CrCasAccountReceiptDate);

            FinancialTransactionOfRenterVM FT_RenterVM = new FinancialTransactionOfRenterVM();
            FT_RenterVM.crCasAccountReceipt = FinancialTransactionOfRenterAll?.ToList() ?? new List<CrCasAccountReceipt>();
            FT_RenterVM.crCasRenterLessor = AllRenterLessor?.ToList() ?? new List<CrCasRenterLessor>();
            //FT_RenterVM.All_Counts = All_Counts;
            //FT_RenterVM.Rates = rates?.ToList() ?? new List<CrMasSysEvaluation>();
            FT_RenterVM.FinancialTransactionOfRente_Filtered = FinancialTransactionOfRente_Filtered?? new List<CrCasAccountReceipt>();

            return View(FT_RenterVM);
        }


    }
}

