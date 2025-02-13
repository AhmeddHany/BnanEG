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
using DocumentFormat.OpenXml.Spreadsheet;
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
        private readonly IWebHostEnvironment _env;



        public RenterLessorInformationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IWebHostEnvironment env,
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
            _env = env;
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
            //var RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllAsync(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus==Status.Active, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });

    var RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
    predicate: x => x.CrCasRenterLessorStatus == Status.Active && x.CrCasRenterLessorCode == user.CrMasUserInformationLessor,
    //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
    selectProjection: query => query.Select(x => new RenterLessorInformation_SingleVM
    {
        CrCasRenterLessorCode = x.CrCasRenterLessorCode,
        CrCasRenterLessorId = x.CrCasRenterLessorId,
        CrCasRenterLessorCopyId = x.CrCasRenterLessorCopyId,
        CrCasRenterLessorDateLastContractual = x.CrCasRenterLessorDateLastContractual,
        CrCasRenterLessorContractCount = x.CrCasRenterLessorContractCount,
        CrCasRenterLessorStatisticsAge = x.CrCasRenterLessorStatisticsAge,
        CrCasRenterLessorStatisticsTraded = x.CrCasRenterLessorStatisticsTraded,
        CrCasRenterLessorDealingMechanism = x.CrCasRenterLessorDealingMechanism,
        CrCasRenterLessorStatus = x.CrCasRenterLessorStatus,
        CrCasRenterLessorReasons = x.CrCasRenterLessorReasons,
        addressAr = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateArName,
        addressEn = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateEnName,
        jobAr = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsArName,
        jobEn = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsEnName,
        nationalityAr = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesArName,
        nationalityEn = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesEnName,
        //RateAr = x.CrMasUserInformationOperationStatus,
        //RateEn = x.CrMasUserInformationStatus,
        RenterNameAr = x.CrCasRenterLessorNavigation.CrMasRenterInformationArName,
        RenterNameEn = x.CrCasRenterLessorNavigation.CrMasRenterInformationEnName,
        Email = x.CrCasRenterLessorNavigation.CrMasRenterInformationEmail,

    })
    , includes: new string[] { "CrCasRenterLessorNavigation", "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" }
    );

            // If no active licenses, retrieve all licenses
            if (RenterLessorInformationAll.Count() == 0)
            {
                //RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllAsync(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus == Status.Rented, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });

                RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasRenterLessorStatus == Status.Rented && x.CrCasRenterLessorCode==user.CrMasUserInformationLessor,
                    //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
                    selectProjection: query => query.Select(x => new RenterLessorInformation_SingleVM
                    {
                        CrCasRenterLessorCode = x.CrCasRenterLessorCode,
                        CrCasRenterLessorId = x.CrCasRenterLessorId,
                        CrCasRenterLessorCopyId = x.CrCasRenterLessorCopyId,
                        CrCasRenterLessorDateLastContractual = x.CrCasRenterLessorDateLastContractual,
                        CrCasRenterLessorContractCount = x.CrCasRenterLessorContractCount,
                        CrCasRenterLessorStatisticsAge = x.CrCasRenterLessorStatisticsAge,
                        CrCasRenterLessorStatisticsTraded = x.CrCasRenterLessorStatisticsTraded,
                        CrCasRenterLessorDealingMechanism = x.CrCasRenterLessorDealingMechanism,
                        CrCasRenterLessorStatus = x.CrCasRenterLessorStatus,
                        CrCasRenterLessorReasons = x.CrCasRenterLessorReasons,
                        addressAr = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateArName,
                        addressEn = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateEnName,
                        jobAr = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsArName,
                        jobEn = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsEnName,
                        nationalityAr = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesArName,
                        nationalityEn = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesEnName,
                        //RateAr = x.CrMasUserInformationOperationStatus,
                        //RateEn = x.CrMasUserInformationStatus,
                        RenterNameAr = x.CrCasRenterLessorNavigation.CrMasRenterInformationArName,
                        RenterNameEn = x.CrCasRenterLessorNavigation.CrMasRenterInformationEnName,
                        Email = x.CrCasRenterLessorNavigation.CrMasRenterInformationEmail,

                    })
                    , includes: new string[] {  "CrCasRenterLessorNavigation", "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" }
                    );
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

            var rates = await _unitOfWork.CrMasSysEvaluation.FindAllAsync(x => x.CrMasSysEvaluationsClassification == "1");
            var ratesList= rates?.ToList();
            //ViewData["Rates"] = rates;
            RenterLessorInformation_CASVM VM = new RenterLessorInformation_CASVM();
            VM.all_Rates = ratesList?? new List<CrMasSysEvaluation>();
            VM.all_RentersData = RenterLessorInformationAll ?? new List<RenterLessorInformation_SingleVM>();
            return View(VM);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetRenterLessorInformationByStatus(string status,string search)
        {

            var user = await _userManager.GetUserAsync(User);
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var rates = await _unitOfWork.CrMasSysEvaluation.FindAllAsync(x => x.CrMasSysEvaluationsClassification == "1");
                var ratesList = rates?.ToList();

                //var RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllAsync(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus == Status.Rented, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });

                var RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasRenterLessorStatus != Status.Deleted && x.CrCasRenterLessorCode == user.CrMasUserInformationLessor,
                    //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
                    selectProjection: query => query.Select(x => new RenterLessorInformation_SingleVM
                    {
                        CrCasRenterLessorCode = x.CrCasRenterLessorCode,
                        CrCasRenterLessorId = x.CrCasRenterLessorId,
                        CrCasRenterLessorCopyId = x.CrCasRenterLessorCopyId,
                        CrCasRenterLessorDateLastContractual = x.CrCasRenterLessorDateLastContractual,
                        CrCasRenterLessorContractCount = x.CrCasRenterLessorContractCount,
                        CrCasRenterLessorStatisticsAge = x.CrCasRenterLessorStatisticsAge,
                        CrCasRenterLessorStatisticsTraded = x.CrCasRenterLessorStatisticsTraded,
                        CrCasRenterLessorDealingMechanism = x.CrCasRenterLessorDealingMechanism,
                        CrCasRenterLessorStatus = x.CrCasRenterLessorStatus,
                        CrCasRenterLessorReasons = x.CrCasRenterLessorReasons,
                        addressAr = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateArName,
                        addressEn = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateEnName,
                        jobAr = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsArName,
                        jobEn = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsEnName,
                        nationalityAr = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesArName,
                        nationalityEn = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesEnName,
                        //RateAr = rates.Find(c=>c.CrMasSysEvaluationsCode== x.CrCasRenterLessorDealingMechanism).CrMasSysEvaluationsArDescription,
                        //RateEn = rates.Find(c=>c.CrMasSysEvaluationsCode== x.CrCasRenterLessorDealingMechanism).CrMasSysEvaluationsArDescription,
                        RenterNameAr = x.CrCasRenterLessorNavigation.CrMasRenterInformationArName,
                        RenterNameEn = x.CrCasRenterLessorNavigation.CrMasRenterInformationEnName,
                        Email = x.CrCasRenterLessorNavigation.CrMasRenterInformationEmail,

                    })
                    , includes: new string[] { "CrCasRenterLessorNavigation", "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" }
                    );
                IEnumerable<RenterLessorInformation_SingleVM>? enumerable = RenterLessorInformationAll;
                if (!string.IsNullOrEmpty(search))
                {
                    enumerable = enumerable.Where(x => (x.addressAr != null && x.addressAr.Contains(search)) || (x.addressEn != null && x.addressEn.ToLower().Contains(search.ToLower()))||
                    (x.jobAr != null && x.jobAr.Contains(search)) || (x.jobEn != null && x.jobEn.ToLower().Contains(search.ToLower())) ||
                    (x.nationalityAr != null && x.nationalityAr.Contains(search)) || (x.nationalityEn != null && x.nationalityEn.ToLower().Contains(search.ToLower())) ||
                    (x.RenterNameAr != null && x.RenterNameAr.Contains(search)) || (x.RenterNameEn != null && x.RenterNameEn.ToLower().Contains(search.ToLower())) ||
                    (x.WorkPlaceAr != null && x.WorkPlaceAr.Contains(search)) || (x.WorkPlaceEn != null && x.WorkPlaceEn.ToLower().Contains(search.ToLower())) ||
                   x.Email == search || x.CrCasRenterLessorId == search || x.CrCasRenterLessorDateLastContractual?.ToString("dd/MM/yyyy") == search
                        );
                }

                RenterLessorInformation_CASVM vm = new RenterLessorInformation_CASVM();
                if (status == Status.All)
                {
                    var FilterAll = enumerable.Where(x => x.CrCasRenterLessorStatus == Status.Active || x.CrCasRenterLessorStatus == Status.Rented);
                    vm.all_RentersData = FilterAll?.ToList() ?? new List<RenterLessorInformation_SingleVM>();
                    vm.all_Rates = ratesList ?? new List<CrMasSysEvaluation>();
                    return PartialView("_DataTableRenterLessorInformation", vm);
                }
                var FilterByStatus = enumerable.Where(x => x.CrCasRenterLessorStatus == status);
                vm.all_RentersData = FilterByStatus?.ToList() ?? new List<RenterLessorInformation_SingleVM>();
                vm.all_Rates = ratesList ?? new List<CrMasSysEvaluation>();
                return PartialView("_DataTableRenterLessorInformation", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            await SetPageTitleAsync(Status.Update, pageNumber);

            var rates = await _unitOfWork.CrMasSysEvaluation.FindAllAsync(x => x.CrMasSysEvaluationsClassification == "1");
            var ratesList = rates?.ToList();
            //var RenterLessorInformationAll = await _unitOfWork.CrCasRenterLessor.FindAllAsync(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorStatus == Status.Rented, new[] { "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation" });

            var ThisRenterData = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasRenterLessorStatus != Status.Deleted && x.CrCasRenterLessorCode == user.CrMasUserInformationLessor && x.CrCasRenterLessorId == id,
                //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
                selectProjection: query => query.Select(x => new RenterLessorInformation_Single_Edit_VM
                {
                    CrCasRenterLessorCode = x.CrCasRenterLessorCode,
                    CrCasRenterLessorId = x.CrCasRenterLessorId,
                    CrCasRenterLessorCopyId = x.CrCasRenterLessorCopyId,
                    CrCasRenterLessorDateLastContractual = x.CrCasRenterLessorDateLastContractual,
                    CrCasRenterLessorContractCount = x.CrCasRenterLessorContractCount,
                    CrCasRenterLessorStatisticsAge = x.CrCasRenterLessorStatisticsAge,
                    CrCasRenterLessorStatisticsTraded = x.CrCasRenterLessorStatisticsTraded,
                    CrCasRenterLessorDealingMechanism = x.CrCasRenterLessorDealingMechanism,
                    CrCasRenterLessorStatus = x.CrCasRenterLessorStatus,
                    CrCasRenterLessorReasons = x.CrCasRenterLessorReasons,
                    addressAr = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateArName,
                    addressEn = x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateEnName,
                    jobAr = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsArName,
                    jobEn = x.CrCasRenterLessorStatisticsJobsNavigation.CrMasSupRenterProfessionsEnName,
                    nationalityAr = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesArName,
                    nationalityEn = x.CrCasRenterLessorStatisticsNationalitiesNavigation.CrMasSupRenterNationalitiesEnName,
                    //RateAr = rates.Find(c=>c.CrMasSysEvaluationsCode== x.CrCasRenterLessorDealingMechanism).CrMasSysEvaluationsArDescription,
                    //RateEn = rates.Find(c=>c.CrMasSysEvaluationsCode== x.CrCasRenterLessorDealingMechanism).CrMasSysEvaluationsArDescription,
                    RenterNameAr = x.CrCasRenterLessorNavigation.CrMasRenterInformationArName,
                    RenterNameEn = x.CrCasRenterLessorNavigation.CrMasRenterInformationEnName,
                    Email = x.CrCasRenterLessorNavigation.CrMasRenterInformationEmail,
                    SignatureImage = x.CrCasRenterLessorNavigation.CrMasRenterInformationSignature,
                    IdTypeAr = x.CrCasRenterLessorIdtrypeNavigation.CrMasSupRenterIdtypeArName,
                    IdTypeEn = x.CrCasRenterLessorIdtrypeNavigation.CrMasSupRenterIdtypeEnName,
                    WorkPlaceAr = x.CrCasRenterLessorNavigation.CrMasRenterInformationEmployerNavigation.CrMasSupRenterEmployerArName,
                    WorkPlaceEn = x.CrCasRenterLessorNavigation.CrMasRenterInformationEmployerNavigation.CrMasSupRenterEmployerEnName,

                    License_Ar = x.CrCasRenterLessorNavigation.CrMasRenterInformationDrivingLicenseTypeNavigation.CrMasSupRenterDrivingLicenseArName,
                    License_En = x.CrCasRenterLessorNavigation.CrMasRenterInformationDrivingLicenseTypeNavigation.CrMasSupRenterDrivingLicenseEnName,

                    MempershipAr = x.CrCasRenterLessorMembershipNavigation.CrMasSupRenterMembershipArName,
                    MempershipEn = x.CrCasRenterLessorMembershipNavigation.CrMasSupRenterMembershipEnName,
                    BankAr = x.CrCasRenterLessorNavigation.CrMasRenterInformationBankNavigation.CrMasSupAccountBankArName,
                    BankEn = x.CrCasRenterLessorNavigation.CrMasRenterInformationBankNavigation.CrMasSupAccountBankEnName,
                    GenderAr = x.CrCasRenterLessorStatisticsGenderNavigation.CrMasSupRenterGenderArName,
                    GenderEn = x.CrCasRenterLessorStatisticsGenderNavigation.CrMasSupRenterGenderEnName,
                    Bank_Iban = x.CrCasRenterLessorNavigation.CrMasRenterInformationIban,
                    License_No = x.CrCasRenterLessorNavigation.CrMasRenterInformationDrivingLicenseNo,
                    birthDate = x.CrCasRenterLessorNavigation.CrMasRenterInformationBirthDate,
                    License_ExpiryDate = x.CrCasRenterLessorNavigation.CrMasRenterInformationExpiryDrivingLicenseDate,
                })
                , includes: new string[] { "CrCasRenterLessorCodeNavigation.CrCasAccountBanks", "CrCasRenterLessorNavigation.CrMasRenterInformationEmployerNavigation", "CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation", "CrCasRenterLessorStatisticsNationalitiesNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorIdtrypeNavigation", "CrCasRenterLessorStatisticsGenderNavigation", "CrCasRenterLessorMembershipNavigation" }
                );
            
            if (ThisRenterData == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterLessorInformation");
            }

            // الحصول على المسار لحد المجلد wwwroot
            string wwwrootPath = _env.WebRootPath;

            var image = ThisRenterData?.FirstOrDefault()?.SignatureImage?.Replace("~","") ?? " ";

            // تحديد المسار  الكامل 
            string filePath = wwwrootPath + image;

            if (System.IO.File.Exists(filePath))
            {
                //Console.WriteLine("الملف موجود.");
            }
            else
            {
                //Console.WriteLine("الملف غير موجود.");
                ThisRenterData.FirstOrDefault().SignatureImage = null;
            }

            RenterLessorInformation_CASVM VM = new RenterLessorInformation_CASVM();
            VM.This_RentersData_edit = ThisRenterData?.FirstOrDefault();
            VM.all_Rates = ratesList ?? new List<CrMasSysEvaluation>();
            VM.CrCasRenterLessorId = ThisRenterData?.FirstOrDefault()?.CrCasRenterLessorId?? " ";
            VM.CrCasRenterLessorCode = ThisRenterData?.FirstOrDefault()?.CrCasRenterLessorCode ?? " ";
            VM.CrCasRenterLessorReasons = ThisRenterData?.FirstOrDefault()?.CrCasRenterLessorReasons ?? " ";
            VM.CrCasRenterLessorDealingMechanism = ThisRenterData?.FirstOrDefault()?.CrCasRenterLessorDealingMechanism ?? " ";
            VM.CrCasRenterLessorStatus = ThisRenterData?.FirstOrDefault()?.CrCasRenterLessorStatus ?? " ";
            VM.RenterNameAr = ThisRenterData?.FirstOrDefault()?.RenterNameAr ?? " ";
            VM.RenterNameEn = ThisRenterData?.FirstOrDefault()?.RenterNameEn ?? " ";

            return View(VM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RenterLessorInformation_CASVM model)
        {
            var rates = await _unitOfWork.CrMasSysEvaluation.FindAllAsync(x => x.CrMasSysEvaluationsClassification == "1");
            var ratesList = rates?.ToList();
            model.all_Rates = ratesList ?? new List<CrMasSysEvaluation>();

            var user = await _userManager.GetUserAsync(User);
            var singlExist = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == model.CrCasRenterLessorId && x.CrCasRenterLessorCode == user.CrMasUserInformationLessor);

            if (user == null && singlExist == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterLessorInformation");
            }

            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", model);
                }
                singlExist.CrCasRenterLessorDealingMechanism = model.CrCasRenterLessorDealingMechanism;
                singlExist.CrCasRenterLessorReasons = model.CrCasRenterLessorReasons;

                _unitOfWork.CrCasRenterLessor.Update(singlExist);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, model, Status.Update);
                return RedirectToAction("Index", "RenterLessorInformation");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", model);
            }
        }

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, RenterLessorInformation_CASVM thisRenter, string status)
        {

            var recordAr = thisRenter.RenterNameAr;
            var recordEn = thisRenter.RenterNameEn;
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
            return RedirectToAction("Index", "LessorOwners_CAS");
        }
    }
}
