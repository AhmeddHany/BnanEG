using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.MAS;
using Bnan.Ui.ViewModels.MAS.WhatsupVMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using NToastNotify;
using System.Net;
using System.Net.Mail;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Net.Http;

namespace Bnan.Ui.Areas.MAS.Controllers.Companies
{

    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class CompanyMessagesController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CompanyMessagesController> _localizer;
        private readonly string pageNumber = SubTasks.CompanyMessages;



        public CompanyMessagesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CompanyMessagesController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
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
            // استعلام رئيسي مع NoTracking
            IQueryable<CrMasLessorInformation> query = _unitOfWork.CrMasLessorInformation.GetTableNoTracking().Include("CrCasBranchInformations.CrCasBranchPost.CrCasBranchPostCityNavigation")/*.Where(x => x.CrMasLessorInformationCode != "0000")*/;
            // ارجاع الشركات الفعّالة
            var lessors = await query.Where(x => x.CrMasLessorInformationStatus == Status.Active).ToListAsync();
            // إذا لم توجد شركات فعّالة، استرجاع الشركات المعلّقة
            if (!lessors.Any())
            {
                lessors = await query.Where(x => x.CrMasLessorInformationStatus == Status.Hold).ToListAsync();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

          //  var all_status_sms = await _unitOfWork.CrMasLessorCommunication.FindAllWithSelectAsNoTrackingAsync(
          //predicate: null,
          //selectProjection: query => query.Select(x => new lessor_SMS_VM
          //{
          //    lessor_Code = x.CrMasLessorCommunicationsLessorCode.Trim(),
          //    sms_Name = x.CrMasLessorCommunicationsSmsName,
          //    sms_Api = x.CrMasLessorCommunicationsSmsApi,
          //    sms_Status = x.CrMasLessorCommunicationsSmsStatus,
          //}));

            Mas_CompanyMessages_VM VM = new Mas_CompanyMessages_VM();
            VM.all_lessors = lessors;
            //VM.all_status_sms = all_status_sms;
            return View(VM);
        }


