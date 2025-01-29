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
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bnan.Ui.Areas.CAS.Controllers.CASStatistics
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CasStatistics_CarContractsController : BaseController
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
        private readonly IStringLocalizer<CasStatistics_CarContractsController> _localizer;
        private readonly string pageNumber = SubTasks.CasStatistics_CarContracts;



        public CasStatistics_CarContractsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CasStatistics_CarContractsController> localizer) : base(userManager, unitOfWork, mapper)
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
            predicate: x=> x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor,
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
            CasStatistics_ContractsVM.thisFunctionRunned = "brandChart";


            return View(CasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Brands(string start, string end)
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

                var all_Contract_Brands = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsBrand,
              }));

                all_Contract_Brands = all_Contract_Brands.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Brands.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Brands.Count;

                if (all_Contract_Brands.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Brand = await _unitOfWork.CrMasSupCarBrand.FindAllWithSelectAsNoTrackingAsync(
                              predicate: x => x.CrMasSupCarBrandStatus != Status.Deleted,
                              selectProjection: query => query.Select(x => new cas_list_String_4
                              {
                                  id_key = x.CrMasSupCarBrandCode,
                                  nameAr = x.CrMasSupCarBrandArName,
                                  nameEn = x.CrMasSupCarBrandEnName,
                              }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Brands.Count(x => x.Type_Id == single.Type_Id);
                    var thisBrand = all_names_Brand.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisBrand?.nameAr;
                    chartBranchDataVM.EnName = thisBrand?.nameEn;
                    chartBranchDataVM.Code = thisBrand?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisBrand == null)
                    {
                        CategoryCount = 0;
                    }
                    else
                    {
                        listCaschartBranchDataVM.Add(chartBranchDataVM);
                    }
                    count_Contracts = CategoryCount + count_Contracts;
                }
                listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
                ViewBag.count_Contracts = count_Contracts;


                // pass --> 1  if no Other --> 2 if were other
                // // // for make other colomn based on average percentage

                var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Contracts);

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
                for (var v = 0; v < listCaschartBranchDataVM2.Count; v++)
                {
                    listCaschartBranchDataVM2[v].backgroundColor = colorBackGround[v];
                    listCaschartBranchDataVM2[v].borderColor = colorBorder[v];
                }

                //ViewBag.singleType = "0";
                //ViewBag.singleType = concate_DropDown[0].ToString();
                CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();
                // pass --> 1  if no Other --> 2 if were other
                //CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
                //CasStatistics_CarsVM.count_Contracts = count_Contracts;

                listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();

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
            //return PartialView("_PartialCASChartData", CasStatistics_ContractsVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Models(string start, string end)
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

                var all_Contract_Models = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsModel,
              }));

                all_Contract_Models = all_Contract_Models.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Models.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Models.Count;

                if(all_Contract_Models.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Model = await _unitOfWork.CrMasSupCarModel.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupCarModelStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupCarModelCode,
                  nameAr = x.CrMasSupCarModelArConcatenateName,
                  nameEn = x.CrMasSupCarModelConcatenateEnName,
              }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Models.Count(x => x.Type_Id == single.Type_Id);
                    var thisModel = all_names_Model.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisModel?.nameAr;
                    chartBranchDataVM.EnName = thisModel?.nameEn;
                    chartBranchDataVM.Code = thisModel?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisModel == null)
                    {
                        CategoryCount = 0;
                    }
                    else
                    {
                        listCaschartBranchDataVM.Add(chartBranchDataVM);
                    }
                    count_Contracts = CategoryCount + count_Contracts;
                }
                listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
                ViewBag.count_Contracts = count_Contracts;


                // pass --> 1  if no Other --> 2 if were other
                // // // for make other colomn based on average percentage

                var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Contracts);

                List<string> colorBackGround = new List<string>()
            {
                "rgba(255, 99, 132, 0.6)","rgba(54, 162, 235, 0.6)","rgba(75, 192, 192, 0.6)","rgba(255, 206, 86, 0.6)",
                "rgba(153, 102, 255, 0.6)","#F552F3","#FFCC99","#B3C244","#22E4C8","#0F990B","#C0C9C","#1A31F0",
                 "#F1F2F3","#FFCC99","#B3C2E5","#B4E4C8","#FF999B","#CCCCCC","#DAA1F7",
                "rgba(255, 99, 132, 0.6)","rgba(54, 162, 235, 0.6)","rgba(75, 192, 192, 0.6)","rgba(255, 206, 86, 0.6)",
            };
                List<string> colorBorder = new List<string>()
            {
                "rgba(255, 99, 132, 1)","rgba(54, 162, 235, 1)","rgba(75, 192, 192, 1)","rgba(255, 206, 86, 1)",
                "rgba(153, 102, 255, 1)","#C9CBCF","#FF9F40","#4B6EC0","#62C88C","#FF1515","#8C8C8C","#A413EC",
                "rgba(255, 99, 132, 1)","rgba(54, 162, 235, 1)","rgba(75, 192, 192, 1)","rgba(255, 206, 86, 1)",
            };
                for (var v = 0; v < listCaschartBranchDataVM2.Count; v++)
                {
                    listCaschartBranchDataVM2[v].backgroundColor = colorBackGround[v];
                    listCaschartBranchDataVM2[v].borderColor = colorBorder[v];
                }

                CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();
                // pass --> 1  if no Other --> 2 if were other
                CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
                CasStatistics_CarsVM.count_Contracts = count_Contracts;

                listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();

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
            //return PartialView("_PartialCASChartData", CasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Categos(string start, string end)
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

                var all_Contract_Categos = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsCategory,
              }));

                all_Contract_Categos = all_Contract_Categos.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Categos.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Categos.Count;

                if (all_Contract_Categos.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Catego = await _unitOfWork.CrMasSupCarCategory.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupCarCategoryStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupCarCategoryCode,
                  nameAr = x.CrMasSupCarCategoryArName,
                  nameEn = x.CrMasSupCarCategoryEnName,
              }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Categos.Count(x => x.Type_Id == single.Type_Id);
                    var thisCatego = all_names_Catego.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisCatego?.nameAr;
                    chartBranchDataVM.EnName = thisCatego?.nameEn;
                    chartBranchDataVM.Code = thisCatego?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisCatego == null)
                    {
                        CategoryCount = 0;
                    }
                    else
                    {
                        listCaschartBranchDataVM.Add(chartBranchDataVM);
                    }
                    count_Contracts = CategoryCount + count_Contracts;
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
                CasStatistics_CarsVM.count_Contracts = count_Contracts;

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
            //return PartialView("_PartialCASChartData", CasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Years(string start, string end)
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

                var all_Contract_Years = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsCarYear,
              }));

                all_Contract_Years = all_Contract_Years.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Years.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Years.Count;

                if (all_Contract_Years.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Years.Count(x => x.Type_Id == single.Type_Id);
                    var thisCategory = single.Type_Id;
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisCategory;
                    chartBranchDataVM.EnName = thisCategory;
                    chartBranchDataVM.Code = thisCategory;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisCategory == null)
                    {
                        CategoryCount = 0;
                    }
                    else
                    {
                        listCaschartBranchDataVM.Add(chartBranchDataVM);
                    }
                    count_Contracts = CategoryCount + count_Contracts;
                }
                listCaschartBranchDataVM = listCaschartBranchDataVM.OrderBy(x => int.Parse(x.Code??"0")).ToList();
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
                CasStatistics_CarsVM.count_Contracts = count_Contracts;

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
            //return PartialView("_PartialCASChartData", CasStatistics_ContractsVM);
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

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {
            //await _userLoginsService.SaveTracing(currentContract.CrCasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrCasSysMainTasksCode,
            //subTask.CrCasSysSubTasksCode, mainTask.CrCasSysMainTasksArName, subTask.CrCasSysSubTasksArName, mainTask.CrCasSysMainTasksEnName,
            //subTask.CrCasSysSubTasksEnName, system.CrCasSysSystemCode, "بنان", "Bnan");

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

