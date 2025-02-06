using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.CAS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using Pipelines.Sockets.Unofficial.Buffers;
using System.Globalization;
using System.Numerics;
using ErrorResponse = Bnan.Ui.ViewModels.CAS.ErrorResponse;

namespace Bnan.Ui.Areas.CAS.Controllers.Services
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class RenterDriverController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IRenterDriver _renterDriver;
        private readonly IBaseRepo _baseRepo;
        private readonly IRenterDriver_CAS _RenterDriver_CAS;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterDriverController> _localizer;
        private readonly string pageNumber = SubTasks.ServicesCas_Drivers;

        public RenterDriverController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IRenterDriver_CAS RenterDriver_CAS,
            IMapper mapper, IUserService userService, IRenterDriver renterDriver, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterDriverController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _renterDriver = renterDriver;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _RenterDriver_CAS = RenterDriver_CAS;
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
            var RenterDrivers = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAllAsNoTrackingAsync(x => (x.CrCasRenterPrivateDriverInformationStatus == Status.Active || x.CrCasRenterPrivateDriverInformationStatus == Status.Rented) && x.CrCasRenterPrivateDriverInformationLessor==user.CrMasUserInformationLessor,
                new[] { "CrCasRenterPrivateDriverInformationGenderNavigation", "CrCasRenterPrivateDriverInformationIdtrypeNavigation", "CrCasRenterPrivateDriverInformationLicenseTypeNavigation", "CrCasRenterPrivateDriverInformationNationalityNavigation" } );

            // If no active licenses, retrieve all licenses
            if (RenterDrivers.Count == 0)
            {
                RenterDrivers = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAllAsNoTrackingAsync(x => (x.CrCasRenterPrivateDriverInformationStatus == Status.Hold || x.CrCasRenterPrivateDriverInformationStatus == Status.Rented) && x.CrCasRenterPrivateDriverInformationLessor == user.CrMasUserInformationLessor,
                                new[] { "CrCasRenterPrivateDriverInformationGenderNavigation", "CrCasRenterPrivateDriverInformationIdtrypeNavigation", "CrCasRenterPrivateDriverInformationLicenseTypeNavigation", "CrCasRenterPrivateDriverInformationNationalityNavigation" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

            return View(RenterDrivers);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetRenterDriverByStatus(string status)
        {
            // Get Lessor Code
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (!string.IsNullOrEmpty(status))
            {
                var RenterDriverbyStatusAll = _unitOfWork.CrCasRenterPrivateDriverInformation.FindAll(x => x.CrCasRenterPrivateDriverInformationLessor == lessorCode,
                new[] { "CrCasRenterPrivateDriverInformationGenderNavigation", "CrCasRenterPrivateDriverInformationIdtrypeNavigation", "CrCasRenterPrivateDriverInformationLicenseTypeNavigation", "CrCasRenterPrivateDriverInformationNationalityNavigation" }).ToList();
                if (status == Status.All) return PartialView("_DataTableRenterDriver", RenterDriverbyStatusAll.Where(x=>x.CrCasRenterPrivateDriverInformationStatus!=Status.Deleted));
                if(status == Status.Active) return PartialView("_DataTableRenterDriver", RenterDriverbyStatusAll.Where(x=>x.CrCasRenterPrivateDriverInformationStatus==Status.Active || x.CrCasRenterPrivateDriverInformationStatus == Status.Rented));
                 RenterDriverbyStatusAll = _unitOfWork.CrCasRenterPrivateDriverInformation.FindAll(l => l.CrCasRenterPrivateDriverInformationStatus == status && l.CrCasRenterPrivateDriverInformationLessor == lessorCode).ToList();
                return PartialView("_DataTableRenterDriver", RenterDriverbyStatusAll.Where(x=>x.CrCasRenterPrivateDriverInformationStatus==status));
            }
            return PartialView();
        }


        [HttpGet]
        public JsonResult GetRenterDriverNationalityEn(string? prefix)
        {

            var res = _unitOfWork.CrMasSupRenterNationality.GetAll();
            var list = res.ToList();
            var NationEnglish = (from c in list
                                 where c.CrMasSupRenterNationalitiesEnName.Contains(prefix) && c.CrMasSupRenterNationalitiesStatus == "A" &&
                                 c.CrMasSupRenterNationalitiesCode != "1000000001" && c.CrMasSupRenterNationalitiesCode != "1000000002"
                                 select new
                                 {
                                     label = c.CrMasSupRenterNationalitiesEnName,
                                     val = c.CrMasSupRenterNationalitiesEnName
                                 }).ToList();
            return Json(NationEnglish);
        }

        [HttpGet]
        public JsonResult GetRenterDriverNationalityAr(string? prefix)
        {

            var res = _unitOfWork.CrMasSupRenterNationality.GetAll();
            var list = res.ToList();
            var NationAr = (from c in list
                            where c.CrMasSupRenterNationalitiesArName.Contains(prefix) && c.CrMasSupRenterNationalitiesStatus == "A" &&
                            c.CrMasSupRenterNationalitiesCode != "1000000001" && c.CrMasSupRenterNationalitiesCode != "1000000002"
                            select new
                            {
                                label = c.CrMasSupRenterNationalitiesArName,
                                val = c.CrMasSupRenterNationalitiesArName
                            }).ToList();
            return Json(NationAr);
        }


        [HttpGet]
        public ActionResult GetCode(string selectedOption)
        {

            var res = _unitOfWork.CrMasSupRenterNationality.GetAll();
            var list = res.ToList();
            var Code = (from c in list
                        where c.CrMasSupRenterNationalitiesArName == selectedOption || c.CrMasSupRenterNationalitiesEnName == selectedOption
                        select c.CrMasSupRenterNationalitiesCode).ToList()[0].ToString();

            return Json(new { data1 = Code });
        }




        [HttpGet]
        public async Task<IActionResult> AddRenterDriver()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "RenterDriver");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterDriver");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);


            var Drivers = await _unitOfWork.CrCasRenterPrivateDriverInformation.GetAllAsync();

            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem2 { Value = c.CrMasSysCallingKeysCode.ToString(), text_Ar = c.CrMasSysCallingKeysNo }).ToList();


            // View the License
            var LicenseType = _unitOfWork.CrMasSupRenterDrivingLicense.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active);
            var LicenseTypeAr = LicenseType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterDrivingLicenseCode.ToString(), text_Ar = c.CrMasSupRenterDrivingLicenseArName, text_En = c.CrMasSupRenterDrivingLicenseEnName }).ToList();

            // View the License
            var Nationality = _unitOfWork.CrMasSupRenterNationality.FindAll(x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted);
            var NationalitiesAr = Nationality.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterNationalitiesCode.ToString(), text_Ar = c.CrMasSupRenterNationalitiesArName, text_En = c.CrMasSupRenterNationalitiesEnName }).ToList();

            // View the License
            var IDType = _unitOfWork.CrMasSupRenterIdtype.FindAll(x => x.CrMasSupRenterIdtypeStatus == Status.Active && x.CrMasSupRenterIdtypeCode != "3" && x.CrMasSupRenterIdtypeCode != "4");
            var IDTypeAr = IDType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterIdtypeCode.ToString(), text_Ar = c.CrMasSupRenterIdtypeArName, text_En = c.CrMasSupRenterIdtypeEnName }).ToList();


            // Get Lessor Code
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            ViewBag.LessorID = lessorCode;
            RenterDriverVM renterDriverVM = new RenterDriverVM();
            renterDriverVM.CrCasRenterPrivateDriverInformationLessor = lessorCode;
            renterDriverVM.ToDay = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            renterDriverVM.before7Y = DateTime.Now.AddYears(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            renterDriverVM.before100Y = DateTime.Now.AddYears(-100).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            renterDriverVM.after10Y = DateTime.Now.AddYears(10).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            renterDriverVM.all_callingKey = callingKeyList;
            renterDriverVM.all_IdType = IDTypeAr;
            renterDriverVM.all_LicenseType = LicenseTypeAr;
            renterDriverVM.all_Nationalities = NationalitiesAr;
            return View(renterDriverVM);
        }



        [HttpPost]
        public async Task<IActionResult> AddRenterDriver(RenterDriverVM RenterDrivermodel, IFormFile? SignatureImg, IFormFile? IDImg, IFormFile? LicenseImg)
        {

            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem2 { Value = c.CrMasSysCallingKeysCode.ToString(), text_Ar = c.CrMasSysCallingKeysNo }).ToList();


            // View the License
            var LicenseType = _unitOfWork.CrMasSupRenterDrivingLicense.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active);
            var LicenseTypeAr = LicenseType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterDrivingLicenseCode.ToString(), text_Ar = c.CrMasSupRenterDrivingLicenseArName, text_En = c.CrMasSupRenterDrivingLicenseEnName }).ToList();

            // View the License
            var Nationality = _unitOfWork.CrMasSupRenterNationality.FindAll(x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted);
            var NationalitiesAr = Nationality.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterNationalitiesCode.ToString(), text_Ar = c.CrMasSupRenterNationalitiesArName, text_En = c.CrMasSupRenterNationalitiesEnName }).ToList();

            // View the License
            var IDType = _unitOfWork.CrMasSupRenterIdtype.FindAll(x => x.CrMasSupRenterIdtypeStatus == Status.Active && x.CrMasSupRenterIdtypeCode != "3" && x.CrMasSupRenterIdtypeCode != "4");
            var IDTypeAr = IDType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterIdtypeCode.ToString(), text_Ar = c.CrMasSupRenterIdtypeArName, text_En = c.CrMasSupRenterIdtypeEnName }).ToList();

            RenterDrivermodel.all_callingKey = callingKeyList;
            RenterDrivermodel.all_IdType = IDTypeAr;
            RenterDrivermodel.all_LicenseType = LicenseTypeAr;
            RenterDrivermodel.all_Nationalities = NationalitiesAr;
            RenterDrivermodel.ToDay = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            RenterDrivermodel.before7Y = DateTime.Now.AddYears(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            RenterDrivermodel.before100Y = DateTime.Now.AddYears(-100).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            RenterDrivermodel.after10Y = DateTime.Now.AddYears(10).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Get Lessor Code
            var user = await _userManager.GetUserAsync(User);
            var lessorCode = user.CrMasUserInformationLessor;
            RenterDrivermodel.CrCasRenterPrivateDriverInformationLessor = lessorCode;
            ViewBag.LessorID = lessorCode;
            string currentCulture = CultureInfo.CurrentCulture.Name;
            var RenterDrivers = _unitOfWork.CrCasRenterPrivateDriverInformation.FindAll(x=>x.CrCasRenterPrivateDriverInformationLessor == lessorCode);
            var existingIDriverLessor = RenterDrivers.FirstOrDefault(x => x.CrCasRenterPrivateDriverInformationId == RenterDrivermodel.CrCasRenterPrivateDriverInformationId );
            var existingIDriverArNameLessor = RenterDrivers.FirstOrDefault(x => x.CrCasRenterPrivateDriverInformationArName == RenterDrivermodel.CrCasRenterPrivateDriverInformationArName );
            var existingIDriverEnNameLessor = RenterDrivers.FirstOrDefault(x => x.CrCasRenterPrivateDriverInformationEnName == RenterDrivermodel.CrCasRenterPrivateDriverInformationEnName );

            if (existingIDriverLessor != null) ModelState.AddModelError("CrCasRenterPrivateDriverInformationId", _localizer["DriverIdIsExist"]);
            if (existingIDriverArNameLessor != null) ModelState.AddModelError("CrCasRenterPrivateDriverInformationArName", _localizer["IsExist"]);
            if (existingIDriverEnNameLessor != null) ModelState.AddModelError("CrCasRenterPrivateDriverInformationEnName", _localizer["IsExist"]);


            if (!ModelState.IsValid || RenterDrivermodel == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterDriver", RenterDrivermodel);
            }
            try
            {
                string foldername = $"{"images\\Company"}\\{lessorCode}\\{"Drivers"}\\{RenterDrivermodel.CrCasRenterPrivateDriverInformationId}";
                string filePathImageSignature = null;
                string filePathImageID = null;
                string filePathImageLicense = null;
                if (IDImg != null)
                {
                    string fileNameImg = RenterDrivermodel.CrCasRenterPrivateDriverInformationEnName + "_ID_" + RenterDrivermodel.CrCasRenterPrivateDriverInformationId.ToString().Substring(RenterDrivermodel.CrCasRenterPrivateDriverInformationId.ToString().Length - 3);
                    filePathImageID = await IDImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                }
                if (LicenseImg != null)
                {
                    string fileNameImg = RenterDrivermodel.CrCasRenterPrivateDriverInformationEnName + "_License_" + RenterDrivermodel.CrCasRenterPrivateDriverInformationId.ToString().Substring(RenterDrivermodel.CrCasRenterPrivateDriverInformationId.ToString().Length - 3);
                    filePathImageLicense = await LicenseImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                }
                if (SignatureImg != null)
                {
                    string fileNameImg = RenterDrivermodel.CrCasRenterPrivateDriverInformationEnName + "_SignatureImg_" + RenterDrivermodel.CrCasRenterPrivateDriverInformationId.ToString().Substring(RenterDrivermodel.CrCasRenterPrivateDriverInformationId.ToString().Length - 3);
                    filePathImageSignature = await SignatureImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                }

                RenterDrivermodel.CrCasRenterPrivateDriverInformationIdImage = filePathImageID;
                RenterDrivermodel.CrCasRenterPrivateDriverInformationSignature = filePathImageSignature;
                RenterDrivermodel.CrCasRenterPrivateDriverInformationLicenseImage = filePathImageLicense;

                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var RenterDriverEntity = _mapper.Map<CrCasRenterPrivateDriverInformation>(RenterDrivermodel);

                // Check if the entity already exists
                if (await _RenterDriver_CAS.ExistsByDetails_AddAsync(RenterDriverEntity))
                {
                    await AddModelErrorsAsync(RenterDriverEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterDriver", RenterDrivermodel);
                }


                RenterDriverEntity.CrCasRenterPrivateDriverInformationStatus = "A";
                RenterDriverEntity.CrCasRenterPrivateDriverInformationIssueIdDate = DateTime.Now;
                await _unitOfWork.CrCasRenterPrivateDriverInformation.AddAsync(RenterDriverEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, RenterDriverEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterDriver", RenterDrivermodel);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, string lessor)
        {
            var user = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(lessor)) lessor = user?.CrMasUserInformationLessor ?? " ";
            await SetPageTitleAsync(Status.Update, pageNumber);

            //var drivers = await _unitOfWork.CrCasRenterPrivateDriverInformation.GetAllAsync();
            //var driver = drivers.Where(x => x.CrCasRenterPrivateDriverInformationId == id && x.CrCasRenterPrivateDriverInformationLessor == lessor).FirstOrDefault();
            var driver = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAsync(x => x.CrCasRenterPrivateDriverInformationId == id && x.CrCasRenterPrivateDriverInformationLessor == lessor);

            if (driver == null || user==null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterDriver");
            }
            var RenterDrivermodel = _mapper.Map<RenterDriverVM>(driver);

            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem2 { Value = c.CrMasSysCallingKeysCode.ToString(), text_Ar = c.CrMasSysCallingKeysNo }).ToList();


            // View the License
            var LicenseType = _unitOfWork.CrMasSupRenterDrivingLicense.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active);
            var LicenseTypeAr = LicenseType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterDrivingLicenseCode.ToString(), text_Ar = c.CrMasSupRenterDrivingLicenseArName, text_En = c.CrMasSupRenterDrivingLicenseEnName }).ToList();

            // View the License
            var Nationality = _unitOfWork.CrMasSupRenterNationality.FindAll(x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted && x.CrMasSupRenterNationalitiesCode == driver.CrCasRenterPrivateDriverInformationNationality);
            var NationalitiesAr = Nationality.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterNationalitiesCode.ToString(), text_Ar = c.CrMasSupRenterNationalitiesArName, text_En = c.CrMasSupRenterNationalitiesEnName }).ToList();

            // View the License
            var IDType = _unitOfWork.CrMasSupRenterIdtype.FindAll(x => x.CrMasSupRenterIdtypeStatus == Status.Active && x.CrMasSupRenterIdtypeCode != "3" && x.CrMasSupRenterIdtypeCode != "4");
            var IDTypeAr = IDType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterIdtypeCode.ToString(), text_Ar = c.CrMasSupRenterIdtypeArName, text_En = c.CrMasSupRenterIdtypeEnName }).ToList();

            // View the License
            var Genders = _unitOfWork.CrMasSupRenterGender.FindAll(x => x.CrMasSupRenterGenderStatus == Status.Active);
            var GendersAr = Genders.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterGenderCode.ToString(), text_Ar = c.CrMasSupRenterGenderArName, text_En = c.CrMasSupRenterGenderEnName }).ToList();

            RenterDrivermodel.all_callingKey = callingKeyList;
            RenterDrivermodel.all_IdType = IDTypeAr;
            RenterDrivermodel.all_LicenseType = LicenseTypeAr;
            RenterDrivermodel.all_Nationalities = NationalitiesAr;
            RenterDrivermodel.all_Genders = GendersAr;
            RenterDrivermodel.ToDay = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            RenterDrivermodel.before7Y = DateTime.Now.AddYears(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            RenterDrivermodel.before100Y = DateTime.Now.AddYears(-100).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            RenterDrivermodel.after10Y = DateTime.Now.AddYears(10).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Get Lessor Code
            var lessorCode = user?.CrMasUserInformationLessor??" ";
            RenterDrivermodel.CrCasRenterPrivateDriverInformationLessor = lessorCode;
            return View(RenterDrivermodel);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RenterDriverVM model, IFormFile? SignatureImg, IFormFile? IDImg, IFormFile? LicenseImg)
        {

            var user = await _userManager.GetUserAsync(User);

            var driver =await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAsync(x => x.CrCasRenterPrivateDriverInformationId == model.CrCasRenterPrivateDriverInformationId&&x.CrCasRenterPrivateDriverInformationLessor==user.CrMasUserInformationLessor);
            if (user == null || driver == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterDriver");
            }
            
            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem2 { Value = c.CrMasSysCallingKeysCode.ToString(), text_Ar = c.CrMasSysCallingKeysNo }).ToList();


            // View the License
            var LicenseType = _unitOfWork.CrMasSupRenterDrivingLicense.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active);
            var LicenseTypeAr = LicenseType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterDrivingLicenseCode.ToString(), text_Ar = c.CrMasSupRenterDrivingLicenseArName, text_En = c.CrMasSupRenterDrivingLicenseEnName }).ToList();

            // View the License
            var Nationality = _unitOfWork.CrMasSupRenterNationality.FindAll(x => x.CrMasSupRenterNationalitiesStatus != Status.Deleted);
            var NationalitiesAr = Nationality.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterNationalitiesCode.ToString(), text_Ar = c.CrMasSupRenterNationalitiesArName, text_En = c.CrMasSupRenterNationalitiesEnName }).ToList();

            // View the License
            var IDType = _unitOfWork.CrMasSupRenterIdtype.FindAll(x => x.CrMasSupRenterIdtypeStatus == Status.Active && x.CrMasSupRenterIdtypeCode != "3" && x.CrMasSupRenterIdtypeCode != "4");
            var IDTypeAr = IDType.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterIdtypeCode.ToString(), text_Ar = c.CrMasSupRenterIdtypeArName, text_En = c.CrMasSupRenterIdtypeEnName }).ToList();

            // View the License
            var Genders = _unitOfWork.CrMasSupRenterGender.FindAll(x => x.CrMasSupRenterGenderStatus == Status.Active);
            var GendersAr = Genders.Select(c => new SelectListItem2 { Value = c.CrMasSupRenterGenderCode.ToString(), text_Ar = c.CrMasSupRenterGenderArName, text_En = c.CrMasSupRenterGenderEnName }).ToList();

            model.all_callingKey = callingKeyList;
            model.all_IdType = IDTypeAr;
            model.all_LicenseType = LicenseTypeAr;
            model.all_Nationalities = NationalitiesAr;
            model.all_Genders = GendersAr;
            model.ToDay = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            model.before7Y = DateTime.Now.AddYears(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            model.before100Y = DateTime.Now.AddYears(-100).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            model.after10Y = DateTime.Now.AddYears(10).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Get Lessor Code
            var lessorCode = user.CrMasUserInformationLessor;
            model.CrCasRenterPrivateDriverInformationLessor = lessorCode;

            try
            {
                if (ModelState.IsValid)
                {
                    string foldername = $"{"images\\Company"}\\{user.CrMasUserInformationLessor}\\{"Drivers"}\\{model.CrCasRenterPrivateDriverInformationId}";
                    string filePathImageSignature = null;
                    string filePathImageID = null;
                    string filePathImageLicense = null;
                    if (IDImg != null)
                    {
                        string fileNameImg = model.CrCasRenterPrivateDriverInformationEnName + "_ID_" + model.CrCasRenterPrivateDriverInformationId.ToString().Substring(model.CrCasRenterPrivateDriverInformationId.ToString().Length - 3);
                        filePathImageID = await IDImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                        model.CrCasRenterPrivateDriverInformationIdImage = filePathImageID;
                    }
                    if (LicenseImg != null)
                    {
                        string fileNameImg = model.CrCasRenterPrivateDriverInformationEnName + "_License_" + model.CrCasRenterPrivateDriverInformationId.ToString().Substring(model.CrCasRenterPrivateDriverInformationId.ToString().Length - 3);
                        filePathImageLicense = await LicenseImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                        model.CrCasRenterPrivateDriverInformationLicenseImage = filePathImageLicense;
                    }
                    if (SignatureImg != null)
                    {
                        string fileNameImg = model.CrCasRenterPrivateDriverInformationEnName + "_SignatureImg_" + model.CrCasRenterPrivateDriverInformationId.ToString().Substring(model.CrCasRenterPrivateDriverInformationId.ToString().Length - 3);
                        filePathImageSignature = await SignatureImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                        model.CrCasRenterPrivateDriverInformationSignature = filePathImageSignature;
                    }
                    var RenterDriver = _mapper.Map<CrCasRenterPrivateDriverInformation>(model);
                    // Check if the entity already exists
                    if (await _RenterDriver_CAS.ExistsByDetailsAsync(RenterDriver))
                    {
                        await SetPageTitleAsync(Status.Update, pageNumber);
                        await AddModelErrorsAsync(RenterDriver);
                        _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return View("Edit", model);
                    }
                    driver.CrCasRenterPrivateDriverInformationBirthDate = RenterDriver.CrCasRenterPrivateDriverInformationBirthDate;
                    driver.CrCasRenterPrivateDriverInformationEmail = RenterDriver.CrCasRenterPrivateDriverInformationEmail;
                    driver.CrCasRenterPrivateDriverInformationKeyMobile = RenterDriver.CrCasRenterPrivateDriverInformationKeyMobile;
                    driver.CrCasRenterPrivateDriverInformationMobile = RenterDriver.CrCasRenterPrivateDriverInformationMobile;
                    driver.CrCasRenterPrivateDriverInformationReasons = RenterDriver.CrCasRenterPrivateDriverInformationReasons;
                    driver.CrCasRenterPrivateDriverInformationLicenseDate = RenterDriver.CrCasRenterPrivateDriverInformationLicenseDate;
                    driver.CrCasRenterPrivateDriverInformationLicenseExpiry = RenterDriver.CrCasRenterPrivateDriverInformationLicenseExpiry;
                    driver.CrCasRenterPrivateDriverInformationLicenseNo = RenterDriver.CrCasRenterPrivateDriverInformationLicenseNo;
                    driver.CrCasRenterPrivateDriverInformationLicenseType = RenterDriver.CrCasRenterPrivateDriverInformationLicenseType;

                    _unitOfWork.CrCasRenterPrivateDriverInformation.Update(driver);
                    if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                    await SaveTracingForLicenseChange(user, RenterDriver, Status.Update);
                    return RedirectToAction("Index", "RenterDriver");
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", model);
            }
            return View("Edit", model);
        }


        [HttpPost]
        public async Task<string> EditStatus(string status, string code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAsync(x => x.CrCasRenterPrivateDriverInformationId == code && x.CrCasRenterPrivateDriverInformationLessor == user.CrMasUserInformationLessor);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _RenterDriver_CAS.CheckIfCanEditStatus_It(licence.CrCasRenterPrivateDriverInformationId, user.CrMasUserInformationLessor)) return "udelete"; }
                if ( status == Status.Hold) { if (!await _RenterDriver_CAS.CheckIfCanEditStatus_It(licence.CrCasRenterPrivateDriverInformationId, user.CrMasUserInformationLessor)) return "un_NoUpdateStatus"; }

                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrCasRenterPrivateDriverInformationStatus = status;
                _unitOfWork.CrCasRenterPrivateDriverInformation.Update(licence);
                _unitOfWork.Complete();
                await SaveTracingForLicenseChange(user, licence, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }

        }



        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrCasRenterPrivateDriverInformation entity)
        {


            if (await _RenterDriver_CAS.ExistsByArabicNameAsync(entity.CrCasRenterPrivateDriverInformationArName, entity.CrCasRenterPrivateDriverInformationId, entity.CrCasRenterPrivateDriverInformationLessor))
            {
                ModelState.AddModelError("CrCasRenterPrivateDriverInformationArName", _localizer["Existing"]);
            }

            if (await _RenterDriver_CAS.ExistsByEnglishNameAsync(entity.CrCasRenterPrivateDriverInformationEnName, entity.CrCasRenterPrivateDriverInformationId, entity.CrCasRenterPrivateDriverInformationLessor))
            {
                ModelState.AddModelError("CrCasRenterPrivateDriverInformationEnName", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var user = await _userManager.GetUserAsync(User);

            var All_RenterDriver_CASs = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAllAsync(x => x.CrCasRenterPrivateDriverInformationLessor == user.CrMasUserInformationLessor);
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterDriver_CASs != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrCasRenterPrivateDriverInformationId" && All_RenterDriver_CASs.Any(x => x.CrCasRenterPrivateDriverInformationId == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasRenterPrivateDriverInformationId", Message = _localizer["Existing"] });
                }
                // Check for existing Arabic driving license
                if (existName == "CrCasRenterPrivateDriverInformationArName" && All_RenterDriver_CASs.Any(x => x.CrCasRenterPrivateDriverInformationArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasRenterPrivateDriverInformationArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrCasRenterPrivateDriverInformationEnName" && All_RenterDriver_CASs.Any(x => x.CrCasRenterPrivateDriverInformationEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasRenterPrivateDriverInformationEnName", Message = _localizer["Existing"] });
                }

            }

            return Json(new { errors });
        }

        //Helper Methods 
        //private async Task<string> GenerateLicenseCodeAsync(string lessorCode)
        //{
        //    var allLicenses = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAllAsync(x => x.CrCasRenterPrivateDriverInformationLessor == lessorCode);
        //    return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrCasRenterPrivateDriverInformationId) + 1).ToString() : "7004000001";
        //}
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrCasRenterPrivateDriverInformation licence, string status)
        {


            var recordAr = licence.CrCasRenterPrivateDriverInformationArName;
            var recordEn = licence.CrCasRenterPrivateDriverInformationEnName;
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
            return RedirectToAction("Index", "RenterDriver");
        }
    }
}
