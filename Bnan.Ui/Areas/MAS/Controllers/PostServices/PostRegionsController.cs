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
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.PostServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class PostRegionsController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasPostRegions _masPostRegions;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<PostRegionsController> _localizer;
        private readonly string pageNumber = SubTasks.Region;


        public PostRegionsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasPostRegions masPostRegions, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<PostRegionsController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masPostRegions = masPostRegions;
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
            var Regions = await _unitOfWork.CrMasSupPostRegion
                .FindAllAsNoTrackingAsync(x => x.CrMasSupPostRegionsStatus == Status.Active);

            var Region_count = await _unitOfWork.CrMasRenterPost.FindCountByColumnAsync<CrMasRenterPost>(
                predicate: x => x.CrMasRenterPostStatus != Status.Deleted,
                columnSelector: x => x.CrMasRenterPostRegions  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var cites_count = await _unitOfWork.CrMasSupPostCity.FindCountByColumnAsync<CrMasSupPostCity>(
                predicate: x => x.CrMasSupPostCityStatus != Status.Deleted,
                columnSelector: x => x.CrMasSupPostCityRegionsCode  // تحديد العمود الذي نريد التجميع بناءً عليه
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );

            // If no active licenses, retrieve all licenses
            if (!Regions.Any())
            {
                Regions = await _unitOfWork.CrMasSupPostRegion
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupPostRegionsStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            PostRegionsVM vm = new PostRegionsVM();
            vm.PostRegions = Regions;
            vm.Region_count = Region_count;
            vm.cites_count = cites_count;
            return View(vm);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetPostRegionsByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var PostRegionssAll = await _unitOfWork.CrMasSupPostRegion.FindAllAsNoTrackingAsync(x => x.CrMasSupPostRegionsStatus == Status.Active ||
                                                                                                                            x.CrMasSupPostRegionsStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupPostRegionsStatus == Status.Hold);

                var Region_count = await _unitOfWork.CrMasRenterPost.FindCountByColumnAsync<CrMasRenterPost>(
                    predicate: x => x.CrMasRenterPostStatus != Status.Deleted,
                    columnSelector: x => x.CrMasRenterPostRegions  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                var cites_count = await _unitOfWork.CrMasSupPostCity.FindCountByColumnAsync<CrMasSupPostCity>(
                    predicate: x => x.CrMasSupPostCityStatus != Status.Deleted,
                    columnSelector: x => x.CrMasSupPostCityRegionsCode  // تحديد العمود الذي نريد التجميع بناءً عليه
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                PostRegionsVM vm = new PostRegionsVM();
                vm.Region_count = Region_count;
                vm.cites_count = cites_count;
                if (status == Status.All)
                {
                    var FilterAll = PostRegionssAll.FindAll(x => x.CrMasSupPostRegionsStatus != Status.Deleted &&
                                                                         (x.CrMasSupPostRegionsArName.Contains(search) ||
                                                                          x.CrMasSupPostRegionsEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupPostRegionsCode.Contains(search)));
                    vm.PostRegions = FilterAll;
                    return PartialView("_DataTablePostRegions", vm);
                }
                var FilterByStatus = PostRegionssAll.FindAll(x => x.CrMasSupPostRegionsStatus == status &&
                                                                            (
                                                                           x.CrMasSupPostRegionsArName.Contains(search) ||
                                                                           x.CrMasSupPostRegionsEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupPostRegionsCode.Contains(search)));
                vm.PostRegions = FilterByStatus;
                return PartialView("_DataTablePostRegions", vm);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddPostRegions()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "PostRegions");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "PostRegions");
            }
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "PostRegions");
            }
            // Set Title 
            PostRegionsVM RegionVM = new PostRegionsVM();
            RegionVM.CrMasSupPostRegionsCode = await GenerateLicenseCodeAsync();
            return View(RegionVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddPostRegions(PostRegionsVM RegionVM)
        {
            if (RegionVM.CrMasSupPostRegionsLongitude == null) RegionVM.CrMasSupPostRegionsLongitude = 0;
            if (RegionVM.CrMasSupPostRegionsLatitude == null) RegionVM.CrMasSupPostRegionsLatitude = 0;

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (!ModelState.IsValid || RegionVM == null)
            {
                return View("AddPostRegions", RegionVM);
            }
            try
            {
                // Map ViewModel to Entity
                var RegionEntity = _mapper.Map<CrMasSupPostRegion>(RegionVM);


                // Check if the entity already exists
                if (await _masPostRegions.ExistsByDetailsAsync(RegionEntity))
                {
                    await AddModelErrorsAsync(RegionEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddPostRegions", RegionVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 99)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddPostRegions", RegionVM);
                }
                // Generate and set the Driving License Code
                RegionVM.CrMasSupPostRegionsCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                RegionEntity.CrMasSupPostRegionsStatus = "A";
                await _unitOfWork.CrMasSupPostRegion.AddAsync(RegionEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, RegionEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("AddPostRegions", RegionVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 11 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "PostRegions");
            }
            var contract = await _unitOfWork.CrMasSupPostRegion.FindAsync(x => x.CrMasSupPostRegionsCode == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "PostRegions");
            }
            var model = _mapper.Map<PostRegionsVM>(contract);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(PostRegionsVM RegionVM)
        {
            if (RegionVM.CrMasSupPostRegionsLongitude == null) RegionVM.CrMasSupPostRegionsLongitude = 0;
            if (RegionVM.CrMasSupPostRegionsLatitude == null) RegionVM.CrMasSupPostRegionsLatitude = 0;
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null && RegionVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "PostRegions");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", RegionVM);
                }
                var RegionEntity = _mapper.Map<CrMasSupPostRegion>(RegionVM);
                RegionEntity.CrMasSupPostRegionsArName = "أأ";
                RegionEntity.CrMasSupPostRegionsEnName = "AA";
                // Check if the entity already exists
                if (await _masPostRegions.ExistsByDetailsAsync(RegionEntity))
                {
                    await AddModelErrorsAsync(RegionEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", RegionVM);
                }
                var contract = await _unitOfWork.CrMasSupPostRegion.FindAsync(x => x.CrMasSupPostRegionsCode == RegionVM.CrMasSupPostRegionsCode);
                if (contract == null) { return View("Edit", RegionVM); }
                contract.CrMasSupPostRegionsLocation = RegionVM.CrMasSupPostRegionsLocation;
                contract.CrMasSupPostRegionsLongitude = RegionVM.CrMasSupPostRegionsLongitude;
                contract.CrMasSupPostRegionsLatitude = RegionVM.CrMasSupPostRegionsLatitude;
                contract.CrMasSupPostRegionsReasons = RegionVM.CrMasSupPostRegionsReasons;
                _unitOfWork.CrMasSupPostRegion.Update(contract);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, contract, Status.Update);
                return RedirectToAction("Index", "PostRegions");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("Edit", RegionVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupPostRegion.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masPostRegions.CheckIfCanDeleteIt(licence.CrMasSupPostRegionsCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupPostRegionsStatus = status;
                _unitOfWork.CrMasSupPostRegion.Update(licence);
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
        private async Task AddModelErrorsAsync(CrMasSupPostRegion entity)
        {

            if (await _masPostRegions.ExistsByArabicNameAsync(entity.CrMasSupPostRegionsArName, entity.CrMasSupPostRegionsCode))
            {
                ModelState.AddModelError("CrMasSupPostRegionsArName", _localizer["Existing"]);
            }

            if (await _masPostRegions.ExistsByEnglishNameAsync(entity.CrMasSupPostRegionsEnName, entity.CrMasSupPostRegionsCode))
            {
                ModelState.AddModelError("CrMasSupPostRegionsEnName", _localizer["Existing"]);
            }

            if (await _masPostRegions.ExistsByLocationAsync(entity.CrMasSupPostRegionsLocation, entity.CrMasSupPostRegionsCode))
            {
                ModelState.AddModelError("CrMasSupPostRegionsLocation", _localizer["Existing"]);
            }

            if (await _masPostRegions.ExistsByLongitudeAsync((decimal)entity.CrMasSupPostRegionsLongitude, entity.CrMasSupPostRegionsCode))
            {
                ModelState.AddModelError("CrMasSupPostRegionsLongitude", _localizer["Existing"]);
            }

            if (await _masPostRegions.ExistsByLatitudeAsync((decimal)entity.CrMasSupPostRegionsLatitude, entity.CrMasSupPostRegionsCode))
            {
                ModelState.AddModelError("CrMasSupPostRegionsLatitude", _localizer["Existing"]);
            }
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_PostRegionss = await _unitOfWork.CrMasSupPostRegion.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_PostRegionss != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupPostRegionsArName" && All_PostRegionss.Any(x => x.CrMasSupPostRegionsArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostRegionsArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupPostRegionsEnName" && All_PostRegionss.Any(x => x.CrMasSupPostRegionsEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostRegionsEnName", Message = _localizer["Existing"] });
                }
                // Check for existing 
                else if (existName == "CrMasSupPostRegionsLocation" && All_PostRegionss.Any(x => x.CrMasSupPostRegionsLocation?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostRegionsLocation", Message = _localizer["Existing"] });
                }
                // Check for existing 
                else if (existName == "CrMasSupPostRegionsLongitude" && decimal.TryParse(dataField, out var code) && code != 0 && All_PostRegionss.Any(x => x.CrMasSupPostRegionsLongitude == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostRegionsLongitude", Message = _localizer["Existing"] });
                }
                // Check for existing 
                else if (existName == "CrMasSupPostRegionsLatitude" && decimal.TryParse(dataField, out var id) && id != 0 && All_PostRegionss.Any(x => x.CrMasSupPostRegionsLatitude == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupPostRegionsLatitude", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupPostRegion.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupPostRegionsCode) + 1).ToString() : "10";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupPostRegion licence, string status)
        {


            var recordAr = licence.CrMasSupPostRegionsArName;
            var recordEn = licence.CrMasSupPostRegionsEnName;
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
            return RedirectToAction("Index", "PostRegions");
        }


    }
}
