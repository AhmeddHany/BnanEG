using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.MAS.Controllers.Companies
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class LessorImagesController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<LessorImagesController> _localizer;
        private readonly IToastNotification _toastNotification;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.Support_Photos;

        public LessorImagesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment webHostEnvironment, IStringLocalizer<LessorImagesController> localizer, IToastNotification toastNotification, IUserLoginsService userLoginsService, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _toastNotification = toastNotification;
            _userLoginsService = userLoginsService;
            _baseRepo = baseRepo;
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
            IQueryable<CrMasLessorInformation> query = _unitOfWork.CrMasLessorInformation.GetTableNoTracking().Include("CrCasBranchInformations.CrCasBranchPost.CrCasBranchPostCityNavigation")/*.Where(x => x.CrMasLessorInformationCode != "0000")*/;
            var lessors = await query.Where(x => x.CrMasLessorInformationStatus == Status.Active).ToListAsync();
            if (!lessors.Any())
            {
                lessors = await query.Where(x => x.CrMasLessorInformationStatus == Status.Hold).ToListAsync();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(lessors);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetLessorsByStatus(string status, string search)
        {
            // استعلام أساسي مع Include
            IQueryable<CrMasLessorInformation> query = _unitOfWork.CrMasLessorInformation.GetTableNoTracking().Include("CrCasBranchInformations.CrCasBranchPost.CrCasBranchPostCityNavigation")/*.Where(x => x.CrMasLessorInformationCode != "0000")*/;

            // فلترة حسب حالة status
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All) query = query.Where(x => x.CrMasLessorInformationStatus != Status.Deleted);
                else query = query.Where(x => x.CrMasLessorInformationStatus == status);
            }

            // فلترة حسب search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(x => x.CrMasLessorInformationArLongName.Contains(search) || x.CrMasLessorInformationEnLongName.ToLower().Contains(search) || x.CrMasLessorInformationCode.Contains(search));
            }

            // تنفيذ الاستعلام وتحميل البيانات
            var lessors = await query.ToListAsync();
            return PartialView("_DataTableCompaniesInformations", lessors);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var user = await _userManager.GetUserAsync(User);

            // جلب بيانات المؤجر
            var lessor = await _unitOfWork.CrMasLessorInformation.FindAsync(x => x.CrMasLessorInformationCode == id);
            var lessorImgs = await _unitOfWork.CrMasLessorImage.FindAsync(x => x.CrMasLessorImageCode == id);
            if (lessor == null || lessorImgs == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "LessorImages");
            }

            var lessorVM = _mapper.Map<LessorImagesVM>(lessorImgs);
            lessorVM.CrMasLessorNameAr = lessor.CrMasLessorInformationArLongName;
            lessorVM.CrMasLessorNameEn = lessor.CrMasLessorInformationEnLongName;
            return View(lessorVM);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(LessorImagesVM model)
        {
            await SetPageTitleAsync(Status.Update, pageNumber);
            var user = await _userManager.GetUserAsync(User);

            // جلب بيانات المؤجر
            var lessorImgs = await _unitOfWork.CrMasLessorImage.FindAsync(x => x.CrMasLessorImageCode == model.CrMasLessorImageCode);
            if (user == null || lessorImgs == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "LessorImages");
            }
            var files = GetImages(model);
            string foldername = $"{"images\\Company"}\\{lessorImgs.CrMasLessorImageCode}\\{"Support Images"}";

            foreach (var item in files)
            {
                var propertyName = item.PropertyName;
                var NewName = propertyName.Replace("CrMasLessor", "") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقت
                var file = item.File;
                if (file != null)
                {
                    var propertyInfo = lessorImgs.GetType().GetProperty(propertyName);
                    if (propertyInfo != null)
                    {
                        var currentValue = propertyInfo.GetValue(lessorImgs)?.ToString();
                        var path = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", currentValue);
                        // تعيين المسار الجديد للخاصية في الكائن
                        if (!string.IsNullOrEmpty(path))
                        {
                            propertyInfo.SetValue(lessorImgs, path); // تعيين المسار الجديد
                        }
                    }
                }
            }
            if (_unitOfWork.CrMasLessorImage.Update(lessorImgs) != null)
            {
                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index", "LessorImages");
                }

            };
            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "LessorImages");
        }

        private List<(string PropertyName, IFormFile? File)> GetImages(LessorImagesVM model)
        {
            var files = new List<(string PropertyName, IFormFile? File)>
                {
                    ("CrMasLessorImageLogo", model.FileCrMasLessorImageLogo),
                    ("CrMasLessorImageStamp", model.FileCrMasLessorImageStamp),
                    ("CrMasLessorImageLoaderLogo", model.FileCrMasLessorImageLoaderLogo),
                    ("CrMasLessorImageWhatsupLogo", model.FileCrMasLessorImageWhatsupLogo),
                    ("CrMasLessorImageQrCodeSite", model.FileCrMasLessorImageQrCodeSite),
                    ("CrMasLessorImageAuthenticatedElectronically", model.FileCrMasLessorImageAuthenticatedElectronically),
                    ("CrMasLessorImageQrCodeAuthenticated", model.FileCrMasLessorImageQrCodeAuthenticated),
                    ("CrMasLessorImageMangerSignture", model.FileCrMasLessorImageMangerSignture),
                    ("CrMasLessorImageContractCard", model.FileCrMasLessorImageContractCard),
                    ("CrMasLessorImagePerformInvoice", model.FileCrMasLessorImagePerformInvoice),
                    ("CrMasLessorImageTaxInvoice", model.FileCrMasLessorImageTaxInvoice),
                    ("CrMasLessorImageReceipt", model.FileCrMasLessorImageReceipt),
                    ("CrMasLessorImageExchange", model.FileCrMasLessorImageExchange),
                    ("CrMasLessorImageContractPage1", model.FileCrMasLessorImageContractPage1),
                    ("CrMasLessorImageContractPage2", model.FileCrMasLessorImageContractPage2),
                    ("CrMasLessorImageContractPage3", model.FileCrMasLessorImageContractPage3),
                    ("CrMasLessorImageContractPage4", model.FileCrMasLessorImageContractPage4),
                    ("CrMasLessorImageContractPage5", model.FileCrMasLessorImageContractPage5),
                    ("CrMasLessorImageContractPage6", model.FileCrMasLessorImageContractPage6),
                    ("CrMasLessorImageContractPage7", model.FileCrMasLessorImageContractPage7),
                    ("CrMasLessorImageContractPage8", model.FileCrMasLessorImageContractPage8),
                    ("CrMasLessorImageContractPage9", model.FileCrMasLessorImageContractPage9),
                    ("CrMasLessorImageContractPage10", model.FileCrMasLessorImageContractPage10),
                    ("CrMasLessorImageContractPage11", model.FileCrMasLessorImageContractPage11),
                    ("CrMasLessorImageContractPage12", model.FileCrMasLessorImageContractPage12),
                    ("CrMasLessorImageArConditionPage1", model.FileCrMasLessorImageArConditionPage1),
                    ("CrMasLessorImageArConditionPage2", model.FileCrMasLessorImageArConditionPage2),
                    ("CrMasLessorImageArConditionPage3", model.FileCrMasLessorImageArConditionPage3),
                    ("CrMasLessorImageEnConditionPage1", model.FileCrMasLessorImageEnConditionPage1),
                    ("CrMasLessorImageEnConditionPage2", model.FileCrMasLessorImageEnConditionPage2),
                    ("CrMasLessorImageEnConditionPage3", model.FileCrMasLessorImageEnConditionPage3),
                    ("CrMasLessorImageArDailyReport", model.FileCrMasLessorImageArDailyReport),
                    ("CrMasLessorImageEnDailyReport", model.FileCrMasLessorImageEnDailyReport)
                };
            return files;
        }
        
        public IActionResult SuccesssMessageForLessorImages()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index");
        }

    }
}