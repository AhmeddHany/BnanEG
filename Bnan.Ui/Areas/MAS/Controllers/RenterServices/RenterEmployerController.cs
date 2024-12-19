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
namespace Bnan.Ui.Areas.MAS.Controllers.RenterServices
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class RenterEmployerController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterEmployer _masRenterEmployer;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterEmployerController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSupRenterEmployer;


        public RenterEmployerController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterEmployer masRenterEmployer, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterEmployerController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterEmployer = masRenterEmployer;
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
            var renterEmployers = await _unitOfWork.CrMasSupRenterEmployer
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterEmployerStatus == Status.Active, new[] { "CrMasRenterInformations" });

            // If no active licenses, retrieve all licenses
            if (!renterEmployers.Any())
            {
                renterEmployers = await _unitOfWork.CrMasSupRenterEmployer
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterEmployerStatus == Status.Hold,
                                              new[] { "CrMasRenterInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterEmployers);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterEmployerByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterEmployersAll = await _unitOfWork.CrMasSupRenterEmployer.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterEmployerStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterEmployerStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterEmployerStatus == Status.Hold, new[] { "CrMasRenterInformations" });

                if (status == Status.All)
                {
                    var FilterAll = RenterEmployersAll.FindAll(x => x.CrMasSupRenterEmployerStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterEmployerArName.Contains(search) ||
                                                                          x.CrMasSupRenterEmployerEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterEmployerCode.Contains(search)));
                    return PartialView("_DataTableRenterEmployer", FilterAll);
                }
                var FilterByStatus = RenterEmployersAll.FindAll(x => x.CrMasSupRenterEmployerStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterEmployerArName.Contains(search) ||
                                                                           x.CrMasSupRenterEmployerEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterEmployerCode.Contains(search)));
                return PartialView("_DataTableRenterEmployer", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterEmployer()
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "RenterEmployer");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterEmployer");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);
            // Check If code > 9 get error , because code is char(1)
            if (long.Parse(await GenerateLicenseCodeAsync()) > 1899999999)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterEmployer");
            }
            // Set Title 
            RenterEmployerVM renterEmployerVM = new RenterEmployerVM();
            renterEmployerVM.CrMasSupRenterEmployerCode = await GenerateLicenseCodeAsync();
            return View(renterEmployerVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterEmployer(RenterEmployerVM renterEmployerVM)
        {


            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid || renterEmployerVM == null)
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterEmployer", renterEmployerVM);
            }
            try
            {
                await SetPageTitleAsync(Status.Insert, pageNumber);
                // Map ViewModel to Entity
                var renterEmployerEntity = _mapper.Map<CrMasSupRenterEmployer>(renterEmployerVM);

                renterEmployerEntity.CrMasSupRenterEmployerSectorCode = "0";
                renterEmployerEntity.CrMasSupRenterEmployerCounter = 0;

                // Check if the entity already exists
                if (await _masRenterEmployer.ExistsByDetailsAsync(renterEmployerEntity))
                {
                    await AddModelErrorsAsync(renterEmployerEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddRenterEmployer", renterEmployerVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (long.Parse(await GenerateLicenseCodeAsync()) > 1899999999)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("AddRenterEmployer", renterEmployerVM);
                }
                // Generate and set the Driving License Code
                renterEmployerVM.CrMasSupRenterEmployerCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterEmployerEntity.CrMasSupRenterEmployerStatus = "A";
                await _unitOfWork.CrMasSupRenterEmployer.AddAsync(renterEmployerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, renterEmployerEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return View("AddRenterEmployer", renterEmployerVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 1800000002 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "RenterEmployer");
            }
            var contract = await _unitOfWork.CrMasSupRenterEmployer.FindAsync(x => x.CrMasSupRenterEmployerCode == id, new[] { "CrMasRenterInformations" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterEmployer");
            }
            var model = _mapper.Map<RenterEmployerVM>(contract);

            model.RentersHave_withType_Count = contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterEmployerVM renterEmployerVM)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null && renterEmployerVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "RenterEmployer");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return View("Edit", renterEmployerVM);
                }
                var renterEmployerEntity = _mapper.Map<CrMasSupRenterEmployer>(renterEmployerVM);

                // Check if the entity already exists
                if (await _masRenterEmployer.ExistsByDetailsAsync(renterEmployerEntity))
                {
                    await SetPageTitleAsync(Status.Update, pageNumber);
                    await AddModelErrorsAsync(renterEmployerEntity);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterEmployerVM);
                }

                _unitOfWork.CrMasSupRenterEmployer.Update(renterEmployerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, renterEmployerEntity, Status.Update);
                return RedirectToAction("Index", "RenterEmployer");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return View("Edit", renterEmployerVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSupRenterEmployer.GetByIdAsync(code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _masRenterEmployer.CheckIfCanDeleteIt(licence.CrMasSupRenterEmployerCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSupRenterEmployerStatus = status;
                _unitOfWork.CrMasSupRenterEmployer.Update(licence);
                _unitOfWork.Complete();
                await SaveTracingForLicenseChange(user, licence, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrMasSupRenterEmployer entity)
        {

            if (await _masRenterEmployer.ExistsByArabicNameAsync(entity.CrMasSupRenterEmployerArName, entity.CrMasSupRenterEmployerCode))
            {
                ModelState.AddModelError("CrMasSupRenterEmployerArName", _localizer["Existing"]);
            }

            if (await _masRenterEmployer.ExistsByEnglishNameAsync(entity.CrMasSupRenterEmployerEnName, entity.CrMasSupRenterEmployerCode))
            {
                ModelState.AddModelError("CrMasSupRenterEmployerEnName", _localizer["Existing"]);
            }

        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterEmployers = await _unitOfWork.CrMasSupRenterEmployer.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterEmployers != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterEmployerArName" && All_RenterEmployers.Any(x => x.CrMasSupRenterEmployerArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterEmployerArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterEmployerEnName" && All_RenterEmployers.Any(x => x.CrMasSupRenterEmployerEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterEmployerEnName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterEmployer.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterEmployerCode) + 1).ToString() : "1800000001";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSupRenterEmployer licence, string status)
        {


            var recordAr = licence.CrMasSupRenterEmployerArName;
            var recordEn = licence.CrMasSupRenterEmployerEnName;
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
            return RedirectToAction("Index", "RenterEmployer");
        }


    }
}
