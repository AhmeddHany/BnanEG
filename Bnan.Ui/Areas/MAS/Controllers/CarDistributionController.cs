using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.MAS.Controllers.Companies;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.MAS.Controllers
{

    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class CarDistributionController : BaseController
    {
        private readonly IStringLocalizer<LessorsKSAController> _localizer;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICarDistribution _CarDistribution;
        private readonly IToastNotification _toastNotification;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _UserService;
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.CrMasSupCarDistribution;


        public CarDistributionController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IStringLocalizer<LessorsKSAController> localizer, IWebHostEnvironment webHostEnvironment, ICarDistribution carDistribution, IToastNotification toastNotification, IUserLoginsService userLoginsService, IUserService userService, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _localizer = localizer;
            _webHostEnvironment = webHostEnvironment;
            _CarDistribution = carDistribution;
            _toastNotification = toastNotification;
            _userLoginsService = userLoginsService;
            _UserService = userService;
            _baseRepo = baseRepo;
        }
        [HttpGet]
        public async Task<IActionResult> CarDistribution()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            IQueryable<CrMasSupCarDistribution> query = _unitOfWork.CrMasSupCarDistribution.GetTableNoTracking(); // جلب البيانات بدون تتبع
            var carDistribution = await query.Where(x => x.CrMasSupCarDistributionStatus == Status.Active).ToListAsync();

            var cardistribution = await _unitOfWork.CrMasSupCarDistribution.GetAllAsync();
            if (!cardistribution.Any())
            {
                cardistribution = await query.Where(x => x.CrMasSupCarDistributionStatus == Status.Hold).ToListAsync();
                ViewBag.radio = "All";
            }
            else
            {
                ViewBag.radio = "A";
            }
            return View(cardistribution);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetCarDistributionByStatus(string status, string search)
        {
            // Get the current user (if needed for further filtering)
            var user = await _userManager.GetUserAsync(User);

            // Start building the query
            IQueryable<CrMasSupCarDistribution> query = _unitOfWork.CrMasSupCarDistribution.GetTableNoTracking(); // Use strongly-typed Include
            // Filter by status if not "All"
            if (status != Status.All) query = query.Where(x => x.CrMasSupCarDistributionStatus == status);
            else query = query.Where(x => x.CrMasSupCarDistributionStatus != Status.Deleted);

            // Search in multiple fields if the search parameter is provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.CrMasSupCarDistributionConcatenateArName.Contains(search) ||
                    x.CrMasSupCarDistributionConcatenateEnName.ToLower().Contains(search.ToLower()) ||
                    x.CrMasSupCarDistributionCode.Contains(search) ||
                    x.CrMasSupCarDistributionYear.Contains(search));
            }

            // Execute the query and retrieve the results
            var result = await query.AsNoTracking().ToListAsync();

            // Return the results in a partial view
            return PartialView("_DataTableDistribution", result);
        }

        [HttpGet]
        public async Task<IActionResult> AddCarDistribution()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("CarDistribution", "CarDistribution");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("CarDistribution", "CarDistribution");
            }
            // Pass the category Arabic key to the view 
            var categoryAr = await _unitOfWork.CrMasSupCarCategory.GetAllAsyncAsNoTrackingAsync();
            var categoryDropDownAr = categoryAr.Select(c => new SelectListItem { Value = c.CrMasSupCarCategoryCode?.ToString(), Text = c.CrMasSupCarCategoryArName }).ToList();
            categoryDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["categoryAr"] = categoryDropDownAr;

            // Pass the category Arabic key to the view 
            var categoryEn = await _unitOfWork.CrMasSupCarCategory.GetAllAsyncAsNoTrackingAsync();
            var categoryDropDownEn = categoryEn.Select(c => new SelectListItem { Value = c.CrMasSupCarCategoryCode?.ToString(), Text = c.CrMasSupCarCategoryEnName }).ToList();
            categoryDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["categoryEn"] = categoryDropDownEn;

            // Pass the Model English  key to the view 
            var ModelEn = await _unitOfWork.CrMasSupCarModel.GetAllAsyncAsNoTrackingAsync();
            var ModelDropDownEn = ModelEn.Select(c => new SelectListItem { Value = c.CrMasSupCarModelCode?.ToString(), Text = c.CrMasSupCarModelConcatenateEnName }).ToList();
            ModelDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ModelEn"] = ModelDropDownEn;

            // Pass the Model English key to the view 
            var ModelAr = await _unitOfWork.CrMasSupCarModel.GetAllAsyncAsNoTrackingAsync();
            var ModelDropDownAr = ModelAr.Select(c => new SelectListItem { Value = c.CrMasSupCarModelCode?.ToString(), Text = c.CrMasSupCarModelArConcatenateName }).ToList();
            ModelDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ModelAr"] = ModelDropDownAr;

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> AddCarDistribution(CrMasSupCarDistributionVM crMasSupCarDistributionVM, IFormFile? CarDistributionFile)
        {
            var pageNumber = SubTasks.CrMasUserInformationFromMASToCAS;
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            if (crMasSupCarDistributionVM == null || user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View("CarDistribution");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("CarDistribution", "CarDistribution");
            }
            if (await CheckIfDistributionIsExist(crMasSupCarDistributionVM.CrMasSupCarDistributionModel, crMasSupCarDistributionVM.CrMasSupCarDistributionYear, crMasSupCarDistributionVM.CrMasSupCarDistributionCategory)) ModelState.AddModelError("IsExist", _localizer["IsExistModel"]);
            if (ModelState.IsValid)
            {
                var crMasSupCarDistribution = _mapper.Map<CrMasSupCarDistribution>(crMasSupCarDistributionVM);
                crMasSupCarDistribution.CrMasSupCarDistributionCode = await GetDistributionCode(crMasSupCarDistributionVM.CrMasSupCarDistributionYear);
                string foldername = $"{"images\\Bnan\\Models"}";
                string filePathImage;

                if (CarDistributionFile != null)
                {
                    string fileNameImg = crMasSupCarDistribution.CrMasSupCarDistributionCode;
                    filePathImage = await CarDistributionFile.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                    crMasSupCarDistribution.CrMasSupCarDistributionImage = filePathImage;
                }
                else
                {
                    crMasSupCarDistribution.CrMasSupCarDistributionImage = "~/images/common/DefaultCar.png";
                }

                var result = await _CarDistribution.AddCarDisribtion(crMasSupCarDistribution);
                if (result && _unitOfWork.Complete() > 0)
                {
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    await SaveTracingForUserChange(user, Status.Insert);
                    return RedirectToAction("CarDistribution", "CarDistribution");
                }

            }

            // Pass the category Arabic key to the view 
            var categoryAr = await _unitOfWork.CrMasSupCarCategory.GetAllAsync();
            var categoryDropDownAr = categoryAr.Select(c => new SelectListItem { Value = c.CrMasSupCarCategoryCode?.ToString(), Text = c.CrMasSupCarCategoryArName }).ToList();
            //categoryDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["categoryAr"] = categoryDropDownAr;

            // Pass the category Arabic key to the view 
            var categoryEn = await _unitOfWork.CrMasSupCarCategory.GetAllAsync();
            var categoryDropDownEn = categoryEn.Select(c => new SelectListItem { Value = c.CrMasSupCarCategoryCode?.ToString(), Text = c.CrMasSupCarCategoryEnName }).ToList();
            //categoryDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["categoryEn"] = categoryDropDownEn;

            // Pass the Model English  key to the view 
            var ModelEn = await _unitOfWork.CrMasSupCarModel.GetAllAsync();
            var ModelDropDownEn = ModelEn.Select(c => new SelectListItem { Value = c.CrMasSupCarModelCode?.ToString(), Text = c.CrMasSupCarModelConcatenateEnName }).ToList();
            //ModelDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ModelEn"] = ModelDropDownEn;

            // Pass the Model English key to the view 
            var ModelAr = await _unitOfWork.CrMasSupCarModel.GetAllAsync();
            var ModelDropDownAr = ModelAr.Select(c => new SelectListItem { Value = c.CrMasSupCarModelCode?.ToString(), Text = c.CrMasSupCarModelArConcatenateName }).ToList();
            //ModelDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ModelAr"] = ModelDropDownAr;
            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return View(crMasSupCarDistributionVM);
        }
        public async Task<string> GetDistributionCode(string year)
        {
            var CarDistributions = await _unitOfWork.CrMasSupCarDistribution.FindAllAsNoTrackingAsync(l => l.CrMasSupCarDistributionYear == year);
            var CarDistribution = CarDistributions.Max(x => x.CrMasSupCarDistributionCode.Substring(x.CrMasSupCarDistributionCode.Length - 5, 5));
            string Serial;
            if (CarDistribution != null)
            {
                Int64 val = Int64.Parse(CarDistribution) + 1;
                Serial = val.ToString("000000");
            }
            else
            {
                Serial = "000001";
            }
            return year + Serial;
        }
        public async Task<bool> CheckIfDistributionIsExist(string model, string year, string category)
        {
            var result = await _unitOfWork.CrMasSupCarDistribution.FindAllAsNoTrackingAsync(x => x.CrMasSupCarDistributionModel == model && x.CrMasSupCarDistributionCategory == category && x.CrMasSupCarDistributionYear == year);
            return result.Count() > 0;
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            await SetPageTitleAsync(Status.Update, pageNumber);
            var DistruibtionCar = await _unitOfWork.CrMasSupCarDistribution.GetByIdAsync(id);
            if (DistruibtionCar == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("CarDistribution", "CarDistribution");
            }

            var DistruibtionCarVM = _mapper.Map<CrMasSupCarDistributionVM>(DistruibtionCar);
            return View(DistruibtionCarVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CrMasSupCarDistributionVM CrMasSupCarDistributionVM, IFormFile? CarDistributionFile)
        {
            var user = await _userManager.GetUserAsync(User);

            string foldername = Path.Combine("images", "Bnan", "Models"); // تحديد المجلد لحفظ الصورة
            string filePathImage;

            // العثور على الكائن في قاعدة البيانات
            var oldCrMasSupCarDistribution = await _unitOfWork.CrMasSupCarDistribution.FindAsync(x => x.CrMasSupCarDistributionCode == CrMasSupCarDistributionVM.CrMasSupCarDistributionCode);
            if (oldCrMasSupCarDistribution == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("CarDistribution", "CarDistribution");
            }
            var crMasSupCarDistribution = _mapper.Map<CrMasSupCarDistribution>(CrMasSupCarDistributionVM);
            if (CarDistributionFile != null)
            {
                // إذا تم رفع صورة جديدة، قم بتخزينها في المجلد المخصص
                string fileNameImg = $"{oldCrMasSupCarDistribution.CrMasSupCarDistributionModel}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
                filePathImage = await CarDistributionFile.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png", oldCrMasSupCarDistribution.CrMasSupCarDistributionImage);
                crMasSupCarDistribution.CrMasSupCarDistributionImage = filePathImage; // تحديث المسار في الكائن
            }
            else if (!string.IsNullOrEmpty(CrMasSupCarDistributionVM.CrMasSupCarDistributionImage)) crMasSupCarDistribution.CrMasSupCarDistributionImage = CrMasSupCarDistributionVM.CrMasSupCarDistributionImage;
            else crMasSupCarDistribution.CrMasSupCarDistributionImage = "~/images/common/DefaultCar.png";
            var result = await _CarDistribution.UpdateCarDisribtion(crMasSupCarDistribution);
            if (result && await _unitOfWork.CompleteAsync() > 0)
            {
                _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SaveTracingForUserChange(user, Status.Update);
                return RedirectToAction("CarDistribution", "CarDistribution");
            };
            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("CarDistribution", "CarDistribution");
        }

        [HttpPost]
        public async Task<string> EditStatus(string status, string code)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var CarDistribution = await _unitOfWork.CrMasSupCarDistribution.GetByIdAsync(code);
            if (CarDistribution == null) return "false";
            try
            {
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                if (status == Status.Deleted && CarDistribution.CrMasSupCarDistributionCount > 0) return "udelete";
                CarDistribution.CrMasSupCarDistributionStatus = status;
                _unitOfWork.CrMasSupCarDistribution.Update(CarDistribution);
                await _unitOfWork.CompleteAsync();
                await SaveTracingForUserChange(user, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }
        private async Task SaveTracingForUserChange(CrMasUserInformation userCreated, string status)
        {


            var recordAr = $"{userCreated.CrMasUserInformationArName} - {userCreated.CrMasUserInformationTasksArName}";
            var recordEn = $"{userCreated.CrMasUserInformationEnName} - {userCreated.CrMasUserInformationTasksEnName}";
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
        public IActionResult DisplayToastSuccess_withIndex()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            return RedirectToAction("CarDistribution", "CarDistribution");
        }
    }
}
