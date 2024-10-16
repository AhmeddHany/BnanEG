﻿using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;



namespace Bnan.Ui.Areas.CAS.Controllers
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class RenterLessorInformationController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IRenterLessorInformation _RenterLessorInformation;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterLessorInformationController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;


        public RenterLessorInformationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IRenterLessorInformation RenterLessorInformation, IAdminstritiveProcedures AdminstritiveProcedures,
        IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterLessorInformationController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _RenterLessorInformation = RenterLessorInformation;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _adminstritiveProcedures = AdminstritiveProcedures;
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {

            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "0";

            var (mainTask, subTask, system, currentUser) = await SetTrace("203", "2203001", "2");


            var titles = await setTitle("203", "2203001", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterLessorInformationAll = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" }).ToList();

            //if (RenterLessorInformationAll?.Count() < 1)
            //{
            //    ViewBag.Data = "0";
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}

            var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();
            ViewData["Rates"] = rates;



            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
           subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
           subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            return View(RenterLessorInformationAll);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetRenterLessorInformationByStatusAsync(string status)
        {

            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "0";

            var (mainTask, subTask, system, currentUser) = await SetTrace("203", "2203001", "2");

            if (!string.IsNullOrEmpty(status))
            {
                var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();

                ViewData["Rates"] = rates;

                if (status == Status.All)
                {
                    var RenterLessorInformationbyStatusAll = _unitOfWork.CrCasRenterLessor.FindAll(l => (l.CrCasRenterLessorStatus == Status.Active || l.CrCasRenterLessorStatus == Status.Rented) && l.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" }).ToList();
                    return PartialView("_DataTableRenterLessorInformation", RenterLessorInformationbyStatusAll);
                }
                var RenterLessorInformationbyStatus = _unitOfWork.CrCasRenterLessor.FindAll(l => l.CrCasRenterLessorStatus == status && l.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" }).ToList();
                return PartialView("_DataTableRenterLessorInformation", RenterLessorInformationbyStatus);
            }
            return PartialView();
        }




        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "0";

            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("203", "2203001", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);
            var (mainTask, subTask, system, currentUser) = await SetTrace("203", "2203001", "2");


            var contract = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == id && x.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterLessorStatisticsNationalitiesNavigation", "CrCasRenterLessorNavigation.CrMasRenterPost", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorIdtrypeNavigation", "CrCasRenterLessorSectorNavigation", "CrCasRenterLessorStatisticsGenderNavigation", "CrCasRenterLessorMembershipNavigation" });
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            var Mechanism = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1" && x.CrMasSysEvaluationsStatus == "A").ToList();
            ViewData["Mechanism"] = Mechanism;

            var workPlace = _unitOfWork.CrMasSupRenterEmployer.Find(x => x.CrMasSupRenterEmployerCode == contract.CrCasRenterLessorNavigation.CrMasRenterInformationEmployer);
            ViewBag.workPlaceAr = workPlace?.CrMasSupRenterEmployerArName;
            ViewBag.workPlaceEn = workPlace?.CrMasSupRenterEmployerEnName;

            var Bank = _unitOfWork.CrMasSupAccountBanks.Find(x => x.CrMasSupAccountBankCode == contract.CrCasRenterLessorNavigation.CrMasRenterInformationBank);
            ViewBag.BankAr = Bank?.CrMasSupAccountBankArName;
            ViewBag.BankEn = Bank?.CrMasSupAccountBankEnName;

            var DrivingType = _unitOfWork.CrMasSupRenterDrivingLicense.Find(x => x.CrMasSupRenterDrivingLicenseCode == contract.CrCasRenterLessorNavigation.CrMasRenterInformationDrivingLicenseType);
            ViewBag.DrivingTypeAr = DrivingType?.CrMasSupRenterDrivingLicenseArName;
            ViewBag.DrivingTypeEn = DrivingType?.CrMasSupRenterDrivingLicenseEnName;

            int countRenterLessorInformations = 0;
            ViewBag.RenterLessorInformations_Count = countRenterLessorInformations;
            var model = _mapper.Map<RenterLessorInformationVM>(contract);

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RenterLessorInformationVM model)
        {

            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "0";

            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);

            if (user != null)
            {
                var (mainTask, subTask, system, currentUser) = await SetTrace("203", "2203001", "2");

                if (model != null)
                {
                    var contract = _mapper.Map<CrCasRenterLessor>(model);

                    var singlExist = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == contract.CrCasRenterLessorId && x.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor);
                    if (singlExist != null)
                    {
                        singlExist.CrCasRenterLessorDealingMechanism = contract.CrCasRenterLessorDealingMechanism;
                        singlExist.CrCasRenterLessorReasons = contract.CrCasRenterLessorReasons;
                        _unitOfWork.CrCasRenterLessor.Update(singlExist);
                    }
                    _unitOfWork.Complete();

                    // SaveTracing
                    var RecordAr = contract.CrCasRenterLessorNavigation.CrMasRenterInformationArName;
                    var RecordEn = contract.CrCasRenterLessorNavigation.CrMasRenterInformationEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    // Save Adminstrive Procedures
                    await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "247", "20", currentUser.CrMasUserInformationLessor, "100",
                    model.CrCasRenterLessorId, null, null, null, null, null, null, null, null, "تعديل", "Edit", "U", null);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "RenterLessorInformation");
        }

        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarRenter";
            ViewBag.no = "0";
            var titles = await setTitle("205", "2203001", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            //var (mainTask, subTask, system, currentUser) = await SetTrace("203", "2203001", "2");

            //var RenterLessorInformationAll = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == currentUser.CrMasUserInformationLessor, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost" }).ToList();

            ViewBag.Data = "0";
            //_toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return View();


        }
    }
}
