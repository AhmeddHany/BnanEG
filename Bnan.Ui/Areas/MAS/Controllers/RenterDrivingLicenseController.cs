using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
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
    public class RenterDrivingLicenseController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IMasRenterDrivingLicense _masRenterDrivingLicense;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterDrivingLicenseController> _localizer;


        public RenterDrivingLicenseController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterDrivingLicense masRenterDrivingLicense,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterDrivingLicenseController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _masRenterDrivingLicense = masRenterDrivingLicense;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
            //sidebar Active

            await _userLoginsService.SaveTracing(user.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            var RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseStatus == Status.Active, new[] { "CrMasRenterInformations" });
            return View(RenterDrivingLicenses);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetRenterDrivingLicenseByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var RenterDrivingLicensesAll = await _unitOfWork.CrMasSupRenterDrivingLicense.FindAllAsNoTrackingAsync(x => x.CrMasSupRenterDrivingLicenseCode != "1" && x.CrMasSupRenterDrivingLicenseCode != "2" &&
                                                                                                                           (x.CrMasSupRenterDrivingLicenseStatus == Status.Active ||
                                                                                                                            x.CrMasSupRenterDrivingLicenseStatus == Status.Deleted ||
                                                                                                                            x.CrMasSupRenterDrivingLicenseStatus == Status.Hold), new[] { "CrMasRenterInformations" });

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
            //sidebar Active

            // Set Title 
            await SetPageTitleAddAsync();
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
                await SetPageTitleAddAsync();
                return View("AddRenterDrivingLicense", renterDrivingLicenseVM);
            }

            // Map ViewModel to Entity
            var renterDrivingLicenseEntity = _mapper.Map<CrMasSupRenterDrivingLicense>(renterDrivingLicenseVM);

            // Check if the entity already exists
            if (await _masRenterDrivingLicense.ExistsByDetailsAsync(renterDrivingLicenseEntity))
            {
                await AddModelErrorsAsync(renterDrivingLicenseEntity);
                await SetPageTitleAddAsync();
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
        private async Task AddModelErrorsAsync(CrMasSupRenterDrivingLicense entity)
        {
            if (await _masRenterDrivingLicense.ExistsByArabicNameAsync(entity.CrMasSupRenterDrivingLicenseArName))
            {
                ModelState.AddModelError("ExistAr", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByEnglishNameAsync(entity.CrMasSupRenterDrivingLicenseEnName))
            {
                ModelState.AddModelError("ExistEn", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByNaqlCodeAsync((int)entity.CrMasSupRenterDrivingLicenseNaqlCode))
            {
                ModelState.AddModelError("ExistCode", _localizer["Existing"]);
            }

            if (await _masRenterDrivingLicense.ExistsByNaqlIdAsync((int)entity.CrMasSupRenterDrivingLicenseNaqlId))
            {
                ModelState.AddModelError("ExistId", _localizer["Existing"]);
            }
        }
        private async Task SetPageTitleAddAsync()
        {
            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
        }
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSupRenterDrivingLicenseCode) + 1).ToString() : "1";
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "1";
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var contract = await _unitOfWork.CrMasSupRenterDrivingLicense.GetByIdAsync(id);
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            int countRenterDrivingLicenses = 0;
            //countRenterDrivingLicenses = _carColor.GetOneRenterDrivingLicenseCount(id);
            ViewBag.RenterDrivingLicenses_Count = countRenterDrivingLicenses;
            var model = _mapper.Map<RenterDrivingLicenseVM>(contract);

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RenterDrivingLicenseVM model)
        {
            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);

            if (user != null)
            {
                if (model != null)
                {

                    var contract = _mapper.Map<CrMasSupRenterDrivingLicense>(model);

                    _unitOfWork.CrMasSupRenterDrivingLicense.Update(contract);
                    _unitOfWork.Complete();

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
                    var RecordAr = contract.CrMasSupRenterDrivingLicenseArName;
                    var RecordEn = contract.CrMasSupRenterDrivingLicenseEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "RenterDrivingLicense");
        }


        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var Contract = await _unitOfWork.CrMasSupRenterDrivingLicense.GetByIdAsync(code);
            if (Contract != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Contract.CrMasSupRenterDrivingLicenseStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    int CountRenterDrivingLicenses = 0;
                    //CountRenterDrivingLicenses = _carColor.GetOneRenterDrivingLicenseCount(code);
                    if (CountRenterDrivingLicenses == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Contract.CrMasSupRenterDrivingLicenseStatus = Status.Deleted;
                    }
                    else
                    {
                        return View(Contract);
                    }

                }
                else if (status == "Reactivate")
                {
                    sAr = "استرجاع";
                    sEn = "Retrive";
                    Contract.CrMasSupRenterDrivingLicenseStatus = Status.Active;
                }

                await _unitOfWork.CompleteAsync();

                // SaveTracing
                var RecordAr = Contract.CrMasSupRenterDrivingLicenseArName;
                var RecordEn = Contract.CrMasSupRenterDrivingLicenseEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return RedirectToAction("Index", "RenterDrivingLicense");
            }


            return View(Contract);

        }

        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_RenterDrivingLicenses != null)
            {
                // Check for existing Arabic driving license
                if (Exist_lang == "ExistAr" && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "RenterDrivingLicenseNameAr", Message = _localizer["Existing"] });
                }
                // Check for existing English driving license
                else if (Exist_lang == "ExistEn" && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseEnName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "RenterDrivingLicenseNameEn", Message = _localizer["Existing"] });
                }
                // Check for existing rental system number
                else if (Exist_lang == "ExistCode" && int.TryParse(dataField, out var code) && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseNaqlCode == code))
                {
                    errors.Add(new ErrorResponse { Field = "RenterDrivingLicenseNaqlCode", Message = _localizer["Existing"] });
                }
                // Check for existing rental system ID
                else if (Exist_lang == "ExistId" && int.TryParse(dataField, out var id) && All_RenterDrivingLicenses.Any(x => x.CrMasSupRenterDrivingLicenseNaqlId == id))
                {
                    errors.Add(new ErrorResponse { Field = "RenterDrivingLicenseNaqlId", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }



        public IActionResult CannotDelete()
        {

            _toastNotification.AddErrorToastMessage(_localizer["SureTo_Cannot_delete"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

            return View();
        }
    }
}