        [HttpPost]
        public async Task<IActionResult> send_ToAll_Whatsapp(List<IFormFile> files, string text, string address, string selectedValues, string all_mobiles, string all_mails)
        {
            if (string.IsNullOrEmpty(all_mobiles))
            {
                //return Json(new { status = false, message = "رقم الهاتف والرسالة مطلوبان" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_M_phoneAndMessageRequired"]}" });
            }
            IFormFile file= null;           
            List<ResultResponseWithNo> listResultResponseWithNo = new List<ResultResponseWithNo>();
            var messageErrors = " ";
            //if (string.IsNullOrEmpty(text) || text == "undefined") text= "اهلا وسهلا";
            string[] splitArray_mob = all_mobiles.Split(',');
            List<string> list_mobiles = splitArray_mob.ToList();
            string[] splitArray_mails = all_mails.Split(',');
            List<string> list_emails = splitArray_mails.ToList();
            string[] splitArray_companys = selectedValues.Split(',');
            List<string> list_companys = splitArray_companys.ToList();
            //if(address == "undefined" ||address == "" || address == " " || address == null)

            MediaRequest request = new MediaRequest();
            request.Phone = splitArray_mob[0];
            request.CompanyId = "0000";
            request.Message = text;

            var checkConneted = await WhatsAppServicesExtension.CheckIsConnected(request.CompanyId);
            // تحويل النص إلى كائن JSON
            // تحويل النص إلى كائن من النوع Response
            var jsonObject_check_Response = JsonConvert.DeserializeObject<dynamic>(checkConneted);
            if (jsonObject_check_Response == null  || jsonObject_check_Response?.status == false || jsonObject_check_Response?.key != "3")
            {
                //return Json(new { status = false, message = $"جوال نظام بنان غير متصل" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_address_M_Error_BnanPhone_notConnected"]} " });
            }
            var result = "";

            if (files.Count>0)
            {
                file = files[0];
            }            

            if (file != null && file.Length > 0)
            {
                string fileName = Path.GetFileName(file.FileName);  // اسم الملف
                // قراءة محتويات الملف في مصفوفة بايت
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    // تحويل المصفوفة البايت إلى Base64 string
                    string base64String = Convert.ToBase64String(memoryStream.ToArray());

                    // طباعة الـ Base64 string للتأكيد
                    Console.WriteLine(base64String);
                    request.fileBase64 = base64String;
                    request.filename = fileName;
                    //// يمكنك الآن إرسال base64String أو تخزينه حسب الحاجة
                    //return Ok(new { base64 = base64String });
                    var ii = 0;
                    try
                    {
                        for (ii = 0; ii < list_companys.Count; ii++)
                        {
                            request.Phone = list_mobiles[ii];
                            result = await WhatsAppServicesExtension.SendMediaAsync(request.Phone, request.Message, request.CompanyId, request.fileBase64, request.filename);
                            if (result != ApiResponseStatus.Success)
                            {
                                ResultResponseWithNo singleResult_Obj = new ResultResponseWithNo();
                                singleResult_Obj.message = result;
                                singleResult_Obj.companyId = list_companys[ii];
                                listResultResponseWithNo.Add(singleResult_Obj);
                            }
                        }
                        listResultResponseWithNo.DistinctBy(y => y.companyId).ToList();
                        foreach (var error in listResultResponseWithNo)
                        {
                            messageErrors = messageErrors + $" {_localizer["toasterForWhats_" + error.message]} : {error.companyId} ,";
                        }
                        // إرجاع الاستجابة الناجحة
                        return Json(new { status = true, message = messageErrors });
                    }
                    catch (Exception ex)
                    {
                        // التعامل مع الأخطاء
                        //return Json(new { status = false, message = $"حدث خطأ أثناء إرسال الرسالة للشركة رقم: {list_companys[ii]} وكل ما بعدها" });
                        return Json(new { status = false, message = $"{_localizer["CompanyMessages_address_M_ErrorPart1"]} {list_companys[ii]} {_localizer["CompanyMessages_address_M_ErrorPart2"]} " });
                    }
                }
            }
            if (string.IsNullOrEmpty(text))
            {
                //return Json(new { status = false, message = "رقم الهاتف والرسالة مطلوبان" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_M_phoneAndMessageRequired"]}" });
            }
            var i = 0;
            try
            {
                // // استخدام الـ Extension Method لإرسال الرسالة
                //var result = await WhatsAppServicesExtension.SendMessageAsync(request.Phone, request.Message, request.CompanyId);
                //string phone, string message, string companyId, string fileBase64, string filename
                for (i = 0; i < list_companys.Count; i++)
                {
                    request.Phone = list_mobiles[i];
                    result = await WhatsAppServicesExtension.SendMessageAsync(request.Phone, request.Message, request.CompanyId);
                    if (result != ApiResponseStatus.Success)
                    {
                        ResultResponseWithNo singleResult_Obj = new ResultResponseWithNo();
                        singleResult_Obj.message = result;
                        singleResult_Obj.companyId = list_companys[i];
                        listResultResponseWithNo.Add(singleResult_Obj);
                    }
                }
                listResultResponseWithNo.DistinctBy(y => y.companyId).ToList();
                foreach (var error in listResultResponseWithNo)
                {
                    messageErrors = messageErrors + $" {_localizer["toasterForWhats_" + error.message]} : {error.companyId} ,";
                }
                // إرجاع الاستجابة الناجحة
                return Json(new { status = true, message = messageErrors });
                //return BadRequest("No file uploaded.");
            }
            catch (Exception ex)
            {
                // التعامل مع الأخطاء
                //return Json(new { status = false, message = $"حدث خطأ أثناء إرسال الرسالة للشركة رقم: {list_companys[i]} وكل ما بعدها" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_address_M_ErrorPart1"]} {list_companys[i]} {_localizer["CompanyMessages_address_M_ErrorPart2"]} " });
            }

        }


        [HttpPost]
        public async Task<IActionResult> send_ToAll_Email(List<IFormFile> files, string text, string address, string selectedValues, string all_mobiles, string all_mails)
        {
            if (string.IsNullOrEmpty(all_mails))
            {
                //return Json(new { status = false, message = "رقم الهاتف والرسالة مطلوبان" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_M_phoneAndMessageRequired"]}" });
            }
            IFormFile file = null;
            List<ResultResponseWithNo> listResultResponseWithNo = new List<ResultResponseWithNo>();
            var messageErrors = " ";
            //if (string.IsNullOrEmpty(text) || text == "undefined") text= "اهلا وسهلا";
            string[] splitArray_mob = all_mobiles.Split(',');
            List<string> list_mobiles = splitArray_mob.ToList();
            string[] splitArray_mails = all_mails.Split(',');
            List<string> list_emails = splitArray_mails.ToList();
            string[] splitArray_companys = selectedValues.Split(',');
            List<string> list_companys = splitArray_companys.ToList();
            //if(address == "undefined" ||address == "" || address == " " || address == null)

            MediaRequest request = new MediaRequest();
            request.Phone = splitArray_mob[0];
            request.CompanyId = "0000";
            request.Message = text;

            var checkConneted = await WhatsAppServicesExtension.CheckIsConnected(request.CompanyId);
            // تحويل النص إلى كائن JSON
            // تحويل النص إلى كائن من النوع Response
            var jsonObject_check_Response = JsonConvert.DeserializeObject<dynamic>(checkConneted);
            if (jsonObject_check_Response == null || jsonObject_check_Response?.status == false || jsonObject_check_Response?.key != "3")
            {
                //return Json(new { status = false, message = $"جوال نظام بنان غير متصل" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_address_M_Error_BnanPhone_notConnected"]} " });
            }
            var result = "";

            if (files.Count > 0)
            {
                file = files[0];
            }

            if (file != null && file.Length > 0)
            {
                string fileName = Path.GetFileName(file.FileName);  // اسم الملف
                // قراءة محتويات الملف في مصفوفة بايت
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    // تحويل المصفوفة البايت إلى Base64 string
                    string base64String = Convert.ToBase64String(memoryStream.ToArray());

                    // طباعة الـ Base64 string للتأكيد
                    Console.WriteLine(base64String);
                    request.fileBase64 = base64String;
                    request.filename = fileName;
                    //// يمكنك الآن إرسال base64String أو تخزينه حسب الحاجة
                    //return Ok(new { base64 = base64String });

                }
            }
            if (string.IsNullOrEmpty(text))
            {
                //return Json(new { status = false, message = "رقم الهاتف والرسالة مطلوبان" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_M_phoneAndMessageRequired"]}" });
            }
            var i = 0;
            try
            {
                // // استخدام الـ Extension Method لإرسال الرسالة
                //var result = await WhatsAppServicesExtension.SendMessageAsync(request.Phone, request.Message, request.CompanyId);
                //string phone, string message, string companyId, string fileBase64, string filename
                for (i = 0; i < list_companys.Count; i++)
                {
                    request.Phone = list_mobiles[i];
                    result = await SendEmailAsync(request.Phone, request.Message, request.CompanyId, request.CompanyId);
                    if (result != ApiResponseStatus.Success)
                    {
                        ResultResponseWithNo singleResult_Obj = new ResultResponseWithNo();
                        singleResult_Obj.message = result;
                        singleResult_Obj.companyId = list_companys[i];
                        listResultResponseWithNo.Add(singleResult_Obj);
                    }
                }
                listResultResponseWithNo.DistinctBy(y => y.companyId).ToList();
                foreach (var error in listResultResponseWithNo)
                {
                    messageErrors = messageErrors + $" {_localizer["toasterForWhats_" + error.message]} : {error.companyId} ,";
                }
                // إرجاع الاستجابة الناجحة
                return Json(new { status = true, message = messageErrors });
                //return BadRequest("No file uploaded.");
            }
            catch (Exception ex)
            {
                // التعامل مع الأخطاء
                //return Json(new { status = false, message = $"حدث خطأ أثناء إرسال الرسالة للشركة رقم: {list_companys[i]} وكل ما بعدها" });
                return Json(new { status = false, message = $"{_localizer["CompanyMessages_address_M_ErrorPart1"]} {list_companys[i]} {_localizer["CompanyMessages_address_M_ErrorPart2"]} " });
            }

        }

        public static async Task<string> SendEmailAsync(string mail, string message, string title, string companyId)
        {
            // إعداد البيانات بتنسيق x-www-form-urlencoded
            var formData = new Dictionary<string, string>
        {
            { "mail", mail },
            { "message", message },
            { "title", title },

            { "id", companyId }
        };

            var data = new FormUrlEncodedContent(formData);

            try
            {
                // إعداد بيانات البريد الإلكتروني

                // إعداد البيانات اللازمة
                var smtpServer = "smtp.office365.com"; // خادم SMTP لـ Outlook
                var smtpPort = 587; // المنفذ الذي يدعم STARTTLS
                var fromEmail = "mazen144essam@yahoo.com"; // بريد المرسل
                var fromPassword = "moza123456"; // كلمة مرور التطبيق (إذا كنت تستخدم المصادقة الثنائية)
                var toEmail = "mazen142000@yahoo.com"; // بريد المستلم
                var subject = "Test Email";
                var body = "This is a test email sent via Outlook SMTP.";


                // إعداد SmtpClient
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(fromEmail, fromPassword); // المصادقة باستخدام البريد وكلمة المرور
                    smtpClient.EnableSsl = true; // تأكد من أن SSL مفعل
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // إعداد الرسالة
                    var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);

                    // إرسال البريد الإلكتروني
                    smtpClient.Send(mailMessage);
                    Console.WriteLine("Email sent successfully.");
                }

                // إرسال رد بنجاح
                //return Content("Email sent successfully.");

                //var content = await response.Content.ReadAsStringAsync();
                //var jsonResult = JsonConvert.DeserializeObject<dynamic>(content);

                // // التحقق من الحالة
                //if (jsonResult != null && (jsonResult.status == true || jsonResult.status.ToString().ToLower() == "true")) return ApiResponseStatus.Success;
                return ApiResponseStatus.Success;
                return ApiResponseStatus.Failure;
            }
            catch (HttpRequestException)
            {
                return ApiResponseStatus.ServerError;
            }
            catch (Exception)
            {
                return ApiResponseStatus.ServerError;
            }
        }

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {
            //await _userLoginsService.SaveTracing(currentCar.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            //subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            //subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, "بنان", "Bnan");

            var recordAr = licence.CrMasRenterInformationArName;
            var recordEn = licence.CrMasRenterInformationEnName;
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
            return RedirectToAction("Index", "ReportActiveContract");
        }
    }
}

