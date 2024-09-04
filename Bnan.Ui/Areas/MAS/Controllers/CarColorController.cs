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
    public class CarColorController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IMasCarColor _carColor;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarColorController> _localizer;


        public CarColorController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarColor carColor,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarColorController> localizer) : base(userManager, unitOfWork, mapper)
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
            var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107009", "1");
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "8";

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("107", "1107009", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var contracts = await _unitOfWork.CrMasSupCarColor.GetAllAsync();
            var contract = contracts.Where(x => x.CrMasSupCarColorStatus == "A").ToList();
            var CarsInfo_count_all = _carColor.GetAllCarColorsCount();
            Tuple<IEnumerable<CrMasSupCarColor>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupCarColor>, List<List<string>>>(contract, CarsInfo_count_all);
            return View(tb);
        }

        [HttpGet]
        public PartialViewResult GetCarColorByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "8";
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {
                    //var CarColorbyStatusAll = _unitOfWork.CrMasSupCarColor.GetAll();
                    //return PartialView("_DataTableCarColor", CarColorbyStatusAll);

                    var CarColorbyStatusAll = _unitOfWork.CrMasSupCarColor.FindAll(l => l.CrMasSupCarColorStatus == Status.Hold || l.CrMasSupCarColorStatus == Status.Active);
                    var CarsInfo_count_all1 = _carColor.GetAllCarColorsCount();
                    Tuple<IEnumerable<CrMasSupCarColor>, List<List<string>>> tb1 = new Tuple<IEnumerable<CrMasSupCarColor>, List<List<string>>>(CarColorbyStatusAll, CarsInfo_count_all1);
                    return PartialView("_DataTableCarColor", tb1);
                }
                var CarColorbyStatus = _unitOfWork.CrMasSupCarColor.FindAll(l => l.CrMasSupCarColorStatus == status).ToList();
                var CarsInfo_count_all = _carColor.GetAllCarColorsCount();
                Tuple<IEnumerable<CrMasSupCarColor>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupCarColor>, List<List<string>>>(CarColorbyStatus, CarsInfo_count_all);
                return PartialView("_DataTableCarColor", tb);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> AddCarColor()
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "8";
            // Set Title !!!!!!!!!!!!!!!!!!!!!!!!!!
            var titles = await setTitle("107", "1107009", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var CarColorCode = "";
            var CarColors = await _unitOfWork.CrMasSupCarColor.GetAllAsync();
            if (CarColors.Count() != 0)
            {
                CarColorCode = (BigInteger.Parse(CarColors.LastOrDefault().CrMasSupCarColorCode) + 1).ToString();
            }
            else
            {
                CarColorCode = "10";
            }
            ViewBag.CarColorCode = CarColorCode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCarColor(CarColorVM CarColors)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "8";
            string currentCulture = CultureInfo.CurrentCulture.Name;

            if (ModelState.IsValid)
            {
                if (CarColors != null)
                {
                    var CarColorVMT = _mapper.Map<CrMasSupCarColor>(CarColors);
                    var All_CarColors = await _unitOfWork.CrMasSupCarColor.GetAllAsync();
                    var existingCarColor_En = All_CarColors.FirstOrDefault(x =>
                        x.CrMasSupCarColorEnName == CarColorVMT.CrMasSupCarColorEnName);
                    var existingCarColor_Ar = All_CarColors.FirstOrDefault(x =>
                        x.CrMasSupCarColorArName == CarColorVMT.CrMasSupCarColorArName);

                    // Generate code for the second time
                    var CarColorCode = (BigInteger.Parse(All_CarColors.LastOrDefault().CrMasSupCarColorCode) + 1).ToString();
                    CarColors.CrMasSupCarColorCode = CarColorCode;
                    ViewBag.CarColorCode = CarColorCode;
                    if (CarColorVMT.CrMasSupCarColorArName != null && CarColorVMT.CrMasSupCarColorEnName != null)
                    {
                        if (existingCarColor_Ar != null && existingCarColor_En != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(CarColors);
                        }
                        else if (existingCarColor_En != null)
                        {
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(CarColors);
                        }
                        else if (existingCarColor_Ar != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            return View(CarColors);
                        }
                    }

                    CarColorVMT.CrMasSupCarColorStatus = "A";
                    //CarColorVMT.CrMasSupCarColorGroup = "33";
                    await _unitOfWork.CrMasSupCarColor.AddAsync(CarColorVMT);

                    _unitOfWork.Complete();

                    var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107009", "1");
                    var RecordAr = CarColorVMT.CrMasSupCarColorArName;
                    var RecordEn = CarColorVMT.CrMasSupCarColorEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }
                return RedirectToAction("Index");
            }
            return View("AddCarColor", CarColors);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "8";
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("107", "1107009", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var contract = await _unitOfWork.CrMasSupCarColor.GetByIdAsync(id);
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            int countCarColors = 0;
            countCarColors = _carColor.GetOneCarColorCount(id);
            ViewBag.CarColors_Count = countCarColors;
            var model = _mapper.Map<CarColorVM>(contract);

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(CarColorVM model)
        {
            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);

            if (user != null)
            {
                if (model != null)
                {

                    var contract = _mapper.Map<CrMasSupCarColor>(model);

                    _unitOfWork.CrMasSupCarColor.Update(contract);
                    _unitOfWork.Complete();

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107009", "1");
                    var RecordAr = contract.CrMasSupCarColorArName;
                    var RecordEn = contract.CrMasSupCarColorEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "CarColor");
        }


        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var Contract = await _unitOfWork.CrMasSupCarColor.GetByIdAsync(code);
            if (Contract != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Contract.CrMasSupCarColorStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    int CountCarColors = 0;
                    CountCarColors = _carColor.GetOneCarColorCount(code);
                    if (CountCarColors == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Contract.CrMasSupCarColorStatus = Status.Deleted;
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
                    Contract.CrMasSupCarColorStatus = Status.Active;
                }

                await _unitOfWork.CompleteAsync();

                // SaveTracing
                var RecordAr = Contract.CrMasSupCarColorArName;
                var RecordEn = Contract.CrMasSupCarColorEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107009", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return RedirectToAction("Index", "CarColor");
            }


            return View(Contract);

        }

        [HttpPost]
        public async Task<IActionResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_CarColors = await _unitOfWork.CrMasSupCarColor.GetAllAsync();

            if (dataField != null && All_CarColors != null)
            {
                if (Exist_lang == "ExistAr")
                {
                    var existingCarColor_Ar = All_CarColors.FirstOrDefault(x =>
                        x.CrMasSupCarColorArName == dataField);
                    if (existingCarColor_Ar != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }
                else if (Exist_lang == "ExistEn")
                {
                    var existingCarColor_En = All_CarColors.FirstOrDefault(x =>
                        x.CrMasSupCarColorEnName == dataField);
                    if (existingCarColor_En != null)
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
