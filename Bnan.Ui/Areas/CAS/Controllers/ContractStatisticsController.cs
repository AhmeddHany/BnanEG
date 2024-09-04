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
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;

namespace Bnan.Ui.Areas.CAS.Controllers
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class ContractStatisticsController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IFinancialTransactionOfRenter _CarContract;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ContractStatisticsController> _localizer;


        public ContractStatisticsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter CarContract, 
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ContractStatisticsController> localizer) : base(userManager, unitOfWork, mapper)
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
            ViewBag.no = "14";

            var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205015", "2");

            var titles = await setTitle("205", "2205015", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllContracts = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor ).OrderByDescending(x=>x.CrCasRenterContractStatisticsDate).ToList();
            ViewBag.StartDate = AllContracts?.LastOrDefault()?.CrCasRenterContractStatisticsDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewBag.EndDate = AllContracts?.FirstOrDefault()?.CrCasRenterContractStatisticsDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            CasStatisticLayoutVM casStatisticLayoutVM2 = new CasStatisticLayoutVM();

            if (AllContracts?.Count() < 1)
            {
                var nnow = DateTime.Now.Date;
                ViewBag.StartDate = nnow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ViewBag.EndDate = nnow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ViewBag.concate_DropDown = "";
                ViewBag.singleType = "0";
                ViewBag.count_Renters = 0;
                //return RedirectToAction("FailedMessageReport_NoData");
                return View(casStatisticLayoutVM2);
            }


            var Branch_count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsBranch).Count();
            var Day_Create_count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsDayCreate).Count();
            var Time_Create_count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsTimeCreate).Count();
            var Day_count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsDayCount).Count();
            var Value_Contract_count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsValueNo).Count();
            var KM_count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsKm).Count();
            var Days_count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisicsDays).Count();
            var Hmonth_Create = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsHmonthCreate).Count();
            var Gmonth_Create = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsGmonthCreate).Count();

            //if (Branch_count < 2 && Day_Create_count < 2 && Time_Create_count < 2 && Day_count < 2 && Value_Contract_count < 2 && KM_count < 2 && Days_count < 2 && Hmonth_Create < 2 && Gmonth_Create < 2)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}
            string concate_DropDown = "";
            if (Branch_count > 1) concate_DropDown = concate_DropDown + "0";
            if (Day_Create_count > 1) concate_DropDown = concate_DropDown + "1";
            if (Time_Create_count > 1) concate_DropDown = concate_DropDown + "2";
            if (Day_count > 1) concate_DropDown = concate_DropDown + "3";
            if (Value_Contract_count > 1) concate_DropDown = concate_DropDown + "4";
            if (KM_count > 1) concate_DropDown = concate_DropDown + "5";
            //if (Days_count > 1) concate_DropDown = concate_DropDown + "6";
            if (Hmonth_Create > 0) concate_DropDown = concate_DropDown + "7";
            if (Gmonth_Create > 0) concate_DropDown = concate_DropDown + "8";
            ViewBag.concate_DropDown = concate_DropDown;



            var AllBranches = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor, new[] { "CrCasRenterContractStatistics" }).Where(x => x.CrCasRenterContractStatistics?.CrCasBranchInformationStatus != Status.Deleted).DistinctBy(x => x.CrCasRenterContractStatisticsBranch).ToList();


            List<ChartBranchDataVM> chartBranchDataVMs = new List<ChartBranchDataVM>();
            var count_Renters = 0;
            foreach (var branch in AllBranches)
            {
                var BranchCount = 0;
                BranchCount = AllContracts.Count(x=>x.CrCasRenterContractStatisticsBranch == branch.CrCasRenterContractStatistics?.CrCasBranchInformationCode);
                ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                chartBranchDataVM.ArName = branch.CrCasRenterContractStatistics?.CrCasBranchInformationArShortName;
                chartBranchDataVM.EnName = branch.CrCasRenterContractStatistics?.CrCasBranchInformationEnShortName;
                chartBranchDataVM.Code = branch.CrCasRenterContractStatistics?.CrCasBranchInformationCode;
                chartBranchDataVM.Value = BranchCount;
                chartBranchDataVMs.Add(chartBranchDataVM);
                count_Renters = BranchCount + count_Renters;

            }
            chartBranchDataVMs = chartBranchDataVMs.OrderByDescending(x => x.Value).ToList();
            var Type_Avarage = chartBranchDataVMs.Average(x => x.Value);
            var Type_Sum = chartBranchDataVMs.Sum(x => x.Value);
            var Type_Count = chartBranchDataVMs.Count();
            var Type_Avarage_percentage = Type_Avarage/Type_Sum;
            var Static_Percentage_rate = 0.10;

            ViewBag.count_Renters = count_Renters;
            var max_Colomns = 15;
            var max = chartBranchDataVMs.Max(x => x.Value);
            var max1 = (int)max;
            ChartBranchDataVM other = new ChartBranchDataVM();
            other.Value = 0;
            other.ArName = "أخرى  ";
            other.EnName = "  Others";
            other.Code = "Aa";

            List<ChartBranchDataVM> chartBranchDataVMs_2 = new List<ChartBranchDataVM>(chartBranchDataVMs);
            int counter_For_max_Colomn = 0;
            foreach (var branch_1 in chartBranchDataVMs_2)
            {
                counter_For_max_Colomn ++;
                if(counter_For_max_Colomn <= max_Colomns)
                {
                    branch_1.IsTrue = true;
                }
                else
                {
                    branch_1.IsTrue = false;
                }
                if ((int)branch_1.Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage) || (int)branch_1.Value <= max1 * (double)Type_Avarage_percentage)
                {
                    branch_1.IsTrue = false;
                }
            }
            foreach (var branch_1 in chartBranchDataVMs_2)
            {
                if (branch_1.IsTrue == false)
                {
                    other.Value = branch_1.Value + other.Value;
                    chartBranchDataVMs.Remove(branch_1);
                }
            }
            if ((int)other.Value > 0)
            {
                chartBranchDataVMs.Add(other);
                int listCount = 0;
                listCount = chartBranchDataVMs.Count() - 1;
                chartBranchDataVMs_2.Insert(listCount, other);
                //chartBranchDataVMs_2.Add(other);
                
            }
            ViewBag.singleType =  "0";
            CasStatisticLayoutVM casStatisticLayoutVM = new CasStatisticLayoutVM();
            casStatisticLayoutVM.ChartBranchDataVM = chartBranchDataVMs;
            //casStatisticLayoutVM.ChartBranchDataVM_2ForAll = chartBranchDataVMs_2;
            casStatisticLayoutVM.ChartBranchDataVM_2ForAll = chartBranchDataVMs;

            await _userLoginsService.SaveTracing(currentCar.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            return View(casStatisticLayoutVM);
        }




        [HttpGet]
        public async Task<IActionResult> GetAllByType(string Type ,string listDrop ,string singleNo ,string startDate,string endDate)
        {

            if (listDrop == "" || listDrop == null)
            {
                return RedirectToAction("Index");
            }
            //sidebar Active
            ViewBag.id = "#sidebarReport";
            ViewBag.no = "14";
            ViewBag.concate_DropDown = listDrop;
            ViewBag.singleType = singleNo;
            ViewBag.StartDate = DateTime.Parse(startDate).Date.ToString("yyyy-MM-dd");
            ViewBag.EndDate = DateTime.Parse(endDate).Date.ToString("yyyy-MM-dd");
            var _max = DateTime.Parse(endDate).Date.AddDays(1);
            var _mini = DateTime.Parse(startDate).Date;

            var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205015", "2");

            var titles = await setTitle("205", "2205015", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllContracts = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor && x.CrCasRenterContractStatisticsDate < _max && x.CrCasRenterContractStatisticsDate >= _mini).ToList();

            CasStatisticLayoutVM casStatisticLayoutVM2 = new CasStatisticLayoutVM();

            if (AllContracts?.Count() < 1)
            {
                var nnow = DateTime.Now.Date;
                //ViewBag.StartDate = nnow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                //ViewBag.EndDate = nnow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ViewBag.concate_DropDown = listDrop;
                ViewBag.singleType = singleNo;
                ViewBag.count_Renters = 0;
                //return RedirectToAction("FailedMessageReport_NoData");
                return View(casStatisticLayoutVM2);
            }

            List<ChartBranchDataVM> chartBranchDataVMs = new List<ChartBranchDataVM>();
            var count_Renters = 0;

            if (Type == "Branch")
            {
                var AllBranches = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor, new[] { "CrCasRenterContractStatistics" }).Where(x => x.CrCasRenterContractStatistics?.CrCasBranchInformationStatus != Status.Deleted).DistinctBy(x => x.CrCasRenterContractStatisticsBranch).ToList();

                foreach (var single in AllBranches)
                {
                    var CategoryCount = 0;
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsBranch == single.CrCasRenterContractStatistics?.CrCasBranchInformationCode);
                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    chartBranchDataVM.ArName = single.CrCasRenterContractStatistics?.CrCasBranchInformationArShortName;
                    chartBranchDataVM.EnName = single.CrCasRenterContractStatistics?.CrCasBranchInformationEnShortName;
                    chartBranchDataVM.Code = single.CrCasRenterContractStatistics?.CrCasBranchInformationCode;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }
            if (Type == "Day_Create")
            {
                var AllContracts2_Day_Create = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsDayCreate).OrderBy(x => x.CrCasRenterContractStatisticsDayCreate).ToList();

                foreach (var single in AllContracts2_Day_Create)
                {
                    var CategoryCount = 0;
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsDayCreate == single.CrCasRenterContractStatisticsDayCreate);
                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    switch (single.CrCasRenterContractStatisticsDayCreate)
                    {
                        case "1":
                            chartBranchDataVM.ArName = "السبت";
                            chartBranchDataVM.EnName = "Saturday";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "الأحد";
                            chartBranchDataVM.EnName = "Sunday";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "الإثنين";
                            chartBranchDataVM.EnName = "Monday";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "الثلاثاء";
                            chartBranchDataVM.EnName = "Tuesday";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "الأربعاء";
                            chartBranchDataVM.EnName = "Wednesday";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "الخميس";
                            chartBranchDataVM.EnName = "Thursday";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "الجمعة";
                            chartBranchDataVM.EnName = "Friday";
                            break;
                    }

                    chartBranchDataVM.Code = single.CrCasRenterContractStatisticsDayCreate;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }


            if (Type == "Time_Create")
            {
                //var AllContracts2_Time_Create = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor ).DistinctBy(x => x.CrCasRenterContractStatisticsTimeCreate).OrderBy(x => x.CrCasRenterContractStatisticsTimeCreate).ToList();

                //foreach (var single in AllContracts2_Time_Create)
                for (int single = 1; single < 9; single++)

                {
                    var CategoryCount = 0;
                    //CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsTimeCreate == single.CrCasRenterContractStatisticsTimeCreate);
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsTimeCreate == single.ToString());

                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    //switch (single.CrCasRenterContractStatisticsTimeCreate)
                    switch (single.ToString())

                    {
                        case "1":
                            chartBranchDataVM.ArName = "00:00 - 02:59";
                            chartBranchDataVM.EnName = "00:00 - 02:59";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "03:00 - 05:59";
                            chartBranchDataVM.EnName = "03:00 - 05:59";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "06:00 - 08:59";
                            chartBranchDataVM.EnName = "06:00 - 08:59";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "09:00 - 11:59";
                            chartBranchDataVM.EnName = "09:00 - 11:59";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "12:00 - 14:59";
                            chartBranchDataVM.EnName = "12:00 - 14:59";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "15:00 - 17:59";
                            chartBranchDataVM.EnName = "15:00 - 17:59";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "18:00 - 20:59";
                            chartBranchDataVM.EnName = "18:00 - 20:59";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "21:00 - 23:59";
                            chartBranchDataVM.EnName = "21:00 - 23:59";
                            break;
                    }

                    chartBranchDataVM.Code = single.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }


            if (Type == "Day_Count")
            {
                //var AllContracts2_Day_Count = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsDayCount).OrderBy(x => x.CrCasRenterContractStatisticsDayCount).ToList();

                //foreach (var single in AllContracts2_Day_Count)
                for (int single = 1; single < 9; single++)
                {
                    var CategoryCount = 0;
                    //CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsDayCount == single.CrCasRenterContractStatisticsDayCount);
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsDayCount == single.ToString());
                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    //switch (single.CrCasRenterContractStatisticsDayCount)
                    switch (single.ToString())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "1 - 3";
                            chartBranchDataVM.EnName = "1 - 3";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "4 - 7";
                            chartBranchDataVM.EnName = "4 - 7";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "8 - 10";
                            chartBranchDataVM.EnName = "8 - 10";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "11 - 15";
                            chartBranchDataVM.EnName = "11 - 15";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "16 - 20";
                            chartBranchDataVM.EnName = "16 - 20";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "21 - 25";
                            chartBranchDataVM.EnName = "21 - 25";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "26 - 30";
                            chartBranchDataVM.EnName = "26 - 30";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "أكثر من 30";
                            chartBranchDataVM.EnName = "More Than 30";
                            break;
                    }

                    //chartBranchDataVM.Code = single.CrCasRenterContractStatisticsDayCount;
                    chartBranchDataVM.Code = single.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }
        
            
            
            if (Type == "Value_No")
            {
                var AllContracts2_Contract_Value = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor ).DistinctBy(x => x.CrCasRenterContractStatisticsValueNo).OrderBy(x => x.CrCasRenterContractStatisticsValueNo).ToList();

                foreach (var single in AllContracts2_Contract_Value)
                {
                    var CategoryCount = 0;
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsValueNo == single.CrCasRenterContractStatisticsValueNo);
                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    switch (single.CrCasRenterContractStatisticsValueNo)
                    {
                        case "1":
                            chartBranchDataVM.ArName = "أقل من 300";
                            chartBranchDataVM.EnName = "Less Than 300";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "301 - 500";
                            chartBranchDataVM.EnName = "301 - 500";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "501 - 1000";
                            chartBranchDataVM.EnName = "501 - 1000";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "1001 - 1500";
                            chartBranchDataVM.EnName = "1001 - 1500";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "1501 - 2000";
                            chartBranchDataVM.EnName = "1501 - 2000";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "2001 - 3000";
                            chartBranchDataVM.EnName = "2001 - 3000";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "3001 - 4000";
                            chartBranchDataVM.EnName = "3001 - 4000";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "4001 - 5000";
                            chartBranchDataVM.EnName = "4001 - 5000";
                            break;
                        case "9":
                            chartBranchDataVM.ArName = "أكثر من 5000";
                            chartBranchDataVM.EnName = "More Than 5000";
                            break;
                    }

                    chartBranchDataVM.Code = single.CrCasRenterContractStatisticsValueNo;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }
            
            
            if (Type == "KM")
            {
                //var AllContracts2_KM = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsKm).OrderBy(x => x.CrCasRenterContractStatisticsKm).ToList();

                //foreach (var single in AllContracts2_KM)
                for (int single = 1; single < 6; single++)
                {
                    var CategoryCount = 0;
                    //CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsKm == single.CrCasRenterContractStatisticsKm);
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsKm == single.ToString());
                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    //switch (single.CrCasRenterContractStatisticsKm)
                    switch (single.ToString())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "أقل من 100";
                            chartBranchDataVM.EnName = "Less Than 100";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "101 - 200";
                            chartBranchDataVM.EnName = "101 - 200";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "201 - 300";
                            chartBranchDataVM.EnName = "201 - 300";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "301 - 400";
                            chartBranchDataVM.EnName = "301 - 400";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "أكثر من 400";
                            chartBranchDataVM.EnName = "More Than 400";
                            break;
                    }

                    //chartBranchDataVM.Code = single.CrCasRenterContractStatisticsKm;
                    chartBranchDataVM.Code = single.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }


            if (Type == "Hmonth_Create")
            {
                var AllContracts2_Contract_Value = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsHmonthCreate).OrderBy(x => x.CrCasRenterContractStatisticsHmonthCreate).ToList();

                foreach (var single in AllContracts2_Contract_Value)
                {
                    var CategoryCount = 0;
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsHmonthCreate == single.CrCasRenterContractStatisticsHmonthCreate);
                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    switch (single.CrCasRenterContractStatisticsHmonthCreate?.Trim())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "مُحَرَّم";
                            chartBranchDataVM.EnName = "Muharram";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "صَفَر";
                            chartBranchDataVM.EnName = "Safar";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "ربيع الأول";
                            chartBranchDataVM.EnName = "Rabi Al-Awwal";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "ربيع الآخر";
                            chartBranchDataVM.EnName = "Rabi Al-Akhar";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "جُمادى الأولى";
                            chartBranchDataVM.EnName = "Jumada Al-Awwal";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "جُمادى الآخرة";
                            chartBranchDataVM.EnName = "Jumada Al-Akhirah";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "رجب";
                            chartBranchDataVM.EnName = "Rajab";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "شعبان";
                            chartBranchDataVM.EnName = "Shaban";
                            break;
                        case "9":
                            chartBranchDataVM.ArName = "رمضان";
                            chartBranchDataVM.EnName = "Ramadan";
                            break;
                        case "10":
                            chartBranchDataVM.ArName = "شوّال";
                            chartBranchDataVM.EnName = "Shawwal";
                            break;
                        case "11":
                            chartBranchDataVM.ArName = "ذو القعدة";
                            chartBranchDataVM.EnName = "Dhul Qadah";
                            break;
                        case "12":
                            chartBranchDataVM.ArName = "ذو الحجة";
                            chartBranchDataVM.EnName = "Dhul Hijjah";
                            break;
                    }

                    chartBranchDataVM.Code = single.CrCasRenterContractStatisticsHmonthCreate;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }


            if (Type == "Gmonth_Create")
            {
                var AllContracts2_Contract_Value = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsGmonthCreate).OrderBy(x => x.CrCasRenterContractStatisticsGmonthCreate).ToList();

                foreach (var single in AllContracts2_Contract_Value)
                {
                    var CategoryCount = 0;
                    CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsGmonthCreate == single.CrCasRenterContractStatisticsGmonthCreate);
                    ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
                    switch (single.CrCasRenterContractStatisticsGmonthCreate?.Trim())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "يناير";
                            chartBranchDataVM.EnName = "January";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "فبراير";
                            chartBranchDataVM.EnName = "February";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "مارس";
                            chartBranchDataVM.EnName = "March";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "أبريل";
                            chartBranchDataVM.EnName = "April";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "مايو";
                            chartBranchDataVM.EnName = "May";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "يونيو";
                            chartBranchDataVM.EnName = "June";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "يوليو";
                            chartBranchDataVM.EnName = "July";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "أغسطس";
                            chartBranchDataVM.EnName = "August";
                            break;
                        case "9":
                            chartBranchDataVM.ArName = "سبتمبر";
                            chartBranchDataVM.EnName = "September";
                            break;
                        case "10":
                            chartBranchDataVM.ArName = "أكتوبر";
                            chartBranchDataVM.EnName = "October";
                            break;
                        case "11":
                            chartBranchDataVM.ArName = "نوفمبر";
                            chartBranchDataVM.EnName = "November";
                            break;
                        case "12":
                            chartBranchDataVM.ArName = "ديسمبر";
                            chartBranchDataVM.EnName = "December";
                            break;
                    }

                    chartBranchDataVM.Code = single.CrCasRenterContractStatisticsGmonthCreate;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVMs.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                ViewBag.count_Renters = count_Renters;
            }
            //if (Type == "KM")
            //{
            //    var AllRenters2_Age = _unitOfWork.CrCasRenterContractStatistic.FindAll(x => currentCar.CrMasUserInformationLessor == x.CrCasRenterContractStatisticsLessor).DistinctBy(x => x.CrCasRenterContractStatisticsAgeNo).ToList();

            //    foreach (var single in AllRenters2_Age)
            //    {
            //        var CategoryCount = 0;
            //        CategoryCount = AllContracts.Count(x => x.CrCasRenterContractStatisticsAgeNo == single.CrCasRenterContractStatisticsAgeNo);
            //        ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
            //        switch (single.CrCasRenterContractStatisticsAgeNo)
            //        {
            //            case "1":
            //                chartBranchDataVM.ArName = "أقل من 20";
            //                chartBranchDataVM.EnName = "Less Than 20";
            //                break;
            //            case "2":
            //                chartBranchDataVM.ArName = "21 - 30";
            //                chartBranchDataVM.EnName = "21 - 30";
            //                break;
            //            case "3":
            //                chartBranchDataVM.ArName = "31 - 40";
            //                chartBranchDataVM.EnName = "31 - 40";
            //                break;
            //            case "4":
            //                chartBranchDataVM.ArName = "41 - 50";
            //                chartBranchDataVM.EnName = "41 - 50";
            //                break;
            //            case "5":
            //                chartBranchDataVM.ArName = "51 - 60";
            //                chartBranchDataVM.EnName = "51 - 60";
            //                break;
            //            case "6":
            //                chartBranchDataVM.ArName = "أكثر من 60";
            //                chartBranchDataVM.EnName = "More Than 60";
            //                break;
            //            default:
            //                chartBranchDataVM.ArName = "أقل من 20";
            //                chartBranchDataVM.EnName = "Less Than 20";
            //                break;
            //        }

            //        chartBranchDataVM.Code = single.CrCasRenterContractStatisticsAgeNo;
            //        chartBranchDataVM.Value = CategoryCount;
            //        chartBranchDataVMs.Add(chartBranchDataVM);
            //        count_Renters = CategoryCount + count_Renters;
            //    }
            //    ViewBag.count_Renters = count_Renters;
            //}

            //condition For Stay with this order Not Value
            if (Type == "Day_Create" || Type == "Time_Create" || Type == "Day_Count" || Type == "Value_No" || Type == "KM" || Type == "Hmonth_Create" || Type == "Gmonth_Create")
            {
                chartBranchDataVMs = chartBranchDataVMs.ToList();
            }
            else
            {
                chartBranchDataVMs = chartBranchDataVMs.OrderByDescending(x => x.Value).ToList();
            }
            var Type_Avarage = chartBranchDataVMs.Average(x => x.Value);
            var Type_Sum = chartBranchDataVMs.Sum(x => x.Value);
            var Type_Count = chartBranchDataVMs.Count();
            var Type_Avarage_percentage = Type_Avarage / Type_Sum;
            var Static_Percentage_rate = 0.10;

            //ViewBag.count_Renters = count_Renters;
            var max_Colomns = 15;
            var max = chartBranchDataVMs.Max(x => x.Value);
            var max1 = (int)max;
            ChartBranchDataVM other = new ChartBranchDataVM();
            other.Value = 0;
            other.ArName = "أخرى  ";
            other.EnName = "  Others";
            other.Code = "Aa";

            List<ChartBranchDataVM> chartBranchDataVMs_2 = new List<ChartBranchDataVM>(chartBranchDataVMs);
            int counter_For_max_Colomn = 0;

            foreach (var branch_1 in chartBranchDataVMs_2)
            {
                counter_For_max_Colomn++;
                if (counter_For_max_Colomn <= max_Colomns)
                {
                    branch_1.IsTrue = true;
                }
                else
                {
                    branch_1.IsTrue = false;
                }

                if (Type == "Day_Create" || Type == "Time_Create" || Type == "Day_Count" || Type == "Value_No" || Type == "KM" || Type == "Hmonth_Create" || Type == "Gmonth_Create")
                {
                    branch_1.IsTrue = true;
                }
                else if (chartBranchDataVMs_2.Count() > 5)
                {
                    if ((int)branch_1.Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage) || (int)branch_1.Value <= max1 * (double)Type_Avarage_percentage)
                    {
                        branch_1.IsTrue = false;
                    }
                }

            }
            foreach (var branch_1 in chartBranchDataVMs_2)
            {
                if (branch_1.IsTrue == false)
                {
                    other.Value = branch_1.Value + other.Value;
                    chartBranchDataVMs.Remove(branch_1);
                }
            }
            if ((int)other.Value > 0)
            {
                chartBranchDataVMs.Add(other);
                int listCount = 0;
                listCount = chartBranchDataVMs.Count() - 1;
                chartBranchDataVMs_2.Insert(listCount, other);
                //chartBranchDataVMs_2.Add(other);
            }

            CasStatisticLayoutVM casStatisticLayoutVM = new CasStatisticLayoutVM();
            casStatisticLayoutVM.ChartBranchDataVM = chartBranchDataVMs;
            //casStatisticLayoutVM.ChartBranchDataVM_2ForAll = chartBranchDataVMs_2;
            casStatisticLayoutVM.ChartBranchDataVM_2ForAll = chartBranchDataVMs;

            return View(casStatisticLayoutVM);
        }


    public async Task<IActionResult> FailedMessageReport_NoData()
    {
        //sidebar Active
        ViewBag.id = "#sidebarReport";
        ViewBag.no = "14";
        var titles = await setTitle("205", "2205015", "2");
        await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
        var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205015", "2");
        ViewBag.Data = "0";
        return View();

    }
}
}

