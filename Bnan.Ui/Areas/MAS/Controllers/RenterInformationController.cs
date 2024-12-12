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
namespace Bnan.Ui.Areas.MAS.Controllers
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class RenterInformationController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterInformationController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasRenterInformation;


        public RenterInformationController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo,IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterInformationController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterInformation = masRenterInformation;
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
            // Retrieve active driving licenses
            var renterSectors = await _unitOfWork.CrMasRenterInformation
                .FindAllAsNoTrackingAsync(x => x.CrMasRenterInformationStatus == Status.Active, new[] { "CrCasRenterLessors" });

            // If no active licenses, retrieve all licenses
            if (!renterSectors.Any())
            {
                renterSectors = await _unitOfWork.CrMasRenterInformation
                    .FindAllAsNoTrackingAsync(x => x.CrMasRenterInformationStatus == Status.Hold,
                                              new[] { "CrCasRenterLessors" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterSectors);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterInformationByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterInformationsAll = await _unitOfWork.CrMasRenterInformation.FindAllAsNoTrackingAsync(x => x.CrMasRenterInformationStatus == Status.Active ||
                                                                                                                            x.CrMasRenterInformationStatus == Status.Deleted ||
                                                                                                                            x.CrMasRenterInformationStatus == Status.Hold, new[] { "CrCasRenterLessors" });

                var FilterAll = RenterInformationsAll.FindAll(x => x.CrMasRenterInformationStatus != Status.Deleted &&
                                                                        (x.CrMasRenterInformationArName.Contains(search) ||
                                                                        x.CrMasRenterInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                        x.CrMasRenterInformationId.Contains(search)));
                return PartialView("_DataTableRenterInformation", FilterAll);
                              
            }
            return PartialView();
        }

        
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            var contract = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == id, new[] { "CrCasRenterLessors" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterInformation");
            }
            var model = _mapper.Map<RenterInformationVM>(contract);
            //model.RentersHave_withType_Count = contract.CrCasRenterLessors.Count;
            return View(model);
        }
        

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasRenterInformation.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasRenterInformationId) + 1).ToString() : "0";
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
            return RedirectToAction("Index", "RenterInformation");
        }


    }
}
