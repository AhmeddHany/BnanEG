using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
//using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bnan.Ui.Areas.CAS.Controllers.CASStatistics
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CasStatistics_ContractsController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CasStatistics_ContractsController> _localizer;
        private readonly string pageNumber = SubTasks.CasStatistics_Contracts;



        public CasStatistics_ContractsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CasStatistics_ContractsController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
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
            CasStatistics_ContractsVM CasStatistics_ContractsVM = new CasStatistics_ContractsVM();

            //var Most_Frequance_Company_list = _unitOfWork.CrCasRenterContractStatistic.GetAll()
            //                        .GroupBy(q => q.CrCasRenterContractStatisticCode)
            //                        .OrderByDescending(gp => gp.Count());

            var listmaxDate = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor,
            selectProjection: query => query.Select(x => new Date_ReportClosedContract_CAS_VM
            {
                dates = x.CrCasRenterContractStatisticsDate,
            }));

            if (listmaxDate?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var maxDate = listmaxDate.Max(x => x.dates)?.ToString("yyyy-MM-dd");

            var end_Date = DateTime.Now;
            var start_Date = DateTime.Now.AddMonths(-1).AddDays(-1);
            if (maxDate != null)
            {
                end_Date = DateTime.Parse(maxDate);
                start_Date = DateTime.Parse(maxDate).AddMonths(-1).AddDays(-1);
            }

            CasStatistics_ContractsVM.start_Date = start_Date.AddDays(1).ToString("yyyy-MM-dd");
            CasStatistics_ContractsVM.end_Date = end_Date.ToString("yyyy-MM-dd");

              var all_Contract_Days = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
              }));
            all_Contract_Days = all_Contract_Days.DistinctBy(x => x.Contract_Code).ToList();
            var count_Contracts = all_Contract_Days.Count;

            CasStatistics_ContractsVM.Contracts_Count = count_Contracts;
            CasStatistics_ContractsVM.thisFunctionRunned = "branchChart";


            return View(CasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Days(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_Days = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsDayCreate,
              }));

                all_Contract_Days = all_Contract_Days.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Days.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Days.Count;


                List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
                var maxStatusSwitch = 7;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_Days.Count(x => x.Type_Id == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                    switch (i.ToString())
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

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    list_chartBranchDataVM.Add(chartBranchDataVM);
                }
                list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => int.Parse(x.Code ?? "0")).ToList();
                //if (CultureInfo.CurrentUICulture.Name == "en-US") { list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => x.Code).ToList(); }
                //else { list_chartBranchDataVM = list_chartBranchDataVM.OrderByDescending(x => x.Code).ToList(); }
                if (list_chartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(list_chartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = list_chartBranchDataVM,
                    count = count_Contracts,
                };


                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };


            return Json(response2);
            //return PartialView("_PartialCASChartData", CasStatistics_ContractsVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Time(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_Time = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsTimeCreate,
              }));

                all_Contract_Time = all_Contract_Time.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Time.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Time.Count;


                List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
                var maxStatusSwitch = 8;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_Time.Count(x => x.Type_Id == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                    switch (i.ToString())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "02:59 - 00:00";
                            chartBranchDataVM.EnName = "00:00 - 02:59";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "05:59 - 03:00";
                            chartBranchDataVM.EnName = "03:00 - 05:59";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "08:59 - 06:00";
                            chartBranchDataVM.EnName = "06:00 - 08:59";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "11:59 - 09:00";
                            chartBranchDataVM.EnName = "09:00 - 11:59";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "14:59 - 12:00";
                            chartBranchDataVM.EnName = "12:00 - 14:59";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "17:59 - 15:00";
                            chartBranchDataVM.EnName = "15:00 - 17:59";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "20:59 - 18:00";
                            chartBranchDataVM.EnName = "18:00 - 20:59";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "23:59 - 21:00";
                            chartBranchDataVM.EnName = "21:00 - 23:59";
                            break;
                    }

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    list_chartBranchDataVM.Add(chartBranchDataVM);
                }

                list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => int.Parse(x.Code ?? "0")).ToList();
                //if (CultureInfo.CurrentUICulture.Name == "en-US") { list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => x.Code).ToList(); }
                //else { list_chartBranchDataVM = list_chartBranchDataVM.OrderByDescending(x => x.Code).ToList(); }
                if (list_chartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(list_chartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = list_chartBranchDataVM,
                    count = count_Contracts,
                };


                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };


            return Json(response2);
            //return PartialView("_PartialCASChartData", CasStatistics_ContractsVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_ContractLimit(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_DayCount = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsDayCount,
              }));

                all_Contract_DayCount = all_Contract_DayCount.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_DayCount.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_DayCount.Count;


                List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
                var maxStatusSwitch = 8;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_DayCount.Count(x => x.Type_Id == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                    switch (i.ToString())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "3 - 1";
                            chartBranchDataVM.EnName = "1 - 3";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "7 - 4";
                            chartBranchDataVM.EnName = "4 - 7";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "10 - 8";
                            chartBranchDataVM.EnName = "8 - 10";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "15 - 11";
                            chartBranchDataVM.EnName = "11 - 15";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "20 - 16";
                            chartBranchDataVM.EnName = "16 - 20";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "25 - 21";
                            chartBranchDataVM.EnName = "21 - 25";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "30 - 26";
                            chartBranchDataVM.EnName = "26 - 30";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "أكثر من 30";
                            chartBranchDataVM.EnName = "More Than 30";
                            break;
                    }

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    list_chartBranchDataVM.Add(chartBranchDataVM);
                }

                list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => int.Parse(x.Code ?? "0")).ToList();
                //if (CultureInfo.CurrentUICulture.Name == "en-US") { list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => x.Code).ToList(); }
                //else { list_chartBranchDataVM = list_chartBranchDataVM.OrderByDescending(x => x.Code).ToList(); }

                if (list_chartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(list_chartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = list_chartBranchDataVM,
                    count = count_Contracts,
                };

                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };

        return Json(response2);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_HigritsMonths(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_HigritsMonths = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsHmonthCreate,
              }));

                all_Contract_HigritsMonths = all_Contract_HigritsMonths.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_HigritsMonths.DistinctBy(y => y.Type_Id?.Trim()).ToList();
                var count_Contracts = all_Contract_HigritsMonths.Count;


                List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
                var maxStatusSwitch = 12;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_HigritsMonths.Count(x => x.Type_Id?.Trim() == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                    switch (i.ToString())
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

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    list_chartBranchDataVM.Add(chartBranchDataVM);
                }

                list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x =>  int.Parse(x.Code??"0")).ToList();
                //if (CultureInfo.CurrentUICulture.Name == "en-US") { list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => x.Code).ToList(); }
                //else { list_chartBranchDataVM = list_chartBranchDataVM.OrderByDescending(x => x.Code).ToList(); }
                if (list_chartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(list_chartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = list_chartBranchDataVM,
                    count = count_Contracts,
                };

                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };

            return Json(response2);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_GregorianMonths(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_GregorianMonths = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsGmonthCreate,
              }));

                all_Contract_GregorianMonths = all_Contract_GregorianMonths.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_GregorianMonths.DistinctBy(y => y.Type_Id?.Trim()).ToList();
                var count_Contracts = all_Contract_GregorianMonths.Count;


                List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
                var maxStatusSwitch = 12;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_GregorianMonths.Count(x => x.Type_Id?.Trim() == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                    switch (i.ToString())
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

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    list_chartBranchDataVM.Add(chartBranchDataVM);
                }

                list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => int.Parse(x.Code ?? "0")).ToList();
                //if (CultureInfo.CurrentUICulture.Name == "en-US") { list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => x.Code).ToList(); }
                //else { list_chartBranchDataVM = list_chartBranchDataVM.OrderByDescending(x => x.Code).ToList(); }
                if (list_chartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(list_chartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = list_chartBranchDataVM,
                    count = count_Contracts,
                };

                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };

            return Json(response2);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_ValueNo(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_ValueNo = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsValueNo,
              }));

                all_Contract_ValueNo = all_Contract_ValueNo.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_ValueNo.DistinctBy(y => y.Type_Id?.Trim()).ToList();
                var count_Contracts = all_Contract_ValueNo.Count;


                List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
                var maxStatusSwitch = 9;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_ValueNo.Count(x => x.Type_Id?.Trim() == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                    switch (i.ToString())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "أقل من 300";
                            chartBranchDataVM.EnName = "Less Than 300";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "500 - 301";
                            chartBranchDataVM.EnName = "301 - 500";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "1000 - 501";
                            chartBranchDataVM.EnName = "501 - 1000";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "1500 - 1001";
                            chartBranchDataVM.EnName = "1001 - 1500";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "2000 - 1501";
                            chartBranchDataVM.EnName = "1501 - 2000";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "3000 - 2001";
                            chartBranchDataVM.EnName = "2001 - 3000";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "4000 - 3001";
                            chartBranchDataVM.EnName = "3001 - 4000";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "5000 - 4001";
                            chartBranchDataVM.EnName = "4001 - 5000";
                            break;
                        case "9":
                            chartBranchDataVM.ArName = "أكثر من 5000";
                            chartBranchDataVM.EnName = "More Than 5000";
                            break;
                    }

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    list_chartBranchDataVM.Add(chartBranchDataVM);
                }

                list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => int.Parse(x.Code ?? "0")).ToList();
                if (list_chartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(list_chartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = list_chartBranchDataVM,
                    count = count_Contracts,
                };

                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };

            return Json(response2);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_KM(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_KM = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsKm,
              }));

                all_Contract_KM = all_Contract_KM.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_KM.DistinctBy(y => y.Type_Id?.Trim()).ToList();
                var count_Contracts = all_Contract_KM.Count;


                List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
                var maxStatusSwitch = 6;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_KM.Count(x => x.Type_Id?.Trim() == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                    switch (i.ToString())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "أقل من 100";
                            chartBranchDataVM.EnName = "Less Than 100";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "200 - 101";
                            chartBranchDataVM.EnName = "101 - 200";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "300 - 201";
                            chartBranchDataVM.EnName = "201 - 300";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "400 - 301";
                            chartBranchDataVM.EnName = "301 - 400";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "500 - 401";
                            chartBranchDataVM.EnName = "401 - 500";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "أكثر من 500";
                            chartBranchDataVM.EnName = "More Than 500";
                            break;
                    }

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    list_chartBranchDataVM.Add(chartBranchDataVM);
                }

                list_chartBranchDataVM = list_chartBranchDataVM.OrderBy(x => int.Parse(x.Code ?? "0")).ToList();
                if (list_chartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(list_chartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = list_chartBranchDataVM,
                    count = count_Contracts,
                };

                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };

            return Json(response2);
        }

        public async Task<List<CASChartBranchDataVM>?> Cas_statistics_other(List<CASChartBranchDataVM> listCaschartBranchDataVM, int count_Contracts)
        {
            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on other

            CASChartBranchDataVM other = new CASChartBranchDataVM();
            other.Value = 0;
            other.ArName = "أخرى  ";
            other.EnName = "  Others";
            other.Code = "Aa";

            List<CASChartBranchDataVM>? listCaschartBranchDataVM2 = new List<CASChartBranchDataVM>();

            if (listCaschartBranchDataVM.Count < 11)
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM;
            }
            else
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM;

                if (listCaschartBranchDataVM.Count > 10)
                {
                    listCaschartBranchDataVM2 = listCaschartBranchDataVM.Take(9).ToList();
                    other.Value = count_Contracts - listCaschartBranchDataVM2.Sum(x => x.Value);
                    listCaschartBranchDataVM2.Add(other);
                }

            }
            // till here //  //  //  //


            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();
            if (listCaschartBranchDataVM2.Count > 0)
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            return listCaschartBranchDataVM2;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Branches(string start, string end)
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start).AddDays(-1);
                var end_Date = DateTime.Parse(end);

                var all_Contract_branch = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsBranch,
              }));
            all_Contract_branch = all_Contract_branch.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Contract_branch.DistinctBy(y => y.Type_Id).ToList();


            if (all_Contract_branch.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                return Json(listCaschartBranchDataVM4);
            }

            var all_names_branch = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasBranchInformationStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrCasBranchInformationCode,
                  nameAr = x.CrCasBranchInformationArShortName,
                  nameEn = x.CrCasBranchInformationEnShortName,
              }));


            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Contracts = 0;

            foreach (var single in all_Type)
            {
                var branchCount = 0;
                branchCount = all_Contract_branch.Count(x => x.Type_Id == single.Type_Id);
                var thisbranch = all_names_branch.Find(x => x.id_key == single.Type_Id);
                CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                chartBranchDataVM.ArName = thisbranch?.nameAr;
                chartBranchDataVM.EnName = thisbranch?.nameEn;
                chartBranchDataVM.Code = thisbranch?.id_key;
                chartBranchDataVM.Value = branchCount;
                chartBranchDataVM.IsTrue = true;
                if (thisbranch == null)
                {
                    branchCount = 0;
                }
                else
                {
                    listCaschartBranchDataVM.Add(chartBranchDataVM);
                }
                count_Contracts = branchCount + count_Contracts;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Contracts = count_Contracts;

            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Contracts);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();

            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            // pass --> 1  if no Other --> 2 if were other
            CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
            CasStatistics_CarsVM.Cars_Count = count_Contracts;
                if (listCaschartBranchDataVM2.Count > 0)
                {
                    count_Contracts = int.Parse(listCaschartBranchDataVM2.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM2,
                    count = count_Contracts,
                };

                return Json(response);
            }

            CasStatistics_ContractsVM CasStatistics_ContractsVM2 = new CasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = CasStatistics_ContractsVM2.listCasChartdataVM,
                count = 0,
            };

            return Json(response2);
        }

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {
            //await _userLoginsService.SaveTracing(currentContract.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            //subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            //subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, "بنان", "Bnan");

            var recordAr = licence.CrMasRenterInformationArName;
            var recordEn = licence.CrMasRenterInformationEnName;
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
            return RedirectToAction("Index", "ReportActiveContract");
        }
    }
}

