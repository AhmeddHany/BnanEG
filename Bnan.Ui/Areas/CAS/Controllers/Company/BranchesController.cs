
using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.MAS.Controllers.Companies;
using Bnan.Ui.ViewModels.CAS;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Bnan.Ui.Areas.CAS.Controllers.Company
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class BranchesController : BaseController
    {
        public IUserService _UserService;
        public IBranchInformation _BranchInformation;
        public IPostBranch _PostBranch;
        public IBranchDocument _BranchDocument;
        public ISalesPoint _SalesPoint;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<LessorsKSAController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBaseRepo _baseRepo;
        private readonly IWebHostEnvironment _hostingEnvironment;


        private readonly string pageNumber = SubTasks.Branches_CAS;


        public BranchesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IBranchInformation branchInformation, IUserLoginsService userLoginsService, IStringLocalizer<LessorsKSAController> localizer, IToastNotification toastNotification, IPostBranch postBranch, IBranchDocument branchDocument, ISalesPoint salesPoint, IAdminstritiveProcedures adminstritiveProcedures, IWebHostEnvironment webHostEnvironment, IBaseRepo baseRepo, IWebHostEnvironment hostingEnvironment) : base(userManager, unitOfWork, mapper)
        {
            _UserService = userService;
            _BranchInformation = branchInformation;
            _userLoginsService = userLoginsService;
            _localizer = localizer;
            _toastNotification = toastNotification;
            _PostBranch = postBranch;
            _BranchDocument = branchDocument;
            _SalesPoint = salesPoint;
            _adminstritiveProcedures = adminstritiveProcedures;
            _webHostEnvironment = webHostEnvironment;
            _baseRepo = baseRepo;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Branches()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var Branches = await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(l => l.CrCasBranchInformationLessor == user.CrMasUserInformationLessor,
                                                                                                  new[] { "CrCasCarInformations", "CrCasBranchPost.CrCasBranchPostCityNavigation", "CrCasBranchDocuments.CrCasBranchDocumentsProceduresNavigation" });
            if (!Branches.Any())
            {
                Branches = Branches.Where(x => x.CrCasBranchInformationStatus == Status.Hold).ToList();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(Branches);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetBranchesByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(status))
            {
                var Branches = await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(l => l.CrCasBranchInformationLessor == user.CrMasUserInformationLessor,
                                                                                                              new[] { "CrCasCarInformations", "CrCasBranchPost.CrCasBranchPostCityNavigation", "CrCasBranchDocuments.CrCasBranchDocumentsProceduresNavigation" });
                if (status == Status.All)
                {
                    var BranchbyStatusAll = Branches.FindAll(x => x.CrCasBranchInformationStatus != Status.Deleted &&
                                                                         (x.CrCasBranchInformationArName.Contains(search) ||
                                                                          x.CrCasBranchInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrCasBranchPost.CrCasBranchPostArConcatenate.Contains(search) ||
                                                                          x.CrCasBranchPost.CrCasBranchPostEnConcatenate.Contains(search.ToLower()) ||
                                                                          x.CrCasBranchInformationCode.Contains(search)));
                    return PartialView("_DataTableBranches", BranchbyStatusAll);
                }
                var BranchbyStatus = Branches.FindAll(x => x.CrCasBranchInformationStatus == status &&
                                                                                        (x.CrCasBranchInformationArName.Contains(search) ||
                                                                                         x.CrCasBranchInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                                         x.CrCasBranchPost.CrCasBranchPostArConcatenate.Contains(search) ||
                                                                                         x.CrCasBranchPost.CrCasBranchPostEnConcatenate.Contains(search.ToLower()) ||
                                                                                         x.CrCasBranchInformationCode.Contains(search)));
                return PartialView("_DataTableBranches", BranchbyStatus);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddBranch()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Branches", "Branches");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Branches", "Branches");
            }
            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList;
            var isArabic = CultureInfo.CurrentCulture.Name == "ar-EG";
            var Cities = await _unitOfWork.CrMasSupPostCity.FindAllAsNoTrackingAsync(l => l.CrMasSupPostCityStatus != Status.Deleted);
            var citiesArray = Cities.Select(c => new { text = isArabic ? c.CrMasSupPostCityConcatenateArName : c.CrMasSupPostCityConcatenateEnName, value = c.CrMasSupPostCityCode }).ToList();
            ViewData["Cities"] = citiesArray;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBranch(BranchVM branchVM, string SigntureFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || branchVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Branches", "Branches");
            }

            // Check Validation
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Branches", "Branches");
            }

            var IsValidCity = await _unitOfWork.CrMasSupPostCity.FindAsync(l => l.CrMasSupPostCityCode == branchVM.BranchPostVM.CrCasBranchPostCity);
            bool NameArIsExist = (await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationArName == branchVM.CrCasBranchInformationArName && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor))!=null;
            bool NameArShortIsExist = (await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationArShortName == branchVM.CrCasBranchInformationArShortName && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor))!= null;
            bool NameEnIsExist = (await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationEnName == branchVM.CrCasBranchInformationEnName && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor))!= null;
            bool NameEnShortIsExist = (await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationEnShortName == branchVM.CrCasBranchInformationEnShortName && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor))!= null;

            if (IsValidCity == null) ModelState.AddModelError("BranchPostVM.CrCasBranchPostCity", _localizer["IsNotValidCity"]);
            if (NameArIsExist) ModelState.AddModelError("CrCasBranchInformationArName", _localizer["IsExist"]);
            if (NameArShortIsExist) ModelState.AddModelError("CrCasBranchInformationArShortName", _localizer["IsExist"]);
            if (NameEnIsExist) ModelState.AddModelError("CrCasBranchInformationEnName", _localizer["IsExist"]);
            if (NameEnShortIsExist) ModelState.AddModelError("CrCasBranchInformationEnShortName", _localizer["IsExist"]);

            if (!ModelState.IsValid)
            {
                var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active && x.CrMasSysCallingKeysNo == "966");
                ViewData["CallingKeys"] = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View(branchVM);
            }

            try
            {
                var BranchInformaiton = _mapper.Map<CrCasBranchInformation>(branchVM);
                BranchInformaiton.CrCasBranchInformationLessor = user.CrMasUserInformationLessor;
                BranchInformaiton.CrCasBranchInformationCode = await getNextBranchCode(user.CrMasUserInformationLessor);
                BranchInformaiton.CrCasBranchInformationTgaCode = int.Parse(branchVM.CrCasBranchInformationTgaCode);

                var BranchPost = _mapper.Map<CrCasBranchPost>(branchVM.BranchPostVM);
                BranchPost.CrCasBranchPostLessor = user.CrMasUserInformationLessor;
                BranchPost.CrCasBranchPostBranch = BranchInformaiton.CrCasBranchInformationCode;

                string foldername = $"images/Company/{BranchInformaiton.CrCasBranchInformationLessor}/Branches/{BranchInformaiton.CrCasBranchInformationCode}";
                string filePathSignture = string.IsNullOrEmpty(SigntureFile)
                    ? "~/images/common/DefualtUserSignature.png"
                    : await FileExtensions.SaveSigntureImage(_hostingEnvironment, SigntureFile, BranchInformaiton.CrCasBranchInformationCode, string.Empty, foldername);

                BranchInformaiton.CrCasBranchInformationDirectorSignature = filePathSignture;

                await _BranchInformation.AddBranchInformation(BranchInformaiton);
                await _PostBranch.AddPostBranch(BranchPost, IsValidCity);
                await _BranchDocument.AddBranchDocument(BranchInformaiton.CrCasBranchInformationLessor, BranchInformaiton.CrCasBranchInformationCode);
                await _SalesPoint.AddSalesPoint(BranchInformaiton);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    await FileExtensions.CreateFolderBranch(_webHostEnvironment, user.CrMasUserInformationLessor, BranchInformaiton.CrCasBranchInformationCode);
                    await SaveTracingForBranchChange(user, branchVM.CrCasBranchInformationArShortName, branchVM.CrCasBranchInformationEnShortName, Status.Insert);
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                }

                return RedirectToAction("Branches");
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Branches");
            }
        }
        private async Task<string> getNextBranchCode(string lessor)
        {
            var branch = (await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(l => l.CrCasBranchInformationLessor == lessor)).OrderByDescending(x => x.CrCasBranchInformationCode).FirstOrDefault();
            return (int.Parse(branch?.CrCasBranchInformationCode) + 1).ToString();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "0";

            // Set Title
            var titles = await setTitle("201", "2201001", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            /*            var IsValidCity = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityConcatenateArName == branchVM.BranchPostVM.CrCasBranchPostCity || l.CrMasSupPostCityConcatenateEnName == branchVM.BranchPostVM.CrCasBranchPostCity).FirstOrDefault();
            */
            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList;
            var lessor = await _UserService.GetUserLessor(User);
            var branchinformation = _unitOfWork.CrCasBranchInformation.Find(l => l.CrCasBranchInformationLessor == lessor.CrMasUserInformationLessor && l.CrCasBranchInformationCode == id);
            var BranchPost = _unitOfWork.CrCasBranchPost.Find(l => l.CrCasBranchPostLessor == lessor.CrMasUserInformationLessor && l.CrCasBranchPostBranch == id, new[] { "CrCasBranchPostNavigation", "CrCasBranchPostCityNavigation" });
            ViewBag.SalesPointCount = _unitOfWork.CrCasAccountSalesPoint.FindAll(l => l.CrCasAccountSalesPointLessor == lessor.CrMasUserInformationLessor && l.CrCasAccountSalesPointBrn == id && l.CrCasAccountSalesPointStatus != Status.Deleted && l.CrCasAccountSalesPointBank != "00").Count();
            ViewBag.CarsCount = _unitOfWork.CrCasCarInformation.FindAll(l => l.CrCasCarInformationLessor == lessor.CrMasUserInformationLessor && l.CrCasCarInformationBranch == id && l.CrCasCarInformationStatus != Status.Deleted && l.CrCasCarInformationStatus != Status.Sold).Count();

            BranchVM branchVM = _mapper.Map<BranchVM>(branchinformation);
            branchVM.BranchPostVM = _mapper.Map<BranchPost1VM>(BranchPost);

            if (CultureInfo.CurrentCulture.Name == "en-US")
            {
                ViewBag.CrCasBranchPostCity = BranchPost.CrCasBranchPostCityNavigation.CrMasSupPostCityConcatenateEnName;
            }
            else
            {
                ViewBag.CrCasBranchPostCity = BranchPost.CrCasBranchPostCityNavigation.CrMasSupPostCityConcatenateArName;
            }


            return View(branchVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BranchVM branchVM, IFormFile? SigntureFile)
        {
            var IsValidCity = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityCode == branchVM.BranchPostVM.CrCasBranchPostCity).FirstOrDefault();
            if (IsValidCity == null) ModelState.AddModelError("BranchPostVM.CrCasBranchPostCity", _localizer["IsNotValidCity"]);

            if (ModelState.IsValid)
            {
                var BranchInformaiton = _mapper.Map<CrCasBranchInformation>(branchVM);
                var BranchPost = _mapper.Map<CrCasBranchPost>(branchVM.BranchPostVM);
                BranchPost.CrCasBranchPostLessor = BranchInformaiton.CrCasBranchInformationLessor;
                BranchPost.CrCasBranchPostBranch = BranchInformaiton.CrCasBranchInformationCode;
                BranchPost.CrCasBranchPostCity = IsValidCity?.CrMasSupPostCityCode;
                string foldername = $"{"images\\Company"}\\{BranchInformaiton.CrCasBranchInformationLessor}\\{"Branches"}\\{BranchInformaiton.CrCasBranchInformationCode}";
                string filePathSignture;
                if (SigntureFile != null)
                {
                    string Name = "DirectorSignature";
                    var fileNameImg = Name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقت
                    filePathSignture = await SigntureFile.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png", BranchInformaiton.CrCasBranchInformationDirectorSignature);
                }
                else if (branchVM.CrCasBranchInformationDirectorSignature != null)
                {
                    filePathSignture = branchVM.CrCasBranchInformationDirectorSignature;
                }
                else
                {
                    filePathSignture = "~/images/common/DefualtUserSignature.png";
                }
                BranchInformaiton.CrCasBranchInformationDirectorSignature = filePathSignture;
                if (BranchInformaiton.CrCasBranchInformationAvailableBalance == null) BranchInformaiton.CrCasBranchInformationAvailableBalance = 0;
                if (BranchInformaiton.CrCasBranchInformationTotalBalance == null) BranchInformaiton.CrCasBranchInformationTotalBalance = 0;
                if (BranchInformaiton.CrCasBranchInformationReservedBalance == null) BranchInformaiton.CrCasBranchInformationReservedBalance = 0;
                var checkBranch = _unitOfWork.CrCasBranchInformation.Update(BranchInformaiton);

                var checkBranchPost = _PostBranch.UpdatePostBranch(BranchPost, IsValidCity);
                if (checkBranch != null && checkBranchPost == true && await _unitOfWork.CompleteAsync() > 0)
                {
                    var (mainTask, subTask, system, currentUser) = await SetTrace("201", "2201001", "2");

                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تعديل فرع", "Edit branch", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                    // Save Adminstrive Procedures
                    await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "201", "20", currentUser.CrMasUserInformationLessor, "100",
                   BranchInformaiton.CrCasBranchInformationCode, null, null, null, null, null, null, null, null, "تعديل", "Edit", "U", null);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Branches");
                }
            }

            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "0";
            _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active && x.CrMasSysCallingKeysNo == "966");
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList;

            return View(branchVM);
        }

        [HttpGet]
        public JsonResult GetCities()
        {
            if (CultureInfo.CurrentCulture.Name == "ar-EG")
            {
                var citiesAr = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityRegionsCode != "10" && l.CrMasSupPostCityRegionsCode != "11");
                var citiesArarrayAr = citiesAr.Select(c => new { text = c.CrMasSupPostCityConcatenateArName, value = c.CrMasSupPostCityCode });
                return Json(citiesArarrayAr);
            }

            var citiesEn = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityRegionsCode != "10" && l.CrMasSupPostCityRegionsCode != "11");
            var citiesArarrayEn = citiesEn.Select(c => new { text = c.CrMasSupPostCityConcatenateEnName, value = c.CrMasSupPostCityCode });
            return Json(citiesArarrayEn);
        }

        [HttpPost]
        public async Task<IActionResult> EditStatus(string lessorCode, string Branchcode, string status)
        {
            string sAr = "";
            string sEn = "";
            var branch = _unitOfWork.CrCasBranchInformation.Find(l => l.CrCasBranchInformationLessor == lessorCode && l.CrCasBranchInformationCode == Branchcode);
            var docs = _unitOfWork.CrCasBranchDocument.FindAll(l => l.CrCasBranchDocumentsLessor == lessorCode && l.CrCasBranchDocumentsBranch == Branchcode);
            var salesPoints = _unitOfWork.CrCasAccountSalesPoint.FindAll(l => l.CrCasAccountSalesPointLessor == lessorCode && l.CrCasAccountSalesPointBrn == Branchcode);
            var cars = _unitOfWork.CrCasCarInformation.FindAll(l => l.CrCasCarInformationLessor == lessorCode && l.CrCasCarInformationBranch == Branchcode && l.CrCasCarInformationStatus != Status.Sold);
            var userBranchValidity = _unitOfWork.CrMasUserBranchValidity.FindAll(l => l.CrMasUserBranchValidityLessor == lessorCode && l.CrMasUserBranchValidityBranch == Branchcode);
            if (branch != null)
            {
                if (await CheckUserSubValidationProcdures("2201001", status))
                {
                    if (status == Status.Hold)
                    {
                        sAr = "ايقاف";
                        sEn = "Hold";
                        branch.CrCasBranchInformationStatus = Status.Hold;
                        foreach (var doc in docs) doc.CrCasBranchDocumentsBranchStatus = Status.Hold;
                        foreach (var salesPoint in salesPoints) salesPoint.CrCasAccountSalesPointBranchStatus = Status.Hold;
                        foreach (var car in cars) car.CrCasCarInformationBranchStatus = Status.Hold;
                        foreach (var user in userBranchValidity) user.CrMasUserBranchValidityBranchRecStatus = Status.Hold;
                    }
                    else if (status == Status.Deleted)
                    {
                        sAr = "حذف";
                        sEn = "Remove";
                        branch.CrCasBranchInformationStatus = Status.Deleted;
                        foreach (var doc in docs) doc.CrCasBranchDocumentsBranchStatus = Status.Deleted;
                        foreach (var salesPoint in salesPoints) salesPoint.CrCasAccountSalesPointBranchStatus = Status.Deleted;
                        foreach (var car in cars) car.CrCasCarInformationBranchStatus = Status.Deleted;
                        foreach (var user in userBranchValidity) user.CrMasUserBranchValidityBranchRecStatus = Status.Deleted;
                    }
                    else if (status == Status.Active)
                    {
                        sAr = "استرجاع";
                        sEn = "Retrive";
                        branch.CrCasBranchInformationStatus = Status.Active;
                        foreach (var doc in docs) doc.CrCasBranchDocumentsBranchStatus = Status.Active;
                        foreach (var salesPoint in salesPoints) salesPoint.CrCasAccountSalesPointBranchStatus = Status.Active;
                        foreach (var car in cars) car.CrCasCarInformationBranchStatus = Status.Active;
                        foreach (var user in userBranchValidity) user.CrMasUserBranchValidityBranchRecStatus = Status.Active;
                    }

                    await _unitOfWork.CompleteAsync();
                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("201", "2201001", "2");
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                    // Save Adminstrive Procedures
                    await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "201", "20", currentUser.CrMasUserInformationLessor, "100",
                   branch.CrCasBranchInformationCode, null, null, null, null, null, null, null, null, sAr, sEn, "U", null);

                    return RedirectToAction("Index", "LessorsKSA");
                }
            }

            return View(branch);

        }

        private async Task SaveTracingForBranchChange(CrMasUserInformation user, string arBranch, string enBranch, string status)
        {
            var (operationAr, operationEn) = GetStatusTranslation(status);

            var (mainTask, subTask, system, currentUser) = await SetTrace(pageNumber);

            await _userLoginsService.SaveTracing(
                currentUser.CrMasUserInformationCode,
                arBranch,
                enBranch,
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
    }
}
