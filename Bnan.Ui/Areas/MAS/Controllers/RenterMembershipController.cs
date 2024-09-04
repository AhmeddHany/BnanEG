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
    public class RenterMembershipController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IMasRenterMembership _carFuel;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<RenterMembershipController> _localizer;


        public RenterMembershipController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterMembership carFuel,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<RenterMembershipController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _carFuel = carFuel;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var (mainTask, subTask, system, currentUser) = await SetTrace("106", "1106008", "1");
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "5";
            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var titles = await setTitle("107", "1106008", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var contracts = await _unitOfWork.CrMasSupRenterMembership.GetAllAsync();
            var contract = contracts.Where(x => x.CrMasSupRenterMembershipStatus == "A").ToList();
            var CarsInfo_count_all = _carFuel.GetAllRenterMembershipsCount();
            Tuple<IEnumerable<CrMasSupRenterMembership>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterMembership>, List<List<string>>>(contract, CarsInfo_count_all);
            return View(tb);
        }

        [HttpGet]
        public PartialViewResult GetRenterMembershipByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "5";
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {
                    //var RenterMembershipbyStatusAll = _unitOfWork.CrMasSupRenterMembership.GetAll();
                    //return PartialView("_DataTableRenterMembership", RenterMembershipbyStatusAll);

                    var RenterMembershipbyStatusAll = _unitOfWork.CrMasSupRenterMembership.FindAll(l => l.CrMasSupRenterMembershipStatus == Status.Hold || l.CrMasSupRenterMembershipStatus == Status.Active);
                    var CarsInfo_count_all1 = _carFuel.GetAllRenterMembershipsCount();
                    Tuple<IEnumerable<CrMasSupRenterMembership>, List<List<string>>> tb1 = new Tuple<IEnumerable<CrMasSupRenterMembership>, List<List<string>>>(RenterMembershipbyStatusAll, CarsInfo_count_all1);
                    return PartialView("_DataTableRenterMembership", tb1);
                }
                var RenterMembershipbyStatus = _unitOfWork.CrMasSupRenterMembership.FindAll(l => l.CrMasSupRenterMembershipStatus == status).ToList();
                var CarsInfo_count_all = _carFuel.GetAllRenterMembershipsCount();
                Tuple<IEnumerable<CrMasSupRenterMembership>, List<List<string>>> tb = new Tuple<IEnumerable<CrMasSupRenterMembership>, List<List<string>>>(RenterMembershipbyStatus, CarsInfo_count_all);
                return PartialView("_DataTableRenterMembership", tb);
            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> AddRenterMembership()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "5";

            // Set Title !!!!!!!!!!!!!!!!!!!!!!!!!!
            var titles = await setTitle("107", "1106008", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterMembershipCode = "";
            var RenterMemberships = await _unitOfWork.CrMasSupRenterMembership.GetAllAsync();
            if (RenterMemberships.Count() != 0)
            {
                RenterMembershipCode = (BigInteger.Parse(RenterMemberships.LastOrDefault().CrMasSupRenterMembershipCode) + 1).ToString();
            }
            else
            {
                RenterMembershipCode = "1600000001";
            }
            ViewBag.RenterMembershipCode = RenterMembershipCode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRenterMembership(RenterMembershipVM RenterMemberships, IFormFile? AcceptImg , IFormFile? RejectImg)
        {
            string currentCulture = CultureInfo.CurrentCulture.Name;
            string foldername = $"{"images\\Common"}";
            string filePathImageAccept = "";
            string filePathImageReject = "";


            if (ModelState.IsValid)
            {
                if (RenterMemberships != null)
                {
                    var RenterMembershipVMT = _mapper.Map<CrMasSupRenterMembership>(RenterMemberships);
                    var All_RenterMemberships = await _unitOfWork.CrMasSupRenterMembership.GetAllAsync();
                    var existingRenterMembership_En = All_RenterMemberships.FirstOrDefault(x =>
                        x.CrMasSupRenterMembershipEnName == RenterMembershipVMT.CrMasSupRenterMembershipEnName);
                    var existingRenterMembership_Ar = All_RenterMemberships.FirstOrDefault(x =>
                        x.CrMasSupRenterMembershipArName == RenterMembershipVMT.CrMasSupRenterMembershipArName);

                    // Generate code for the second time
                    var RenterMembershipCode = (BigInteger.Parse(All_RenterMemberships.LastOrDefault().CrMasSupRenterMembershipCode) + 1).ToString();
                    RenterMemberships.CrMasSupRenterMembershipCode = RenterMembershipCode;
                    ViewBag.RenterMembershipCode = RenterMembershipCode;
                    if (RenterMembershipVMT.CrMasSupRenterMembershipArName != null && RenterMembershipVMT.CrMasSupRenterMembershipEnName != null)
                    {
                        if (existingRenterMembership_Ar != null && existingRenterMembership_En != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterMemberships);
                        }
                        else if (existingRenterMembership_En != null)
                        {
                            ModelState.AddModelError("ExistEn", _localizer["Existing"]);
                            return View(RenterMemberships);
                        }
                        else if (existingRenterMembership_Ar != null)
                        {
                            ModelState.AddModelError("ExistAr", _localizer["Existing"]);
                            return View(RenterMemberships);
                        }
                    }

                    if (AcceptImg != null)
                    {
                        string fileNameImg = "RenterMembership_AcceptImg_" + RenterMemberships.CrMasSupRenterMembershipCode.ToString();
                        filePathImageAccept = await AcceptImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                    }
                    if (RejectImg != null)
                    {
                        string fileNameImg2 = "RenterMembership_RejectImg_" + RenterMemberships.CrMasSupRenterMembershipCode.ToString();
                        filePathImageReject = await RejectImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg2, ".png");
                    }

                    RenterMembershipVMT.CrMasSupRenterMembershipAcceptPicture = filePathImageAccept;
                    RenterMembershipVMT.CrMasSupRenterMembershipRejectPicture = filePathImageReject;
                    RenterMembershipVMT.CrMasSupRenterMembershipStatus = "A";
                    await _unitOfWork.CrMasSupRenterMembership.AddAsync(RenterMembershipVMT);

                    _unitOfWork.Complete();

                    var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1106008", "1");
                    var RecordAr = RenterMembershipVMT.CrMasSupRenterMembershipArName;
                    var RecordEn = RenterMembershipVMT.CrMasSupRenterMembershipEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }
                return RedirectToAction("Index");
            }
            return View("AddRenterMembership", RenterMemberships);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsersServices";
            ViewBag.no = "5";

            //To Set Title !!!!!!!!!!!!!
            var titles = await setTitle("107", "1106008", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);

            var contract = await _unitOfWork.CrMasSupRenterMembership.GetByIdAsync(id);
            if (contract == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Index");
            }
            int countRenterMemberships = 0;
            countRenterMemberships = _carFuel.GetOneRenterMembershipCount(id);
            ViewBag.RenterMemberships_Count = countRenterMemberships;
            var model = _mapper.Map<RenterMembershipVM>(contract);

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(RenterMembershipVM model, IFormFile? AcceptImg , IFormFile? RejectImg)
        {
            string foldername = $"{"images\\Common"}";
            string filePathImageAccept = null;
            string filePathImageReject = null;
            
            var user = await _userService.GetUserByUserNameAsync(HttpContext.User.Identity.Name);
            var membership =await _unitOfWork.CrMasSupRenterMembership.FindAsync(x=>x.CrMasSupRenterMembershipCode==model.CrMasSupRenterMembershipCode);
            if (user != null)
            {
                if (model != null && membership!=null)
                {

                    
                    if (AcceptImg != null)
                    {
                        string fileNameImg = "RenterMembership_AcceptImg_" + model.CrMasSupRenterMembershipCode.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوق
                        filePathImageAccept = await AcceptImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png", membership.CrMasSupRenterMembershipAcceptPicture);
                    }
                    else if (!string.IsNullOrEmpty(membership.CrMasSupRenterMembershipAcceptPicture))
                    {
                        filePathImageAccept = membership.CrMasSupRenterMembershipAcceptPicture;
                    }
                    //else
                    //{
                    //    filePathImageAccept = "~/images/common/DefaultCar.png";
                    //}

                    if (RejectImg != null)
                    {
                        string fileNameImg2 = "RenterMembership_RejectImg_" + model.CrMasSupRenterMembershipCode.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوق
                        filePathImageReject = await RejectImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg2, ".png", membership.CrMasSupRenterMembershipRejectPicture);
                    }
                    else if (!string.IsNullOrEmpty(membership.CrMasSupRenterMembershipRejectPicture))
                    {
                        filePathImageReject = membership.CrMasSupRenterMembershipRejectPicture;
                    }
                    //else
                    //{
                    //    filePathImageAccept = "~/images/common/DefaultCar.png";
                    //}


                    //var contract = _mapper.Map<CrMasSupRenterMembership>(model);
                    membership.CrMasSupRenterMembershipAcceptPicture = filePathImageAccept;
                    membership.CrMasSupRenterMembershipRejectPicture = filePathImageReject;
                    membership.CrMasSupRenterMembershipReasons = model.CrMasSupRenterMembershipReasons;

                    _unitOfWork.CrMasSupRenterMembership.Update(membership);
                    _unitOfWork.Complete();

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1106008", "1");
                    var RecordAr = membership.CrMasSupRenterMembershipArName;
                    var RecordEn = membership.CrMasSupRenterMembershipEnName;
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                }

            }

            return RedirectToAction("Index", "RenterMembership");
        }


        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var Contract = await _unitOfWork.CrMasSupRenterMembership.GetByIdAsync(code);
            if (Contract != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold";
                    Contract.CrMasSupRenterMembershipStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    int CountRenterMemberships = 0;
                    CountRenterMemberships = _carFuel.GetOneRenterMembershipCount(code);
                    if (CountRenterMemberships == 0)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        Contract.CrMasSupRenterMembershipStatus = Status.Deleted;
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
                    Contract.CrMasSupRenterMembershipStatus = Status.Active;
                }

                await _unitOfWork.CompleteAsync();

                // SaveTracing
                var RecordAr = Contract.CrMasSupRenterMembershipArName;
                var RecordEn = Contract.CrMasSupRenterMembershipEnName;
                var (mainTask, subTask, system, currentUser) = await SetTrace("107", "1106008", "1");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, RecordAr, RecordEn, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                return RedirectToAction("Index", "RenterMembership");
            }


            return View(Contract);

        }

        [HttpPost]
        public async Task<IActionResult> CheckChangedField(string Exist_lang, string dataField)
        {
            var All_RenterMemberships = await _unitOfWork.CrMasSupRenterMembership.GetAllAsync();

            if (dataField != null && All_RenterMemberships != null)
            {
                if (Exist_lang == "ExistAr")
                {
                    var existingRenterMembership_Ar = All_RenterMemberships.FirstOrDefault(x =>
                        x.CrMasSupRenterMembershipArName == dataField);
                    if (existingRenterMembership_Ar != null)
                    {
                        ModelState.AddModelError(Exist_lang, _localizer["Existing"]);
                        return View();
                    }
                }
                else if (Exist_lang == "ExistEn")
                {
                    var existingRenterMembership_En = All_RenterMemberships.FirstOrDefault(x =>
                        x.CrMasSupRenterMembershipEnName == dataField);
                    if (existingRenterMembership_En != null)
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
