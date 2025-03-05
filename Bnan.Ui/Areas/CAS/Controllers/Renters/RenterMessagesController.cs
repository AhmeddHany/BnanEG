using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.CAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.MAS.WhatsupVMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using NToastNotify;
using Bnan.Ui.ViewModels.CAS.Renters;
using Bnan.Ui.ViewModels.CAS;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;

namespace Bnan.Ui.Areas.CAS.Controllers.Renters
{

    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class RenterMessagesController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterMessagesController> _localizer;
        private readonly string pageNumber = SubTasks.RentersCas_RentersMessages;



        public RenterMessagesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterMessagesController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
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
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation) || user==null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            bool Master_sms = true;
            bool Master_whatsapp = true;
            bool Master_email = true;
            bool Master_whatsapp_connect = true;

            var Master_Lessor = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode == user.CrMasUserInformationLessor).FirstOrDefault();
            var Master_sms_Lessor = _unitOfWork.CrCasLessorSmsConnect.FindAll(x => x.CrMasLessorSmsConnectLessor == user.CrMasUserInformationLessor).FirstOrDefault();
            if (Master_Lessor==null)
            {
                Master_whatsapp = false;
                Master_email = false;
            }
            if ((Master_Lessor?.CrMasLessorInformationContWhatsappKey+ Master_Lessor?.CrMasLessorInformationContWhatsapp)?.Length < 5)
            {
                Master_whatsapp = false;
            }
            if (Master_Lessor?.CrMasLessorInformationContEmail?.Length < 5 && Master_Lessor?.CrMasLessorInformationContEmailToken?.Length < 5)
            {
                Master_email = false;
            }
            if (Master_sms_Lessor == null || Master_sms_Lessor?.CrMasLessorSmsConnectStatus != "A")
            {
                Master_sms = false;
            }
            var checkConneted = await WhatsAppServicesExtension.CheckIsConnected(user.CrMasUserInformationLessor??"0");
            // تحويل النص إلى كائن JSON
            // تحويل النص إلى كائن من النوع Response
            var jsonObject_check_Response = JsonConvert.DeserializeObject<dynamic>(checkConneted);
            if (jsonObject_check_Response == null || jsonObject_check_Response?.status == false || jsonObject_check_Response?.key != "3")
            {
                Master_whatsapp_connect = false;
                Master_whatsapp = false;
            }

            if (!Master_sms && !Master_email && (!Master_whatsapp || !Master_whatsapp_connect))
            {
                _toastNotification.AddErrorToastMessage(_localizer["toasterForWhats_NoConnWay"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            var rates = await _unitOfWork.CrMasSysEvaluation.FindAllAsNoTrackingAsync(x => x.CrMasSysEvaluationsClassification == "1");

            var thisCompanyData = await _unitOfWork.CrMasLessorInformation.FindAllAsNoTrackingAsync(x => x.CrMasLessorInformationCode == user.CrMasUserInformationLessor);

            // استعلام رئيسي مع NoTracking
            IQueryable<CrCasRenterLessor> query = _unitOfWork.CrCasRenterLessor.GetTableNoTracking().Include("CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation")/*.Where(x => x.CrCasRenterLessorId != "0000")*/;
            
            // ارجاع الشركات الفعّالة
            var Renters = await query.Where(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor).ToListAsync();
            // إذا لم توجد شركات فعّالة، استرجاع الشركات المعلّقة
            if (!Renters.Any())
            {
                Renters = await query.Where(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor).ToListAsync();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

            //   var all_status_sms = await _unitOfWork.CrMasLessorCommunication.FindAllWithSelectAsNoTrackingAsync(
            // predicate: null,
            //selectProjection: query => query.Select(x => new Renter_SMS_VM
            //{
            //    Renter_Code = x.CrMasLessorCommunicationsLessorCode.Trim(),
            //    sms_Name = x.CrMasLessorCommunicationsSmsName,
            //    sms_Api = x.CrMasLessorCommunicationsSmsApi,
            //    sms_Status = x.CrMasLessorCommunicationsSmsStatus,
            //}));

            Cas_RenterMessages_VM VM = new Cas_RenterMessages_VM();
            VM.all_Renters = Renters;
            VM.Rates = rates;
            VM.thisCompanyData = thisCompanyData?.FirstOrDefault();
            VM.Master_sms = Master_sms;
            VM.Master_email = Master_email;
            VM.Master_whatsapp = Master_whatsapp;
            VM.Master_whatsapp_connect = Master_whatsapp_connect;

            //VM.all_status_sms = all_status_sms;
            return View(VM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAccountSalesPoint_CASByStatus(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            //sidebar Active

            if (!string.IsNullOrEmpty(search))
            {
                var rates = await _unitOfWork.CrMasSysEvaluation.FindAllAsNoTrackingAsync(x => x.CrMasSysEvaluationsClassification == "1");

                var thisCompanyData = await _unitOfWork.CrMasLessorInformation.FindAllAsNoTrackingAsync(x => x.CrMasLessorInformationCode == user.CrMasUserInformationLessor);

                // استعلام رئيسي مع NoTracking
                IQueryable<CrCasRenterLessor> query = _unitOfWork.CrCasRenterLessor.GetTableNoTracking().Include("CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation")/*.Where(x => x.CrCasRenterLessorId != "0000")*/;

                // ارجاع الشركات الفعّالة
                var Renters = await query.Where(x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor).ToListAsync();


                //  var all_status_sms = await _unitOfWork.CrMasLessorCommunication.FindAllWithSelectAsNoTrackingAsync(
                //predicate: null,
                //selectProjection: query => query.Select(x => new Renter_SMS_VM
                //{
                //    Renter_Code = x.CrMasLessorCommunicationsLessorCode.Trim(),
                //    sms_Name = x.CrMasLessorCommunicationsSmsName,
                //    sms_Api = x.CrMasLessorCommunicationsSmsApi,
                //    sms_Status = x.CrMasLessorCommunicationsSmsStatus,
                //}));

                var FilterAll_Renters = Renters.FindAll(x => x.CrCasRenterLessorStatus != Status.Deleted &&
                                                         (x.CrCasRenterLessorNavigation.CrMasRenterInformationArName.Contains(search) ||
                                                          x.CrCasRenterLessorNavigation.CrMasRenterInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                          x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateArName.Contains(search) ||
                                                          x.CrCasRenterLessorNavigation.CrMasRenterPost.CrMasRenterPostCityNavigation.CrMasSupPostCityConcatenateEnName.ToLower().Contains(search.ToLower()) ||
                                                          x.CrCasRenterLessorCode.Contains(search)));

                Cas_RenterMessages_VM VM = new Cas_RenterMessages_VM();
                VM.all_Renters = FilterAll_Renters;
                VM.Rates = rates;
                VM.thisCompanyData = thisCompanyData?.FirstOrDefault();
                return PartialView("_DataTableRenterMessages", VM);
                
            }
            return PartialView();
        }
    
        [HttpPost]
        public async Task<IActionResult> send_ToAll_Whatsapp(List<IFormFile> files, string text, string subject, string selectedValues, string all_mobiles, string all_mails)
        {
            if (string.IsNullOrEmpty(all_mobiles) || all_mobiles.Length < 5)
            {
                //return Json(new { status = false, message = " خطأ بالهاتف المسجل رقم الهاتف والرسالة مطلوبان"  });
                return Json(new { status = false, message = $"{_localizer["toasterForWhats_phoneRecordError"]}" });
            }
            List<string> list_mobiles = new List<string>(all_mobiles.Split(','));
            list_mobiles = list_mobiles.Where(x => x.Length > 5).ToList();
            if (list_mobiles.Count == 0)
            {
                // خطأ بالهاتف المسجل 
                return Json(new { status = false, message = $"{_localizer["toasterForWhats_phoneRecordError"]}" });
            }
            if (text.Length < 2)
            {
                // رجى كتابة الرسالة بشكل صحيح
                return Json(new { status = false, message = $"{_localizer["toasterForWhats_MessageEnterError"]}" });
            }
            var user = await _userManager.GetUserAsync(User);

            IFormFile file = null;           
            List<ResultResponseWithNo> listResultResponseWithNo = new List<ResultResponseWithNo>();
            var messageErrors = " ";
            string[] splitArray_mails = all_mails.Split(',');
            List<string> list_emails = splitArray_mails.ToList();
            string[] splitArray_Renters = selectedValues.Split(',');
            List<string> list_Renters = splitArray_Renters.ToList();

            MediaRequest request = new MediaRequest();
            request.Phone = list_mobiles[0];
            //request.CompanyId = "0000";
            request.CompanyId = user?.CrMasUserInformationLessor??"0";
            request.Message = text;

            var checkConneted = await WhatsAppServicesExtension.CheckIsConnected(request.CompanyId);
            // تحويل النص إلى كائن JSON
            // تحويل النص إلى كائن من النوع Response
            var jsonObject_check_Response = JsonConvert.DeserializeObject<dynamic>(checkConneted);
            if (jsonObject_check_Response == null  || jsonObject_check_Response?.status == false || jsonObject_check_Response?.key != "3")
            {
                //return Json(new { status = false, message = $"جوال الشركة غير متصل" });
                return Json(new { status = false, message = $"{_localizer["toasterForWhats_whatsappServicesNotConnected_ForthisLessor"]} " });
            }

            ///////
            var userData = _unitOfWork.CrMasUserInformation.FindAll(x => x.CrMasUserInformationCode == user.CrMasUserInformationCode).FirstOrDefault();

            if (Convert.ToInt16(user.CrMasUserInformationLessor ?? "1000") < 4006)
            {
                //list_mails.RemoveAll(x => x.Contains("@"));
                var mobs_count = list_mobiles.Count;
                list_mobiles.RemoveRange(0, list_mobiles.Count);
                for (int coun=0; coun < mobs_count; coun++)
                {
                    list_mobiles.Add((userData?.CrMasUserInformationCallingKey + userData?.CrMasUserInformationMobileNo) ?? "0");
                }

                list_mobiles = list_mobiles.Where(x => x.Length > 5).ToList();
                if (list_mobiles.Count == 0)
                {
                    return Json(new { status = false, message = $"{_localizer["toasterForWhats_phoneRecordError"]}" });
                }
            }
            ///////
            
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
                        for (ii = 0; ii < list_Renters.Count; ii++)
                        {
                            request.Phone = list_mobiles[ii];
                            result = await WhatsAppServicesExtension.SendMediaAsync(request.Phone, request.Message, request.CompanyId, request.fileBase64, request.filename);
                           
                        }
                     
                        // إرجاع الاستجابة الناجحة
                        return Json(new { status = true, message = ApiResponseStatus.Success });
                    }
                    catch (Exception ex)
                    {
                        // التعامل مع الأخطاء
                        return Json(new { status = false, message = _localizer["ToastFailed"] });
                    }
                }
            }
            if (string.IsNullOrEmpty(text))
            {
                //return Json(new { status = false, message = "يرجى كتابة الرسالة بشكل صحي" });
                return Json(new { status = false, message = $"{_localizer["toasterForWhats_MessageEnterError"]}" });
            }
            var i = 0;
            try
            {
                // // استخدام الـ Extension Method لإرسال الرسالة
                for (i = 0; i < list_Renters.Count; i++)
                {
                    request.Phone = list_mobiles[i];
                    result = await WhatsAppServicesExtension.SendMessageAsync(request.Phone, request.Message, request.CompanyId);
                    
                }
              

                // إرجاع الاستجابة الناجحة
                return Json(new { status = true, message = ApiResponseStatus.Success });

                //return BadRequest("No file uploaded.");
            }
            catch (Exception ex)
            {
                // التعامل مع الأخطاء
                return Json(new { status = false, message = _localizer["ToastFailed"] });
            }

        }

        [HttpPost]
        public async Task<IActionResult> send_ToAll_Email(List<IFormFile> files, string text, string subject, string selectedValues, string all_mobiles, string all_mails)

        {
            IFormFile file = null;
            if(all_mails == null || all_mails.Length < 5)
            {
                return Json(new { status = 2, message = $"{_localizer["toasterForWhats_EmailRecordError"]}", description = "no mails selected" });
            }
            if ( text == " " || text == "" || subject == " " || subject == "")
            {
                return Json(new { status = 0, message = $"{_localizer["toasterForWhats_EmailEnterError"]}", description = "no Text or Subject" });
            }
            List<string> list_mails = new List<string>(all_mails.Split(','));
            list_mails = list_mails.Where(x=>x.Length > 5).ToList();
            if(list_mails.Count == 0)
            {
                return Json(new { status = 3, message = $"{_localizer["toasterForWhats_EmailRecordError"]}", description = "no mails selected" });
            }

            if (files.Count > 0)
            {
                file = files[0];
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var Master_Lessor = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode == user.CrMasUserInformationLessor).FirstOrDefault();
                var Master_Lessor_mail = Master_Lessor?.CrMasLessorInformationContEmail;
                var Master_Lessor_EnName = Master_Lessor?.CrMasLessorInformationEnShortName;
                //var Master_Lessor_Password = "cgve uili qekq tlcu";
                var Master_Lessor_Password = Master_Lessor?.CrMasLessorInformationContEmailToken;

                ///////
                var userData = _unitOfWork.CrMasUserInformation.FindAll(x => x.CrMasUserInformationCode == user.CrMasUserInformationCode).FirstOrDefault();

                if (Convert.ToInt16(user.CrMasUserInformationLessor??"1000")<4006 )
                {
                    //list_mails.RemoveAll(x => x.Contains("@"));
                    list_mails.RemoveRange(0,list_mails.Count);
                    list_mails.Add(userData?.CrMasUserInformationEmail??"0");

                    list_mails = list_mails.Where(x => x.Length > 5).ToList();
                    if (list_mails.Count == 0)
                    {
                        return Json(new { status = 3, message = $"{_localizer["toasterForWhats_EmailRecordError"]}", description = "no mails selected" });
                    }
                }
                ///////

                if (Master_Lessor_mail?.Length < 5 && Master_Lessor_Password?.Length < 5) 
                {
                    return Json(new { status = 4, message = $"{_localizer["toasterForWhats_EmailNo_ForthisLessor"]}", description = "no lessor Mail لا يوجد بريد للشركة" });
                }

                    var message = new MimeMessage();
                //message.From.Add(new MailboxAddress("hazem", "hazem14442000@gmail.com"));
                message.From.Add(new MailboxAddress(Master_Lessor_EnName, Master_Lessor_mail));
                //message.To.Add(new MailboxAddress("essam", "mazen144essam@gmail.com"));

                message.Subject = subject;

                var body = new TextPart("plain")
                {
                    Text = text
                };
                ////  //////
                // //message.Body = body;
                var multipart = new Multipart("mixed");
                multipart.Add(body);

                if (file != null && file.Length > 0)
                {
                    // تحويل IFormFile إلى MimePart وإضافته كملف مرفق
                    var attachment = new MimePart(file.ContentType)
                    {
                        Content = new MimeContent(file.OpenReadStream(), ContentEncoding.Default),
                        FileName = file.FileName
                    };
                    multipart.Add(attachment);
                }

                message.Body = multipart;
                ////// /////

                using (var smtpClient = new SmtpClient())
                {

                    // الاتصال بـ SMTP Yahoo Mail باستخدام المنفذ 465 لـ SSL أو 587 لـ TLS
                    //smtpClient.Connect("smtp.mail.yahoo.com", 465, true);  // true تعني تفعيل SSL
                    //smtpClient.Connect("smtp.gmail.com", 587, false);
                    smtpClient.Connect("smtp.gmail.com", 465, true);// true تعني تفعيل SSL !!!!!!!

                    // التطبيق في Gmail التحقق من الهوية باستخدام البريد الإلكتروني وكلمة المرور
                    //smtpClient.Authenticate("hazem14442000@gmail.com", "cgve uili qekq tlcu");
                    smtpClient.Authenticate(Master_Lessor_mail, Master_Lessor_Password);
                    foreach (var single_Email in list_mails)
                    {
                        try
                        {
                            // إرسال البريد الإلكتروني
                            //message.To.Add(new MailboxAddress("essam", "khaled14442000@gmail.com"));
                            message.To.Add(new MailboxAddress("Hi Rrenter From " + Master_Lessor_EnName, single_Email));
                            await smtpClient.SendAsync(message);
                        }
                        catch (Exception ex)
                        {
                            var inb = 0;
                        }
                    }
                    smtpClient.Disconnect(true);
                }

                //return Ok("Email sent successfully!");
                return Json(new { status = true, message = ApiResponseStatus.Success });

                //return ApiResponseStatus.Success;
            }
            catch (HttpRequestException)
            {
                //return ApiResponseStatus.ServerError;
                return Json(new { status = false, message = ApiResponseStatus.ServerError });

            }
            catch (Exception ex)
            {
                //return ex.Message;
                return Json(new { status = false, message = ex.Message });

            }
        }

        [HttpPost]
        public async Task<IActionResult> send_Single_Email(List<IFormFile> files, string text, string subject, string selectedValues, string all_mobiles, string all_mails)

        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("hazem", "hazem14442000@gmail.com"));
                message.To.Add(new MailboxAddress("essam", "mazen144essam@gmail.com"));
                message.Subject = "Test Email";

                var body = new TextPart("plain")
                {
                    Text = "This is a test email new mazen."
                };
                message.Body = body;

                using (var smtpClient = new SmtpClient())
                {
                    ////smtpClient.Connect("smtp.yourserver.com", 587, false); 
                    //////smtpClient.Authenticate("your-email@example.com", "your-password");
                    ////smtpClient.Send(message);
                    ////smtpClient.Disconnect(true);

                    // الاتصال بـ SMTP Yahoo Mail باستخدام المنفذ 465 لـ SSL أو 587 لـ TLS
                    //smtpClient.Connect("smtp.mail.yahoo.com", 465, true);  // true تعني تفعيل SSL
                    //smtpClient.Connect("smtp.gmail.com", 587, false);
                    smtpClient.Connect("smtp.gmail.com", 465, true);// true تعني تفعيل SSL !!!!!!!

                    // التحقق من الهوية باستخدام البريد الإلكتروني وكلمة المرور
                    smtpClient.Authenticate("hazem14442000@gmail.com", "cgve uili qekq tlcu");
                    //smtpClient.Authenticate("mazen144essam@yahoo.com", "mozaessam@123456");

                    // إرسال البريد الإلكتروني
                    await smtpClient.SendAsync(message);
                    smtpClient.Disconnect(true);
                }

                //return Ok("Email sent successfully!");
                return Json(new { status = true, message = ApiResponseStatus.Success });

                //return ApiResponseStatus.Success;
            }
            catch (HttpRequestException)
            {
                //return ApiResponseStatus.ServerError;
                return Json(new { status = false, message = ApiResponseStatus.ServerError });

            }
            catch (Exception ex)
            {
                //return ex.Message;
                return Json(new { status = false, message = ex.Message });

            }
        }

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {

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

