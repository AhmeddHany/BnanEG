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
        private readonly IWhatsupConnect _technicalConnect;

        private readonly IStringLocalizer<TechnicalConnectivityController> _localizer;
        private readonly string pageNumber = SubTasks.TechnicalConnectivityMAS;
        public TechnicalConnectivityController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<TechnicalConnectivityController> localizer, IWhatsupConnect technicalConnect) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _technicalConnect = technicalConnect;
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
            //var Communication = await _unitOfWork.CrMasLessorCommunication.FindAsync(x => x.CrMasLessorCommunicationsLessorCode == user.CrMasUserInformationLessor);
            //var communicationVM = _mapper.Map<CommunicationsVM>(Communication);

            return View();
        }
        //[HttpPost]
        //public async Task<IActionResult> Connect(CommunicationsVM model)
        //{
        //    try
        //    {
        //        var user = await _userManager.GetUserAsync(User);

        //        // Set page titles
        //        await SetPageTitleAsync(string.Empty, pageNumber);

        //        // Check validation
        //        if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
        //        {
        //            _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
        //            return RedirectToAction("Index", "Home");
        //        }

        //        var existingCommunication = await _unitOfWork.CrMasLessorCommunication.FindAsync(
        //            x => x.CrMasLessorCommunicationsLessorCode == user.CrMasUserInformationLessor
        //        );

        //        var newCommunication = _mapper.Map<CrMasLessorCommunication>(model);
        //        newCommunication.CrMasLessorCommunicationsLessorCode = user.CrMasUserInformationLessor;
        //        bool result = existingCommunication == null
        //        ? await _communications.AddCommunications(newCommunication)
        //            : await _communications.UpdateCommunications(newCommunication);

        //        if (result && await _unitOfWork.CompleteAsync() > 0)
        //        {
        //            _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
        //            await SaveTracingForUserChange(Status.Update, pageNumber);
        //            return RedirectToAction("Index", "Home");
        //        }

        //        _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
        //        return View(model);
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> GenerateQrCode()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var response = await WhatsAppServicesExtension.GenerateQrCode(user.CrMasUserInformationLessor);
                return Content(response, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = $"حدث خطأ أثناء توليد رمز الاستجابة السريعة. {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsClientReady()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var content = await WhatsAppServicesExtension.IsClientReady(user.CrMasUserInformationLessor);
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                if (apiResponse?.Status == true)
                {
                    var clientData = apiResponse.Data;
                    bool updateResult = await UpdateClientInfo(user.CrMasUserInformationLessor, clientData);

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
        public async Task<IActionResult> LogoutClient()
        {

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var response = await WhatsAppServicesExtension.LogoutAsync(user.CrMasUserInformationLessor);
                if (response.IsSuccessStatusCode)
                {
                    await _technicalConnect.ChangeStatusOldWhatsupConnect(user.CrMasUserInformationLessor, user.CrMasUserInformationCode);
                    await _technicalConnect.AddNewWhatsupConnect(user.CrMasUserInformationLessor);
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
                var user = await _userManager.GetUserAsync(User);

                if (string.IsNullOrEmpty(request.Phone) || string.IsNullOrEmpty(request.Message))
                {
                    return Json(new { status = false, message = "رقم الهاتف والرسالة مطلوبان" });
                }

                // استخدام الـ Extension Method لإرسال الرسالة
                var result = await WhatsAppServicesExtension.SendMessageAsync(request.Phone, request.Message, user.CrMasUserInformationLessor);

                // إرجاع الاستجابة الناجحة
                return Json(new { status = true, message = result });
            }
            catch (Exception ex)
            {
                // التعامل مع الأخطاء
                return Json(new { status = false, message = $"حدث خطأ أثناء إرسال الرسالة: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckLessorIsConnected()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // استدعاء الميثود الجديدة التي تحقق الاتصال
                var content = await WhatsAppServicesExtension.CheckIsConnected(user.CrMasUserInformationLessor);
                // تحويل الـ Response إلى كائن C#
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
                if (apiResponse.Key == "3" && apiResponse?.Status == true) return Json(new { status = true, message = "1" });
                else if (apiResponse.Key == "4" && apiResponse?.Status == true && apiResponse.Data != null)
                {
                    var clientData = apiResponse.Data;
                    var lastConnect = await GetLastConnect(user.CrMasUserInformationLessor);
                    if (lastConnect != null && lastConnect.CrCasLessorWhatsupConnectStatus == "N") return Json(new { status = true, message = "2" });
                    bool updateConnect = await _technicalConnect.ChangeStatusOldWhenDisconnectFromWhatsup(user.CrMasUserInformationLessor, clientData.LastLogOut);
                    bool addNewConnect = await _technicalConnect.AddNewWhatsupConnect(user.CrMasUserInformationLessor);
                    if (updateConnect && addNewConnect && await _unitOfWork.CompleteAsync() > 0) return Json(new { status = true, message = "3" });
                    else return Json(new { status = false, message = "حدث خطأ اثناء حفظ البيانات" });
                }
                else if (apiResponse?.Status == false && apiResponse.Key == "2") return Json(new { status = true, message = "4" });
                else return Json(new { status = false, message = "حدث خطأ ف الاتصال" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"حدث خطأ ف الاتصال: {ex.Message}" });
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

                var updateClient = await _technicalConnect.UpdateWhatsupConnectInfo(
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
        private async Task SaveTracingForUserChange(string status, string pageNumber)
        {


            var recordAr = "الربط التقني";
            var recordEn = "TechnicalConnectivity";
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
    }
}
