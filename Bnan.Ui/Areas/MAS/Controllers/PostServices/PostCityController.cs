using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Drawing;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.PostServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class PostCityController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasPostCity _masPostCity;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<PostCityController> _localizer;
        private readonly string pageNumber = SubTasks.City;


        public PostCityController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasPostCity masPostCity, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<PostCityController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masPostCity = masPostCity;
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
            var regions_Activated = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasSupPostRegionsStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSupPostRegion
                {
                    CrMasSupPostRegionsCode = x.CrMasSupPostRegionsCode,
                    CrMasSupPostRegionsArName = x.CrMasSupPostRegionsArName,
                    CrMasSupPostRegionsEnName = x.CrMasSupPostRegionsEnName
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var regions_Holded = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasSupPostRegionsStatus == Status.Hold,
                selectProjection: query => query.Select(x => new CrMasSupPostRegion
                {
                    CrMasSupPostRegionsCode = x.CrMasSupPostRegionsCode,
                    CrMasSupPostRegionsArName = x.CrMasSupPostRegionsArName,
                    CrMasSupPostRegionsEnName = x.CrMasSupPostRegionsEnName
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            // Retrieve active driving licenses
            var all_Cities2 = await _unitOfWork.CrMasSupPostCity.GetAllAsyncAsNoTrackingAsync();
            List<CrMasSupPostCity>? allCities = new List<CrMasSupPostCity>();
            allCities = all_Cities2.Where(x => x.CrMasSupPostCityStatus == Status.Active && regions_Activated.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim())).ToList();

            var City_count = await _unitOfWork.CrMasRenterPost.FindCountByColumnAsync<CrMasRenterPost>(
                predicate: x => x.CrMasRenterPostStatus != Status.Deleted,
                columnSelector: x => x.CrMasRenterPostCity  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            // If no active licenses, retrieve all licenses
            if (!allCities.Any())
            {
                allCities = all_Cities2.Where(x => regions_Holded.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim()) || x.CrMasSupPostCityStatus == Status.Hold && regions_Activated.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim())
              ).ToList();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

            PostCityVM vm = new PostCityVM();
            vm.PostCity = allCities;
            vm.City_count = City_count;
            vm.Regions = regions_Activated;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetPostCityByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {

                var all_Cities2 = await _unitOfWork.CrMasSupPostCity.GetAllAsyncAsNoTrackingAsync();
                List<CrMasSupPostCity>? allCities = new List<CrMasSupPostCity>();
                var City_count = await _unitOfWork.CrMasRenterPost.FindCountByColumnAsync<CrMasRenterPost>(
                    predicate: x => x.CrMasRenterPostStatus != Status.Deleted,
                    columnSelector: x => x.CrMasRenterPostCity  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                var allregions = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
                    predicate: null,
                    selectProjection: query => query.Select(x => new CrMasSupPostRegion
                    {
                        CrMasSupPostRegionsCode = x.CrMasSupPostRegionsCode,
                        CrMasSupPostRegionsArName = x.CrMasSupPostRegionsArName,
                        CrMasSupPostRegionsEnName = x.CrMasSupPostRegionsEnName,
                        CrMasSupPostRegionsStatus = x.CrMasSupPostRegionsStatus
                    })
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                var allregions_Activated = allregions.Where(x => x.CrMasSupPostRegionsStatus == Status.Active).ToList();
                PostCityVM vm = new PostCityVM();
                vm.City_count = City_count;
                vm.Regions = allregions_Activated;
                if (status == Status.All)
                {
                    var FilterAll = all_Cities2.Where(x => (x.CrMasSupPostCityStatus != Status.Deleted || allregions.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim() && y.CrMasSupPostRegionsStatus == Status.Deleted)) &&
                                                                         (x.CrMasSupPostCityArName.Contains(search) ||
                                                                          x.CrMasSupPostCityEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupPostCityCode.Contains(search))).ToList();
                    vm.PostCity = FilterAll;
                    return PartialView("_DataTablePostCity", vm);
                }
                if (status == Status.Active)
                {
                    var FilterByStatus3 = all_Cities2.Where(x => x.CrMasSupPostCityStatus == status && allregions.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim() && y.CrMasSupPostRegionsStatus == Status.Active) &&
                                                                                (
                                                                               x.CrMasSupPostCityArName.Contains(search) ||
                                                                               x.CrMasSupPostCityEnName.ToLower().Contains(search.ToLower()) ||
                                                                               x.CrMasSupPostCityCode.Contains(search))).ToList();
                    vm.PostCity = FilterByStatus3;
                    return PartialView("_DataTablePostCity", vm);
                }
                if (status == Status.Hold)
                {
                    var FilterByStatus = all_Cities2.Where(x => (allregions.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim() && y.CrMasSupPostRegionsStatus == Status.Hold) || x.CrMasSupPostCityStatus == status && allregions.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim() && y.CrMasSupPostRegionsStatus == Status.Active)) &&
                                                                                (
                                                                               x.CrMasSupPostCityArName.Contains(search) ||
                                                                               x.CrMasSupPostCityEnName.ToLower().Contains(search.ToLower()) ||
                                                                               x.CrMasSupPostCityCode.Contains(search))).ToList();
                    vm.PostCity = FilterByStatus;
                    return PartialView("_DataTablePostCity", vm);
                }
                if (status == Status.Deleted)
                {
                    var FilterByStatus2 = all_Cities2.Where(x => (allregions.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim() && y.CrMasSupPostRegionsStatus == Status.Deleted) || x.CrMasSupPostCityStatus == status && allregions.Any(y => y.CrMasSupPostRegionsCode.Trim() == x.CrMasSupPostCityRegionsCode.Trim() && y.CrMasSupPostRegionsStatus == Status.Active)) &&
                                                                                (
                                                                               x.CrMasSupPostCityArName.Contains(search) ||
                                                                               x.CrMasSupPostCityEnName.ToLower().Contains(search.ToLower()) ||
                                                                               x.CrMasSupPostCityCode.Contains(search))).ToList();
                    vm.PostCity = FilterByStatus2;
                    return PartialView("_DataTablePostCity", vm);
                }

            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddPostCity()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "PostCity");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "PostCity");
            }
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 1799999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "PostCity");
            }
            var regions_Activated = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasSupPostRegionsStatus != Status.Active,
                selectProjection: query => query.Select(x => new CrMasSupPostRegion
                {
                    CrMasSupPostRegionsCode = x.CrMasSupPostRegionsCode,
                    CrMasSupPostRegionsArName = x.CrMasSupPostRegionsArName,
                    CrMasSupPostRegionsEnName = x.CrMasSupPostRegionsEnName
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            // Set Title 
            PostCityVM CityVM = new PostCityVM();
            CityVM.CrMasSupPostCityCode = await GenerateLicenseCodeAsync();
            CityVM.Regions = regions_Activated;
            return View(CityVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddPostCity(PostCityVM CityVM)
        {
            if (CityVM.CrMasSupPostCityLongitude == null) CityVM.CrMasSupPostCityLongitude = 0;
            if (CityVM.CrMasSupPostCityLatitude == null) CityVM.CrMasSupPostCityLatitude = 0;

            var regions_Activated = await _unitOfWork.CrMasSupPostRegion.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasSupPostRegionsStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSupPostRegion
                {
                    CrMasSupPostRegionsCode = x.CrMasSupPostRegionsCode,
                    CrMasSupPostRegionsArName = x.CrMasSupPostRegionsArName,
                    CrMasSupPostRegionsEnName = x.CrMasSupPostRegionsEnName
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || CityVM == null)
            {
                CityVM.Regions = regions_Activated;
                return View("AddPostCity", CityVM);
            }
            try
            {
                // Map ViewModel to Entity
                var CityEntity = _mapper.Map<CrMasSupPostCity>(CityVM);


                // Check if the entity already exists
                if (await _masPostCity.ExistsByDetailsAsync(CityEntity))
                {
                    await AddModelErrorsAsync(CityEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    CityVM.Regions = regions_Activated;
                    return View("AddPostCity", CityVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 1799999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    CityVM.Regions = regions_Activated;
                    return View("AddPostCity", CityVM);
                }
                var region = await _unitOfWork.CrMasSupPostRegion.FindAsync(x => x.CrMasSupPostRegionsCode == CityEntity.CrMasSupPostCityRegionsCode);
                // Generate and set the Driving License Code
                CityVM.CrMasSupPostCityCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                CityEntity.CrMasSupPostCityStatus = "A";
                CityEntity.CrMasSupPostCityCounter = 0;
                CityEntity.CrMasSupPostCityConcatenateArName = region?.CrMasSupPostRegionsArName + " - " + CityEntity.CrMasSupPostCityArName;
                CityEntity.CrMasSupPostCityConcatenateEnName = region?.CrMasSupPostRegionsEnName + " - " + CityEntity.CrMasSupPostCityEnName;
                CityEntity.CrMasSupPostCityRegionsStatus = region?.CrMasSupPostRegionsStatus ?? "D";

                await _unitOfWork.CrMasSupPostCity.AddAsync(CityEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, CityEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                CityVM.Regions = regions_Activated;
                return View("AddPostCity", CityVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 1700000002 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "PostCity");
            }
            var contract = await _unitOfWork.CrMasSupPostCity.FindAsync(x => x.CrMasSupPostCityCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "PostCity");
            }
            var model = _mapper.Map<PostCityVM>(contract);
            var allregions_Activated = await _unitOfWork.CrMasSupPostRegion.FindAllAsNoTrackingAsync(x => x.CrMasSupPostRegionsStatus == Status.Active);
            if (!allregions_Activated.Any(x => x.CrMasSupPostRegionsCode.Trim() == contract.CrMasSupPostCityRegionsCode.Trim()))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "PostCity");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(PostCityVM CityVM)
        {
            if (CityVM.CrMasSupPostCityLongitude == null) CityVM.CrMasSupPostCityLongitude = 0;
            if (CityVM.CrMasSupPostCityLatitude == null) CityVM.CrMasSupPostCityLatitude = 0;
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && CityVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "PostCity");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", CityVM);
                }
                var CityEntity = _mapper.Map<CrMasSupPostCity>(CityVM);
                CityEntity.CrMasSupPostCityArName = "أأ";
                CityEntity.CrMasSupPostCityEnName = "AA";
                // Check if the entity already exists
                if (await _masPostCity.ExistsByDetailsAsync(CityEntity))
                {
                    await AddModelErrorsAsync(CityEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", CityVM);
                }
                var contract = await _unitOfWork.CrMasSupPostCity.FindAsync(x => x.CrMasSupPostCityCode == CityVM.CrMasSupPostCityCode);
                if (contract == null) { return View("Edit", CityVM); }
                contract.CrMasSupPostCityLocation = CityVM.CrMasSupPostCityLocation;
                contract.CrMasSupPostCityLongitude = CityVM.CrMasSupPostCityLongitude;
                contract.CrMasSupPostCityLatitude = CityVM.CrMasSupPostCityLatitude;
                contract.CrMasSupPostCityReasons = CityVM.CrMasSupPostCityReasons;
                _unitOfWork.CrMasSupPostCity.Update(contract);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, contract, Status.Update);
                return RedirectToAction("Index", "PostCity");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", CityVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupPostCity.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masPostCity.CheckIfCanDeleteIt(licence.CrMasSupPostCityCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupPostCityStatus = status;
                _unitOfWork.CrMasSupPostCity.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupPostCity entity)
        {

            if (await _masPostCity.ExistsByArabicNameAsync(entity.CrMasSupPostCityArName, entity.CrMasSupPostCityCode))
            {
                ModelState.AddModelError("CrMasSupPostCityArName", _localizer["Existing"]);
            }

            if (await _masPostCity.ExistsByEnglishNameAsync(entity.CrMasSupPostCityEnName, entity.CrMasSupPostCityCode))
            {
                ModelState.AddModelError("CrMasSupPostCityEnName", _localizer["Existing"]);
            }

            if (await _masPostCity.ExistsByLocationAsync(entity.CrMasSupPostCityLocation, entity.CrMasSupPostCityCode))
            {
                ModelState.AddModelError("CrMasSupPostCityLocation", _localizer["Existing"]);
            }

            if (await _masPostCity.ExistsByLongitudeAsync((decimal)entity.CrMasSupPostCityLongitude, entity.CrMasSupPostCityCode))
            {
                ModelState.AddModelError("CrMasSupPostCityLongitude", _localizer["Existing"]);
            }

            if (await _masPostCity.ExistsByLatitudeAsync((decimal)entity.CrMasSupPostCityLatitude, entity.CrMasSupPostCityCode))
            {
                ModelState.AddModelError("CrMasSupPostCityLatitude", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_PostCitys = await _unitOfWork.CrMasSupPostCity.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_PostCitys != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupPostCityArName" && All_PostCitys.Any(x => x.CrMasSupPostCityArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostCityArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupPostCityEnName" && All_PostCitys.Any(x => x.CrMasSupPostCityEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostCityEnName", Message = _localizer["Existing"] });
                }
                // Check for existing 
                else if (existName == "CrMasSupPostCityLocation" && All_PostCitys.Any(x => x.CrMasSupPostCityLocation?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostCityLocation", Message = _localizer["Existing"] });
                }
                // Check for existing 
                else if (existName == "CrMasSupPostCityLongitude" && decimal.TryParse(dataField, out var code) && code != 0 && All_PostCitys.Any(x => x.CrMasSupPostCityLongitude == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostCityLongitude", Message = _localizer["Existing"] });
                }
                // Check for existing 
                else if (existName == "CrMasSupPostCityLatitude" && decimal.TryParse(dataField, out var id) && id != 0 && All_PostCitys.Any(x => x.CrMasSupPostCityLatitude == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostCityLatitude", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupPostCity.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupPostCityCode) + 1).ToString() : "1700000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupPostCity licence, string status)
        {


            var recordAr = licence.CrMasSupPostCityArName;
            var recordEn = licence.CrMasSupPostCityEnName;
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
            return RedirectToAction("Index", "PostCity");
        }


    }
}
