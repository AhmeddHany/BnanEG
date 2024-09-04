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
    public class CarModelController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IMasCarModel _carColor;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarModelController> _localizer;


        public CarModelController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasCarModel carColor,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarModelController> localizer) : base(userManager, unitOfWork, mapper)
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
            var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107005", "1");
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "4";

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("107", "1107005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var contracts = await _unitOfWork.CrMasSupCarModel.GetAllAsync();
            var contract = contracts.Where(x => x.CrMasSupCarModelStatus == "A").ToList();
            var CarsInfo_count_all = _carColor.GetAllCarModelsCount();
            Tuple<IEnumerable<CrMasSupCarModel>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupCarModel>, List<List<string>>>(contract, CarsInfo_count_all);
            return View(tb);
        }

        [HttpGet]
        public PartialViewResult GetCarModelByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "4";
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {

                    var CarModelbyStatusAll = _unitOfWork.CrMasSupCarModel.FindAll(l => l.CrMasSupCarModelStatus == Status.Hold || l.CrMasSupCarModelStatus == Status.Active);
                    var CarsInfo_count_all1 = _carColor.GetAllCarModelsCount();
                    Tuple<IEnumerable<CrMasSupCarModel>, List<List<string>>> tb1 = new Tuple<IEnumerable<CrMasSupCarModel>, List<List<string>>>(CarModelbyStatusAll, CarsInfo_count_all1);
                    return PartialView("_DataTableCarModel", tb1);
                }
                var CarModelbyStatus = _unitOfWork.CrMasSupCarModel.FindAll(l => l.CrMasSupCarModelStatus == status).ToList();
                var CarsInfo_count_all = _carColor.GetAllCarModelsCount();
                Tuple<IEnumerable<CrMasSupCarModel>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupCarModel>, List<List<string>>>(CarModelbyStatus, CarsInfo_count_all);
                return PartialView("_DataTableCarModel", tb);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> AddCarModel()
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "4";
            // Set Title !!!!!!!!!!!!!!!!!!!!!!!!!!
            var titles = await setTitle("107", "1107005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var CarModelCode = "";
            var CarModels = await _unitOfWork.CrMasSupCarModel.GetAllAsync();
            if (CarModels.Count() != 0)
            {
                CarModelCode = (BigInteger.Parse(CarModels.LastOrDefault().CrMasSupCarModelCode) + 1).ToString();
            }
            else
            {
                CarModelCode = "3100000001";
            }
            ViewBag.CarModelCode = CarModelCode;
            var All_Brands = _unitOfWork.CrMasSupCarBrand.GetAll().ToList();
            CarModelVM carModelVM = new CarModelVM();
            carModelVM.All_Brands = All_Brands;
            return View(carModelVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddCarModel(CarModelVM CarModels)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "4";
            string currentCulture = CultureInfo.CurrentCulture.Name;

            if (ModelState.IsValid)
            {
                if (CarModels != null)
                {
                    var CarModelVMT = _mapper.Map<CrMasSupCarModel>(CarModels);
                    var All_CarModels = await _unitOfWork.CrMasSupCarModel.GetAllAsync();
                    var existingCarModel_En = All_CarModels.FirstOrDefault(x =>
                        x.CrMasSupCarModelEnName == CarModelVMT.CrMasSupCarModelEnName);
                    var existingCarModel_Ar = All_CarModels.FirstOrDefault(x =>
                        x.CrMasSupCarModelArName == CarModelVMT.CrMasSupCarModelArName);

                    // Generate code for the second time
                    var CarModelCode = (BigInteger.Parse(All_CarModels.LastOrDefault().CrMasSupCarModelCode) + 1).ToString();
                    CarModels.CrMasSupCarModelCode = CarModelCode;
                    ViewBag.CarModelCode = CarModelCode;
                    if (CarModelVMT.CrMasSupCarModelArName != null && CarModelVMT.CrMasSupCarModelEnName != null)
                    {
                        if (existingCarModel_Ar != null && existingCarModel_En != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(CarModels);
                        }
                        else if (existingCarModel_En != null)
                        {
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(CarModels);
                        }
                        else if (existingCarModel_Ar != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            return View(CarModels);
                        }
                    }

                    CarModelVMT.CrMasSupCarModelStatus = "A";
                    CarModelVMT.CrMasSupCarModelGroup = "31";
                    CarModelVMT.CrMasSupCarModelCounter = 0;
                    await _unitOfWork.CrMasSupCarModel.AddAsync(CarModelVMT);

                    _unitOfWork.Complete();

                    var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107005", "1");
                    var RecordAr = CarModelVMT.CrMasSupCarModelArName;
                    var RecordEn = CarModelVMT.CrMasSupCarModelEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }
                return RedirectToAction("Index");
            }
            return View("AddCarModel", CarModels);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCarsServices";
            ViewBag.no = "4";
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("107", "1107005", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var contract = await _unitOfWork.CrMasSupCarModel.GetByIdAsync(id);
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            int countCarModels = 0;
            countCarModels = _carColor.GetOneCarModelCount(id);
            ViewBag.CarModels_Count = countCarModels;
            var model = _mapper.Map<CarModelVM>(contract);
            var All_Brands = _unitOfWork.CrMasSupCarBrand.GetAll().ToList();
            model.All_Brands = All_Brands;
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(CarModelVM model)
        {
            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);

            if (user != null)
            {
                if (model != null)
                {

                    var contract = _mapper.Map<CrMasSupCarModel>(model);

                    _unitOfWork.CrMasSupCarModel.Update(contract);
                    _unitOfWork.Complete();

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107005", "1");
                    var RecordAr = contract.CrMasSupCarModelArName;
                    var RecordEn = contract.CrMasSupCarModelEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "CarModel");
        }


        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var Contract = await _unitOfWork.CrMasSupCarModel.GetByIdAsync(code);
            if (Contract != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Contract.CrMasSupCarModelStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    int CountCarModels = 0;
                    CountCarModels = _carColor.GetOneCarModelCount(code);
                    if (CountCarModels == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Contract.CrMasSupCarModelStatus = Status.Deleted;
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
                    Contract.CrMasSupCarModelStatus = Status.Active;
                }

                await _unitOfWork.CompleteAsync();

                // SaveTracing
                var RecordAr = Contract.CrMasSupCarModelArName;
                var RecordEn = Contract.CrMasSupCarModelEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1107005", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return RedirectToAction("Index", "CarModel");
            }


            return View(Contract);

        }

        [HttpPost]
        public async Task<IActionResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_CarModels = await _unitOfWork.CrMasSupCarModel.GetAllAsync();

            if (dataField != null && All_CarModels != null)
            {
                if (Exist_lang == "ExistAr")
                {
                    var existingCarModel_Ar = All_CarModels.FirstOrDefault(x =>
                        x.CrMasSupCarModelArName == dataField);
                    if (existingCarModel_Ar != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }
                else if (Exist_lang == "ExistEn")
                {
                    var existingCarModel_En = All_CarModels.FirstOrDefault(x =>
                        x.CrMasSupCarModelEnName == dataField);
                    if (existingCarModel_En != null)
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
