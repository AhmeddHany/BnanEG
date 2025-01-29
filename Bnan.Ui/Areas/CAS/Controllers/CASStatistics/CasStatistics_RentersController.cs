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
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bnan.Ui.Areas.CAS.Controllers.CASStatistics
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CasStatistics_RentersController : BaseController
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
        private readonly IStringLocalizer<CasStatistics_RentersController> _localizer;
        private readonly string pageNumber = SubTasks.CasStatistics_Renters;



        public CasStatistics_RentersController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter CarContract, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CasStatistics_RentersController> localizer) : base(userManager, unitOfWork, mapper)
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
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }


            //var Most_Frequance_Company_list = _unitOfWork.CrMasRenterInformation.GetAll()
            //                        .GroupBy(q => q.CrMasRenterInformationCode)
            //                        .OrderByDescending(gp => gp.Count());

            var all_Renter_Nationality = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsNationalities,
              }));
            all_Renter_Nationality = all_Renter_Nationality.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Nationality.DistinctBy(y => y.Type_Id).ToList();

            var all_names_Nationality = await _unitOfWork.CrMasSupRenterNationality.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupRenterNationalitiesCode,
                  nameAr = x.CrMasSupRenterNationalitiesArName,
                  nameEn = x.CrMasSupRenterNationalitiesEnName,
              }));


            if (all_Renter_Nationality.Count == 0)
            {
                CasStatistics_RentersVM CasStatistics_RentersVM4 = new CasStatistics_RentersVM();
                CasStatistics_RentersVM4.Renters_Count = 0;

                return View(CasStatistics_RentersVM4);
            }

            //var Nationality_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsNationalities).Count();
            //var MemperShip_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationMembership).Count();
            //var profession_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsJobs).Count();
            //var Rigon_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsRegions).Count();
            //var City_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsCity).Count();
            //var Age_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsAge).Count();
            //var Traded_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsTraded).Count();
            //var Dealing_Mechanism_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationDealingMechanism).Count();
            //var Status_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatus).Count();

            //if (Status_count < 2 && Nationality_count < 2 && MemperShip_count < 2 && profession_count < 2 && Rigon_count < 2 && City_count < 2 && Age_count < 2 && City_count < 2 && Traded_count < 2 && Dealing_Mechanism_count < 2 && Status_count < 2)
            //{
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}

            //string concate_DropDown = "";
            //if (Nationality_count > 1) concate_DropDown = concate_DropDown + "0";
            //if (MemperShip_count > 1) concate_DropDown = concate_DropDown + "1";
            //if (profession_count > 1) concate_DropDown = concate_DropDown + "2";
            //if (Rigon_count > 1) concate_DropDown = concate_DropDown + "3";
            //if (City_count > 1) concate_DropDown = concate_DropDown + "4";
            //if (Age_count > 1) concate_DropDown = concate_DropDown + "5";
            //if (Traded_count > 1) concate_DropDown = concate_DropDown + "6";
            //if (Dealing_Mechanism_count > 1) concate_DropDown = concate_DropDown + "7";
            //ViewBag.concate_DropDown = concate_DropDown;

            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Renters = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_Nationality.Count(x => x.Type_Id == single.Type_Id);
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
                count_Renters = CategoryCount + count_Renters;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Renters = count_Renters;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Renters);

            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            // pass --> 1  if no Other --> 2 if were other

            //ViewBag.singleType = "0";
            //ViewBag.singleType = concate_DropDown[0].ToString();
            CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            //// pass --> 1  if no Other --> 2 if were other
            CasStatistics_RentersVM.listCasChartdataVM = listCaschartBranchDataVM2;
            CasStatistics_RentersVM.Renters_Count = count_Renters;

            return View(CasStatistics_RentersVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_MemberShip()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Renter_membership = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorMembership,
              }));
            all_Renter_membership = all_Renter_membership.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_membership.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_membership.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
            }

            var all_names_membership = await _unitOfWork.CrMasSupRenterMembership.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrMasSupRenterMembershipStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupRenterMembershipCode,
                  nameAr = x.CrMasSupRenterMembershipArName,
                  nameEn = x.CrMasSupRenterMembershipEnName,
              }));


            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            var count_Renters = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_membership.Count(x => x.Type_Id == single.Type_Id);
                var thismembership = all_names_membership.Find(x => x.id_key == single.Type_Id);
                CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                chartBranchDataVM.ArName = thismembership?.nameAr;
                chartBranchDataVM.EnName = thismembership?.nameEn;
                chartBranchDataVM.Code = thismembership?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                if (thismembership == null)
                {
                    CategoryCount = 0;
                }
                else
                {
                    listCaschartBranchDataVM.Add(chartBranchDataVM);
                }
                count_Renters = CategoryCount + count_Renters;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Renters = count_Renters;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Renters);

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
            CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
            if (listCaschartBranchDataVM2.Count > 0)
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_RentersVM.listCasChartdataVM = listCaschartBranchDataVM2;
            //CasStatistics_RentersVM.Renters_Count = count_Renters;

            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Renters,
            };

            return Json(response);
            //return Json(CasStatistics_RentersVM.listCasChartdataVM);

            //return PartialView("_PartialCASChartData", CasStatistics_RentersVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Profession()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Renter_Profession = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsJobs,
              }));
            all_Renter_Profession = all_Renter_Profession.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Profession.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_Profession.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
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
            var count_Renters = 0;

            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_Profession.Count(x => x.Type_Id == single.Type_Id);
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
                count_Renters = CategoryCount + count_Renters;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Renters = count_Renters;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Renters);

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
            for(var v =0; v < listCaschartBranchDataVM2.Count; v++)
            {
                listCaschartBranchDataVM2[v].backgroundColor = colorBackGround[v];
                listCaschartBranchDataVM2[v].borderColor = colorBorder[v];
            }

            CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
            if (listCaschartBranchDataVM2.Count > 0)
            {
                listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            }
            // pass --> 1  if no Other --> 2 if were other
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Renters,
            };

            return Json(response);
            //return Json(CasStatistics_RentersVM.listCasChartdataVM);

            //return PartialView("_PartialCASChartData", CasStatistics_RentersVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_City()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Renter_City = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsCity,
              }));
            all_Renter_City = all_Renter_City.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_City.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_City.Count == 0)
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
            //var count_Renters = all_Renter_City.Count;
            var count_Renters = 0;
            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_City.Count(x => x.Type_Id == single.Type_Id);
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
                count_Renters = CategoryCount + count_Renters;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Renters = count_Renters;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Renters);

            CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}
            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_RentersVM.listCasChartdataVM = listCaschartBranchDataVM2;
            //CasStatistics_RentersVM.Renters_Count = count_Renters;

            //return Json(CasStatistics_RentersVM.listCasChartdataVM);

            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Renters,
            };

            return Json(response);
            //return PartialView("_PartialCASChartData", CasStatistics_RentersVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Region()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Renter_Region = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsRegions,
              }));
            all_Renter_Region = all_Renter_Region.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Region.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_Region.Count == 0)
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
              predicate: null,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSupPostRegionsCode,
                  nameAr = x.CrMasSupPostRegionsArName,
                  nameEn = x.CrMasSupPostRegionsEnName,
              }));


            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            //var count_Renters = all_Renter_Region.Count;
            var count_Renters = 0;
            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_Region.Count(x => x.Type_Id == single.Type_Id);
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
                count_Renters = CategoryCount + count_Renters;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Renters = count_Renters;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Renters);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();

            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
            //CasStatistics_CarsVM.Cars_Count = count_Renters;

            //return Json(listCaschartBranchDataVM2);
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Renters,
            };

            return Json(response);
            //return PartialView("_PartialCASChartData", CasStatistics_RentersVM);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllBy_Age()
        {
            var user = await _userManager.GetUserAsync(User);

            var all_Renter_Age = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorStatisticsAge,
              }));
            all_Renter_Age = all_Renter_Age.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_Age.DistinctBy(y => y.Type_Id).ToList();

            List<CASChartBranchDataVM> list_chartBranchDataVM = new List<CASChartBranchDataVM>();
            var maxStatusSwitch = 8;
            for (var i = 1; i < maxStatusSwitch + 1; i++)
            {
                var CategoryCount = all_Renter_Age.Count(x => x.Type_Id == i.ToString());

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
            list_chartBranchDataVM.Add(chartBranchDataVM);
        }   


            CasStatistics_RentersVM CasStatistics_RentersVM = new CasStatistics_RentersVM();
            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_RentersVM.listCasChartdataVM = list_chartBranchDataVM;
            //CasStatistics_RentersVM.Renters_Count = all_Renter_Age?.Count;

            //return Json(CasStatistics_RentersVM.listCasChartdataVM);
            var count_Contracts = 0;
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

        [HttpGet]
        public async Task<IActionResult> GetAllBy_Rates()
        {
            //// Set page titles
            var user = await _userManager.GetUserAsync(User);
            //await SetPageTitleAsync(string.Empty, pageNumber);
            //// Check Validition

            var all_Renter_rate = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
              predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus != Status.Deleted,
              selectProjection: query => query.Select(x => new CAS_Renter_TypeVM
              {
                  Renter_Code = x.CrCasRenterLessorId,
                  Type_Id = x.CrCasRenterLessorDealingMechanism,
              }));
            all_Renter_rate = all_Renter_rate.DistinctBy(x => x.Renter_Code).ToList();
            var all_Type = all_Renter_rate.DistinctBy(y => y.Type_Id).ToList();


            if (all_Renter_rate.Count == 0)
            {
                List<CASChartBranchDataVM> listCaschartBranchDataVM4 = new List<CASChartBranchDataVM>();

                var response2 = new
                {
                    list_chartBranchDataVM = listCaschartBranchDataVM4,
                    count = 0,
                };

                return Json(response2);
            }

            var all_names_rate = await _unitOfWork.CrMasSysEvaluation.FindAllWithSelectAsNoTrackingAsync(
              predicate: null,
              selectProjection: query => query.Select(x => new cas_list_String_4
              {
                  id_key = x.CrMasSysEvaluationsCode,
                  nameAr = x.CrMasSysEvaluationsArDescription,
                  nameEn = x.CrMasSysEvaluationsEnDescription,
              }));


            List<CASChartBranchDataVM> listCaschartBranchDataVM = new List<CASChartBranchDataVM>();
            //var count_Renters = all_Renter_rate.Count;
            var count_Renters = 0;
            foreach (var single in all_Type)
            {
                var CategoryCount = 0;
                CategoryCount = all_Renter_rate.Count(x => x.Type_Id == single.Type_Id);
                var thisrate = all_names_rate.Find(x => x.id_key == single.Type_Id);
                CASChartBranchDataVM chartBranchDataVM = new CASChartBranchDataVM();

                chartBranchDataVM.ArName = thisrate?.nameAr;
                chartBranchDataVM.EnName = thisrate?.nameEn;
                chartBranchDataVM.Code = thisrate?.id_key;
                chartBranchDataVM.Value = CategoryCount;
                chartBranchDataVM.IsTrue = true;
                if (thisrate == null)
                {
                    CategoryCount = 0;
                }
                else
                {
                    listCaschartBranchDataVM.Add(chartBranchDataVM);
                }
                count_Renters = CategoryCount + count_Renters;
            }
            listCaschartBranchDataVM = listCaschartBranchDataVM.OrderByDescending(x => x.Value).ToList();
            ViewBag.count_Renters = count_Renters;


            // pass --> 1  if no Other --> 2 if were other
            // // // for make other colomn based on average percentage

            var listCaschartBranchDataVM2 = await Cas_statistics_other(listCaschartBranchDataVM, count_Renters);


            CasStatistics_CarsVM CasStatistics_CarsVM = new CasStatistics_CarsVM();

            //if (listCaschartBranchDataVM2.Count > 0)
            //{
            //    listCaschartBranchDataVM2 = listCaschartBranchDataVM2.OrderBy(x => x.Code).ToList();
            //}

            //// pass --> 1  if no Other --> 2 if were other
            //CasStatistics_CarsVM.listCasChartdataVM = listCaschartBranchDataVM2;
            //CasStatistics_CarsVM.Cars_Count = count_Renters;

            //return Json(listCaschartBranchDataVM2);
            var response = new
            {
                list_chartBranchDataVM = listCaschartBranchDataVM2,
                count = count_Renters,
            };

            return Json(response);

            //return PartialView("_PartialCASChartData", CasStatistics_RentersVM);
        }


        //[HttpGet]
        //public async Task<IActionResult> GetAllByType(string Type, string listDrop, string singleNo, string company_now)
        //{
        //    var company = company_now?.Replace("-1", "") ?? "";

        //    if (listDrop == "" || listDrop == null)
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    //sidebar Active
        //    ViewBag.id = "#sidebarReport";
        //     ViewBag.no = "7";
        //    ViewBag.concate_DropDown = listDrop;
        //    ViewBag.singleType = singleNo;


        //    var (mainTask, subTask, system, currentCar) = await SetTrace("205", "2205014", "2");

        //    var titles = await setTitle("205", "2205014", "2");
        //    await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

        //    var AllRenters = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).ToList();


        //    var AllLessor = _unitOfWork.CrCasLessorInformation.FindAll(x => x.CrCasLessorInformationCode != "0000").ToList();

        //    List<ChartBranchDataVM> chartBranchDataVMs = new List<ChartBranchDataVM>();
        //    var count_Renters = 0;

        //    string concate_DropDown = listDrop;
        //    if (company_now != null && !company_now.StartsWith("-1"))
        //    {
        //        var Nationality_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsNationalities).Count();
        //        var MemperShip_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationMembership).Count();
        //        var profession_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsJobs).Count();
        //        var Rigon_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsRegions).Count();
        //        var City_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsCity).Count();
        //        var Age_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsAge).Count();
        //        var Traded_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsTraded).Count();
        //        var Dealing_Mechanism_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationDealingMechanism).Count();
        //        var Status_count = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatus).Count();

        //        if (Status_count < 2 && Nationality_count < 2 && MemperShip_count < 2 && profession_count < 2 && Rigon_count < 2 && City_count < 2 && Age_count < 2 && City_count < 2 && Traded_count < 2 && Dealing_Mechanism_count < 2 && Status_count < 2)
        //        {
        //            return RedirectToAction("FailedMessageReport_NoData");
        //        }

        //        concate_DropDown = "";
        //        if (Nationality_count > 1) concate_DropDown = concate_DropDown + "0";
        //        if (MemperShip_count > 1) concate_DropDown = concate_DropDown + "1";
        //        if (profession_count > 1) concate_DropDown = concate_DropDown + "2";
        //        if (Rigon_count > 1) concate_DropDown = concate_DropDown + "3";
        //        if (City_count > 1) concate_DropDown = concate_DropDown + "4";
        //        if (Age_count > 1) concate_DropDown = concate_DropDown + "5";
        //        if (Traded_count > 1) concate_DropDown = concate_DropDown + "6";
        //        if (Dealing_Mechanism_count > 1) concate_DropDown = concate_DropDown + "7";
        //        //if (Status_count > 1) concate_DropDown = concate_DropDown + "8";
        //        ViewBag.concate_DropDown = concate_DropDown;
        //    }

        //        if (Type == "Nationality")
        //    {
        //        var AllNationality = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode, new[] { "CrMasRenterInformationStatisticsNationalitiesNavigation" }).Where(x => x.CrMasRenterInformationStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesStatus != Status.Deleted).DistinctBy(x => x.CrMasRenterInformationStatisticsNationalities).ToList();

        //        foreach (var single in AllNationality)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationStatisticsNationalities == single.CrMasRenterInformationStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesCode);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            chartBranchDataVM.ArName = single.CrMasRenterInformationStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesArName;
        //            chartBranchDataVM.EnName = single.CrMasRenterInformationStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesEnName;
        //            chartBranchDataVM.Code = single.CrMasRenterInformationStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesCode;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }
        //    if (Type == "MemperShip")
        //    {
        //        var AllRenters2_MemperShip = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode, new[] { "CrMasRenterInformationMembershipNavigation" }).Where(x => x.CrMasRenterInformationMembershipNavigation?.CrCasSupRenterMembershipStatus != Status.Deleted).DistinctBy(x => x.CrMasRenterInformationMembership).ToList();

        //        foreach (var single in AllRenters2_MemperShip)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationMembership == single.CrMasRenterInformationMembershipNavigation?.CrCasSupRenterMembershipCode);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            chartBranchDataVM.ArName = single.CrMasRenterInformationMembershipNavigation?.CrCasSupRenterMembershipArName;
        //            chartBranchDataVM.EnName = single.CrMasRenterInformationMembershipNavigation?.CrCasSupRenterMembershipEnName;
        //            chartBranchDataVM.Code = single.CrMasRenterInformationMembershipNavigation?.CrCasSupRenterMembershipCode;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }
        //    if (Type == "profession")
        //    {
        //        var AllRenters2_profession = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode, new[] { "CrMasRenterInformationStatisticsJobsNavigation" }).Where(x => x.CrMasRenterInformationStatisticsJobsNavigation?.CrCasSupRenterProfessionsStatus != Status.Deleted).DistinctBy(x => x.CrMasRenterInformationStatisticsJobs).ToList();

        //        foreach (var single in AllRenters2_profession)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationStatisticsJobs == single.CrMasRenterInformationStatisticsJobsNavigation?.CrCasSupRenterProfessionsCode);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            chartBranchDataVM.ArName = single.CrMasRenterInformationStatisticsJobsNavigation?.CrCasSupRenterProfessionsArName;
        //            chartBranchDataVM.EnName = single.CrMasRenterInformationStatisticsJobsNavigation?.CrCasSupRenterProfessionsEnName;
        //            chartBranchDataVM.Code = single.CrMasRenterInformationStatisticsJobsNavigation?.CrCasSupRenterProfessionsCode;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }
        //    if (Type == "Rigon")
        //    {
        //        var AllRenters2_Rigon = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode, new[] { "CrMasRenterInformationStatisticsRegionsNavigation" }).Where(x => x.CrMasRenterInformationStatisticsRegionsNavigation?.CrCasSupPostRegionsStatus != Status.Deleted).DistinctBy(x => x.CrMasRenterInformationStatisticsRegions).ToList();

        //        foreach (var single in AllRenters2_Rigon)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationStatisticsRegions == single.CrMasRenterInformationStatisticsRegionsNavigation?.CrCasSupPostRegionsCode);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            chartBranchDataVM.ArName = single.CrMasRenterInformationStatisticsRegionsNavigation?.CrCasSupPostRegionsArName;
        //            chartBranchDataVM.EnName = single.CrMasRenterInformationStatisticsRegionsNavigation?.CrCasSupPostRegionsEnName;
        //            chartBranchDataVM.Code = single.CrMasRenterInformationStatisticsRegionsNavigation?.CrCasSupPostRegionsCode;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }
        //    if (Type == "City")
        //    {
        //        var AllRenters2_City = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode, new[] { "CrMasRenterInformationStatisticsCityNavigation" }).Where(x => x.CrMasRenterInformationStatisticsCityNavigation?.CrCasSupPostCityStatus != Status.Deleted).DistinctBy(x => x.CrMasRenterInformationStatisticsCity).ToList();

        //        foreach (var single in AllRenters2_City)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationStatisticsCity == single.CrMasRenterInformationStatisticsCityNavigation?.CrCasSupPostCityCode);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            chartBranchDataVM.ArName = single.CrMasRenterInformationStatisticsCityNavigation?.CrCasSupPostCityArName;
        //            chartBranchDataVM.EnName = single.CrMasRenterInformationStatisticsCityNavigation?.CrCasSupPostCityEnName;
        //            chartBranchDataVM.Code = single.CrMasRenterInformationStatisticsCityNavigation?.CrCasSupPostCityCode;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }

        //    if (Type == "Age")
        //    {
        //        var AllRenters2_Age = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsAge).ToList();

        //        foreach (var single in AllRenters2_Age)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationStatisticsAge == single.CrMasRenterInformationStatisticsAge);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            switch (single.CrMasRenterInformationStatisticsAge)
        //            {
        //                case "1":
        //                    chartBranchDataVM.ArName = "أقل من 20";
        //                    chartBranchDataVM.EnName = "Less Than 20";
        //                    break;
        //                case "2":
        //                    chartBranchDataVM.ArName = "من 21 إلى 30";
        //                    chartBranchDataVM.EnName = "From 21 To 30";
        //                    break;
        //                case "3":
        //                    chartBranchDataVM.ArName = "من 31 إلى 40";
        //                    chartBranchDataVM.EnName = "From 31 To 40";
        //                    break;
        //                case "4":
        //                    chartBranchDataVM.ArName = "من 41 إلى 50";
        //                    chartBranchDataVM.EnName = "From 41 To 50";
        //                    break;
        //                case "5":
        //                    chartBranchDataVM.ArName = "من 51 إلى 60";
        //                    chartBranchDataVM.EnName = "From 51 To 60";
        //                    break;
        //                case "6":
        //                    chartBranchDataVM.ArName = "أكثر من 60";
        //                    chartBranchDataVM.EnName = "More Than 60";
        //                    break;
        //                default:
        //                    chartBranchDataVM.ArName = "أقل من 20";
        //                    chartBranchDataVM.EnName = "Less Than 20";
        //                    break;
        //            }

        //            chartBranchDataVM.Code = single.CrMasRenterInformationStatisticsAge;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }

        //    if (Type == "Traded")
        //    {
        //        var AllRenters2_Traded = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatisticsTraded).ToList();

        //        foreach (var single in AllRenters2_Traded)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationStatisticsTraded == single.CrMasRenterInformationStatisticsTraded);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            switch (single.CrMasRenterInformationStatisticsTraded)
        //            {
        //                case "1":
        //                    chartBranchDataVM.ArName = "أقل من 1000";
        //                    chartBranchDataVM.EnName = "Less Than 1000";
        //                    break;
        //                case "2":
        //                    chartBranchDataVM.ArName = "من 1001 إلى 2000";
        //                    chartBranchDataVM.EnName = "From 1001 To 2000";
        //                    break;
        //                case "3":
        //                    chartBranchDataVM.ArName = "من 2001 إلى 3000";
        //                    chartBranchDataVM.EnName = "From 2001 To 3000";
        //                    break;
        //                case "4":
        //                    chartBranchDataVM.ArName = "من 3001 إلى 5000";
        //                    chartBranchDataVM.EnName = "From 3001 To 5000";
        //                    break;
        //                case "5":
        //                    chartBranchDataVM.ArName = "من 5001 إلى 7000";
        //                    chartBranchDataVM.EnName = "From 5001 To 7000";
        //                    break;
        //                case "6":
        //                    chartBranchDataVM.ArName = "من 7001 إلى 10,000";
        //                    chartBranchDataVM.EnName = "From 7001 To 10,000";
        //                    break;
        //                case "7":
        //                    chartBranchDataVM.ArName = "من 10,001 إلى 15,000";
        //                    chartBranchDataVM.EnName = "From 10,001 To 15,000";
        //                    break;
        //                case "8":
        //                    chartBranchDataVM.ArName = "من 15,001 إلى 20,000";
        //                    chartBranchDataVM.EnName = "From 15,001 To 20,000";
        //                    break;
        //                case "9":
        //                    chartBranchDataVM.ArName = "أكثر من 20,000";
        //                    chartBranchDataVM.EnName = "More Than 20,000";
        //                    break;
        //                default:
        //                    chartBranchDataVM.ArName = "أقل من 1000";
        //                    chartBranchDataVM.EnName = "Less Than 1000";
        //                    break;
        //            }

        //            chartBranchDataVM.Code = single.CrMasRenterInformationStatisticsTraded;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }

        //    if (Type == "Dealing_Mechanism")
        //    {
        //        var AllDealing_Mechanism = _unitOfWork.CrCasSysEvaluation.GetAll().Where(x => x.CrCasSysEvaluationsStatus != Status.Deleted).ToList();

        //        foreach (var single in AllDealing_Mechanism)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationDealingMechanism == single.CrCasSysEvaluationsCode);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            chartBranchDataVM.ArName = single.CrCasSysEvaluationsArDescription;
        //            chartBranchDataVM.EnName = single.CrCasSysEvaluationsEnDescription;
        //            chartBranchDataVM.Code = single.CrCasSysEvaluationsCode;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }

        //    if (Type == "Status")
        //    {
        //        var AllRenters2_Status = _unitOfWork.CrMasRenterInformation.FindAll(x => company == x.CrMasRenterInformationCode).DistinctBy(x => x.CrMasRenterInformationStatus).ToList();

        //        foreach (var single in AllRenters2_Status)
        //        {
        //            var CategoryCount = 0;
        //            CategoryCount = AllRenters.Count(x => x.CrMasRenterInformationStatus == single.CrMasRenterInformationStatus);
        //            ChartBranchDataVM chartBranchDataVM = new ChartBranchDataVM();
        //            switch (single.CrMasRenterInformationStatus)
        //            {
        //                case "A":
        //                    chartBranchDataVM.ArName = "نشط";
        //                    chartBranchDataVM.EnName = "Active";
        //                    break;
        //                case "R":
        //                    chartBranchDataVM.ArName = "مؤجر";
        //                    chartBranchDataVM.EnName = "Rented";
        //                    break;
        //                default:
        //                    chartBranchDataVM.ArName = "نشط";
        //                    chartBranchDataVM.EnName = "Active";
        //                    break;
        //            }

        //            chartBranchDataVM.Code = single.CrMasRenterInformationStatus;
        //            chartBranchDataVM.Value = CategoryCount;
        //            chartBranchDataVMs.Add(chartBranchDataVM);
        //            count_Renters = CategoryCount + count_Renters;
        //        }
        //        ViewBag.count_Renters = count_Renters;
        //    }

        //    chartBranchDataVMs = chartBranchDataVMs.OrderByDescending(x => x.Value).ToList();
        //    var Type_Avarage = chartBranchDataVMs.Average(x => x.Value);
        //    var Type_Sum = chartBranchDataVMs.Sum(x => x.Value);
        //    var Type_Count = chartBranchDataVMs.Count();
        //    var Type_Avarage_percentage = Type_Avarage / Type_Sum;
        //    var Static_Percentage_rate = 0.10;

        //    //ViewBag.count_Renters = count_Renters;
        //    var max_Colomns = 15;
        //    var max = chartBranchDataVMs.Max(x => x.Value);
        //    var max1 = (int)max;
        //    ChartBranchDataVM other = new ChartBranchDataVM();
        //    other.Value = 0;
        //    other.ArName = "أخرى  ";
        //    other.EnName = "  Others";
        //    other.Code = "Aa";

        //    List<ChartBranchDataVM> chartBranchDataVMs_2 = new List<ChartBranchDataVM>(chartBranchDataVMs);
        //    int counter_For_max_Colomn = 0;

        //    foreach (var branch_1 in chartBranchDataVMs_2)
        //    {
        //        counter_For_max_Colomn++;
        //        if (counter_For_max_Colomn <= max_Colomns)
        //        {
        //            branch_1.IsTrue = true;
        //        }
        //        else
        //        {
        //            branch_1.IsTrue = false;
        //        }
        //        if (chartBranchDataVMs_2.Count() > 5)
        //        {
        //            if ((int)branch_1.Value <= max1 * (Static_Percentage_rate + (double)Type_Avarage_percentage) || (int)branch_1.Value <= max1 * (double)Type_Avarage_percentage)
        //            {
        //                branch_1.IsTrue = false;
        //            }
        //        }

        //    }
        //    foreach (var branch_1 in chartBranchDataVMs_2)
        //    {
        //        if (branch_1.IsTrue == false)
        //        {
        //            other.Value = branch_1.Value + other.Value;
        //            chartBranchDataVMs.Remove(branch_1);
        //        }
        //    }
        //    if ((int)other.Value > 0)
        //    {
        //        chartBranchDataVMs.Add(other);
        //        int listCount = 0;
        //        listCount = chartBranchDataVMs.Count() - 1;
        //        chartBranchDataVMs_2.Insert(listCount, other);
        //    }

        //    CasStatisticLayoutCAS_VM casStatisticLayoutVM = new CasStatisticLayoutCAS_VM();
        //    casStatisticLayoutVM.ChartBranchDataVM = chartBranchDataVMs;
        //    casStatisticLayoutVM.ChartBranchDataVM_2ForAll = chartBranchDataVMs;
        //    casStatisticLayoutVM.All_Lessors = AllLessor;

        //    return View(casStatisticLayoutVM);
        //}

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

