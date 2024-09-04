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
    public class RenterProfessionController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IMasRenterProfession _carColor;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterProfessionController> _localizer;


        public RenterProfessionController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterProfession carColor,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterProfessionController> localizer) : base(userManager, unitOfWork, mapper)
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
            var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106006", "1");
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "3";

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("106", "1106006", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var contracts = await _unitOfWork.CrMasSupRenterProfession.GetAllAsync();
            var contract = contracts.Where(x => x.CrMasSupRenterProfessionsStatus == "A").ToList();
            var CarsInfo_count_all = _carColor.GetAllRenterProfessionsCount();
            Tuple<IEnumerable<CrMasSupRenterProfession>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterProfession>, List<List<string>>>(contract, CarsInfo_count_all);
            return View(tb);
        }

        [HttpGet]
        public PartialViewResult GetRenterProfessionByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "3";
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {

                    var RenterProfessionbyStatusAll = _unitOfWork.CrMasSupRenterProfession.FindAll(l => l.CrMasSupRenterProfessionsStatus == Status.Hold || l.CrMasSupRenterProfessionsStatus == Status.Active);
                    var CarsInfo_count_all1 = _carColor.GetAllRenterProfessionsCount();
                    Tuple<IEnumerable<CrMasSupRenterProfession>, List<List<string>>> tb1 = new Tuple<IEnumerable<CrMasSupRenterProfession>, List<List<string>>>(RenterProfessionbyStatusAll, CarsInfo_count_all1);
                    return PartialView("_DataTableRenterProfession", tb1);
                }
                var RenterProfessionbyStatus = _unitOfWork.CrMasSupRenterProfession.FindAll(l => l.CrMasSupRenterProfessionsStatus == status).ToList();
                var CarsInfo_count_all = _carColor.GetAllRenterProfessionsCount();
                Tuple<IEnumerable<CrMasSupRenterProfession>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterProfession>, List<List<string>>>(RenterProfessionbyStatus, CarsInfo_count_all);
                return PartialView("_DataTableRenterProfession", tb);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> AddRenterProfession()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "3";
            // Set Title !!!!!!!!!!!!!!!!!!!!!!!!!!
            var titles = await setTitle("106", "1106006", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterProfessionCode = "";
            var RenterProfessions = await _unitOfWork.CrMasSupRenterProfession.GetAllAsync();
            if (RenterProfessions.Count() != 0)
            {
                RenterProfessionCode = (BigInteger.Parse(RenterProfessions.LastOrDefault().CrMasSupRenterProfessionsCode) + 1).ToString();
            }
            else
            {
                RenterProfessionCode = "1";
            }
            ViewBag.RenterProfessionCode = RenterProfessionCode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterProfession(RenterProfessionVM RenterProfessions)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "3";
            string currentCulture = CultureInfo.CurrentCulture.Name;

            if (ModelState.IsValid)
            {
                if (RenterProfessions != null)
                {
                    var RenterProfessionVMT = _mapper.Map<CrMasSupRenterProfession>(RenterProfessions);
                    var All_RenterProfessions = await _unitOfWork.CrMasSupRenterProfession.GetAllAsync();
                    var existingRenterProfession_En = All_RenterProfessions.FirstOrDefault(x =>
                        x.CrMasSupRenterProfessionsEnName == RenterProfessionVMT.CrMasSupRenterProfessionsEnName);
                    var existingRenterProfession_Ar = All_RenterProfessions.FirstOrDefault(x =>
                        x.CrMasSupRenterProfessionsArName == RenterProfessionVMT.CrMasSupRenterProfessionsArName);

                    // Generate code for the second time
                    var RenterProfessionCode = (BigInteger.Parse(All_RenterProfessions.LastOrDefault().CrMasSupRenterProfessionsCode) + 1).ToString();
                    RenterProfessions.CRMasSupRenterProfessionsCode = RenterProfessionCode;
                    ViewBag.RenterProfessionCode = RenterProfessionCode;
                    if (RenterProfessionVMT.CrMasSupRenterProfessionsArName != null && RenterProfessionVMT.CrMasSupRenterProfessionsEnName != null)
                    {
                        if (existingRenterProfession_Ar != null && existingRenterProfession_En != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterProfessions);
                        }
                        else if (existingRenterProfession_En != null)
                        {
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterProfessions);
                        }
                        else if (existingRenterProfession_Ar != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            return View(RenterProfessions);
                        }
                    }

                    RenterProfessionVMT.CrMasSupRenterProfessionsStatus = "A";
                    RenterProfessionVMT.CrMasSupRenterProfessionsGroupCode = "14";
                    await _unitOfWork.CrMasSupRenterProfession.AddAsync(RenterProfessionVMT);

                    _unitOfWork.Complete();

                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106006", "1");
                    var RecordAr = RenterProfessionVMT.CrMasSupRenterProfessionsArName;
                    var RecordEn = RenterProfessionVMT.CrMasSupRenterProfessionsEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }
                return RedirectToAction("Index");
            }
            return View("AddRenterProfession", RenterProfessions);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "3";
            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("106", "1106006", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var contract = await _unitOfWork.CrMasSupRenterProfession.GetByIdAsync(id);
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            int countRenterProfessions = 0;
            countRenterProfessions = _carColor.GetOneRenterProfessionCount(id);
            ViewBag.RenterProfessions_Count = countRenterProfessions;
            var model = _mapper.Map<RenterProfessionVM>(contract);

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RenterProfessionVM model)
        {
            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);

            if (user != null)
            {
                if (model != null)
                {

                    var contract = _mapper.Map<CrMasSupRenterProfession>(model);

                    _unitOfWork.CrMasSupRenterProfession.Update(contract);
                    _unitOfWork.Complete();

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106006", "1");
                    var RecordAr = contract.CrMasSupRenterProfessionsArName;
                    var RecordEn = contract.CrMasSupRenterProfessionsEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "RenterProfession");
        }


        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var Contract = await _unitOfWork.CrMasSupRenterProfession.GetByIdAsync(code);
            if (Contract != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Contract.CrMasSupRenterProfessionsStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    int CountRenterProfessions = 0;
                    CountRenterProfessions = _carColor.GetOneRenterProfessionCount(code);
                    if (CountRenterProfessions == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Contract.CrMasSupRenterProfessionsStatus = Status.Deleted;
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
                    Contract.CrMasSupRenterProfessionsStatus = Status.Active;
                }

                await _unitOfWork.CompleteAsync();

                // SaveTracing
                var RecordAr = Contract.CrMasSupRenterProfessionsArName;
                var RecordEn = Contract.CrMasSupRenterProfessionsEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106006", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return RedirectToAction("Index", "RenterProfession");
            }


            return View(Contract);

        }

        [HttpPost]
        public async Task<IActionResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_RenterProfessions = await _unitOfWork.CrMasSupRenterProfession.GetAllAsync();

            if (dataField != null && All_RenterProfessions != null)
            {
                if (Exist_lang == "ExistAr")
                {
                    var existingRenterProfession_Ar = All_RenterProfessions.FirstOrDefault(x =>
                        x.CrMasSupRenterProfessionsArName == dataField);
                    if (existingRenterProfession_Ar != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }
                else if (Exist_lang == "ExistEn")
                {
                    var existingRenterProfession_En = All_RenterProfessions.FirstOrDefault(x =>
                        x.CrMasSupRenterProfessionsEnName == dataField);
                    if (existingRenterProfession_En != null)
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
