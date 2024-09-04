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


namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CarContractController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IFinancialTransactionOfRenter _CarContract;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarContractController> _localizer;


        public CarContractController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter CarContract,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarContractController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _CarContract = CarContract;
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
            ViewBag.no = "7";

            var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205008", "2");

            var titles = await setTitle("205", "2205008", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllCars = _unitOfWork.CrCasCarInformation.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasCarInformationLessor && x.CrCasCarInformationConractCount > 0, new[] { "CrCasCarInformation1", "CrCasRenterContractBasics" }).OrderByDescending(x => x.CrCasCarInformationLastContractDate).ToList();
            //var CrCasRenterContractStatistics = _unitOfWork.CrCasRenterContractStatistic.FindAll(x=>x.CrCasRenterContractStatisticsLessor== currentCar.CrMasUserInformationLessor).OrderByDescending(x=>x.CrCasRenterContractStatisticsCarSerialNo).ToList();

            //if (AllCars.Count() < 1)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}

            var AdvatagesAll = _unitOfWork.CrCasCarAdvantage.FindAll(x => x.CrCasCarAdvantagesLessor == currentCar.CrMasUserInformationLessor).DistinctBy(x => x.CrCasCarAdvantagesSerialNo).ToList();

            CarContractVM userContractVM = new CarContractVM();
            userContractVM.crCasCarInformation = AllCars;
            userContractVM.AdvantagesAll = AdvatagesAll;

            //userContractVM.crCasRenterContractStatistic = CrCasRenterContractStatistics;

            await _userLoginsService.SaveTracing(currentCar.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            return View(userContractVM);
        }




        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarReport";
            ViewBag.no = "7";
            var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205008", "2");

            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("205", "2205008", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var _max = DateTime.Now.Date.AddDays(1).ToString("yyyy-MM-dd");
            var _mini = DateTime.Now.Date.AddDays(-30).ToString("yyyy-MM-dd");


            //var contract = _unitOfWork.CrCasRenterContractBasic.FindAll(l => l.CrCasRenterContractBasic3?.CrCasRenterPrivateCarInformationId == id, new[] { "CrCasRenterContractBasic1", "CrCasRenterContractBasic2", "CrCasRenterContractBasic3", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation", "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasicNavigation", "CrCasRenterContractBasic4" }).Where(x => x.CrCasRenterContractBasicPrivateCarId != null).Where(x => x.CrCasRenterContractBasic3?.CrCasRenterPrivateCarInformationContractCount > 0).ToList();
            var CarContractAll = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicIssuedDate < DateTime.Parse(_max).Date && x.CrCasRenterContractBasicIssuedDate >= DateTime.Parse(_mini).Date && currentCar.CrMasUserInformationLessor == x.CrCasRenterContractBasicLessor && id == x.CrCasRenterContractBasicCarSerailNo && x.CrCasRenterContractBasicStatus != Status.Extension, new[] { "CrCasRenterContractBasic1", "CrCasRenterContractBasic2", "CrCasRenterContractBasic3", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation", "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasicNavigation", "CrCasRenterContractBasic4" }).OrderByDescending(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();
            var AllCars = _unitOfWork.CrCasCarInformation.GetAll(new[] { "CrCasCarInformation1" }).Where(x => CarContractAll.Any(y => y.CrCasRenterContractBasicCarSerailNo == x.CrCasCarInformationSerailNo)).ToList();

            var CrCasRenterContractStatistics = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => x.CrCasRenterContractStatisticsLessor == currentCar.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsCarSerialNo == id && x.CrCasRenterContractStatisticsDate < DateTime.Parse(_max).Date && x.CrCasRenterContractStatisticsDate >= DateTime.Parse(_mini).Date).OrderByDescending(x => x.CrCasRenterContractStatisticsCarSerialNo).ToList();

            List<CrCasRenterContractBasic>? ContractBasic_Filtered = new List<CrCasRenterContractBasic>();

            List<List<string>>? contracts_Counts = new List<List<string>>();

            foreach (var contract in CarContractAll)
            {
                var contract_count = 0;
                decimal? contract_Total = 0;
                var x = ContractBasic_Filtered.Find(x => x.CrCasRenterContractBasicCarSerailNo == contract.CrCasRenterContractBasicCarSerailNo);
                if (x == null)
                {
                    var counter = 0;
                    foreach (var contract_2 in CarContractAll)
                    {
                        if (contract.CrCasRenterContractBasicCarSerailNo == contract_2.CrCasRenterContractBasicCarSerailNo)
                        {
                            contract_Total = contract_2.CrCasRenterContractBasicExpectedTotal + contract_Total;
                            counter = counter + 1;
                        }

                    }
                    contracts_Counts.Add(new List<string> { contract.CrCasRenterContractBasicCarSerailNo, counter.ToString(), contract_Total.ToString() });
                    ContractBasic_Filtered.Add(contract);
                }
            }

            var AllInvoices_308 = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == currentCar.CrMasUserInformationLessor && x.CrCasAccountInvoiceType == "308").ToList();
            var AllInvoices_309 = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == currentCar.CrMasUserInformationLessor && x.CrCasAccountInvoiceType == "309").ToList();

            CarContractVM userContractVM = new CarContractVM();
            userContractVM.crCasRenterContractBasics = CarContractAll;
            userContractVM.crCasCarInformation = AllCars;
            userContractVM.contracts_Counts = contracts_Counts;
            userContractVM.ContractBasic_Filtered = ContractBasic_Filtered;
            userContractVM.crCasRenterContractStatistic = CrCasRenterContractStatistics;
            userContractVM.CrCasAccountInvoice_308 = AllInvoices_308;
            userContractVM.CrCasAccountInvoice_309 = AllInvoices_309;

            if (CarContractAll == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }

            ViewBag.CountRecord = CarContractAll.Count;

            //var queryMax = _unitOfWork.CrCasRenterContractBasic.GetAll().Where(x => x.CrCasRenterContractBasicPrivateCarId == id && currentCar.CrMasUserInformationLessor == x.CrCasRenterContractBasicLessor).GroupBy(x => x.CrCasRenterContractBasicNo).Select(x => x.OrderByDescending(t => t.CrCasRenterContractBasicCopy));
            var Single_data = _unitOfWork.CrCasCarInformation.Find(x => currentCar.CrMasUserInformationLessor == x.CrCasCarInformationLessor && id == x.CrCasCarInformationSerailNo);

            ViewBag.Single_CarId = Single_data.CrCasCarInformationSerailNo;
            ViewBag.Single_CarNameAr = Single_data.CrCasCarInformationConcatenateArName;
            ViewBag.Single_CarNameEn = Single_data.CrCasCarInformationConcatenateEnName;

            return View(userContractVM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit2Date(string _max, string _mini, string id)
        {

            var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205008", "2");

            //sidebar Active
            ViewBag.id = "#sidebarReport";
            ViewBag.no = "7";
            if (id != null)
            {
                ViewBag.startDate = DateTime.Parse(_mini).Date.ToString("yyyy-MM-dd");
                ViewBag.EndDate = DateTime.Parse(_max).Date.ToString("yyyy-MM-dd");
            }
            else
            {
                return RedirectToAction("Index");
            }
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("205", "2205008", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);
            if (!string.IsNullOrEmpty(_max) && !string.IsNullOrEmpty(_mini) && _max.Length > 0)
            {
                _max = DateTime.Parse(_max).Date.AddDays(1).ToString("yyyy-MM-dd");
                //var CarContractAll = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicIssuedDate < DateTime.Parse(_max).Date && x.CrCasRenterContractBasicIssuedDate >= DateTime.Parse(_mini).Date && currentCar.CrMasUserInformationLessor == x.CrCasRenterContractBasicLessor && id == x.CrCasRenterContractBasicCarSerailNo && x.CrCasRenterContractBasicStatus != Status.Extension && x.CrCasRenterContractBasic3.CrCasRenterPrivateCarInformationContractCount > 0, new[] { "CrCasRenterContractBasic1", "CrCasRenterContractBasic2", "CrCasRenterContractBasic3", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation", "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasicNavigation", "CrCasRenterContractBasic4" }).OrderByDescending(x => x.CrCasRenterContractBasicExpectedTotal).ToList();
                var CarContractAll = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicIssuedDate < DateTime.Parse(_max).Date && x.CrCasRenterContractBasicIssuedDate >= DateTime.Parse(_mini).Date && currentCar.CrMasUserInformationLessor == x.CrCasRenterContractBasicLessor && id == x.CrCasRenterContractBasicCarSerailNo && x.CrCasRenterContractBasicStatus != Status.Extension, new[] { "CrCasRenterContractBasic1", "CrCasRenterContractBasic2", "CrCasRenterContractBasic3", "CrCasRenterContractBasic5.CrCasRenterLessorNavigation", "CrCasRenterContractBasicCarSerailNoNavigation", "CrCasRenterContractBasicNavigation", "CrCasRenterContractBasic4" }).OrderByDescending(x => x.CrCasRenterContractBasicExpectedStartDate).ToList();

                var AllCars = _unitOfWork.CrCasCarInformation.GetAll(new[] { "CrCasCarInformation1" }).Where(x => CarContractAll.Any(y => y.CrCasRenterContractBasicCarSerailNo == x.CrCasCarInformationSerailNo)).ToList();

                var CrCasRenterContractStatistics = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => x.CrCasRenterContractStatisticsLessor == currentCar.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsCarSerialNo == id && x.CrCasRenterContractStatisticsDate < DateTime.Parse(_max).Date && x.CrCasRenterContractStatisticsDate >= DateTime.Parse(_mini).Date).OrderByDescending(x => x.CrCasRenterContractStatisticsCarSerialNo).ToList();

                List<CrCasRenterContractBasic>? ContractBasic_Filtered = new List<CrCasRenterContractBasic>();

                List<List<string>>? contracts_Counts = new List<List<string>>();

                foreach (var contract in CarContractAll)
                {
                    var contract_count = 0;
                    decimal? contract_Total = 0;
                    var x = ContractBasic_Filtered.Find(x => x.CrCasRenterContractBasicCarSerailNo == contract.CrCasRenterContractBasicCarSerailNo);
                    if (x == null)
                    {
                        var counter = 0;
                        foreach (var contract_2 in CarContractAll)
                        {
                            if (contract.CrCasRenterContractBasicCarSerailNo == contract_2.CrCasRenterContractBasicCarSerailNo)
                            {
                                contract_Total = contract_2.CrCasRenterContractBasicExpectedTotal + contract_Total;
                                counter = counter + 1;
                            }

                        }
                        contracts_Counts.Add(new List<string> { contract.CrCasRenterContractBasicCarSerailNo, counter.ToString(), contract_Total.ToString() });
                        ContractBasic_Filtered.Add(contract);
                    }
                }
                var AllInvoices_308 = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == currentCar.CrMasUserInformationLessor && x.CrCasAccountInvoiceType == "308").ToList();
                var AllInvoices_309 = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == currentCar.CrMasUserInformationLessor && x.CrCasAccountInvoiceType == "309").ToList();

                CarContractVM userContractVM = new CarContractVM();
                userContractVM.crCasRenterContractBasics = CarContractAll;
                userContractVM.crCasCarInformation = AllCars;
                userContractVM.contracts_Counts = contracts_Counts;
                userContractVM.ContractBasic_Filtered = ContractBasic_Filtered;
                userContractVM.crCasRenterContractStatistic = CrCasRenterContractStatistics;
                userContractVM.CrCasAccountInvoice_308 = AllInvoices_308;
                userContractVM.CrCasAccountInvoice_309 = AllInvoices_309;
                if (CarContractAll == null)
                {
                    ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                    return View("Index");
                }

                ViewBag.CountRecord = CarContractAll.Count;

                var Single_data = _unitOfWork.CrCasCarInformation.Find(x => currentCar.CrMasUserInformationLessor == x.CrCasCarInformationLessor && id == x.CrCasCarInformationSerailNo);

                ViewBag.Single_CarId = Single_data.CrCasCarInformationSerailNo;
                ViewBag.Single_CarNameAr = Single_data.CrCasCarInformationConcatenateArName;
                ViewBag.Single_CarNameEn = Single_data.CrCasCarInformationConcatenateEnName;

                return View(userContractVM);

            }

            return View();
        }


        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarReport";
            ViewBag.no = "7";
            var titles = await setTitle("205", "2205008", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            ViewBag.Data = "0";
            return View();
        }


    }
}

