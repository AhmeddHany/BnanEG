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
    public class RateController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRate _masRate;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RateController> _localizer;
        private readonly string pageNumber = SubTasks.Evaluation;


        public RateController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRate masRate, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RateController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRate = masRate;
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
            var renter_Rate = await _unitOfWork.CrMasSysEvaluation.FindAllAsNoTrackingAsync(x => x.CrMasSysEvaluationsClassification == "1");
            var lessor_Rate = await _unitOfWork.CrMasSysEvaluation.FindAllAsNoTrackingAsync(x => x.CrMasSysEvaluationsClassification == "2");
            if (renter_Rate == null && lessor_Rate == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Rate");
            }
            RateVVM vm = new RateVVM();
            var renters = _mapper.Map<List<RateVM>>(renter_Rate);
            var lessors = _mapper.Map<List<RateVM>>(lessor_Rate);
            vm.renter_Rates = renters;
            vm.lessor_Rates = lessors;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RateVVM twoLists)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Edit", "Rate");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null || twoLists == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Edit", "Rate");
            }
            try
            {
                var renter_Rate = await _unitOfWork.CrMasSysEvaluation.FindAllAsync(x => x.CrMasSysEvaluationsClassification == "1");
                var lessor_Rate = await _unitOfWork.CrMasSysEvaluation.FindAllAsync(x => x.CrMasSysEvaluationsClassification == "2");
                foreach (var item in renter_Rate)
                {
                    var thisviewRenter = twoLists.renter_Rates.Find(x => x.CrMasSysEvaluationsCode == item.CrMasSysEvaluationsCode);
                    item.CrMasSysServiceEvaluationsValue = thisviewRenter?.CrMasSysServiceEvaluationsValue ?? 0;
                    item.CrMasSysEvaluationsArDescription = thisviewRenter?.CrMasSysEvaluationsArDescription ?? item.CrMasSysEvaluationsArDescription;
                    item.CrMasSysEvaluationsEnDescription = thisviewRenter?.CrMasSysEvaluationsEnDescription ?? item.CrMasSysEvaluationsEnDescription;
                    _unitOfWork.CrMasSysEvaluation.Update(item);
                }
                foreach (var item2 in lessor_Rate)
                {
                    var thisviewLessor = twoLists.lessor_Rates.Find(x => x.CrMasSysEvaluationsCode == item2.CrMasSysEvaluationsCode);
                    item2.CrMasSysServiceEvaluationsValue = thisviewLessor?.CrMasSysServiceEvaluationsValue ?? 0;
                    item2.CrMasSysEvaluationsArDescription = thisviewLessor?.CrMasSysEvaluationsArDescription ?? item2.CrMasSysEvaluationsArDescription;
                    item2.CrMasSysEvaluationsEnDescription = thisviewLessor?.CrMasSysEvaluationsEnDescription ?? item2.CrMasSysEvaluationsEnDescription;
                    _unitOfWork.CrMasSysEvaluation.Update(item2);
                }
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, Status.Update);
                return RedirectToAction("Edit", "Rate");
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
            var All_Rates = await _unitOfWork.CrMasSysEvaluation.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_Rates != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSysEvaluationsArDescription" && All_Rates.Any(x => x.CrMasSysEvaluationsArDescription == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysEvaluationsArDescription", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSysEvaluationsEnDescription" && All_Rates.Any(x => x.CrMasSysEvaluationsEnDescription?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSysEvaluationsEnDescription", Message = _localizer["Existing"] });
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
            return RedirectToAction("Index", "Rate");
        }


    }
}
