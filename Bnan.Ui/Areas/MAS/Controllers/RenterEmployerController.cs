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
    public class RenterEmployerController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IMasRenterEmployer _carColor;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterEmployerController> _localizer;


        public RenterEmployerController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterEmployer carColor,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterEmployerController> localizer) : base(userManager, unitOfWork, mapper)
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
            var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106007", "1");
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "4";

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("106", "1106007", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var contracts = await _unitOfWork.CrMasSupRenterEmployer.GetAllAsync();
            var contract = contracts.Where(x => x.CrMasSupRenterEmployerStatus == "A").ToList();
            var CarsInfo_count_all = _carColor.GetAllRenterEmployersCount();
            Tuple<IEnumerable<CrMasSupRenterEmployer>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterEmployer>, List<List<string>>>(contract, CarsInfo_count_all);
            return View(tb);
        }

        [HttpGet]
        public PartialViewResult GetRenterEmployerByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "4";
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {

                    var RenterEmployerbyStatusAll = _unitOfWork.CrMasSupRenterEmployer.FindAll(l => l.CrMasSupRenterEmployerStatus == Status.Hold || l.CrMasSupRenterEmployerStatus == Status.Active);
                    var CarsInfo_count_all1 = _carColor.GetAllRenterEmployersCount();
                    Tuple<IEnumerable<CrMasSupRenterEmployer>, List<List<string>>> tb1 = new Tuple<IEnumerable<CrMasSupRenterEmployer>, List<List<string>>>(RenterEmployerbyStatusAll, CarsInfo_count_all1);
                    return PartialView("_DataTableRenterEmployer", tb1);
                }
                var RenterEmployerbyStatus = _unitOfWork.CrMasSupRenterEmployer.FindAll(l => l.CrMasSupRenterEmployerStatus == status).ToList();
                var CarsInfo_count_all = _carColor.GetAllRenterEmployersCount();
                Tuple<IEnumerable<CrMasSupRenterEmployer>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterEmployer>, List<List<string>>>(RenterEmployerbyStatus, CarsInfo_count_all);
                return PartialView("_DataTableRenterEmployer", tb);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> AddRenterEmployer()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "4";
            // Set Title !!!!!!!!!!!!!!!!!!!!!!!!!!
            var titles = await setTitle("106", "1106007", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterEmployerCode = "";
            var RenterEmployers = await _unitOfWork.CrMasSupRenterEmployer.GetAllAsync();
            if (RenterEmployers.Count() != 0)
            {
                RenterEmployerCode = (BigInteger.Parse(RenterEmployers.LastOrDefault().CrMasSupRenterEmployerCode) + 1).ToString();
            }
            else
            {
                RenterEmployerCode = "1800000001";
            }
            ViewBag.RenterEmployerCode = RenterEmployerCode;
            var All_Sectors = _unitOfWork.CrMasSupRenterSector.FindAll(x=>Convert.ToInt16(x.CrMasSupRenterSectorCode) > 1 && Convert.ToInt16(x.CrMasSupRenterSectorCode) < 6).ToList();
            RenterEmployerVM renterEmployerVM = new RenterEmployerVM();
            renterEmployerVM.All_Sectors = All_Sectors;
            return View(renterEmployerVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterEmployer(RenterEmployerVM RenterEmployers)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "4";
            string currentCulture = CultureInfo.CurrentCulture.Name;

            if (ModelState.IsValid)
            {
                if (RenterEmployers != null)
                {
                    var RenterEmployerVMT = _mapper.Map<CrMasSupRenterEmployer>(RenterEmployers);
                    var All_RenterEmployers = await _unitOfWork.CrMasSupRenterEmployer.GetAllAsync();
                    var existingRenterEmployer_En = All_RenterEmployers.FirstOrDefault(x =>
                        x.CrMasSupRenterEmployerEnName == RenterEmployerVMT.CrMasSupRenterEmployerEnName);
                    var existingRenterEmployer_Ar = All_RenterEmployers.FirstOrDefault(x =>
                        x.CrMasSupRenterEmployerArName == RenterEmployerVMT.CrMasSupRenterEmployerArName);

                    // Generate code for the second time
                    var RenterEmployerCode = (BigInteger.Parse(All_RenterEmployers.LastOrDefault().CrMasSupRenterEmployerCode) + 1).ToString();
                    RenterEmployers.CrMasSupRenterEmployerCode = RenterEmployerCode;
                    ViewBag.RenterEmployerCode = RenterEmployerCode;
                    if (RenterEmployerVMT.CrMasSupRenterEmployerArName != null && RenterEmployerVMT.CrMasSupRenterEmployerEnName != null)
                    {
                        if (existingRenterEmployer_Ar != null && existingRenterEmployer_En != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterEmployers);
                        }
                        else if (existingRenterEmployer_En != null)
                        {
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterEmployers);
                        }
                        else if (existingRenterEmployer_Ar != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            return View(RenterEmployers);
                        }
                    }

                    RenterEmployerVMT.CrMasSupRenterEmployerStatus = "A";
                    RenterEmployerVMT.CrMasSupRenterEmployerGroupCode = "18";
                    RenterEmployerVMT.CrMasSupRenterEmployerCounter = 0;
                    await _unitOfWork.CrMasSupRenterEmployer.AddAsync(RenterEmployerVMT);

                    _unitOfWork.Complete();

                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106007", "1");
                    var RecordAr = RenterEmployerVMT.CrMasSupRenterEmployerArName;
                    var RecordEn = RenterEmployerVMT.CrMasSupRenterEmployerEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }
                return RedirectToAction("Index");
            }
            return View("AddRenterEmployer", RenterEmployers);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "4";
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("106", "1106007", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var contract = await _unitOfWork.CrMasSupRenterEmployer.GetByIdAsync(id);
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            int countRenterEmployers = 0;
            countRenterEmployers = _carColor.GetOneRenterEmployerCount(id);
            ViewBag.RenterEmployers_Count = countRenterEmployers;
            var model = _mapper.Map<RenterEmployerVM>(contract);
            var All_Sectors = _unitOfWork.CrMasSupRenterSector.FindAll(x => Convert.ToInt16(x.CrMasSupRenterSectorCode) > 1 && Convert.ToInt16(x.CrMasSupRenterSectorCode) < 6).ToList();
            model.All_Sectors = All_Sectors;
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RenterEmployerVM model)
        {
            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);

            if (user != null)
            {
                if (model != null)
                {

                    var contract = _mapper.Map<CrMasSupRenterEmployer>(model);

                    _unitOfWork.CrMasSupRenterEmployer.Update(contract);
                    _unitOfWork.Complete();

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106007", "1");
                    var RecordAr = contract.CrMasSupRenterEmployerArName;
                    var RecordEn = contract.CrMasSupRenterEmployerEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "RenterEmployer");
        }


        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var Contract = await _unitOfWork.CrMasSupRenterEmployer.GetByIdAsync(code);
            if (Contract != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Contract.CrMasSupRenterEmployerStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    int CountRenterEmployers = 0;
                    CountRenterEmployers = _carColor.GetOneRenterEmployerCount(code);
                    if (CountRenterEmployers == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Contract.CrMasSupRenterEmployerStatus = Status.Deleted;
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
                    Contract.CrMasSupRenterEmployerStatus = Status.Active;
                }

                await _unitOfWork.CompleteAsync();

                // SaveTracing
                var RecordAr = Contract.CrMasSupRenterEmployerArName;
                var RecordEn = Contract.CrMasSupRenterEmployerEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106007", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return RedirectToAction("Index", "RenterEmployer");
            }


            return View(Contract);

        }

        [HttpPost]
        public async Task<IActionResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_RenterEmployers = await _unitOfWork.CrMasSupRenterEmployer.GetAllAsync();

            if (dataField != null && All_RenterEmployers != null)
            {
                if (Exist_lang == "ExistAr")
                {
                    var existingRenterEmployer_Ar = All_RenterEmployers.FirstOrDefault(x =>
                        x.CrMasSupRenterEmployerArName == dataField);
                    if (existingRenterEmployer_Ar != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }
                else if (Exist_lang == "ExistEn")
                {
                    var existingRenterEmployer_En = All_RenterEmployers.FirstOrDefault(x =>
                        x.CrMasSupRenterEmployerEnName == dataField);
                    if (existingRenterEmployer_En != null)
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
