using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
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
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]

    public class RenterLessorInformationController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IRenterLessorInformation _RenterLessorInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterLessorInformationController> _localizer;
        private readonly string pageNumber = SubTasks.RentersCas_RentersData;


        public RenterLessorInformationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IRenterLessorInformation lessorOwners_CAS, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterLessorInformationController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _RenterLessorInformation = lessorOwners_CAS;
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
            // Retrieve active driving licenses
            var RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllAsync(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus==Status.Active, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });

            // If no active licenses, retrieve all licenses
            if (RenterLessorInformationAll.Count() == 0)
            {
                RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllAsync(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus == Status.Rented, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });

                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

            var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();
            //ViewData["Rates"] = rates;
            RenterLessorInformation_CASVM VM = new RenterLessorInformation_CASVM();
            VM.all_Rates = rates;
            VM.all_RentersData = RenterLessorInformationAll?.ToList()?? new List<CrCasRenterLessor>();
            return View(VM);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetRenterLessorInformationByStatusAsync(string status)
        {

            var user = await _userManager.GetUserAsync(User);
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var rates = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();

                var RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllAsync(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus == Status.Rented, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });

              
                RenterLessorInformation_CASVM vm = new RenterLessorInformation_CASVM();
                if (status == Status.All)
                {
                    var FilterAll = RenterLessorInformationAll.Where(x => x.CrCasRenterLessorStatus != Status.Deleted);
                    vm.all_RentersData = FilterAll?.ToList() ?? new List<CrCasRenterLessor>();
                    vm.all_Rates = rates;
                    return PartialView("_DataTableRenterLessorInformation", vm);
                }
                var FilterByStatus = RenterLessorInformationAll.Where(x => x.CrCasRenterLessorStatus == status);
                vm.all_RentersData = FilterByStatus?.ToList() ?? new List<CrCasRenterLessor>();
                vm.all_Rates = rates;
                return PartialView("_DataTableRenterLessorInformation", vm);
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
            var (mainTask, subTask, system, user) = await SetTrace("203", "2203001", "2");


            var contract = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == id && x.CrCasRenterLessorCode == user.CrMasUserInformationLessor, new[] { "CrCasRenterLessorStatisticsNationalitiesNavigation", "CrCasRenterLessorNavigation.CrMasRenterPost", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorIdtrypeNavigation", "CrCasRenterLessorSectorNavigation", "CrCasRenterLessorStatisticsGenderNavigation", "CrCasRenterLessorMembershipNavigation" });
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
                //var (mainTask, subTask, system, user) = await SetTrace("203", "2203001", "2");

                if (model != null)
                {
                    var contract = _mapper.Map<CrCasRenterLessor>(model);

                    var singlExist = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == contract.CrCasRenterLessorId && x.CrCasRenterLessorCode == user.CrMasUserInformationLessor);
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
                    //await _userLoginsService.SaveTracing(user.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    //subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    //subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    //// Save Adminstrive Procedures
                    //await _adminstritiveProcedures.SaveAdminstritive(user.CrMasUserInformationCode, "1", "247", "20", user.CrMasUserInformationLessor, "100",
                    //model.CrCasRenterLessorId, null, null, null, null, null, null, null, null, "تعديل", "Edit", "U", null);

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
            //var (mainTask, subTask, system, user) = await SetTrace("203", "2203001", "2");

            //var RenterLessorInformationAll = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost" }).ToList();

            ViewBag.Data = "0";
            //_toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return View();


        }
    }
}
