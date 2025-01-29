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
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bnan.Ui.Areas.MAS.Controllers.MASStatistics
{

    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class RenterContractStatisticsController : BaseController
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
        private readonly IStringLocalizer<RenterContractStatisticsController> _localizer;
        private readonly string pageNumber = SubTasks.MasStatistics_RenterContracts;



        public RenterContractStatisticsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterContractStatisticsController> localizer) : base(userManager, unitOfWork, mapper)
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
            MasStatistics_ContractsVM MasStatistics_ContractsVM = new MasStatistics_ContractsVM();

            //var Most_Frequance_Company_list = _unitOfWork.CrCasRenterContractStatistic.GetAll()
            //                        .GroupBy(q => q.CrCasRenterContractStatisticCode)
            //                        .OrderByDescending(gp => gp.Count());

            var listmaxDate = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
            predicate: null,
            selectProjection: query => query.Select(x => new Date_ReportClosedContract_MAS_VM
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

            MasStatistics_ContractsVM.start_Date = start_Date.AddDays(1).ToString("yyyy-MM-dd");
            MasStatistics_ContractsVM.end_Date = end_Date.ToString("yyyy-MM-dd");

              var all_Contract_Days = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new MAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
              }));
            all_Contract_Days = all_Contract_Days.DistinctBy(x => x.Contract_Code).ToList();
            var count_Contracts = all_Contract_Days.Count;

            MasStatistics_ContractsVM.Contracts_Count = count_Contracts;
            MasStatistics_ContractsVM.thisFunctionRunned = "NationalityChart";


            return View(MasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Nationality(string start, string end)
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
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

                var all_Contract_Nationalitys = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new MAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsNationalities,
              }));

                all_Contract_Nationalitys = all_Contract_Nationalitys.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Nationalitys.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Nationalitys.Count;

                if (all_Contract_Nationalitys.Count == 0)
                {
                    List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listMaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Nationality = await _unitOfWork.CrMasSupRenterNationality.FindAllWithSelectAsNoTrackingAsync(
                              predicate: x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted,
                              selectProjection: query => query.Select(x => new list_String_4
                              {
                                  id_key = x.CrMasSupRenterNationalitiesCode,
                                  nameAr = x.CrMasSupRenterNationalitiesArName,
                                  nameEn = x.CrMasSupRenterNationalitiesEnName,
                              }));


                List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
                var count_Renters = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Nationalitys.Count(x => x.Type_Id == single.Type_Id);
                    var thisNationality = all_names_Nationality.Find(x => x.id_key == single.Type_Id);
                    MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisNationality?.nameAr;
                    chartBranchDataVM.EnName = thisNationality?.nameEn;
                    chartBranchDataVM.Code = thisNationality?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    listMaschartBranchDataVM.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
                ViewBag.count_Renters = count_Renters;


                // pass --> 1  if no Other --> 2 if were other
                // // // for make other colomn based on average percentage

                MASChartBranchDataVM other = new MASChartBranchDataVM();
                other.Value = 0;
                other.ArName = "أخرى  ";
                other.EnName = "  Others";
                other.Code = "Aa";

                var Type_Avarage = listMaschartBranchDataVM.Average(x => x.Value);
                var Type_Sum = listMaschartBranchDataVM.Sum(x => x.Value);
                var Type_Count = listMaschartBranchDataVM.Count();
                var Type_Avarage_percentage = Type_Avarage / Type_Sum;
                var Static_Percentage_rate = 0.10;

                var max = listMaschartBranchDataVM.Max(x => x.Value);
                var max1 = (int)max;

                List<MASChartBranchDataVM>? listMaschartBranchDataVM2 = new List<MASChartBranchDataVM>();
                var x = true;
                for (var i = 0; i < listMaschartBranchDataVM.Count; i++)
                {

                    if ((int)listMaschartBranchDataVM[i].Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage))
                    {
                        listMaschartBranchDataVM[i].IsTrue = false;
                        x = false;
                        listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(i).ToList();
                        other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                        listMaschartBranchDataVM2.Add(other);
                        break;
                    }
                }
                if (listMaschartBranchDataVM2.Count > 14)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                    other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                }
                if (listMaschartBranchDataVM2.Count == 0)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM;
                }
                // till here //  //  //  //

                List<string> colorBackGround = new List<string>()
            {
                "rgba(255, 99, 132, 0.6)","rgba(54, 162, 235, 0.6)","rgba(75, 192, 192, 0.6)","rgba(255, 206, 86, 0.6)",
                "rgba(153, 102, 255, 0.6)","#F1F2F3","#FFCC99","#B3C2E5","#B4E4C8","#FF999B","#CCCCCC","#DAA1F7",
                "rgba(255, 99, 132, 0.6)","rgba(54, 162, 235, 0.6)","rgba(75, 192, 192, 0.6)","rgba(255, 206, 86, 0.6)",
            };
                List<string> colorBorder = new List<string>()
            {
                "rgba(255, 99, 132, 1)","rgba(54, 162, 235, 1)","rgba(75, 192, 192, 1)","rgba(255, 206, 86, 1)",
                "rgba(153, 102, 255, 1)","#C9CBCF","#FF9F40","#4B6EC0","#62C88C","#FF1515","#8C8C8C","#A413EC",
                "rgba(255, 99, 132, 1)","rgba(54, 162, 235, 1)","rgba(75, 192, 192, 1)","rgba(255, 206, 86, 1)",
            };
                for (var v = 0; v < listMaschartBranchDataVM2.Count; v++)
                {
                    listMaschartBranchDataVM2[v].backgroundColor = colorBackGround[v];
                    listMaschartBranchDataVM2[v].borderColor = colorBorder[v];
                }

                //ViewBag.singleType = "0";
                //ViewBag.singleType = concate_DropDown[0].ToString();
                MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
                //MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
                //MasStatistics_RentersVM.Renters_Count = count_Renters;
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();

                var response = new
                {
                    list_chartBranchDataVM = listMaschartBranchDataVM2,
                    count = count_Contracts,
                };


                return Json(response);
            }

            MasStatistics_ContractsVM MasStatistics_ContractsVM2 = new MasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = MasStatistics_ContractsVM2.listMasChartdataVM,
                count = 0,
            };


            return Json(response2);
            //return PartialView("_PartialMASChartData", MasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Profession(string start, string end)
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
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

                var all_Contract_Professions = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new MAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsJobs,
              }));

                all_Contract_Professions = all_Contract_Professions.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Professions.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Professions.Count;

                if (all_Contract_Professions.Count == 0)
                {
                    List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listMaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Profession = await _unitOfWork.CrMasSupRenterProfession.FindAllWithSelectAsNoTrackingAsync(
                              predicate: x => x.CrMasSupRenterProfessionsStatus != Status.Deleted,
                              selectProjection: query => query.Select(x => new list_String_4
                              {
                                  id_key = x.CrMasSupRenterProfessionsCode,
                                  nameAr = x.CrMasSupRenterProfessionsArName,
                                  nameEn = x.CrMasSupRenterProfessionsEnName,
                              }));


                List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
                var count_Renters = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Professions.Count(x => x.Type_Id == single.Type_Id);
                    var thisProfession = all_names_Profession.Find(x => x.id_key == single.Type_Id);
                    MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisProfession?.nameAr;
                    chartBranchDataVM.EnName = thisProfession?.nameEn;
                    chartBranchDataVM.Code = thisProfession?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    listMaschartBranchDataVM.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
                ViewBag.count_Renters = count_Renters;


                // pass --> 1  if no Other --> 2 if were other
                // // // for make other colomn based on average percentage

                MASChartBranchDataVM other = new MASChartBranchDataVM();
                other.Value = 0;
                other.ArName = "أخرى  ";
                other.EnName = "  Others";
                other.Code = "Aa";

                var Type_Avarage = listMaschartBranchDataVM.Average(x => x.Value);
                var Type_Sum = listMaschartBranchDataVM.Sum(x => x.Value);
                var Type_Count = listMaschartBranchDataVM.Count();
                var Type_Avarage_percentage = Type_Avarage / Type_Sum;
                var Static_Percentage_rate = 0.10;

                var max = listMaschartBranchDataVM.Max(x => x.Value);
                var max1 = (int)max;

                List<MASChartBranchDataVM>? listMaschartBranchDataVM2 = new List<MASChartBranchDataVM>();
                var x = true;
                for (var i = 0; i < listMaschartBranchDataVM.Count; i++)
                {

                    if ((int)listMaschartBranchDataVM[i].Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage))
                    {
                        listMaschartBranchDataVM[i].IsTrue = false;
                        x = false;
                        listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(i).ToList();
                        other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                        listMaschartBranchDataVM2.Add(other);
                        break;
                    }
                }
                if (listMaschartBranchDataVM2.Count > 14)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                    other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                }
                if (listMaschartBranchDataVM2.Count == 0)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM;
                }
                // till here //  //  //  //

                List<string> colorBackGround = new List<string>()
            {
                "rgba(255, 99, 132, 0.6)","rgba(54, 162, 235, 0.6)","rgba(75, 192, 192, 0.6)","rgba(255, 206, 86, 0.6)",
                "rgba(153, 102, 255, 0.6)","#F1F2F3","#FFCC99","#B3C2E5","#B4E4C8","#FF999B","#CCCCCC","#DAA1F7",
                "rgba(255, 99, 132, 0.6)","rgba(54, 162, 235, 0.6)","rgba(75, 192, 192, 0.6)","rgba(255, 206, 86, 0.6)",
            };
                List<string> colorBorder = new List<string>()
            {
                "rgba(255, 99, 132, 1)","rgba(54, 162, 235, 1)","rgba(75, 192, 192, 1)","rgba(255, 206, 86, 1)",
                "rgba(153, 102, 255, 1)","#C9CBCF","#FF9F40","#4B6EC0","#62C88C","#FF1515","#8C8C8C","#A413EC",
                "rgba(255, 99, 132, 1)","rgba(54, 162, 235, 1)","rgba(75, 192, 192, 1)","rgba(255, 206, 86, 1)",
            };
                for (var v = 0; v < listMaschartBranchDataVM2.Count; v++)
                {
                    listMaschartBranchDataVM2[v].backgroundColor = colorBackGround[v];
                    listMaschartBranchDataVM2[v].borderColor = colorBorder[v];
                }

                //ViewBag.singleType = "0";
                //ViewBag.singleType = concate_DropDown[0].ToString();
                MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
                //MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
                //MasStatistics_RentersVM.Renters_Count = count_Renters;
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();

                var response = new
                {
                    list_chartBranchDataVM = listMaschartBranchDataVM2,
                    count = count_Contracts,
                };


                return Json(response);
            }

            MasStatistics_ContractsVM MasStatistics_ContractsVM2 = new MasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = MasStatistics_ContractsVM2.listMasChartdataVM,
                count = 0,
            };


            return Json(response2);
            //return PartialView("_PartialMASChartData", MasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Region(string start, string end)
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
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

                var all_Contract_Regions = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new MAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsRenterRegions,
              }));

                all_Contract_Regions = all_Contract_Regions.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Regions.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Regions.Count;

                if (all_Contract_Regions.Count == 0)
                {
                    List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listMaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Region = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
                    predicate: null,
                    selectProjection: query => query.Select(x => new list_String_4
                    {
                        id_key = x.CrMasSupPostRegionsCode,
                        nameAr = x.CrMasSupPostRegionsArName,
                        nameEn = x.CrMasSupPostRegionsEnName,
                    }));


                List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
                var count_Renters = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Regions.Count(x => x.Type_Id == single.Type_Id);
                    var thisRegion = all_names_Region.Find(x => x.id_key == single.Type_Id);
                    MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisRegion?.nameAr;
                    chartBranchDataVM.EnName = thisRegion?.nameEn;
                    chartBranchDataVM.Code = thisRegion?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    listMaschartBranchDataVM.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
                ViewBag.count_Renters = count_Renters;


                // pass --> 1  if no Other --> 2 if were other
                // // // for make other colomn based on average percentage

                MASChartBranchDataVM other = new MASChartBranchDataVM();
                other.Value = 0;
                other.ArName = "أخرى  ";
                other.EnName = "  Others";
                other.Code = "Aa";

                var Type_Avarage = listMaschartBranchDataVM.Average(x => x.Value);
                var Type_Sum = listMaschartBranchDataVM.Sum(x => x.Value);
                var Type_Count = listMaschartBranchDataVM.Count();
                var Type_Avarage_percentage = Type_Avarage / Type_Sum;
                var Static_Percentage_rate = 0.10;

                var max = listMaschartBranchDataVM.Max(x => x.Value);
                var max1 = (int)max;

                List<MASChartBranchDataVM>? listMaschartBranchDataVM2 = new List<MASChartBranchDataVM>();
                var x = true;
                for (var i = 0; i < listMaschartBranchDataVM.Count; i++)
                {

                    if ((int)listMaschartBranchDataVM[i].Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage))
                    {
                        listMaschartBranchDataVM[i].IsTrue = false;
                        x = false;
                        listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(i).ToList();
                        other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                        listMaschartBranchDataVM2.Add(other);
                        break;
                    }
                }
                if (listMaschartBranchDataVM2.Count > 14)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                    other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                }
                if (listMaschartBranchDataVM2.Count == 0)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM;
                }
                // till here //  //  //  //

                MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
                //MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
                //MasStatistics_RentersVM.Renters_Count = count_Renters;

                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();

                var response = new
                {
                    list_chartBranchDataVM = listMaschartBranchDataVM2,
                    count = count_Contracts,
                };


                return Json(response);
            }

            MasStatistics_ContractsVM MasStatistics_ContractsVM2 = new MasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = MasStatistics_ContractsVM2.listMasChartdataVM,
                count = 0,
            };


            return Json(response2);
            //return PartialView("_PartialMASChartData", MasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_City(string start, string end)
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
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

                var all_Contract_Citys = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new MAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsRenterCity,
              }));

                all_Contract_Citys = all_Contract_Citys.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Citys.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Citys.Count;

                if (all_Contract_Citys.Count == 0)
                {
                    List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listMaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_City = await _unitOfWork.CrMasSupPostCity.FindAllWithSelectAsNoTrackingAsync(
                    predicate: null,
                    selectProjection: query => query.Select(x => new list_String_4
                    {
                        id_key = x.CrMasSupPostCityCode,
                        nameAr = x.CrMasSupPostCityConcatenateArName,
                        nameEn = x.CrMasSupPostCityConcatenateEnName,
                    }));


                List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
                var count_Renters = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Citys.Count(x => x.Type_Id == single.Type_Id);
                    var thisCity = all_names_City.Find(x => x.id_key == single.Type_Id);
                    MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisCity?.nameAr;
                    chartBranchDataVM.EnName = thisCity?.nameEn;
                    chartBranchDataVM.Code = thisCity?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    listMaschartBranchDataVM.Add(chartBranchDataVM);
                    count_Renters = CategoryCount + count_Renters;
                }
                listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
                ViewBag.count_Renters = count_Renters;


                // pass --> 1  if no Other --> 2 if were other
                // // // for make other colomn based on average percentage

                MASChartBranchDataVM other = new MASChartBranchDataVM();
                other.Value = 0;
                other.ArName = "أخرى  ";
                other.EnName = "  Others";
                other.Code = "Aa";

                var Type_Avarage = listMaschartBranchDataVM.Average(x => x.Value);
                var Type_Sum = listMaschartBranchDataVM.Sum(x => x.Value);
                var Type_Count = listMaschartBranchDataVM.Count();
                var Type_Avarage_percentage = Type_Avarage / Type_Sum;
                var Static_Percentage_rate = 0.10;

                var max = listMaschartBranchDataVM.Max(x => x.Value);
                var max1 = (int)max;

                List<MASChartBranchDataVM>? listMaschartBranchDataVM2 = new List<MASChartBranchDataVM>();
                var x = true;
                for (var i = 0; i < listMaschartBranchDataVM.Count; i++)
                {

                    if ((int)listMaschartBranchDataVM[i].Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage))
                    {
                        listMaschartBranchDataVM[i].IsTrue = false;
                        x = false;
                        listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(i).ToList();
                        other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                        listMaschartBranchDataVM2.Add(other);
                        break;
                    }
                }
                if (listMaschartBranchDataVM2.Count > 14)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                    other.Value = count_Renters - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                }
                if (listMaschartBranchDataVM2.Count == 0)
                {
                    listMaschartBranchDataVM2 = listMaschartBranchDataVM;
                }
                // till here //  //  //  //

                MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
                //MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
                //MasStatistics_RentersVM.Renters_Count = count_Renters;

                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();

                var response = new
                {
                    list_chartBranchDataVM = listMaschartBranchDataVM2,
                    count = count_Contracts,
                };


                return Json(response);
            }

            MasStatistics_ContractsVM MasStatistics_ContractsVM2 = new MasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = MasStatistics_ContractsVM2.listMasChartdataVM,
                count = 0,
            };


            return Json(response2);
            //return PartialView("_PartialMASChartData", MasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Age(string start, string end)
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
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

                var all_Contract_Age = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new MAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsAgeNo,
              }));

                all_Contract_Age = all_Contract_Age.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Age.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Age.Count;

                if (all_Contract_Age.Count == 0)
                {
                    List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listMaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
                var maxStatusSwitch = 8;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_Age.Count(x => x.Type_Id == i.ToString());

                    MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();
                    switch (i.ToString())
                    {
                        case "1":
                            chartBranchDataVM.ArName = "أقل من 20";
                            chartBranchDataVM.EnName = "Less Than 20";
                            break;
                        case "2":
                            chartBranchDataVM.ArName = "25 - 21";
                            chartBranchDataVM.EnName = "21 - 25";
                            break;
                        case "3":
                            chartBranchDataVM.ArName = "30 - 26";
                            chartBranchDataVM.EnName = "26 - 30";
                            break;
                        case "4":
                            chartBranchDataVM.ArName = "35 - 31";
                            chartBranchDataVM.EnName = "31 - 35";
                            break;
                        case "5":
                            chartBranchDataVM.ArName = "40 - 36";
                            chartBranchDataVM.EnName = "36 - 40";
                            break;
                        case "6":
                            chartBranchDataVM.ArName = "50 - 41";
                            chartBranchDataVM.EnName = "41 - 50";
                            break;
                        case "7":
                            chartBranchDataVM.ArName = "60 - 51";
                            chartBranchDataVM.EnName = "51 - 60";
                            break;
                        case "8":
                            chartBranchDataVM.ArName = "أكثر من 60";
                            chartBranchDataVM.EnName = "More Than 60";
                            break;
                    }

                    chartBranchDataVM.Code = i.ToString();
                    chartBranchDataVM.Value = CategoryCount;
                    listMaschartBranchDataVM.Add(chartBranchDataVM);
                }

                listMaschartBranchDataVM = listMaschartBranchDataVM.OrderBy(x => x.Code).ToList();
                ViewBag.count_Renters = count_Contracts;


                MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
                //MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
                //MasStatistics_RentersVM.Renters_Count = count_Renters;

                var response = new
                {
                    list_chartBranchDataVM = listMaschartBranchDataVM,
                    count = count_Contracts,
                };


                return Json(response);
            }

            MasStatistics_ContractsVM MasStatistics_ContractsVM2 = new MasStatistics_ContractsVM();
            var response2 = new
            {
                list_chartBranchDataVM = MasStatistics_ContractsVM2.listMasChartdataVM,
                count = 0,
            };


            return Json(response2);
            //return PartialView("_PartialMASChartData", MasStatistics_ContractsVM);
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

