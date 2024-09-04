using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Diagnostics.Contracts;
using System.Globalization;
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
        private readonly IMasRenterDrivingLicense _carColor;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterDrivingLicenseController> _localizer;


        public RenterDrivingLicenseController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterDrivingLicense carColor,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterDrivingLicenseController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _carColor = carColor;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "1";

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var contracts = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            var contract = contracts.Where(x => x.CrMasSupRenterDrivingLicenseStatus == "A").ToList();
            var CarsInfo_count_all = _carColor.GetAllRenterDrivingLicensesCount();
            Tuple<IEnumerable<CrMasSupRenterDrivingLicense>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterDrivingLicense>, List<List<string>>>(contract, CarsInfo_count_all);
            return View(tb);
        }

        [HttpGet]
        public PartialViewResult GetRenterDrivingLicenseByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "1";
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {


                    var RenterDrivingLicensebyStatusAll = _unitOfWork.CrMasSupRenterDrivingLicense.FindAll(l => l.CrMasSupRenterDrivingLicenseStatus == Status.Hold || l.CrMasSupRenterDrivingLicenseStatus == Status.Active);
                    var CarsInfo_count_all1 = _carColor.GetAllRenterDrivingLicensesCount();
                    Tuple<IEnumerable<CrMasSupRenterDrivingLicense>, List<List<string>>> tb1 = new Tuple<IEnumerable<CrMasSupRenterDrivingLicense>, List<List<string>>>(RenterDrivingLicensebyStatusAll, CarsInfo_count_all1);
                    return PartialView("_DataTableRenterDrivingLicense", tb1);
                }
                var RenterDrivingLicensebyStatus = _unitOfWork.CrMasSupRenterDrivingLicense.FindAll(l => l.CrMasSupRenterDrivingLicenseStatus == status).ToList();
                var CarsInfo_count_all = _carColor.GetAllRenterDrivingLicensesCount();
                Tuple<IEnumerable<CrMasSupRenterDrivingLicense>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterDrivingLicense>, List<List<string>>>(RenterDrivingLicensebyStatus, CarsInfo_count_all);
                return PartialView("_DataTableRenterDrivingLicense", tb);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> AddRenterDrivingLicense()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "1";
            // Set Title !!!!!!!!!!!!!!!!!!!!!!!!!!
            var titles = await setTitle("106", "1106003", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterDrivingLicenseCode = "";
            var RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
            if (RenterDrivingLicenses.Count() != 0)
            {
                RenterDrivingLicenseCode = (BigInteger.Parse(RenterDrivingLicenses.LastOrDefault().CrMasSupRenterDrivingLicenseCode) + 1).ToString();
            }
            else
            {
                RenterDrivingLicenseCode = "1";
            }
            ViewBag.RenterDrivingLicenseCode = RenterDrivingLicenseCode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterDrivingLicense(RenterDrivingLicenseVM RenterDrivingLicenses)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "1";
            string currentCulture = CultureInfo.CurrentCulture.Name;

            if (ModelState.IsValid)
            {
                if (RenterDrivingLicenses != null)
                {
                    var RenterDrivingLicenseVMT = _mapper.Map<CrMasSupRenterDrivingLicense>(RenterDrivingLicenses);
                    var All_RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();
                    var existingRenterDrivingLicense_En = All_RenterDrivingLicenses.FirstOrDefault(x =>
                        x.CrMasSupRenterDrivingLicenseEnName == RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseEnName);
                    var existingRenterDrivingLicense_Ar = All_RenterDrivingLicenses.FirstOrDefault(x =>
                        x.CrMasSupRenterDrivingLicenseArName == RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseArName);

                    // Generate code for the second time
                    var RenterDrivingLicenseCode = (BigInteger.Parse(All_RenterDrivingLicenses.LastOrDefault().CrMasSupRenterDrivingLicenseCode) + 1).ToString();
                    RenterDrivingLicenses.CRMasSupRenterDrivingLicenseCode = RenterDrivingLicenseCode;
                    ViewBag.RenterDrivingLicenseCode = RenterDrivingLicenseCode;
                    if (RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseArName != null && RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseEnName != null)
                    {
                        if (existingRenterDrivingLicense_Ar != null && existingRenterDrivingLicense_En != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterDrivingLicenses);
                        }
                        else if (existingRenterDrivingLicense_En != null)
                        {
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterDrivingLicenses);
                        }
                        else if (existingRenterDrivingLicense_Ar != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            return View(RenterDrivingLicenses);
                        }
                    }

                    RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseStatus = "A";
                    //RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseGroup = "33";
                    await _unitOfWork.CrMasSupRenterDrivingLicense.AddAsync(RenterDrivingLicenseVMT);

                    _unitOfWork.Complete();

                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106003", "1");
                    var RecordAr = RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseArName;
                    var RecordEn = RenterDrivingLicenseVMT.CrMasSupRenterDrivingLicenseEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }
                return RedirectToAction("Index");
            }
            return View("AddRenterDrivingLicense", RenterDrivingLicenses);
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
            countRenterDrivingLicenses = _carColor.GetOneRenterDrivingLicenseCount(id);
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
                    CountRenterDrivingLicenses = _carColor.GetOneRenterDrivingLicenseCount(code);
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

        [HttpPost]
        public async Task<IActionResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_RenterDrivingLicenses = await _unitOfWork.CrMasSupRenterDrivingLicense.GetAllAsync();

            if (dataField != null && All_RenterDrivingLicenses != null)
            {
                if (Exist_lang == "ExistAr")
                {
                    var existingRenterDrivingLicense_Ar = All_RenterDrivingLicenses.FirstOrDefault(x =>
                        x.CrMasSupRenterDrivingLicenseArName == dataField);
                    if (existingRenterDrivingLicense_Ar != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }
                else if (Exist_lang == "ExistEn")
                {
                    var existingRenterDrivingLicense_En = All_RenterDrivingLicenses.FirstOrDefault(x =>
                        x.CrMasSupRenterDrivingLicenseEnName == dataField);
                    if (existingRenterDrivingLicense_En != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }

            }
            return View();
        }



        public IActionResult CannotDelete()
        {

            _toastNotification.AddErrorToastMessage(_localizer["SureTo_Cannot_delete"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

            return View();
        }
    }
 }
