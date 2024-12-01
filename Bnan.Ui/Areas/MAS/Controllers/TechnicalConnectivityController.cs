using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.ComponentModel.Design;

namespace Bnan.Ui.Areas.MAS.Controllers
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class TechnicalConnectivityController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<TechnicalConnectivityController> _localizer;
        private readonly string pageNumber = SubTasks.Technical_Connectivity;


        public TechnicalConnectivityController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<TechnicalConnectivityController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }
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
            // استعلام رئيسي مع NoTracking
            var query = _unitOfWork.CrMasLessorInformation.GetTableNoTracking().Include("CrCasBranchInformations.CrCasBranchPost.CrCasBranchPostCityNavigation");
            // استرجاع التراخيص الفعّالة
            var lessors = await query.Where(x => x.CrMasLessorInformationStatus == Status.Active).ToListAsync();
            // إذا لم توجد تراخيص فعّالة، استرجاع التراخيص المعلّقة
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
            IQueryable<CrMasLessorInformation> query = _unitOfWork.CrMasLessorInformation.GetTableNoTracking().Include("CrCasBranchInformations.CrCasBranchPost.CrCasBranchPostCityNavigation");

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
        public async Task<IActionResult> Connect()
        { // Set page titles
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateQrCode(string companyId)
        {
            var url = $"http://62.84.187.79:3000/api/generateQrCodeNew2/{companyId}";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return StatusCode((int)response.StatusCode, "Error fetching QR Code");
        }
        [HttpGet]
        public async Task<IActionResult> IsClientReady(string companyId)
        {
            var url = $"http://62.84.187.79:3000/api/isClientReady_data/{companyId}"; // رابط API الخارجي
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return StatusCode((int)response.StatusCode, "Error checking client connection");
        }
        [HttpGet]
        public async Task<IActionResult> LogoutClient(string select_Connect_id)
        {
            try
            {
                // URL الخاص بالـ API الذي يتعامل مع قطع الاتصال
                var url = $"http://62.84.187.79:3000/api/logout_whats/{select_Connect_id}";
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { status = true, message = "تم قطع الاتصال بنجاح" });
                }
                else
                {
                    return Json(new { status = false, message = "حدث خطأ أثناء قطع الاتصال" });
                }
            }
            catch (Exception ex)
            {
                // التعامل مع الأخطاء في حال حدوث استثناء
                return Json(new { status = false, message = "حدث خطأ أثناء قطع الاتصال: " + ex.Message });
            }
        }
    }
}
