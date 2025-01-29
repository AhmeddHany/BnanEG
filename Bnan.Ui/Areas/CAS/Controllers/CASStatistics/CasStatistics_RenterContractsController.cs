using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
//using Bnan.Core.Interfaces.CAS;
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
    public class CasStatistics_RenterContractsController : BaseController
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
        private readonly IStringLocalizer<CasStatistics_RenterContractsController> _localizer;
        private readonly string pageNumber = SubTasks.CasStatistics_RenterContracts;



        public CasStatistics_RenterContractsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CasStatistics_RenterContractsController> localizer) : base(userManager, unitOfWork, mapper)
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
            CasStatistics_ContractsVM.thisFunctionRunned = "NationalityChart";


            return View(CasStatistics_ContractsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Nationality(string start, string end)
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

                var all_Contract_Nationalitys = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsNationalities,
              }));

                all_Contract_Nationalitys = all_Contract_Nationalitys.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Nationalitys.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Nationalitys.Count;

                if (all_Contract_Nationalitys.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Nationality = await _unitOfWork.CrMasSupRenterNationality.FindAllWithSelectAsNoTrackingAsync(
                              predicate: x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted,
                              selectProjection: query => query.Select(x => new cas_list_String_4
                              {
                                  id_key = x.CrMasSupRenterNationalitiesCode,
                                  nameAr = x.CrMasSupRenterNationalitiesArName,
                                  nameEn = x.CrMasSupRenterNationalitiesEnName,
                              }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Nationalitys.Count(x => x.Type_Id == single.Type_Id);
                    var thisNationality = all_names_Nationality.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisNationality?.nameAr;
                    chartBranchDataVM.EnName = thisNationality?.nameEn;
                    chartBranchDataVM.Code = thisNationality?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisNationality == null)
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

                CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
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
        public async Task<IActionResult> GetAllBy_Membership(string start, string end)
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

                var all_Contract_Memberships = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsMembership,
              }));

                all_Contract_Memberships = all_Contract_Memberships.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Memberships.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Memberships.Count;

                if (all_Contract_Memberships.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Membership = await _unitOfWork.CrMasSupRenterMembership.FindAllWithSelectAsNoTrackingAsync(
                              predicate: x => x.CrMasSupRenterMembershipStatus != Status.Deleted,
                              selectProjection: query => query.Select(x => new cas_list_String_4
                              {
                                  id_key = x.CrMasSupRenterMembershipCode,
                                  nameAr = x.CrMasSupRenterMembershipArName,
                                  nameEn = x.CrMasSupRenterMembershipEnName,
                              }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Memberships.Count(x => x.Type_Id == single.Type_Id);
                    var thisMembership = all_names_Membership.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisMembership?.nameAr;
                    chartBranchDataVM.EnName = thisMembership?.nameEn;
                    chartBranchDataVM.Code = thisMembership?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisMembership == null)
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

                CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
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
        public async Task<IActionResult> GetAllBy_Profession(string start, string end)
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

                var all_Contract_Professions = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsJobs,
              }));

                all_Contract_Professions = all_Contract_Professions.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Professions.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Professions.Count;

                if (all_Contract_Professions.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Profession = await _unitOfWork.CrMasSupRenterProfession.FindAllWithSelectAsNoTrackingAsync(
                              predicate: x => x.CrMasSupRenterProfessionsStatus != Status.Deleted,
                              selectProjection: query => query.Select(x => new cas_list_String_4
                              {
                                  id_key = x.CrMasSupRenterProfessionsCode,
                                  nameAr = x.CrMasSupRenterProfessionsArName,
                                  nameEn = x.CrMasSupRenterProfessionsEnName,
                              }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Professions.Count(x => x.Type_Id == single.Type_Id);
                    var thisProfession = all_names_Profession.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisProfession?.nameAr;
                    chartBranchDataVM.EnName = thisProfession?.nameEn;
                    chartBranchDataVM.Code = thisProfession?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisProfession == null)
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

                CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other

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
        public async Task<IActionResult> GetAllBy_Region(string start, string end)
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

                var all_Contract_Regions = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsRenterRegions,
              }));

                all_Contract_Regions = all_Contract_Regions.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Regions.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Regions.Count;

                if (all_Contract_Regions.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_Region = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
                    predicate: null,
                    selectProjection: query => query.Select(x => new cas_list_String_4
                    {
                        id_key = x.CrMasSupPostRegionsCode,
                        nameAr = x.CrMasSupPostRegionsArName,
                        nameEn = x.CrMasSupPostRegionsEnName,
                    }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Regions.Count(x => x.Type_Id == single.Type_Id);
                    var thisRegion = all_names_Region.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisRegion?.nameAr;
                    chartBranchDataVM.EnName = thisRegion?.nameEn;
                    chartBranchDataVM.Code = thisRegion?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisRegion == null)
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

                CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other

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
        public async Task<IActionResult> GetAllBy_City(string start, string end)
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

                var all_Contract_Citys = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsRenterCity,
              }));

                all_Contract_Citys = all_Contract_Citys.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Citys.DistinctBy(y => y.Type_Id).ToList();
                //var count_Contracts = all_Contract_Citys.Count;

                if (all_Contract_Citys.Count == 0)
                {
                    List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();
                    var response3 = new
                    {
                        list_chartBranchDataVM = listCaschartBranchDataVM4,
                        count = 0,
                    };

                    return Json(response3);
                }

                var all_names_City = await _unitOfWork.CrMasSupPostCity.FindAllWithSelectAsNoTrackingAsync(
                    predicate: null,
                    selectProjection: query => query.Select(x => new cas_list_String_4
                    {
                        id_key = x.CrMasSupPostCityCode,
                        nameAr = x.CrMasSupPostCityConcatenateArName,
                        nameEn = x.CrMasSupPostCityConcatenateEnName,
                    }));


                List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
                var count_Contracts = 0;

                foreach (var single in all_Type)
                {
                    var CategoryCount = 0;
                    CategoryCount = all_Contract_Citys.Count(x => x.Type_Id == single.Type_Id);
                    var thisCity = all_names_City.Find(x => x.id_key == single.Type_Id);
                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                    chartBranchDataVM.ArName = thisCity?.nameAr;
                    chartBranchDataVM.EnName = thisCity?.nameEn;
                    chartBranchDataVM.Code = thisCity?.id_key;
                    chartBranchDataVM.Value = CategoryCount;
                    chartBranchDataVM.IsTrue = true;
                    if (thisCity == null)
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
        public async Task<IActionResult> GetAllBy_Age(string start, string end)
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

                var all_Contract_Age = await _unitOfWork.CrCasRenterContractStatistic.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterContractStatisticsLessor == user.CrMasUserInformationLessor && x.CrCasRenterContractStatisticsDate > start_Date && x.CrCasRenterContractStatisticsDate <= end_Date,
              selectProjection: query => query.Select(x => new CAS_Contract_TypeVM
              {
                  Contract_Code = x.CrCasRenterContractStatisticsNo,
                  Type_Id = x.CrCasRenterContractStatisticsAgeNo,
              }));

                all_Contract_Age = all_Contract_Age.DistinctBy(x => x.Contract_Code).ToList();
                var all_Type = all_Contract_Age.DistinctBy(y => y.Type_Id).ToList();
                var count_Contracts = all_Contract_Age.Count;

                if (all_Contract_Age.Count == 0)
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
                var maxStatusSwitch = 8;
                for (var i = 1; i < maxStatusSwitch + 1; i++)
                {
                    var CategoryCount = all_Contract_Age.Count(x => x.Type_Id == i.ToString());

                    CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
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
                    listCaschartBranchDataVM.Add(chartBranchDataVM);
                }

                listCaschartBranchDataVM = listCaschartBranchDataVM.OrderBy(x => x.Code).ToList();
                ViewBag.count_Contracts = count_Contracts;


                CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
                // pass --> 1  if no Other --> 2 if were other
                if (listCaschartBranchDataVM.Count > 0)
                {
                    count_Contracts = int.Parse(listCaschartBranchDataVM.Sum(x => x.Value).ToString());
                }
                var response = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM,
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

        public async Task<List<CASChartBranchDataVM>?> Cas_statistics_other(List<CASChartBranchDataVM> listCaschartBranchDataVM, int count_Cars)
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
                    other.Value = count_Cars - listCaschartBranchDataVM2.Sum(x => x.Value);
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

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasUserInformation licence, string status)
        {
            //await _userLoginsService.SaveTracing(currentContract.CrCasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrCasSysMainTasksCode,
            //subTask.CrCasSysSubTasksCode, mainTask.CrCasSysMainTasksArName, subTask.CrCasSysSubTasksArName, mainTask.CrCasSysMainTasksEnName,
            //subTask.CrCasSysSubTasksEnName, system.CrCasSysSystemCode, "بنان", "Bnan");

            var recordAr = licence.CrMasUserInformationArName;
            var recordEn = licence.CrMasUserInformationEnName;
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

