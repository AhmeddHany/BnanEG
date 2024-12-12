using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS.WhatsupVMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using NToastNotify;

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
        private readonly IMasWhatsupConnect _masTechnicalConnect;

        private readonly IStringLocalizer<TechnicalConnectivityController> _localizer;
        private readonly string pageNumber = SubTasks.Technical_Connectivity;


        public TechnicalConnectivityController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<TechnicalConnectivityController> localizer, IMasWhatsupConnect masTechnicalConnect) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _masTechnicalConnect = masTechnicalConnect;
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
            try
            {
                var response = await WhatsAppServicesExtension.GenerateQrCode(companyId);
                return Content(response, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = $"حدث خطأ أثناء توليد رمز الاستجابة السريعة. {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsClientReady(string companyId)
        {
            companyId= "0000";
            try
            {
                var content = await WhatsAppServicesExtension.IsClientReady(companyId);
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true)
                {
                    var clientData = apiResponse.Data;
                    bool updateResult = await UpdateClientInfo(companyId, clientData);

                    if (updateResult)
                        return Content(content, "application/json");
                    else
                        return StatusCode(500, new { status = false, message = "فشل في تحديث بيانات العميل." });
                }

                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = $"حدث خطأ أثناء التحقق من جاهزية العميل. {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LogoutClient(string companyId)
        {
            var user = await _userManager.GetUserAsync(User);

            try
            {
                var response = await WhatsAppServicesExtension.LogoutAsync(companyId);

                if (response.IsSuccessStatusCode)
                {
                    await _masTechnicalConnect.ChangeStatusOldWhatsupConnect(companyId, user.CrMasUserInformationCode);
                    await _masTechnicalConnect.AddNewWhatsupConnect(companyId);
                    await _unitOfWork.CompleteAsync();

                    return Json(new { status = true, message = "تم قطع الاتصال بنجاح" });
                }
                else
                {
                    return Json(new { status = false, message = "حدث خطأ أثناء قطع الاتصال. تأكد من حالة الخادم." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"حدث خطأ أثناء قطع الاتصال: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Phone) || string.IsNullOrEmpty(request.Message))
                {
                    return Json(new { status = false, message = "رقم الهاتف والرسالة مطلوبان" });
                }

                // استخدام الـ Extension Method لإرسال الرسالة
                var result = await WhatsAppServicesExtension.SendMessageAsync(request.Phone, request.Message, request.CompanyId);

                // إرجاع الاستجابة الناجحة
                return Json(new { status = true, message = result });
            }
            catch (Exception ex)
            {
                // التعامل مع الأخطاء
                return Json(new { status = false, message = $"حدث خطأ أثناء إرسال الرسالة: {ex.Message}" });
            }
        }


        private async Task<bool> UpdateClientInfo(string lessorCode, ClientInfoWhatsup clientInfoWhatsup)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                bool isBusiness = clientInfoWhatsup.IsBussenis != "false" && clientInfoWhatsup.IsBussenis != "undefined";

                var lastConnect = await GetLastConnect(lessorCode);
                if (lastConnect == null) return false;
                if (lastConnect.CrCasLessorWhatsupConnectStatus == Status.Active) return true;

                var updateClient = await _masTechnicalConnect.UpdateWhatsupConnectInfo(
                    lessorCode,
                    clientInfoWhatsup.Name,
                    clientInfoWhatsup.Mobile,
                    clientInfoWhatsup.DeviceType,
                    isBusiness,
                    user.CrMasUserInformationCode
                );

                if (updateClient) if (await _unitOfWork.CompleteAsync() > 0) return true;
                return false;

            }
            catch (DbUpdateConcurrencyException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<CrCasLessorWhatsupConnect> GetLastConnect(string LessorCode)
        {
            var lastConnect = await _unitOfWork.CrCasLessorWhatsupConnect.FindAllAsync(x => x.CrCasLessorWhatsupConnectLessor == LessorCode);
            return lastConnect.OrderByDescending(x => x.CrCasLessorWhatsupConnectSerial).FirstOrDefault();
        }
    }
}
