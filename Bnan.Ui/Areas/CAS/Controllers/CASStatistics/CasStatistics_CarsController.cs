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

namespace Bnan.Ui.Areas.CAS.Controllers.CASStatistics
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CasStatistics_CarsController : BaseController
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
        private readonly IStringLocalizer<CasStatistics_CarsController> _localizer;
        private readonly string pageNumber = SubTasks.CasStatistics_Cars;



        public CasStatistics_CarsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CasStatistics_CarsController> localizer) : base(userManager, unitOfWork, mapper)
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

            var all_Car_branch = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold && x.CrCasCarInformationBranchStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationBranch,
              }));
            all_Car_branch = all_Car_branch.DistinctBy(x => x.Car_Code).ToList();
            
            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Cars = all_Car_branch.Count;

            
            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();

            // pass --> 1  if no Other --> 2 if were other
            CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM;
            CasStatistics_CarsVM.Cars_Count = count_Cars;



            return View(CasStatistics_CarsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Branches()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_branch = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold && x.CrCasCarInformationBranchStatus!= Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationBranch,
              }));
            all_Car_branch = all_Car_branch.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_branch.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_branch.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };
                return Json(response2);
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
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var branchCount = 0;
                branchCount = all_Car_branch.Count(x => x.Type_Id == single.Type_Id);
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
                count_Cars = branchCount + count_Cars;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;

            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Cars);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();

            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
            //CasStatistics_CarsVM.Cars_Count = count_Cars;

            //return Json(CasStatistics_CarsVM.listCasChartdataVM);
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Cars,
            };

            return Json(response);
        }



        [HttpGet]
        public async Task<IActionResult> GetAllBy_Region()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Region = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationRegion,
              }));
            all_Car_Region = all_Car_Region.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Region.DistinctBy(y => y.Type_Id).ToList();

            if (all_Car_Region.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
            }

            var all_names_Region = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupPostRegionsStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupPostRegionsCode,
                  nameAr = x.CrMasSupPostRegionsArName,
                  nameEn = x.CrMasSupPostRegionsEnName,
              }));


            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Region.Count(x => x.Type_Id == single.Type_Id);
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
                count_Cars = CategoryCount + count_Cars;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Cars);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();
            
            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            // pass --> 1  if no Other --> 2 if were other
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Cars,
            };

            return Json(response);
            //return Json(CasStatistics_CarsVM.listCasChartdataVM);

            //return PartialView("_PartialCASChartData", CasStatistics_CarsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_City()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_City = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationCity,
              }));
            all_Car_City = all_Car_City.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_City.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_City.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
            }

            var all_names_City = await _unitOfWork.CrMasSupPostCity.FindAllWithSelectAsNoTrackingAsync(
              predicate: null,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupPostCityCode,
                  nameAr = x.CrMasSupPostCityArName,
                  nameEn = x.CrMasSupPostCityEnName,
              }));


            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_City.Count(x => x.Type_Id == single.Type_Id);
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
                count_Cars = CategoryCount + count_Cars;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Cars);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();

            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            // pass --> 1  if no Other --> 2 if were other
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Cars,
            };

            return Json(response);

            //return Json(CasStatistics_CarsVM.listCasChartdataVM);

            //return PartialView("_PartialCASChartData", CasStatistics_CarsVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Brand()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Brand = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationBrand,
              }));
            all_Car_Brand = all_Car_Brand.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Brand.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Brand.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
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
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Brand.Count(x => x.Type_Id == single.Type_Id);
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
                count_Cars = CategoryCount + count_Cars;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Cars);


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
            if (listCaschartBranchDataVM2.Count > 0)
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
            //CasStatistics_CarsVM.Cars_Count = count_Cars;

            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Cars,
            };

            return Json(response);
            //return Json(CasStatistics_CarsVM.listCasChartdataVM);

            //return PartialView("_PartialCASChartData", CasStatistics_CarsVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Model()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Model = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationModel,
              }));
            all_Car_Model = all_Car_Model.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Model.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Model.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
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
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Model.Count(x => x.Type_Id == single.Type_Id);
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
                count_Cars = CategoryCount + count_Cars;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Cars);

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

            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();
            if (listCaschartBranchDataVM2.Count > 0)
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
            //CasStatistics_CarsVM.Cars_Count = count_Cars;
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Cars,
            };

            return Json(response);
            //return Json(CasStatistics_CarsVM.listCasChartdataVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Category()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Category = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationCategory,
              }));
            all_Car_Category = all_Car_Category.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Category.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Category.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
            }

            var all_names_Category = await _unitOfWork.CrMasSupCarCategory.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupCarCategoryStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupCarCategoryCode,
                  nameAr = x.CrMasSupCarCategoryArName,
                  nameEn = x.CrMasSupCarCategoryEnName,
              }));


            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Car_Category.Count(x => x.Type_Id == single.Type_Id);
                var thisCategory = all_names_Category.Find(x => x.id_key == single.Type_Id);
                CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                chartBranchDataVM.ArName = thisCategory?.nameAr;
                chartBranchDataVM.EnName = thisCategory?.nameEn;
                chartBranchDataVM.Code = thisCategory?.id_key;
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
                count_Cars = CategoryCount + count_Cars;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;

            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Cars);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();
            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}
            // pass --> 1  if no Other --> 2 if were other
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Cars,
            };

            return Json(response);

            //return Json(CasStatistics_CarsVM.listCasChartdataVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Year()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_Year = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new CAS_Car_TypeVM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationYear,
              }));
            all_Car_Year = all_Car_Year.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_Year.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_Year.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
            }

            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Cars = 0;

            foreach (var single in all_Type)
            {
                var YearCount = 0;
                YearCount = all_Car_Year.Count(x => x.Type_Id == single.Type_Id);
                CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                chartBranchDataVM.ArName = single.Type_Id;
                chartBranchDataVM.EnName = single.Type_Id;
                chartBranchDataVM.Code = single.Type_Id;
                chartBranchDataVM.Value = YearCount;
                chartBranchDataVM.IsTrue = true;
                listCaschartBranchDataVM.Add(chartBranchDataVM);
                count_Cars = YearCount + count_Cars;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Cars = count_Cars;

            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Cars);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();

            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            // pass --> 1  if no Other --> 2 if were other
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Cars,
            };

            return Json(response);

            //return Json(CasStatistics_CarsVM.listCasChartdataVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_status()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Car_status = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Sold,
              selectProjection: query => query.Select(x => new CAS_Car_Type_ForStatus_VM
              {
                  Car_Code = x.CrCasCarInformationSerailNo,
                  Type_Id = x.CrCasCarInformationStatus,
                  CrCasCarInformationStatus = x.CrCasCarInformationStatus,
                  CrCasCarInformationBranchStatus = x.CrCasCarInformationBranchStatus,
                  CrCasCarInformationDocumentationStatus = x.CrCasCarInformationDocumentationStatus,
                  CrCasCarInformationForSaleStatus = x.CrCasCarInformationForSaleStatus,
                  CrCasCarInformationMaintenanceStatus = x.CrCasCarInformationMaintenanceStatus,
                  CrCasCarInformationOwnerStatus = x.CrCasCarInformationOwnerStatus,
                  CrCasCarInformationPriceStatus = x.CrCasCarInformationPriceStatus,
              }));
            all_Car_status = all_Car_status.DistinctBy(x => x.Car_Code).ToList();
            var all_Type = all_Car_status.DistinctBy(y => y.Type_Id).ToList();


            if (all_Car_status.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
            }

            List<CASChartBranchDataVM> all_chartBranchDataVM = new List<CASChartBranchDataVM>();

            foreach (var single in all_Car_status)
            {
                CASChartBranchDataVM chartBranchDataVM_All = new CASChartBranchDataVM();

                if (single.CrCasCarInformationStatus == "H" || single.CrCasCarInformationOwnerStatus == "H" || single.CrCasCarInformationBranchStatus == "H")
                {
                    chartBranchDataVM_All.Code = "H";
                }
                else if (single.CrCasCarInformationStatus == "A" && single.CrCasCarInformationForSaleStatus == "A" && single.CrCasCarInformationOwnerStatus == "A" && single.CrCasCarInformationBranchStatus == "A" && single.CrCasCarInformationPriceStatus == true)
                {
                    chartBranchDataVM_All.Code = "A";
                }
                else if (single.CrCasCarInformationStatus == "A" && (single.CrCasCarInformationForSaleStatus == "T" || single.CrCasCarInformationForSaleStatus == "V"))
                {
                    chartBranchDataVM_All.Code = "S";
                }
                else if (single.CrCasCarInformationStatus == "A" && single.CrCasCarInformationOwnerStatus == "A" && single.CrCasCarInformationBranchStatus == "A" && single.CrCasCarInformationPriceStatus == false)
                {
                    chartBranchDataVM_All.Code = "W";
                }
                else if (single.CrCasCarInformationStatus == "R")
                {
                    chartBranchDataVM_All.Code = "R";
                }
                else if (single.CrCasCarInformationStatus == "M")
                {
                    chartBranchDataVM_All.Code = "M";
                }
                chartBranchDataVM_All.Value = 0;
                all_chartBranchDataVM.Add(chartBranchDataVM_All);
            }

            List<CASChartBranchDataVM> list_chartBranchDataVM3 = new List<CASChartBranchDataVM>();
            var all_statusType = new List<string>() { "A", "H", "S", "W", "R", "M" };
            foreach (var item in all_statusType)
            {
                var statusCount = all_chartBranchDataVM.Count(x => x.Code == item);

                CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();
                switch (item)
                {
                    case "A":
                        chartBranchDataVM.ArName = "نشطة";
                        chartBranchDataVM.EnName = "Active";
                        break;
                    case "H":
                        chartBranchDataVM.ArName = "موقوفة";
                        chartBranchDataVM.EnName = "Hold";
                        break;
                    case "S":
                        chartBranchDataVM.ArName = "للبيع";
                        chartBranchDataVM.EnName = "ForSale";
                        break;
                    case "W":
                        chartBranchDataVM.ArName = "بدون سعر";
                        chartBranchDataVM.EnName = "Without Price";
                        break;
                    case "R":
                        chartBranchDataVM.ArName = "مؤجرة";
                        chartBranchDataVM.EnName = "Rented";
                        break;
                    case "M":
                        chartBranchDataVM.ArName = "إصلاح";
                        chartBranchDataVM.EnName = "Fix";
                        break;                      
                }

                chartBranchDataVM.Code = item;
                chartBranchDataVM.Value = statusCount;
                list_chartBranchDataVM3.Add(chartBranchDataVM);
            }
            var count_Contracts = 0;
            if (list_chartBranchDataVM3.Count > 0)
            {
                count_Contracts = int.Parse(list_chartBranchDataVM3.Sum(x => x.Value).ToString());
            }
            var response = new
            {
                list_chartBranchDataVM = list_chartBranchDataVM3,
                count = count_Contracts,
            };

            return Json(response);
            //return Json(list_chartBranchDataVM3);
        }

        public async Task<List<CASChartBranchDataVM>?> Cas_statistics_other(List<CASChartBranchDataVM> listCaschartBranchDataVM, int count_Cars)
        {
            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            CASChartBranchDataVM other = new CASChartBranchDataVM();
            other.Value = 0;
            other.ArName = "أخرى  ";
            other.EnName = "  Others";
            other.Code = "Aa";

            //var Type_Avarage = listCaschartBranchDataVM.Average(x => x.Value);
            //var Type_Sum = listCaschartBranchDataVM.Sum(x => x.Value);
            //var Type_Count = listCaschartBranchDataVM.Count();
            //var Type_Avarage_percentage = Type_Avarage / Type_Sum;
            //var Static_Percentage_rate = 0.10;

            //var max = listCaschartBranchDataVM.Max(x => x.Value);
            //var max1 = (int)max;
            List<CASChartBranchDataVM>? listCaschartBranchDataVM2 = new List<CASChartBranchDataVM>();

            if (listCaschartBranchDataVM.Count < 11)
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM;
            }
            else
            {
                //var x = true;
                //for (var i = 0; i < listCaschartBranchDataVM.Count; i++)
                //{

                //    if ((int)listCaschartBranchDataVM[i].Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage))
                //    {
                //        listCaschartBranchDataVM[i].IsTrue = false;
                //        x = false;
                //        listCaschartBranchDataVM2 = listCaschartBranchDataVM.Take(i).ToList();
                //        other.Value = count_Cars - listCaschartBranchDataVM2.Sum(x => x.Value);
                //        listCaschartBranchDataVM2.Add(other);
                //        break;
                //    }
                //}
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

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {
            //await _userLoginsService.SaveTracing(currentCar.CrCasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrCasSysMainTasksCode,
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

