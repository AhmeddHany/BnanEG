using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.CAS.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using NToastNotify;

namespace Bnan.Ui.Areas.MAS.Controllers
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class LessorImagesController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<LessorImagesController> _localizer;
        private readonly IToastNotification _toastNotification;
        private readonly IUserLoginsService _userLoginsService;

        public LessorImagesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment webHostEnvironment, IStringLocalizer<LessorImagesController> localizer, IToastNotification toastNotification, IUserLoginsService userLoginsService) : base(userManager, unitOfWork, mapper)
        {
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _toastNotification = toastNotification;
            _userLoginsService = userLoginsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            //save Tracing
            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101002", "1");

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);



            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "1";

            // Set Title
            var titles = await setTitle("101", "1101002", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            //Check User Sub Validation
            var UserValidation = await CheckUserSubValidation("1101002");
            if (UserValidation == false) return RedirectToAction("Index", "Home", new { area = "MAS" });

            var Lessors = _unitOfWork.CrMasLessorInformation.FindAll(l => l.CrMasLessorInformationCode != "0000" && l.CrMasLessorInformationStatus == Status.Active);
            return View(Lessors);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "1";

            var lessor = _unitOfWork.CrMasLessorInformation.Find(x => x.CrMasLessorInformationCode == id);

            var lessorimgs = _unitOfWork.CrMasLessorImage.Find(x => x.CrMasLessorImageCode == lessor.CrMasLessorInformationCode);
            var lessorVM = _mapper.Map<LessorImagesVM>(lessorimgs);
            lessorVM.CrMasLessorNameAr = lessor.CrMasLessorInformationArLongName;
            lessorVM.CrMasLessorNameEn = lessor.CrMasLessorInformationEnLongName;
            // Set Title
            var titles = await setTitle("101", "1101002", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Update", titles[3]);
            return View(lessorVM);
        }

        [HttpPost]
        public async Task<IActionResult> EditImages(IFormCollection formData)
        {
            string lessorCode = Request.Headers["lessorcode"].ToString();

            var lessorImages = _unitOfWork.CrMasLessorImage.GetById(lessorCode);
            if (lessorImages != null)
            {
                string foldername = $"{"images\\Company"}\\{lessorImages.CrMasLessorImageCode}\\{"Support Images"}";
                foreach (var item in formData.Files)
                {
                    var Name = item.Name;
                    var NewName = Name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقت
                    var file = item;
                    if (file != null && Name != null)
                    {

                        if (Name == "CompanyLogo") lessorImages.CrMasLessorImageLogo = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageLogo);
                        if (Name == "Stamp") lessorImages.CrMasLessorImageStamp = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageStamp);
                        if (Name == "StampOutsideCity") lessorImages.CrMasLessorImageStampOutsideCity = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageStampOutsideCity);
                        if (Name == "StampOutsideCountry") lessorImages.CrMasLessorImageStampOutsideCountry = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageStampOutsideCountry);
                        if (Name == "StampFullAmountPaid") lessorImages.CrMasLessorImageStampFullAmountPaid = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageStampFullAmountPaid);
                        if (Name == "SignatureDirector") lessorImages.CrMasLessorImageSignatureDirector = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageSignatureDirector);
                        if (Name == "ContractCard") lessorImages.CrMasLessorImageContractCard = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageContractCard);
                        if (Name == "ExtensionContractCard") lessorImages.CrMasLessorImageContractExtensionCard = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageContractExtensionCard);
                        if (Name == "Contract24HourCard") lessorImages.CrMasLessorImageContract24Hour = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageContract24Hour);
                        if (Name == "Contract4Hour") lessorImages.CrMasLessorImageContract4Hour = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageContract4Hour);
                        if (Name == "ContractFinished") lessorImages.CrMasLessorImageContractFinished = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageContractFinished);
                        if (Name == "ContractClosed") lessorImages.CrMasLessorImageContractClosed = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageContractClosed);
                        if (Name == "ContractCancelled") lessorImages.CrMasLessorImageContractCancelled = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageContractCancelled);



                        if (Name == "ArInitialInvoice") lessorImages.CrMasLessorImageArInitialInvoice = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArInitialInvoice);
                        if (Name == "ArActualInvoice") lessorImages.CrMasLessorImageArActualInvoice = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArActualInvoice);
                        if (Name == "ArExternalCatchReceipt") lessorImages.CrMasLessorImageArExternalCatchReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArExternalCatchReceipt);
                        if (Name == "ArExternalBillExchangeReceipt") lessorImages.CrMasLessorImageArExternalBillExchangeReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArExternalBillExchangeReceipt);
                        if (Name == "ArInternalCatchReceipt") lessorImages.CrMasLessorImageArInternalCatchReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArInternalCatchReceipt);
                        if (Name == "ArInternalBillExchangeReceipt") lessorImages.CrMasLessorImageArInternalBillExchangeReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArInternalBillExchangeReceipt);
                        if (Name == "ArContractPage1") lessorImages.CrMasLessorImageArContractPage1 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage1);
                        if (Name == "ArContractPage2") lessorImages.CrMasLessorImageArContractPage2 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage2);
                        if (Name == "ArContractPage3") lessorImages.CrMasLessorImageArContractPage3 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage3);
                        if (Name == "ArContractPage4") lessorImages.CrMasLessorImageArContractPage4 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage4);
                        if (Name == "ArContractPage5") lessorImages.CrMasLessorImageArContractPage5 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage5);
                        if (Name == "ArContractPage6") lessorImages.CrMasLessorImageArContractPage6 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage6);
                        if (Name == "ArContractPage7") lessorImages.CrMasLessorImageArContractPage7 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage7);
                        if (Name == "ArContractPage8") lessorImages.CrMasLessorImageArContractPage8 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage8);
                        if (Name == "ArContractPage9") lessorImages.CrMasLessorImageArContractPage9 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractPage9);
                        if (Name == "ArSettlementContract") lessorImages.CrMasLessorImageArSettlementContractPage = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArSettlementContractPage);
                        if (Name == "ArSettlementContractExpenses") lessorImages.CrMasLessorImageArSettlementContractExpenses = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArSettlementContractExpenses);
                        if (Name == "ArSettlementContractCompensation") lessorImages.CrMasLessorImageArSettlementContractCompensation = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArSettlementContractCompensation);
                        if (Name == "ArContractTerms1") lessorImages.CrMasLessorImageArContractTerms1 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractTerms1);
                        if (Name == "ArContractTerms2") lessorImages.CrMasLessorImageArContractTerms2 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractTerms2);
                        if (Name == "ArContractTerms3") lessorImages.CrMasLessorImageArContractTerms3 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageArContractTerms3);



                        if (Name == "EnInitialInvoice") lessorImages.CrMasLessorImageEnInitialInvoice = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnInitialInvoice);
                        if (Name == "EnActualInvoice") lessorImages.CrMasLessorImageEnActualInvoice = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnActualInvoice);
                        if (Name == "EnExternalCatchReceipt") lessorImages.CrMasLessorImageEnExternalCatchReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnExternalCatchReceipt);
                        if (Name == "EnExternalBillExchangeReceipt") lessorImages.CrMasLessorImageEnExternalBillExchangeReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnExternalBillExchangeReceipt);
                        if (Name == "EnInternalCatchReceipt") lessorImages.CrMasLessorImageEnInternalCatchReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnInternalCatchReceipt);
                        if (Name == "EnInternalBillExchangeReceipt") lessorImages.CrMasLessorImageEnInternalBillExchangeReceipt = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnInternalBillExchangeReceipt);
                        if (Name == "EnContractPage1") lessorImages.CrMasLessorImageEnContractPage1 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage1);
                        if (Name == "EnContractPage2") lessorImages.CrMasLessorImageEnContractPage2 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage2);
                        if (Name == "EnContractPage3") lessorImages.CrMasLessorImageEnContractPage3 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage3);
                        if (Name == "EnContractPage4") lessorImages.CrMasLessorImageEnContractPage4 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage4);
                        if (Name == "EnContractPage5") lessorImages.CrMasLessorImageEnContractPage5 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage5);
                        if (Name == "EnContractPage6") lessorImages.CrMasLessorImageEnContractPage6 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage6);
                        if (Name == "EnContractPage7") lessorImages.CrMasLessorImageEnContractPage7 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage7);
                        if (Name == "EnContractPage8") lessorImages.CrMasLessorImageEnContractPage8 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage8);
                        if (Name == "EnContractPage9") lessorImages.CrMasLessorImageEnContractPage9 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractPage9);
                        if (Name == "EnSettlementContract") lessorImages.CrMasLessorImageEnSettlementContractPage = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnSettlementContractPage);
                        if (Name == "EnSettlementContractExpenses") lessorImages.CrMasLessorImageEnSettlementContractExpenses = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnSettlementContractExpenses);
                        if (Name == "EnSettlementContractCompensation") lessorImages.CrMasLessorImageEnSettlementContractCompensation = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnSettlementContractCompensation);
                        if (Name == "EnContractTerms1") lessorImages.CrMasLessorImageEnContractTerms1 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractTerms1);
                        if (Name == "EnContractTerms2") lessorImages.CrMasLessorImageEnContractTerms2 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractTerms2);
                        if (Name == "EnContractTerms3") lessorImages.CrMasLessorImageEnContractTerms3 = await file.SaveImageAsync(_webHostEnvironment, foldername, NewName, ".png", lessorImages.CrMasLessorImageEnContractTerms3);


                    }
                    _unitOfWork.CrMasLessorImage.Update(lessorImages);


                    // SaveTracingb 
                    var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101002", "1");
                    var RecordAr = "";
                    var RecordEn = "";
                    RecordAr = _unitOfWork.CrMasLessorInformation.Find(x => x.CrMasLessorInformationCode == lessorImages.CrMasLessorImageCode).CrMasLessorInformationArShortName + " - " + _localizer[item.Name];
                    RecordEn = _unitOfWork.CrMasLessorInformation.Find(x => x.CrMasLessorInformationCode == lessorImages.CrMasLessorImageCode).CrMasLessorInformationEnShortName + " - " + _localizer[item.Name];
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "إضافة صورة", "Add Image", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


                }

                if (Request.Form.ContainsKey("imagesForRemove"))
                {
                    var imagesForRemoveJson = Request.Form["imagesForRemove"];
                    var imagesForRemove = JsonConvert.DeserializeObject<List<RemoveFileViewModel>>(imagesForRemoveJson);
                    if (imagesForRemove != null)
                    {
                        foreach (var image in imagesForRemove)
                        {
                            var id = image.id;
                            if (id == "CompanyLogo")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageLogo);
                                if (Tr == true) lessorImages.CrMasLessorImageLogo = null;
                            }
                            if (id == "Stamp")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageStamp);
                                if (Tr == true) lessorImages.CrMasLessorImageStamp = null;
                            }
                            if (id == "StampOutsideCity")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageStampOutsideCity);
                                if (Tr == true) lessorImages.CrMasLessorImageStampOutsideCity = null;
                            };
                            if (id == "StampOutsideCountry")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageStampOutsideCountry);
                                if (Tr == true) lessorImages.CrMasLessorImageStampOutsideCountry = null;
                            };
                            if (id == "StampFullAmountPaid")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageStampFullAmountPaid);
                                if (Tr == true) lessorImages.CrMasLessorImageStampFullAmountPaid = null;
                            }
                            if (id == "SignatureDirector")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageSignatureDirector);
                                if (Tr == true) lessorImages.CrMasLessorImageSignatureDirector = null;
                            }
                            if (id == "ContractCard")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageContractCard);
                                if (Tr == true) lessorImages.CrMasLessorImageContractCard = null;
                            }
                            if (id == "ContractExtensionCard")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageContractExtensionCard);
                                if (Tr == true) lessorImages.CrMasLessorImageContractExtensionCard = null;
                            }
                            if (id == "Contract24Hour")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageContract24Hour);
                                if (Tr == true) lessorImages.CrMasLessorImageContract24Hour = null;
                            }
                            if (id == "Contract4Hour")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageContract4Hour);
                                if (Tr == true) lessorImages.CrMasLessorImageContract4Hour = null;
                            }
                            if (id == "ContractFinished")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageContractFinished);
                                if (Tr == true) lessorImages.CrMasLessorImageContractFinished = null;
                            }
                            if (id == "ContractClosed")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageContractClosed);
                                if (Tr == true) lessorImages.CrMasLessorImageContractClosed = null;
                            }
                            if (id == "ContractCancelled")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageContractCancelled);
                                if (Tr == true) lessorImages.CrMasLessorImageContractCancelled = null;
                            }
                            if (id == "ArInitialInvoice")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArInitialInvoice);
                                if (Tr == true) lessorImages.CrMasLessorImageArInitialInvoice = null;
                            }
                            if (id == "ArActualInvoice")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArActualInvoice);
                                if (Tr == true) lessorImages.CrMasLessorImageArActualInvoice = null;
                            }

                            if (id == "ArExternalCatchReceipt")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArExternalCatchReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageArExternalCatchReceipt = null;
                            }
                            if (id == "ArExternalBillExchangeReceipt")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArExternalBillExchangeReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageArExternalBillExchangeReceipt = null;
                            }
                            if (id == "ArInternalCatchReceipt")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArInternalCatchReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageArInternalCatchReceipt = null;
                            }
                            if (id == "ArInternalBillExchangeReceipt") 
                            { 
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArInternalBillExchangeReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageArInternalBillExchangeReceipt = null;
                            }
                            if (id == "ArContractPage1")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage1);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage1 = null;
                            }
                            if (id == "ArContractPage2")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage2);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage2 = null;
                            }
                            if (id == "ArContractPage3")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage3);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage3 = null;
                            }
                            if (id == "ArContractPage4")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage4);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage4 = null;
                            }
                            if (id == "ArContractPage5")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage5);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage5 = null;
                            }
                            if (id == "ArContractPage6")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage6);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage6 = null;
                            }
                            if (id == "ArContractPage7")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage7);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage7 = null;
                            }
                            if (id == "ArContractPage8")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage8);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage8 = null;
                            }
                            if (id == "ArContractPage9")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractPage9);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractPage9 = null;
                            }
                            if (id == "ArSettlementContract")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArSettlementContractPage);
                                if (Tr == true) lessorImages.CrMasLessorImageArSettlementContractPage = null;
                            }
                            if (id == "ArSettlementContractExpenses")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArSettlementContractExpenses);
                                if (Tr == true) lessorImages.CrMasLessorImageArSettlementContractExpenses = null;
                            }
                            if (id == "ArSettlementContractCompensation")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArSettlementContractCompensation);
                                if (Tr == true) lessorImages.CrMasLessorImageArSettlementContractCompensation = null;
                            }
                            if (id == "ArContractTerms1")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractTerms1);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractTerms1 = null;
                            }
                            if (id == "ArContractTerms2")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractTerms2);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractTerms2 = null;
                            }
                            if (id == "ArContractTerms3")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageArContractTerms3);
                                if (Tr == true) lessorImages.CrMasLessorImageArContractTerms3 = null;
                            }


                            if (id == "EnInitialInvoice")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnInitialInvoice);
                                if (Tr == true) lessorImages.CrMasLessorImageEnInitialInvoice = null;
                            }
                            if (id == "EnActualInvoice") { 
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnActualInvoice);
                                if (Tr == true) lessorImages.CrMasLessorImageEnActualInvoice = null;
                            }
                            if (id == "EnExternalCatchReceipt") {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnExternalCatchReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageEnExternalCatchReceipt = null;
                            }
                            if (id == "EnExternalBillExchangeReceipt") { 
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnExternalBillExchangeReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageEnExternalBillExchangeReceipt = null;
                            }
                            if (id == "EnInternalCatchReceipt") {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnInternalCatchReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageEnInternalCatchReceipt = null;
                            }
                            if (id == "EnInternalBillExchangeReceipt") {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnInternalBillExchangeReceipt);
                                if (Tr == true) lessorImages.CrMasLessorImageEnInternalBillExchangeReceipt = null;
                            }
                            if (id == "EnContractPage1")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage1);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage1 = null;
                            }
                            if (id == "EnContractPage2")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage2);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage2 = null;
                            }
                            if (id == "EnContractPage3")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage3);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage3 = null;
                            }
                            if (id == "EnContractPage4")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage4);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage4 = null;
                            }
                            if (id == "EnContractPage5")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage5);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage5 = null;
                            }
                            if (id == "EnContractPage6")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage6);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage6 = null;
                            }
                            if (id == "EnContractPage7")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage7);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage7 = null;
                            }
                            if (id == "EnContractPage8")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage8);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage8 = null;
                            }
                            if (id == "EnContractPage9")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractPage9);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractPage9 = null;
                            }

                            if (id == "EnSettlementContract")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnSettlementContractPage);
                                if (Tr == true) lessorImages.CrMasLessorImageEnSettlementContractPage = null;
                            }
                            if (id == "EnSettlementContractExpenses")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnSettlementContractExpenses);
                                if (Tr == true) lessorImages.CrMasLessorImageEnSettlementContractExpenses = null;
                            }
                            if (id == "EnSettlementContractCompensation")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnSettlementContractCompensation);
                                if (Tr == true) lessorImages.CrMasLessorImageEnSettlementContractCompensation = null;
                            }
                            if (id == "EnContractTerms1")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractTerms1);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractTerms1 = null;
                            }
                            if (id == "EnContractTerms2")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractTerms2);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractTerms2 = null;
                            }
                            if (id == "EnContractTerms3")
                            {
                                var Tr = await FileExtensions.RemoveFile(_webHostEnvironment, lessorImages.CrMasLessorImageEnContractTerms3);
                                if (Tr == true) lessorImages.CrMasLessorImageEnContractTerms3 = null;
                            }


                            _unitOfWork.CrMasLessorImage.Update(lessorImages);
                            // SaveTracing
                            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101002", "1");
                            var RecordAr = "";
                            var RecordEn = "";
                            RecordAr = _unitOfWork.CrMasLessorInformation.Find(x => x.CrMasLessorInformationCode == lessorImages.CrMasLessorImageCode).CrMasLessorInformationArShortName + " - " + _localizer[image.id];
                            RecordEn = _unitOfWork.CrMasLessorInformation.Find(x => x.CrMasLessorInformationCode == lessorImages.CrMasLessorImageCode).CrMasLessorInformationEnShortName + " - " + _localizer[image.id];
                            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "حذف صورة", "Delete Image", mainTask.CrMasSysMainTasksCode,
                            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                        }
                    }

                }
                _unitOfWork.Complete();
                return Json(true);
            }
            return Json(false);

        }
        public IActionResult SuccesssMessageForLessorImages()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index");
        }

    }
}