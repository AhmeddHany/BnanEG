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
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bnan.Ui.Areas.MAS.Controllers.MASStatistics
{

    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class CarStatisticsController : BaseController
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
        private readonly IStringLocalizer<CarStatisticsController> _localizer;
        private readonly string pageNumber = SubTasks.MasStatistics_Cars;



        public CarStatisticsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarStatisticsController> localizer) : base(userManager, unitOfWork, mapper)
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


            //var Most_Frequance_Company_list = _unitOfWork.CrCasCarInformation.GetAll()
            //                        .GroupBy(q => q.CrCasCarInformationCode)
            //                        .OrderByDescending(gp => gp.Count());

            var all_Car_Region = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new MAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationRegion,
              }));
            all_Car_Region = all_Car_Region.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Region.DistinctBy(y => y.Type_Id).ToList();

            var all_names_Region = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
              predicate: null,
              selectProjection: query => query.Select(x => new list_String_4
              {
                  id_key = x.CrMasSupPostRegionsCode,
                  nameAr = x.CrMasSupPostRegionsArName,
                  nameEn = x.CrMasSupPostRegionsEnName,
              }));


            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Region.Count(x => x.Type_Id == single.Type_Id);
                var thisRegion = all_names_Region.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisRegion?.nameAr;
                chartBranchDataVM.EnName = thisRegion?.nameEn;
                chartBranchDataVM.Code = thisRegion?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = CategoryCount + count_Cars;
            }
            listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


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
                    other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                    break;
                }
            }
            if (listMaschartBranchDataVM2.Count > 14)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                listMaschartBranchDataVM2.Add(other);
            }
            if (listMaschartBranchDataVM2.Count == 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM;
            }
            // till here //  //  //  //


            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            MasStatistics_CarsVM MasStatistics_CarsVM = new MasStatistics_CarsVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_CarsVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_CarsVM.Cars_Count = count_Cars;



            return View(MasStatistics_CarsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Region()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Region = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new MAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationRegion,
              }));
            all_Car_Region = all_Car_Region.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Region.DistinctBy(y => y.Type_Id).ToList();

            if (all_Car_Region.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
            }

            var all_names_Region = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupPostRegionsStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new list_String_4
              {
                  id_key = x.CrMasSupPostRegionsCode,
                  nameAr = x.CrMasSupPostRegionsArName,
                  nameEn = x.CrMasSupPostRegionsEnName,
              }));


            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Region.Count(x => x.Type_Id == single.Type_Id);
                var thisRegion = all_names_Region.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisRegion?.nameAr;
                chartBranchDataVM.EnName = thisRegion?.nameEn;
                chartBranchDataVM.Code = thisRegion?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = CategoryCount + count_Cars;
            }
            listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


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
                    other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                    break;
                }
            }
            if (listMaschartBranchDataVM2.Count > 14)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                listMaschartBranchDataVM2.Add(other);
            }
            if (listMaschartBranchDataVM2.Count == 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM;
            }
            // till here //  //  //  //


            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            MasStatistics_CarsVM MasStatistics_CarsVM = new MasStatistics_CarsVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_CarsVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_CarsVM.Cars_Count = count_Cars;

            return Json(MasStatistics_CarsVM.listMasChartdataVM);

            //return PartialView("_PartialMASChartData", MasStatistics_CarsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_City()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_City = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new MAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationCity,
              }));
            all_Car_City = all_Car_City.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_City.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_City.Count == 0)
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
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_City.Count(x => x.Type_Id == single.Type_Id);
                var thisCity = all_names_City.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisCity?.nameAr;
                chartBranchDataVM.EnName = thisCity?.nameEn;
                chartBranchDataVM.Code = thisCity?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = CategoryCount + count_Cars;
            }
            listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


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
                    other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                    break;
                }
            }
            if (listMaschartBranchDataVM2.Count > 14)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                listMaschartBranchDataVM2.Add(other);
            }
            if (listMaschartBranchDataVM2.Count == 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM;
            }
            // till here //  //  //  //


            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            MasStatistics_CarsVM MasStatistics_CarsVM = new MasStatistics_CarsVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_CarsVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_CarsVM.Cars_Count = count_Cars;

            return Json(MasStatistics_CarsVM.listMasChartdataVM);

            //return PartialView("_PartialMASChartData", MasStatistics_CarsVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Brand()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Brand = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new MAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationBrand,
              }));
            all_Car_Brand = all_Car_Brand.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Brand.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Brand.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
            }

            var all_names_Brand = await _unitOfWork.CrMasSupCarBrand.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupCarBrandStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new list_String_4
              {
                  id_key = x.CrMasSupCarBrandCode,
                  nameAr = x.CrMasSupCarBrandArName,
                  nameEn = x.CrMasSupCarBrandEnName,
              }));


            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Brand.Count(x => x.Type_Id == single.Type_Id);
                var thisBrand = all_names_Brand.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisBrand?.nameAr;
                chartBranchDataVM.EnName = thisBrand?.nameEn;
                chartBranchDataVM.Code = thisBrand?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = CategoryCount + count_Cars;
            }
            listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


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
                    other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                    break;
                }
            }
            if (listMaschartBranchDataVM2.Count > 14)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
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
            MasStatistics_CarsVM MasStatistics_CarsVM = new MasStatistics_CarsVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_CarsVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_CarsVM.Cars_Count = count_Cars;

            return Json(MasStatistics_CarsVM.listMasChartdataVM);

            //return PartialView("_PartialMASChartData", MasStatistics_CarsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Model()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Model = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new MAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationModel,
              }));
            all_Car_Model = all_Car_Model.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Model.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Model.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
            }

            var all_names_Model = await _unitOfWork.CrMasSupCarModel.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupCarModelStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new list_String_4
              {
                  id_key = x.CrMasSupCarModelCode,
                  nameAr = x.CrMasSupCarModelArConcatenateName,
                  nameEn = x.CrMasSupCarModelConcatenateEnName,
              }));


            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Model.Count(x => x.Type_Id == single.Type_Id);
                var thisModel = all_names_Model.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisModel?.nameAr;
                chartBranchDataVM.EnName = thisModel?.nameEn;
                chartBranchDataVM.Code = thisModel?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = CategoryCount + count_Cars;
            }
            listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


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
                    other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                    break;
                }
            }
            if (listMaschartBranchDataVM2.Count > 14)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
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

            MasStatistics_CarsVM MasStatistics_CarsVM = new MasStatistics_CarsVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_CarsVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_CarsVM.Cars_Count = count_Cars;

            return Json(MasStatistics_CarsVM.listMasChartdataVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Category()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Category = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new MAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationCategory,
              }));
            all_Car_Category = all_Car_Category.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Category.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Category.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
            }

            var all_names_Category = await _unitOfWork.CrMasSupCarCategory.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupCarCategoryStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new list_String_4
              {
                  id_key = x.CrMasSupCarCategoryCode,
                  nameAr = x.CrMasSupCarCategoryArName,
                  nameEn = x.CrMasSupCarCategoryEnName,
              }));


            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Category.Count(x => x.Type_Id == single.Type_Id);
                var thisCategory = all_names_Category.Find(x => x.id_key == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = thisCategory?.nameAr;
                chartBranchDataVM.EnName = thisCategory?.nameEn;
                chartBranchDataVM.Code = thisCategory?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = CategoryCount + count_Cars;
            }
            listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;

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
                    other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                    listMaschartBranchDataVM2.Add(other);
                    break;
                }
            }
            if (listMaschartBranchDataVM2.Count > 14)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                listMaschartBranchDataVM2.Add(other);
            }
            if (listMaschartBranchDataVM2.Count == 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM;
            }
            // till here //  //  //  //

            MasStatistics_CarsVM MasStatistics_CarsVM = new MasStatistics_CarsVM();
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_CarsVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_CarsVM.Cars_Count = count_Cars;

            return Json(MasStatistics_CarsVM.listMasChartdataVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Year()
        {
            //// Set page titles
            //var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Year = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new MAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationYear,
              }));
            all_Car_Year = all_Car_Year.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Year.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Year.Count == 0)
            {
                List<MASChartBranchDataVM> listMaschartBranchDataVM4 = new List<MASChartBranchDataVM>();

                return Json(listMaschartBranchDataVM4);
            }

            List<MASChartBranchDataVM> listMaschartBranchDataVM = new List<MASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var YearCount = 0;
                YearCount = all_Car_Year.Count(x => x.Type_Id == single.Type_Id);
                MASChartBranchDataVM chartBranchDataVM = new MASChartBranchDataVM();

                chartBranchDataVM.ArName = single.Type_Id;
                chartBranchDataVM.EnName = single.Type_Id;
                chartBranchDataVM.Code = single.Type_Id;
                chartBranchDataVM.Value = YearCount;
                chartBranchDataVM.IsTrue = true;
                listMaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = YearCount + count_Cars;
            }
            listMaschartBranchDataVM = listMaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;

            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            MASChartBranchDataVM other = new MASChartBranchDataVM();
            other.Value = 0;
            other.ArName = "أخرى  ";
            other.EnName = "  Others";
            other.Code = "Aa";

            //var Type_Avarage = listMaschartBranchDataVM.Average(x => x.Value);
            //var Type_Sum = listMaschartBranchDataVM.Sum(x => x.Value);
            //var Type_Count = listMaschartBranchDataVM.Count();
            //var Type_Avarage_percentage = Type_Avarage / Type_Sum;
            //var Static_Percentage_rate = 0.10;

            var max = listMaschartBranchDataVM.Max(x => x.Value);
            var max1 = (int)max;

            List<MASChartBranchDataVM>? listMaschartBranchDataVM2 = new List<MASChartBranchDataVM>();
            //var x = true;
            //for (var i = 0; i < listMaschartBranchData22VM.Count; i++)
            //{

            //    if ((int)listMaschartBranchDataVM[i].Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage))
            //    {
            //        listMaschartBranchDataVM[i].IsTrue = false;
            //        x = false;
            //        listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(i).ToList();
            //        other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
            //        listMaschartBranchDataVM2.Add(other);
            //        break;
            //    }
            //}
            if (listMaschartBranchDataVM2.Count > 14)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM.Take(14).ToList();
                other.Value = count_Cars - listMaschartBranchDataVM2.Sum(x => x.Value);
                listMaschartBranchDataVM2.Add(other);
            }
            if (listMaschartBranchDataVM2.Count == 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM;
            }
            if (listMaschartBranchDataVM2.Count > 0)
            {
                listMaschartBranchDataVM2 = listMaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // till here //  //  //  //

            MasStatistics_CarsVM MasStatistics_CarsVM = new MasStatistics_CarsVM();
            // pass --> 1  if no Other --> 2 if were other
            MasStatistics_CarsVM.listMasChartdataVM = listMaschartBranchDataVM2;
            MasStatistics_CarsVM.Cars_Count = count_Cars;

            return Json(MasStatistics_CarsVM.listMasChartdataVM);
        }


        //        if (Type == "Status")
        //            {
        //                List<ChartBranchDataVM> chartBranchDataVMs_Alls = new List<ChartBranchDataVM>();

        //        var AllCars2_Rigon = _unitOfWork.CrCasCarInformation.FindAll(x => company == x.CrCasCarInformationLessor).ToList();

        //                foreach (var single in AllCars2_Rigon)
        //                {
        //                    ChartBranchDataVM chartBranchDataVM_All = new ChartBranchDataVM();

        //                    if (single.CrCasCarInformationStatus == "H" || single.CrCasCarInformationOwnerStatus == "H" || single.CrCasCarInformationBranchStatus == "H")
        //                    {
        //                        chartBranchDataVM_All.ArName = "موقوفة";
        //                        chartBranchDataVM_All.EnName = "Hold";
        //                    }
        //                    else if (single.CrCasCarInformationStatus == "A" && single.CrCasCarInformationForSaleStatus == "A" && single.CrCasCarInformationOwnerStatus == "A" && single.CrCasCarInformationBranchStatus == "A" && single.CrCasCarInformationPriceStatus == true)
        //                    {
        //                        chartBranchDataVM_All.ArName = "نشطة";
        //                        chartBranchDataVM_All.EnName = "Active";
        //                    }
        //                    else if (single.CrCasCarInformationStatus == "A" && (single.CrCasCarInformationForSaleStatus == "T" || single.CrCasCarInformationForSaleStatus == "V"))
        //{
        //    chartBranchDataVM_All.ArName = "للبيع";
        //    chartBranchDataVM_All.EnName = "ForSale";
        //}
        //else if (single.CrCasCarInformationStatus == "A" && single.CrCasCarInformationOwnerStatus == "A" && single.CrCasCarInformationBranchStatus == "A" && single.CrCasCarInformationPriceStatus == false)
        //{
        //    chartBranchDataVM_All.ArName = "بدون سعر";
        //    chartBranchDataVM_All.EnName = "Without Price";
        //}
        //else if (single.CrCasCarInformationStatus == "R")
        //{
        //    chartBranchDataVM_All.ArName = "مؤجرة";
        //    chartBranchDataVM_All.EnName = "Rented";
        //}
        //else if (single.CrCasCarInformationStatus == "M")
        //{
        //    chartBranchDataVM_All.ArName = "إصلاح";
        //    chartBranchDataVM_All.EnName = "Fix";
        //}

        //chartBranchDataVM_All.Code = single.CrCasCarInformationStatus;
        //chartBranchDataVM_All.Value = 0;
        //chartBranchDataVMs_Alls.Add(chartBranchDataVM_All);

        //                }


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

