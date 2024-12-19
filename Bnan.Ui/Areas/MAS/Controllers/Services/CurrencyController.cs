using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Inferastructure.Repository.MAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.Services
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class CurrencyController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasCurrency _masCurrency;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CurrencyController> _localizer;
        private readonly string pageNumber = SubTasks.Currency;


        public CurrencyController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCurrency masCurrency, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CurrencyController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masCurrency = masCurrency;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
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
            var cuurency_ratio = await _unitOfWork.CrMasSysConvertNoToText.FindAllAsNoTrackingAsync(x => x.CrMasSysConvertNoToTextCode == "27" || x.CrMasSysConvertNoToTextCode == "28" || x.CrMasSysConvertNoToTextCode == "29");
            var cuurency_full = await _unitOfWork.CrMasSysConvertNoToText.FindAllAsNoTrackingAsync(x => x.CrMasSysConvertNoToTextCode == "30" || x.CrMasSysConvertNoToTextCode == "31" || x.CrMasSysConvertNoToTextCode == "32" || x.CrMasSysConvertNoToTextCode == "33");
            if (cuurency_full == null && cuurency_ratio == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Currency");
            }
            CurrencyVVM vm = new CurrencyVVM();
            var C_full = _mapper.Map<List<CurrencyVM>>(cuurency_full);
            var C_ratio = _mapper.Map<List<CurrencyVM>>(cuurency_ratio);
            vm.Cuurency_full = C_full;
            vm.Cuurency_ratio = C_ratio;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CurrencyVVM twoLists)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Edit", "Currency");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null || twoLists == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Edit", "Currency");
            }
            try
            {
                var cuurency_ratio = await _unitOfWork.CrMasSysConvertNoToText.FindAllAsNoTrackingAsync(x => x.CrMasSysConvertNoToTextCode == "27" || x.CrMasSysConvertNoToTextCode == "28" || x.CrMasSysConvertNoToTextCode == "29");
                var cuurency_full = await _unitOfWork.CrMasSysConvertNoToText.FindAllAsNoTrackingAsync(x => x.CrMasSysConvertNoToTextCode == "30" || x.CrMasSysConvertNoToTextCode == "31" || x.CrMasSysConvertNoToTextCode == "32" || x.CrMasSysConvertNoToTextCode == "33");
                foreach (var item in cuurency_full)
                {
                    var thisviewFull = twoLists.Cuurency_full.Find(x => x.CrMasSysConvertNoToTextCode == item.CrMasSysConvertNoToTextCode);
                    item.CrMasSysConvertNoToTextArName = thisviewFull?.CrMasSysConvertNoToTextArName ?? item.CrMasSysConvertNoToTextArName;
                    item.CrMasSysConvertNoToTextEnName = thisviewFull?.CrMasSysConvertNoToTextEnName ?? item.CrMasSysConvertNoToTextEnName;
                    _unitOfWork.CrMasSysConvertNoToText.Update(item);
                }
                foreach (var item2 in cuurency_ratio)
                {
                    var thisviewRatio = twoLists.Cuurency_ratio.Find(x => x.CrMasSysConvertNoToTextCode == item2.CrMasSysConvertNoToTextCode);
                    item2.CrMasSysConvertNoToTextArName = thisviewRatio?.CrMasSysConvertNoToTextArName ?? item2.CrMasSysConvertNoToTextArName;
                    item2.CrMasSysConvertNoToTextEnName = thisviewRatio?.CrMasSysConvertNoToTextEnName ?? item2.CrMasSysConvertNoToTextEnName;
                    _unitOfWork.CrMasSysConvertNoToText.Update(item2);
                }
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, Status.Update);
                return RedirectToAction("Edit", "Currency");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", twoLists);
            }
        }



        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_Currencys = await _unitOfWork.CrMasSysConvertNoToText.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_Currencys != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSysConvertNoToTextArName" && All_Currencys.Any(x => x.CrMasSysConvertNoToTextArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysConvertNoToTextArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSysConvertNoToTextEnName" && All_Currencys.Any(x => x.CrMasSysConvertNoToTextEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysConvertNoToTextEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, string status)
        {


            var recordAr = " ";
            var recordEn = " ";
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
            return RedirectToAction("Index", "Currency");
        }


    }
}
