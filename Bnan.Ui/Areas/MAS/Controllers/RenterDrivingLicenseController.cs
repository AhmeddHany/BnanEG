using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
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
    public class RenterDrivingLicenseController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterDrivingLicense _masRenterDrivingLicense;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterDrivingLicenseController> _localizer;


        public RenterDrivingLicenseController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterDrivingLicense masRenterDrivingLicense, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterDrivingLicenseController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterDrivingLicense = masRenterDrivingLicense;
            _userLoginsService = userLoginsService;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);

            // Set trace details
            var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");

            // Set page titles
            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            // Retrieve active driving licenses
            var renterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense
                .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active, new[] { "CrMasRenterInformations" });

            // If no active licenses, retrieve all licenses
            if (!renterDrivingLicenses.Any())
            {
                renterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense
                    .FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Hold || x.CrMasSupRenterDrivingLicenseStatus == Status.Active,
                                              new[] { "CrMasRenterInformations" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(renterDrivingLicenses);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetRenterDrivingLicenseByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterDrivingLicensesAll = await _unitOfWork.CrMasSupRenterDrivingLicense.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterDrivingLicenseStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterDrivingLicenseStatus == Status.Hold, new[] { "CrMasRenterInformations" });

                if (status == Status.All)
                {
                    var FilterAll = RenterDrivingLicensesAll.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus != Status.Deleted &&
                                                                         (x.CrMasSupRenterDrivingLicenseArName.Contains(search) ||
                                                                          x.CrMasSupRenterDrivingLicenseEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSupRenterDrivingLicenseCode.Contains(search)));
                    return PartialView("_DataTableRenterDrivingLicense", FilterAll);
                }
                var FilterByStatus = RenterDrivingLicensesAll.FindAll(x => x.CrMasSupRenterDrivingLicenseStatus == status &&
                                                                            (
                                                                           x.CrMasSupRenterDrivingLicenseArName.Contains(search) ||
                                                                           x.CrMasSupRenterDrivingLicenseEnName.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSupRenterDrivingLicenseCode.Contains(search)));
                return PartialView("_DataTableRenterDrivingLicense", FilterByStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddRenterDrivingLicense()
        {
            var user = await _userManager.GetUserAsync(User);
            // Check Validition
            if (!await _masBase.CheckValidation(user.CrMasUserInformationCode, "1106003", Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterDrivingLicense");
            }

            // Check If code > 9 get error , because code is char(1)
            if (int.Parse(await GenerateLicenseCodeAsync()) > 9)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(OperationsString.AddAr, OperationsString.AddEn);
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            // Set Title 
            await SetPageTitleAsync(OperationsString.AddAr, OperationsString.AddEn);
            RenterDrivingLicenseVM renterDrivingLicenseVM = new RenterDrivingLicenseVM();
            var RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            renterDrivingLicenseVM.CrMasSupRenterDrivingLicenseCode = (RenterDrivingLicenses.ToList().Count > 0)
            ? (BigInteger.Parse(RenterDrivingLicenses.Last().CrMasSupRenterDrivingLicenseCode) + 1).ToString()
            : "1";
            return View(renterDrivingLicenseVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterDrivingLicense(RenterDrivingLicenseVM renterDrivingLicenseVM)
        {
            if (!ModelState.IsValid || renterDrivingLicenseVM == null)
            {
                await SetPageTitleAsync(OperationsString.AddAr, OperationsString.AddEn);
                return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
            }
            try
            {
                // Map ViewModel to Entity
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupRenterDrivingLicense>(renterDrivingLicenseVM);

                if (renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlCode == null) renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlCode = 0;
                if (renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlId == null) renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlId = 0;

                // Check if the entity already exists
                if (await _masRenterDrivingLicense.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    await AddModelErrorsAsync(renterDrivingLicenseEntity);
                    await SetPageTitleAsync(OperationsString.AddAr, OperationsString.AddEn);
                    return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
                }
                // Check If code > 9 get error , because code is char(1)
                if (int.Parse(await GenerateLicenseCodeAsync()) > 9)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    await SetPageTitleAsync(OperationsString.AddAr, OperationsString.AddEn);
                    return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
                }
                // Generate and set the Driving License Code
                renterDrivingLicenseVM.CrMasSupRenterDrivingLicenseCode = await GenerateLicenseCodeAsync();
                // Set status and add the record
                renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseStatus = "A";
                await _unitOfWork.CrMasSupRenterDrivingLicense.AddAsync(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                // Log the addition
                var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseArName, renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseEnName,
                                                     "اضافة", "Add", mainTask.CrMasSysMainTasksCode, subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName,
                                                     mainTask.CrMasSysMainTasksEnName, subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(OperationsString.AddAr, OperationsString.AddEn);
                return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
            }
        }
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterDrivingLicenseCode) + 1).ToString() : "1";
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            // if value with code less than 2 Deleted
            if (int.Parse(id) < 2 + 1) return RedirectToAction("Index", "RenterDrivingLicense");

            var contract = await _unitOfWork.CrMasSupRenterDrivingLicense.FindAsync(x => x.CrMasSupRenterDrivingLicenseCode == id, new[] { "CrCasRenterPrivateDriverInformations", "CrMasRenterInformations" });
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            var model = _mapper.Map<RenterDrivingLicenseVM>(contract);
            model.RentersHaveLicenceCount = contract.CrCasRenterPrivateDriverInformations.Count + contract.CrMasRenterInformations.Count;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RenterDrivingLicenseVM renterDrivingLicenseVM)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null && renterDrivingLicenseVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(OperationsString.UpdateAr, OperationsString.UpdateEn);
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            try
            {
                await SetPageTitleAsync(OperationsString.UpdateAr, OperationsString.UpdateEn);
                //Check Validition
                if (!await _masBase.CheckValidation(user.CrMasUserInformationCode, "1106003", Status.UpdateStatus))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("Edit", renterDrivingLicenseVM);
                }
                var renterDrivingLicenseEntity = _mapper.Map<CrMasSupRenterDrivingLicense>(renterDrivingLicenseVM);


                if (renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlCode == null) renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlCode = 0;
                if (renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlId == null) renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseNaqlId = 0;

                // Check if the entity already exists
                if (await _masRenterDrivingLicense.ExistsByDetailsAsync(renterDrivingLicenseEntity))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Edit", "RenterDrivingLicense", new { id = renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseCode });
                }
                _unitOfWork.CrMasSupRenterDrivingLicense.Update(renterDrivingLicenseEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                // SaveTracing
                var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
                var RecordAr = renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseArName;
                var RecordEn = renterDrivingLicenseEntity.CrMasSupRenterDrivingLicenseEnName;
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, OperationsString.UpdateAr, OperationsString.UpdateEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                return RedirectToAction("Index", "RenterDrivingLicense");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(OperationsString.UpdateAr, OperationsString.UpdateEn);
                return View("Edit", renterDrivingLicenseVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var Licence = await _unitOfWork.CrMasSupRenterDrivingLicense.GetByIdAsync(code);
            if (Licence == null) return "false";

            var LicenceVm = _mapper.Map<RenterDrivingLicenseVM>(Licence);
            try
            {
                if (status == Status.Hold)
                {
                    if (!await _masBase.CheckValidation(user.CrMasUserInformationCode, "1106003", Status.Hold)) return "false_auth";
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Licence.CrMasSupRenterDrivingLicenseStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    if (!await _masBase.CheckValidation(user.CrMasUserInformationCode, "1106003", Status.Deleted)) return "false_auth";
                    var RentersLicence = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code);
                    if (RentersLicence == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Licence.CrMasSupRenterDrivingLicenseStatus = Status.Deleted;
                    }
                    else return "udelete";
                }
                else if (status == Status.UnHold)
                {
                    if (!await _masBase.CheckValidation(user.CrMasUserInformationCode, "1106003", Status.UnHold)) return "false_auth";
                    sAr = "استرجاع الايقاف";
                    sEn = "Retrive Hold";
                    Licence.CrMasSupRenterDrivingLicenseStatus = Status.Active;
                }
                else if (status == Status.UnDeleted)
                {
                    if (!await _masBase.CheckValidation(user.CrMasUserInformationCode, "1106003", Status.UnDeleted)) return "false_auth";
                    sAr = "استرجاع الحذف";
                    sEn = "Retrive Delete";
                    Licence.CrMasSupRenterDrivingLicenseStatus = Status.Active;
                }
                _unitOfWork.CrMasSupRenterDrivingLicense.Update(Licence);
                _unitOfWork.Complete();

                // SaveTracing
                var RecordAr = Licence.CrMasSupRenterDrivingLicenseArName;
                var RecordEn = Licence.CrMasSupRenterDrivingLicenseEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }
        private async Task AddModelErrorsAsync(CrMasSupRenterDrivingLicense entity)
        {

            if (await _masRenterDrivingLicense.ExistsByArabicNameAsync(entity.CrMasSupRenterDrivingLicenseArName, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseArName", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByEnglishNameAsync(entity.CrMasSupRenterDrivingLicenseEnName, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseEnName", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByNaqlCodeAsync((int)entity.CrMasSupRenterDrivingLicenseNaqlCode, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseNaqlCode", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByNaqlIdAsync((int)entity.CrMasSupRenterDrivingLicenseNaqlId, entity.CrMasSupRenterDrivingLicenseCode))
            {
                ModelState.AddModelError("CrMasSupRenterDrivingLicenseNaqlId", _localizer["Existing"]);
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterDrivingLicenses != null)
            {
                // Check for existing Arabic driving license
                if (existName == "CrMasSupRenterDrivingLicenseArName" && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseArName", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (existName == "CrMasSupRenterDrivingLicenseEnName" && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseEnName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseEnName", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (existName == "CrMasSupRenterDrivingLicenseNaqlCode" && int.TryParse(dataField, out var code) && code != 0 && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (existName == "CrMasSupRenterDrivingLicenseNaqlId" && int.TryParse(dataField, out var id) && id != 0 && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasSupRenterDrivingLicenseNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }
        private async Task SetPageTitleAsync(string ArOperation, string EnOperation)
        {
            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], ArOperation, EnOperation, titles[3]);
        }
    }
}
