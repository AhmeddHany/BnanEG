﻿using AutoMapper;
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
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;

namespace Bnan.Ui.Areas.MAS.Controllers.MASStatistics
{

    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class RenterStatisticsController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IFinancialTransactionOfRenter _CarContract;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterStatisticsController> _localizer;
        private readonly string pageNumber = SubTasks.MasStatistics_Renters;



        public RenterStatisticsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter CarContract, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterStatisticsController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _CarContract = CarContract;
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

            //var Most_Frequance_Company_list = _unitOfWork.CrMasRenterInformation.GetAll()
            //                        .GroupBy(q => q.CrMasRenterInformationCode)
            //                        .OrderByDescending(gp => gp.Count());

            var all_Renter_Nationality = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new MAS_Renter_TypeVM
              {
                  Renter_Code = x.CrMasRenterInformationId,
                  Type_Id = x.CrMasRenterInformationNationality,
              }));
            all_Renter_Nationality = all_Renter_Nationality.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Nationality.DistinctBy(y => y.Type_Id).ToList();

            var all_names_Nationality = await _unitOfWork.CrMasSupRenterNationality.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new list_String_4
              {
                  id_key = x.CrMasSupRenterNationalitiesCode,
                  nameAr = x.CrMasSupRenterNationalitiesArName,
                  nameEn = x.CrMasSupRenterNationalitiesEnName,
              }));


            if (all_Renter_Nationality.Count == 0)
            {
                MasStatistics_RentersVM MasStatistics_RentersVM4 = new MasStatistics_RentersVM();
                MasStatistics_RentersVM4.Renters_Count = 0;

                return View(MasStatistics_RentersVM4);
            }

            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Renters = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_Nationality.Count(x => x.Type_Id == single.Type_Id);
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


            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }

            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_RentersVM.Renters_Count = count_Renters;



            return View(MasStatistics_RentersVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Profession()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);

            var all_Renter_Profession = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new MAS_Renter_TypeVM
              {
                  Renter_Code = x.CrMasRenterInformationId,
                  Type_Id = x.CrMasRenterInformationProfession,
              }));
            all_Renter_Profession = all_Renter_Profession.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Profession.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_Profession.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
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
                CategoryCount = all_Renter_Profession.Count(x => x.Type_Id == single.Type_Id);
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
            for(var v =0; v < listMaschartBranchDataVM2.Count; v++)
            {
                listMaschartBranchDataVM2[v].backgroundColor = colorBackGround[v];
                listMaschartBranchDataVM2[v].borderColor = colorBorder[v];
            }

            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_RentersVM.Renters_Count = count_Renters;

            return Json(MasStatistics_RentersVM.listMasChartdataVM);

            //return PartialView("_PartialMASChartData", MasStatistics_RentersVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_City()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Renter_City = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new MAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsCity,
              }));
            all_Renter_City = all_Renter_City.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_City.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_City.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
            }

            var all_names_City = await _unitOfWork.CrMasSupPostCity.FindAllWithSelectAsNoTrackingAsync(
              predicate: null,
              selectProjection: query => query.Select(x => new list_String_4
              {
                  id_key = x.CrMasSupPostCityCode,
                  nameAr = x.CrMasSupPostCityArName,
                  nameEn = x.CrMasSupPostCityEnName,
              }));


            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Renters = all_Renter_City.Count;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_City.Count(x => x.Type_Id == single.Type_Id);
                var thisCity = all_names_City.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisCity?.nameAr;
                chartBranchDataVM.EnName = thisCity?.nameEn;
                chartBranchDataVM.Code = thisCity?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
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


            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_RentersVM.Renters_Count = count_Renters;

            return Json(MasStatistics_RentersVM.listMasChartdataVM);

            //return PartialView("_PartialMASChartData", MasStatistics_RentersVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Region()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);

            var all_Renter_Region = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new MAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsRegions,
              }));
            all_Renter_Region = all_Renter_Region.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Region.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_Region.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
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
            var count_Renters = all_Renter_Region.Count;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_Region.Count(x => x.Type_Id == single.Type_Id);
                var thisRegion = all_names_Region.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisRegion?.nameAr;
                chartBranchDataVM.EnName = thisRegion?.nameEn;
                chartBranchDataVM.Code = thisRegion?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
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


            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_RentersVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_RentersVM.Renters_Count = count_Renters;

            return Json(MasStatistics_RentersVM.listMasChartdataVM);

            //return PartialView("_PartialMASChartData", MasStatistics_RentersVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Age()
        {

            var all_Renter_Age = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new MAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsAge,
              }));
            all_Renter_Age = all_Renter_Age.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Age.DistinctBy(y => y.Type_Id).ToList();

            List<MASChartBranchDataVM> list_chartBranchDataVM = new List<MASChartBranchDataVM>();
            var maxStatusSwitch = 8;
            for (var i = 1; i < maxStatusSwitch + 1; i++)
            {
                var CategoryCount = all_Renter_Age.Count(x => x.Type_Id == i.ToString());

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
            list_chartBranchDataVM.Add(chartBranchDataVM);
        }   


            MasStatistics_RentersVM MasStatistics_RentersVM = new MasStatistics_RentersVM();
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_RentersVM.listMasChartdataVM = list_chartBranchDataVM;
            MasStatistics_RentersVM.Renters_Count = all_Renter_Age?.Count;

            return Json(MasStatistics_RentersVM.listMasChartdataVM);

        }

        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarReport";
             ViewBag.no = "7";
            var titles = await setTitle("205", "2205014", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205014", "2");
            ViewBag.Data = "0";
            return View();

        }


        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {
            //await _userLoginsService.SaveTracing(currentCar.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
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

