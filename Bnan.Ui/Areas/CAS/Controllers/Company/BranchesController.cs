
using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.MAS.Controllers.Companies;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;

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


        public BranchesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IUserService userService, IBranchInformation branchInformation, IUserLoginsService userLoginsService, IStringLocalizer<LessorsKSAController> localizer, IToastNotification toastNotification, IPostBranch postBranch, IBranchDocument branchDocument, ISalesPoint salesPoint, IAdminstritiveProcedures adminstritiveProcedures, IWebHostEnvironment webHostEnvironment, IBaseRepo baseRepo, IWebHostEnvironment hostingEnvironment, IDocumentsMaintainanceCar documentsMaintainance) : base(userManager, unitOfWork, mapper)
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

            var Branches = await _unitOfWork.CrCasBranchInformation.FindAllAsNoTrackingAsync(l => l.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && l.CrCasBranchInformationStatus == Status.Active,
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


            if (!ModelState.IsValid)
            {
                var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
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
                // Check if the entity already exists
                if (await _BranchInformation.ExistsByDetailsAsync(BranchInformaiton))
                {
                    await AddModelErrorsAsync(BranchInformaiton);
                    _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View("AddBranch", BranchInformaiton);
                }

                var BranchPost = _mapper.Map<CrCasBranchPost>(branchVM.BranchPostVM);
                BranchPost.CrCasBranchPostLessor = user.CrMasUserInformationLessor;
                BranchPost.CrCasBranchPostBranch = BranchInformaiton.CrCasBranchInformationCode;

                string foldername = $"images/Company/{BranchInformaiton.CrCasBranchInformationLessor}/Branches/{BranchInformaiton.CrCasBranchInformationCode}";
                string fileName = "Signture_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                string filePathSignture = string.IsNullOrEmpty(SigntureFile)
                    ? "~/images/common/DefaultSignture.jpg"
                    : await FileExtensions.SaveSigntureImage(_hostingEnvironment, SigntureFile, string.Empty, foldername, fileName);

                BranchInformaiton.CrCasBranchInformationDirectorSignature = filePathSignture;

                await _BranchInformation.AddBranchInformation(BranchInformaiton);
                await _PostBranch.AddPostBranch(BranchPost);
                await _BranchDocument.AddBranchDocument(BranchInformaiton.CrCasBranchInformationLessor, BranchInformaiton.CrCasBranchInformationCode);
                await _SalesPoint.AddSalesPoint(BranchInformaiton);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    await FileExtensions.CreateFolderBranch(_webHostEnvironment, user.CrMasUserInformationLessor, BranchInformaiton.CrCasBranchInformationCode);
                    await SaveTracingForBranchChange(user, branchVM.CrCasBranchInformationArShortName, branchVM.CrCasBranchInformationEnShortName, Status.Insert);
                    await SaveAdminstritiveForBranch(user, "201", "20", BranchInformaiton, Status.Insert);
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
            await SetPageTitleAsync(Status.Update, pageNumber);
            var user = await _userManager.GetUserAsync(User);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Branches", "Branches");
            }
            // جلب بيانات المؤجر
            var BranchInfo = await _unitOfWork.CrCasBranchInformation.FindAsync(l => l.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && l.CrCasBranchInformationCode == id,
                                                                                new[] { "CrCasBranchPost.CrCasBranchPostCityNavigation", "CrCasBranchDocuments.CrCasBranchDocumentsProceduresNavigation" });
            if (BranchInfo == null || BranchInfo.CrCasBranchPost == null || BranchInfo.CrCasBranchDocuments.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Branches", "Branches");
            }

            BranchVM branchVM = _mapper.Map<BranchVM>(BranchInfo);
            branchVM.BranchPostVM = _mapper.Map<BranchPost1VM>(BranchInfo.CrCasBranchPost);
            branchVM.BranchDocsVM = _mapper.Map<List<BranchDocsVM>>(BranchInfo.CrCasBranchDocuments);


            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList;
            return View(branchVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BranchVM branchVM, string? SigntureFile)
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, pageNumber);
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Branches", "Branches");
            }
            if (user == null || branchVM == null || branchVM.BranchPostVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Branches", "Branches");
            }
            var branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && x.CrCasBranchInformationCode == branchVM.CrCasBranchInformationCode);
            var branchPost = await _unitOfWork.CrCasBranchPost.FindAsync(x => x.CrCasBranchPostLessor == user.CrMasUserInformationLessor && x.CrCasBranchPostBranch == branchVM.CrCasBranchInformationCode);
            if (branch == null || branchPost == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Branches", "Branches");
            }
            if (!ModelState.IsValid)
            {
                var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
                ViewData["CallingKeys"] = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View(branchVM);
            }
            try
            {
                var BranchInformaiton = _mapper.Map<CrCasBranchInformation>(branchVM);
                var BranchPost = _mapper.Map<CrCasBranchPost>(branchVM.BranchPostVM);

                BranchPost.CrCasBranchPostLessor = user.CrMasUserInformationLessor;
                BranchPost.CrCasBranchPostBranch = branch.CrCasBranchInformationCode;
                BranchInformaiton.CrCasBranchInformationLessor = user.CrMasUserInformationLessor;
                string filePathSignture;


                if (!string.IsNullOrEmpty(SigntureFile))
                {
                    string foldername = $"images/Company/{BranchInformaiton.CrCasBranchInformationLessor}/Branches/{BranchInformaiton.CrCasBranchInformationCode}";
                    string fileName = "Signture_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    filePathSignture = await FileExtensions.SaveSigntureImage(_hostingEnvironment, SigntureFile, branch.CrCasBranchInformationDirectorSignature, foldername, fileName);
                }
                else if (branchVM.CrCasBranchInformationDirectorSignature != null) filePathSignture = branchVM.CrCasBranchInformationDirectorSignature;
                else filePathSignture = "~/images/common/DefaultSignture.jpg";

                BranchInformaiton.CrCasBranchInformationDirectorSignature = filePathSignture;

                var checkBranch = await _BranchInformation.UpdateBranchInformation(BranchInformaiton);
                var checkBranchPost = await _PostBranch.UpdatePostBranch(BranchPost);
                if (checkBranch && checkBranchPost && await _unitOfWork.CompleteAsync() > 0)
                {
                    await SaveTracingForBranchChange(user, branchVM.CrCasBranchInformationArShortName, branchVM.CrCasBranchInformationEnShortName, Status.Insert);
                    await SaveAdminstritiveForBranch(user, "201", "20", branch, Status.Update);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                }
                else _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

                return RedirectToAction("Branches");
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Branches");
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetDocDetails([FromQuery] string ProcedureId, [FromQuery] string BranchId)
        {
            var user = await _UserService.GetUserLessor(User);
            var branchDocument = _unitOfWork.CrCasBranchDocument.Find(l =>
                l.CrCasBranchDocumentsLessor == user.CrMasUserInformationLessor &&
                l.CrCasBranchDocumentsBranch == BranchId &&
                l.CrCasBranchDocumentsProcedures == ProcedureId);

            if (branchDocument == null)
                return null;

            var branchDocumentVM = _mapper.Map<BranchDocsVM>(branchDocument);

            // الحصول على تاريخ اليوم بالتنسيق المناسب
            //string todayDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            //string tomorrowDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
            return Json(new
            {
                docId = branchDocumentVM.CrCasBranchDocumentsProcedures,
                docNumber = branchDocumentVM.CrCasBranchDocumentsNo,
                docDate = branchDocumentVM.CrCasBranchDocumentsDate?.ToString("yyyy-MM-dd"),
                startDate = branchDocumentVM.CrCasBranchDocumentsStartDate?.ToString("yyyy-MM-dd"),
                endDate = branchDocumentVM.CrCasBranchDocumentsEndDate?.ToString("yyyy-MM-dd"),
                image = branchDocumentVM.CrCasBranchDocumentsImage,
                notes = branchDocumentVM.CrCasBranchDocumentsReasons
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateDocDetails([FromBody] BranchDocsVM model)
        {
            if (model == null || string.IsNullOrEmpty(model.CrCasBranchDocumentsProcedures) || string.IsNullOrEmpty(model.CrCasBranchDocumentsBranch))
                return BadRequest("البيانات المدخلة غير صحيحة.");

            var user = await _UserService.GetUserLessor(User);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Branches", "Branches");
            }
            var branchDocument = await _unitOfWork.CrCasBranchDocument.FindAsync(l =>
                l.CrCasBranchDocumentsLessor == user.CrMasUserInformationLessor &&
                l.CrCasBranchDocumentsBranch == model.CrCasBranchDocumentsBranch &&
                l.CrCasBranchDocumentsProcedures == model.CrCasBranchDocumentsProcedures, new[] { "CrCasBranchDocumentsProceduresNavigation" });

            if (branchDocument == null)
                return NotFound("لم يتم العثور على المستند.");

            if (model.CrCasBranchDocumentsDate == default || model.CrCasBranchDocumentsStartDate == default || model.CrCasBranchDocumentsEndDate == default)
                return BadRequest("يرجى إدخال تواريخ صالحة.");

            if (model.CrCasBranchDocumentsStartDate < model.CrCasBranchDocumentsDate)
                return BadRequest("تاريخ البداية يجب أن يكون أكبر من أو يساوي تاريخ المستند.");

            if (model.CrCasBranchDocumentsEndDate <= model.CrCasBranchDocumentsStartDate)
                return BadRequest("تاريخ النهاية يجب أن يكون أكبر من تاريخ البداية بيوم واحد على الأقل.");
            if (!string.IsNullOrEmpty(model.CrCasBranchDocumentsImage))
            {
                string foldername = $"images/Company/{user.CrMasUserInformationLessor}/Branches/{branchDocument.CrCasBranchDocumentsBranch}/Documentions";
                string fileName = $"{branchDocument.CrCasBranchDocumentsProcedures}_{DateTime.Now:yyyyMMddHHmmss}";
                model.CrCasBranchDocumentsImage = await FileExtensions.SaveSigntureImage(_hostingEnvironment, model.CrCasBranchDocumentsImage, branchDocument.CrCasBranchDocumentsImage, foldername, fileName);
            }
            else if (branchDocument.CrCasBranchDocumentsImage == null) model.CrCasBranchDocumentsImage = "~/images/common/صورة سجل تجاري.png";
            else
            {
                model.CrCasBranchDocumentsImage = branchDocument.CrCasBranchDocumentsImage;
            }
            var branchDocumentModel = _mapper.Map<CrCasBranchDocument>(model);
            branchDocumentModel.CrCasBranchDocumentsLessor = branchDocument.CrCasBranchDocumentsLessor;
            var result = await _BranchDocument.UpdateBranchDocument(branchDocumentModel);
            if (result && await _unitOfWork.CompleteAsync() > 0)
            {
                await SaveAdminstritiveForDocsBranch(user, "202", "20", branchDocument, Status.Update);
                return Ok(true);
            }
            return StatusCode(500, "حدث خطأ أثناء تحديث المستند.");
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
        public async Task<string> EditStatus(string status, string code)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationCode == code && x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor);
            if (branch == null) return "false";
            try
            {
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.Deleted) { if (!await _BranchInformation.CheckIfCanDeleteIt(branch.CrCasBranchInformationLessor, branch.CrCasBranchInformationCode)) return "udelete"; }
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                branch.CrCasBranchInformationStatus = status;
                _unitOfWork.CrCasBranchInformation.Update(branch);
                await _unitOfWork.CompleteAsync();
                await SaveTracingForBranchChange(user, branch.CrCasBranchInformationArShortName, branch.CrCasBranchInformationEnShortName, pageNumber);
                await SaveAdminstritiveForBranch(user, "201", "20", branch, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }
        private async Task AddModelErrorsAsync(CrCasBranchInformation entity)
        {
            if (await _BranchInformation.ExistsByLongArabicNameAsync(entity.CrCasBranchInformationArName, entity.CrCasBranchInformationLessor, entity.CrCasBranchInformationCode))
            {
                ModelState.AddModelError("CrCasBranchInformationArName", _localizer["Existing"]);
            }
            if (await _BranchInformation.ExistsByLongEnglishNameAsync(entity.CrCasBranchInformationEnName, entity.CrCasBranchInformationLessor, entity.CrCasBranchInformationCode))
            {
                ModelState.AddModelError("CrCasBranchInformationEnName", _localizer["Existing"]);
            }
            if (await _BranchInformation.ExistsByShortArabicNameAsync(entity.CrCasBranchInformationArShortName, entity.CrCasBranchInformationLessor, entity.CrCasBranchInformationCode))
            {
                ModelState.AddModelError("CrCasBranchInformationArShortName", _localizer["Existing"]);
            }
            if (await _BranchInformation.ExistsByShortEnglishNameAsync(entity.CrCasBranchInformationEnShortName, entity.CrCasBranchInformationLessor, entity.CrCasBranchInformationCode))
            {
                ModelState.AddModelError("CrCasBranchInformationEnShortName", _localizer["Existing"]);
            }
            if (await _BranchInformation.ExistsByTGACodeAsync((int)entity.CrCasBranchInformationTgaCode, entity.CrCasBranchInformationCode))
            {
                ModelState.AddModelError("CrCasBranchInformationTgaCode", _localizer["Existing"]);
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var user = await _userManager.GetUserAsync(User);

            var branches = await _BranchInformation.GetAllAsyncByLessor(user.CrMasUserInformationLessor);
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && branches != null)
            {
                if (existName == "CrCasBranchInformationArName" && branches.Any(x => x.CrCasBranchInformationArName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasBranchInformationArName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrCasBranchInformationEnName" && branches.Any(x => x.CrCasBranchInformationEnName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasBranchInformationEnName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrCasBranchInformationArShortName" && branches.Any(x => x.CrCasBranchInformationArShortName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasBranchInformationArShortName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrCasBranchInformationEnShortName" && branches.Any(x => x.CrCasBranchInformationEnShortName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasBranchInformationEnShortName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrCasBranchInformationTgaCode" && branches.Any(x => x.CrCasBranchInformationTgaCode == int.Parse(dataField)))
                {
                    errors.Add(new ErrorResponse { Field = "CrCasBranchInformationTgaCode", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
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
        private async Task SaveAdminstritiveForDocsBranch(CrMasUserInformation user, string procudureCode, string classification, CrCasBranchDocument branchDocs, string status)
        {
            var (operationAr, operationEn) = GetStatusTranslation(status);
            await _adminstritiveProcedures.SaveAdminstritive(user.CrMasUserInformationCode, "1", procudureCode, classification, user.CrMasUserInformationLessor, branchDocs.CrCasBranchDocumentsBranch,
            branchDocs.CrCasBranchDocumentsNo, null, null, branchDocs.CrCasBranchDocumentsNo, branchDocs.CrCasBranchDocumentsDate, branchDocs.CrCasBranchDocumentsStartDate, branchDocs.CrCasBranchDocumentsEndDate,
            null, null, operationAr, operationEn, status, branchDocs.CrCasBranchDocumentsReasons);
        }
        private async Task SaveAdminstritiveForBranch(CrMasUserInformation user, string procudureCode, string classification, CrCasBranchInformation branchInformation, string status)
        {
            var (operationAr, operationEn) = GetStatusTranslation(status);
            await _adminstritiveProcedures.SaveAdminstritive(user.CrMasUserInformationCode, "1", procudureCode, classification, user.CrMasUserInformationLessor, branchInformation.CrCasBranchInformationCode,
            branchInformation.CrCasBranchInformationCode, null, null, null, null, null, null, null, null, operationAr, operationEn, status, branchInformation.CrCasBranchInformationReasons);
        }

        public IActionResult SuccessToast()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Branches", "Branches");
        }
        public IActionResult SuccessToastSamePage()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Edit", "Branches");
        }
        public IActionResult Failed()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });
            return RedirectToAction("Branches", "Branches");
        }
    }
}
