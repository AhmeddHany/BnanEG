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
    public class RenterSectorController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IMasRenterSector _carColor;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterSectorController> _localizer;


        public RenterSectorController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterSector carColor,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterSectorController> localizer) : base(userManager, unitOfWork, mapper)
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
            var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106001", "1");
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "6";

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("106", "1106001", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var contracts = await _unitOfWork.CrMasSupRenterSector.GetAllAsync();
            var contract = contracts.Where(x => x.CrMasSupRenterSectorStatus == "A").ToList();
            var CarsInfo_count_all = _carColor.GetAllRenterSectorsCount();
            Tuple<IEnumerable<CrMasSupRenterSector>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterSector>, List<List<string>>>(contract, CarsInfo_count_all);
            return View(tb);
        }

        [HttpGet]
        public PartialViewResult GetRenterSectorByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "6";
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {

                    var RenterSectorbyStatusAll = _unitOfWork.CrMasSupRenterSector.FindAll(l => l.CrMasSupRenterSectorStatus == Status.Hold || l.CrMasSupRenterSectorStatus == Status.Active);
                    var CarsInfo_count_all1 = _carColor.GetAllRenterSectorsCount();
                    Tuple<IEnumerable<CrMasSupRenterSector>, List<List<string>>> tb1 = new Tuple<IEnumerable<CrMasSupRenterSector>, List<List<string>>>(RenterSectorbyStatusAll, CarsInfo_count_all1);
                    return PartialView("_DataTableRenterSector", tb1);
                }
                var RenterSectorbyStatus = _unitOfWork.CrMasSupRenterSector.FindAll(l => l.CrMasSupRenterSectorStatus == status).ToList();
                var CarsInfo_count_all = _carColor.GetAllRenterSectorsCount();
                Tuple<IEnumerable<CrMasSupRenterSector>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterSector>, List<List<string>>>(RenterSectorbyStatus, CarsInfo_count_all);
                return PartialView("_DataTableRenterSector", tb);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> AddRenterSector()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "6";
            // Set Title !!!!!!!!!!!!!!!!!!!!!!!!!!
            var titles = await setTitle("106", "1106001", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterSectorCode = "";
            var RenterSectors = await _unitOfWork.CrMasSupRenterSector.GetAllAsync();
            if (RenterSectors.Count() != 0)
            {
                RenterSectorCode = (BigInteger.Parse(RenterSectors.LastOrDefault().CrMasSupRenterSectorCode) + 1).ToString();
            }
            else
            {
                RenterSectorCode = "0";
            }
            ViewBag.RenterSectorCode = RenterSectorCode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterSector(RenterSectorVM RenterSectors)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "6";
            string currentCulture = CultureInfo.CurrentCulture.Name;

            if (ModelState.IsValid)
            {
                if (RenterSectors != null)
                {
                    var RenterSectorVMT = _mapper.Map<CrMasSupRenterSector>(RenterSectors);
                    var All_RenterSectors = await _unitOfWork.CrMasSupRenterSector.GetAllAsync();
                    var existingRenterSector_En = All_RenterSectors.FirstOrDefault(x =>
                        x.CrMasSupRenterSectorEnName == RenterSectorVMT.CrMasSupRenterSectorEnName);
                    var existingRenterSector_Ar = All_RenterSectors.FirstOrDefault(x =>
                        x.CrMasSupRenterSectorArName == RenterSectorVMT.CrMasSupRenterSectorArName);

                    // Generate code for the second time
                    var RenterSectorCode = (BigInteger.Parse(All_RenterSectors.LastOrDefault().CrMasSupRenterSectorCode) + 1).ToString();
                    RenterSectors.CrMasSupRenterSectorCode = RenterSectorCode;
                    ViewBag.RenterSectorCode = RenterSectorCode;
                    if (RenterSectorVMT.CrMasSupRenterSectorArName != null && RenterSectorVMT.CrMasSupRenterSectorEnName != null)
                    {
                        if (existingRenterSector_Ar != null && existingRenterSector_En != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterSectors);
                        }
                        else if (existingRenterSector_En != null)
                        {
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterSectors);
                        }
                        else if (existingRenterSector_Ar != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            return View(RenterSectors);
                        }
                    }

                    RenterSectorVMT.CrMasSupRenterSectorStatus = "A";
                    //RenterSectorVMT.CrMasSupRenterSectorGroupCode = "14";
                    await _unitOfWork.CrMasSupRenterSector.AddAsync(RenterSectorVMT);

                    _unitOfWork.Complete();

                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106001", "1");
                    var RecordAr = RenterSectorVMT.CrMasSupRenterSectorArName;
                    var RecordEn = RenterSectorVMT.CrMasSupRenterSectorEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }
                return RedirectToAction("Index");
            }
            return View("AddRenterSector", RenterSectors);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "6";
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("106", "1106001", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var contract = await _unitOfWork.CrMasSupRenterSector.GetByIdAsync(id);
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            int countRenterSectors = 0;
            countRenterSectors = _carColor.GetOneRenterSectorCount(id);
            ViewBag.RenterSectors_Count = countRenterSectors;
            var model = _mapper.Map<RenterSectorVM>(contract);

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RenterSectorVM model)
        {
            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);

            if (user != null)
            {
                if (model != null)
                {

                    var contract = _mapper.Map<CrMasSupRenterSector>(model);

                    _unitOfWork.CrMasSupRenterSector.Update(contract);
                    _unitOfWork.Complete();

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106001", "1");
                    var RecordAr = contract.CrMasSupRenterSectorArName;
                    var RecordEn = contract.CrMasSupRenterSectorEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "RenterSector");
        }


        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var Contract = await _unitOfWork.CrMasSupRenterSector.GetByIdAsync(code);
            if (Contract != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Contract.CrMasSupRenterSectorStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    int CountRenterSectors = 0;
                    CountRenterSectors = _carColor.GetOneRenterSectorCount(code);
                    if (CountRenterSectors == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Contract.CrMasSupRenterSectorStatus = Status.Deleted;
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
                    Contract.CrMasSupRenterSectorStatus = Status.Active;
                }

                await _unitOfWork.CompleteAsync();

                // SaveTracing
                var RecordAr = Contract.CrMasSupRenterSectorArName;
                var RecordEn = Contract.CrMasSupRenterSectorEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106001", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return RedirectToAction("Index", "RenterSector");
            }


            return View(Contract);

        }

        [HttpPost]
        public async Task<IActionResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_RenterSectors = await _unitOfWork.CrMasSupRenterSector.GetAllAsync();

            if (dataField != null && All_RenterSectors != null)
            {
                if (Exist_lang == "ExistAr")
                {
                    var existingRenterSector_Ar = All_RenterSectors.FirstOrDefault(x =>
                        x.CrMasSupRenterSectorArName == dataField);
                    if (existingRenterSector_Ar != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }
                else if (Exist_lang == "ExistEn")
                {
                    var existingRenterSector_En = All_RenterSectors.FirstOrDefault(x =>
                        x.CrMasSupRenterSectorEnName == dataField);
                    if (existingRenterSector_En != null)
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
