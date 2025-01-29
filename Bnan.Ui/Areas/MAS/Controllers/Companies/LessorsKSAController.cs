using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;


namespace Bnan.Ui.Areas.MAS.Controllers.Companies
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class LessorsKSAController : BaseController
    {

        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<LessorsKSAController> _localizer;
        private readonly ILessorImage _LessorImage;
        private readonly IOwner _Owner;
        private readonly IBeneficiary _Beneficiary;
        private readonly ILessorMembership _LessorMembership;
        private readonly ILessorMechanism _LessorMechanism;
        private readonly ICompnayContract _CompnayContract;
        private readonly IBranchInformation _BranchInformation;
        private readonly IBranchDocument _BranchDocument;
        private readonly IPostBranch _PostBranch;
        private readonly IAccountBank _AccountBank;
        private readonly ISalesPoint _SalesPoint;
        private readonly IAuthService _authService;
        private readonly IWhatsupConnect _whatsupConnect;
        private readonly ITGAConnect _tgaConnect;
        private readonly IShomoosConnect _shomoosConnect;
        private readonly ISMSConnect _smsConnect;
        private readonly IUserMainValidtion _UserMainValidtion;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasLessor _masLessor;

        private readonly string pageNumber = SubTasks.CrMasLessorInformation;



        public LessorsKSAController(IUnitOfWork unitOfWork,
                                    IMapper mapper,
                                    UserManager<CrMasUserInformation> userManager,
                                    IUserLoginsService userLoginsService,
                                    IUserService userService,
                                    IToastNotification toastNotification,
                                    ILessorImage lessorImage,
                                    IOwner owner, IBeneficiary beneficiary,
                                    ILessorMembership lessorMembership,
                                    ILessorMechanism lessorMechanism,
                                    ICompnayContract compnayContract,
                                    IBranchInformation branchInformation,
                                    IBranchDocument branchDocument,
                                    IPostBranch postBranch,
                                    IAccountBank accountBank,
                                    ISalesPoint salesPoint,
                                    IStringLocalizer<LessorsKSAController> localizer,
                                    IAuthService authService,
                                    IUserMainValidtion userMainValidtion, IWebHostEnvironment webHostEnvironment, IBaseRepo baseRepo, IMasLessor masLessor, IWhatsupConnect whatsupConnect, ITGAConnect tgaConnect, IShomoosConnect shomoosConnect, ISMSConnect smsConnect) : base(userManager, unitOfWork, mapper)
        {
            _userLoginsService = userLoginsService;
            _userService = userService;
            _toastNotification = toastNotification;
            _LessorImage = lessorImage;
            _Owner = owner;
            _Beneficiary = beneficiary;
            _LessorMembership = lessorMembership;
            _LessorMechanism = lessorMechanism;
            _CompnayContract = compnayContract;
            _BranchInformation = branchInformation;
            _BranchDocument = branchDocument;
            _PostBranch = postBranch;
            _AccountBank = accountBank;
            _SalesPoint = salesPoint;
            _localizer = localizer;
            _localizer = localizer;
            _authService = authService;
            _UserMainValidtion = userMainValidtion;
            _webHostEnvironment = webHostEnvironment;
            _baseRepo = baseRepo;
            _masLessor = masLessor;
            _whatsupConnect = whatsupConnect;
            _tgaConnect = tgaConnect;
            _shomoosConnect = shomoosConnect;
            _smsConnect = smsConnect;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Set page titles
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            // استعلام رئيسي مع NoTracking
            IQueryable<CrMasLessorInformation> query = _unitOfWork.CrMasLessorInformation.GetTableNoTracking().Include("CrCasBranchInformations.CrCasBranchPost.CrCasBranchPostCityNavigation")/*.Where(x => x.CrMasLessorInformationCode != "0000")*/;
            // استرجاع التراخيص الفعّالة
            var lessors = await query.Where(x => x.CrMasLessorInformationStatus == Status.Active).ToListAsync();
            // إذا لم توجد تراخيص فعّالة، استرجاع التراخيص المعلّقة
            if (!lessors.Any())
            {
                lessors = await query.Where(x => x.CrMasLessorInformationStatus == Status.Hold).ToListAsync();
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(lessors);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetLessorsByStatus(string status, string search)
        {
            // استعلام أساسي مع Include
            IQueryable<CrMasLessorInformation> query = _unitOfWork.CrMasLessorInformation.GetTableNoTracking().Include("CrCasBranchInformations.CrCasBranchPost.CrCasBranchPostCityNavigation")/*.Where(x => x.CrMasLessorInformationCode != "0000")*/;

            // فلترة حسب حالة status
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All) query = query.Where(x => x.CrMasLessorInformationStatus != Status.Deleted);
                else query = query.Where(x => x.CrMasLessorInformationStatus == status);
            }

            // فلترة حسب search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(x => x.CrMasLessorInformationArLongName.Contains(search) || x.CrMasLessorInformationEnLongName.ToLower().Contains(search) || x.CrMasLessorInformationCode.Contains(search));
            }

            // تنفيذ الاستعلام وتحميل البيانات
            var lessors = await query.ToListAsync();
            return PartialView("_DataTableCompaniesInformations", lessors);
        }


        [HttpGet]
        public async Task<IActionResult> AddLessorKSA()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Insert, pageNumber);
                return RedirectToAction("Index", "LessorsKSA");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });
                return RedirectToAction("Index", "LessorsKSA");
            }
            await SetPageTitleAsync(Status.Insert, pageNumber);

            //pass Classification Arabic
            var ClassificationAr = _unitOfWork.CrCasLessorClassification.GetAll();
            var ClassificationDropDownAr = ClassificationAr.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationArName }).ToList();
            ClassificationDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ClassificationDropDownAr"] = ClassificationDropDownAr;


            //pass Classification English
            var ClassificationEn = _unitOfWork.CrCasLessorClassification.GetAll();
            var ClassificationDropDownEn = ClassificationAr.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationEnName }).ToList();
            ClassificationDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ClassificationDropDownEn"] = ClassificationDropDownEn;


            //To Set Title;
            var titles = await setTitle("101", "1101001", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "اضافة", "Create", titles[3]);

            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList;

            // Pass All callingKeys to the view 
            var callingKeysWhats = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyListWhats = callingKeysWhats.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeysWhats"] = callingKeyListWhats;

            //Check User Sub Validation Procdures
            var UserValidation = await CheckUserSubValidationProcdures("1101001", Status.Insert);
            if (UserValidation == false) return RedirectToAction("Index", "Home", new { area = "MAS" });

            var Lessors = await _unitOfWork.CrMasLessorInformation.GetAllAsync();
            var LessorCode = (int.Parse(Lessors.LastOrDefault().CrMasLessorInformationCode) + 1).ToString();
            ViewBag.LessorCode = LessorCode;
            return View();
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
        public async Task<IActionResult> AddLessorKSA(CrMasLessorInformationVM lessorVM, string WelcomeImg)

        {
            var user = await _userManager.GetUserAsync(User);

            var IsNameArLong = _unitOfWork.CrMasLessorInformation.FindAll(l => l.CrMasLessorInformationArLongName == lessorVM.CrMasLessorInformationArLongName).Count() > 0;
            var IsNameArShort = _unitOfWork.CrMasLessorInformation.FindAll(l => l.CrMasLessorInformationArShortName == lessorVM.CrMasLessorInformationArShortName).Count() > 0;
            var IsNameEnLong = _unitOfWork.CrMasLessorInformation.FindAll(l => l.CrMasLessorInformationEnLongName == lessorVM.CrMasLessorInformationEnLongName).Count() > 0;
            var IsNameEnShort = _unitOfWork.CrMasLessorInformation.FindAll(l => l.CrMasLessorInformationEnShortName == lessorVM.CrMasLessorInformationEnShortName).Count() > 0;
            var IsGovNo = _unitOfWork.CrCasBranchInformation.FindAll(l => l.CrCasBranchInformationGovernmentNo == lessorVM.CrMasLessorInformationGovernmentNo).Count() > 0;
            var IsTaxNo = _unitOfWork.CrCasBranchInformation.FindAll(l => l.CrCasBranchInformationTaxNo == lessorVM.CrMasLessorInformationTaxNo).Count() > 0;
            var IsValidCity = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityConcatenateArName == lessorVM.BranchPostVM.CrCasBranchPostCity || l.CrMasSupPostCityConcatenateEnName == lessorVM.BranchPostVM.CrCasBranchPostCity).FirstOrDefault();

            if (IsValidCity == null) ModelState.AddModelError("BranchPostVM.CrCasBranchPostCity", _localizer["IsNotValidCity"]);
            if (IsNameArLong) ModelState.AddModelError("CrMasLessorInformationArLongName", _localizer["IsTaken"]);
            if (IsNameArShort) ModelState.AddModelError("CrMasLessorInformationArShortName", _localizer["IsTaken"]);
            if (IsNameEnLong) ModelState.AddModelError("CrMasLessorInformationEnLongName", _localizer["IsTaken"]);
            if (IsNameEnShort) ModelState.AddModelError("CrMasLessorInformationEnShortName", _localizer["IsTaken"]);
            if (IsGovNo) ModelState.AddModelError("CrMasLessorInformationGovernmentNo", _localizer["IsTakenGov"]);
            if (IsTaxNo) ModelState.AddModelError("CrMasLessorInformationTaxNo", _localizer["IsTakenTax"]);
            if (lessorVM.BranchPostVM.CrCasBranchPostCity == "") ModelState.AddModelError("BranchPostVM.CrCasBranchPostCity", _localizer["requiredFiled"]);
            if (lessorVM.CrMasLessorInformationClassification.Trim() == "") ModelState.AddModelError("CrMasLessorInformationClassification", _localizer["requiredFiled"]);

            if (ModelState.IsValid)
            {
                var LessorVMTlessor = _mapper.Map<CrMasLessorInformation>(lessorVM);

                try
                {
                    await SetPageTitleAsync(Status.Insert, pageNumber);
                    LessorVMTlessor.CrMasLessorInformationStatus = "A";
                    // Check if the entity already exists
                    if (await _masLessor.ExistsByDetailsAsync(LessorVMTlessor))
                    {
                        await AddModelErrorsAsync(LessorVMTlessor);
                        _toastNotification.AddErrorToastMessage(_localizer["toastor_Exist"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return View("AddLessorKSA", LessorVMTlessor);
                    }
                    var BranchPostVMToBranchPost = _mapper.Map<CrCasBranchPost>(lessorVM.BranchPostVM);
                    // To create generate Code start from 4000 ;
                    if (LessorVMTlessor.CrMasLessorInformationCode == "1")
                    {
                        int code = 4000 + int.Parse(lessorVM.CrMasLessorInformationCode);
                        LessorVMTlessor.CrMasLessorInformationCode = code.ToString();
                    }
                    await _unitOfWork.CrMasLessorInformation.AddAsync(LessorVMTlessor);

                    await _LessorImage.AddLessorImage(LessorVMTlessor.CrMasLessorInformationCode);

                    await _Owner.AddOwner(LessorVMTlessor.CrMasLessorInformationCode);

                    await _Beneficiary.AddBeneficiary(LessorVMTlessor.CrMasLessorInformationCode);

                    await _LessorMembership.AddLessorMemberShip(LessorVMTlessor.CrMasLessorInformationCode);

                    await _LessorMechanism.AddLessorMechanism(LessorVMTlessor.CrMasLessorInformationCode);

                    await _CompnayContract.AddCompanyContract(LessorVMTlessor.CrMasLessorInformationCode);

                    await _BranchInformation.AddBranchInformationDefault(LessorVMTlessor.CrMasLessorInformationCode);

                    await _AccountBank.AddAcountBankDefalut(LessorVMTlessor.CrMasLessorInformationCode);

                    await _PostBranch.AddPostBranchDefault(LessorVMTlessor.CrMasLessorInformationCode, BranchPostVMToBranchPost, IsValidCity);

                    await _SalesPoint.AddSalesPointDefault(LessorVMTlessor.CrMasLessorInformationCode);

                    await _BranchDocument.AddBranchDocumentDefault(LessorVMTlessor.CrMasLessorInformationCode);

                    // add Connects
                    await _whatsupConnect.AddDefaultWhatsupConnect(LessorVMTlessor.CrMasLessorInformationCode);
                    await _tgaConnect.AddDefault(LessorVMTlessor.CrMasLessorInformationCode);
                    await _shomoosConnect.AddDefault(LessorVMTlessor.CrMasLessorInformationCode);
                    await _smsConnect.AddDefault(LessorVMTlessor.CrMasLessorInformationCode);
                    await _unitOfWork.CompleteAsync();

                    await SaveTracingForLessorChange(user, LessorVMTlessor, Status.Insert);

                    #region Whatsup
                    await WhatsAppServicesExtension.ConnectLessor(LessorVMTlessor.CrMasLessorInformationCode);
                    string fullPhoneNumber = $"{LessorVMTlessor.CrMasLessorInformationCommunicationMobileKey}{LessorVMTlessor.CrMasLessorInformationCommunicationMobile}";

                    if (!string.IsNullOrEmpty(WelcomeImg)) await WhatsAppServicesExtension.SendMediaAsync(fullPhoneNumber, " ", "0000", WelcomeImg, $"WelcomeImg_{LessorVMTlessor.CrMasLessorInformationCode}.png");
                    else await WhatsAppServicesExtension.SendMessageAsync(fullPhoneNumber, $"تم انشاء الشركة الخاصة بك باسم / {LessorVMTlessor.CrMasLessorInformationArLongName}", "0000");


                    #endregion
                    await FileExtensions.CreateFolderLessor(_webHostEnvironment, LessorVMTlessor.CrMasLessorInformationCode);
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    await SetPageTitleAsync(Status.Insert, pageNumber);
                    return View("AddLessorKSA", LessorVMTlessor);
                }
            }

            var Lessors = await _unitOfWork.CrMasLessorInformation.GetAllAsync();
            var LessorCode = (int.Parse(Lessors.LastOrDefault().CrMasLessorInformationCode) + 1).ToString();
            ViewBag.LessorCode = LessorCode;


            if (lessorVM.CrMasLessorInformationClassification != "")
            {
                //pass Classification Arabic
                var ClassificationAr = _unitOfWork.CrCasLessorClassification.GetAll();
                var ClassificationDropDownAr = ClassificationAr.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationArName, Selected = c.CrCasLessorClassificationCode == lessorVM.CrMasLessorInformationClassification }).ToList();
                ViewData["ClassificationDropDownAr"] = ClassificationDropDownAr;

                //pass Classification English
                var ClassificationEn = _unitOfWork.CrCasLessorClassification.GetAll();
                var ClassificationDropDownEn = ClassificationEn.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationEnName, Selected = c.CrCasLessorClassificationCode == lessorVM.CrMasLessorInformationClassification }).ToList();
                ViewData["ClassificationDropDownEn"] = ClassificationDropDownEn;
            }
            else
            {
                //pass Classification Arabic
                var ClassificationAr = _unitOfWork.CrCasLessorClassification.GetAll();
                var ClassificationDropDownAr = ClassificationAr.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationArName }).ToList();
                ClassificationDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
                ViewData["ClassificationDropDownAr"] = ClassificationDropDownAr;

                //pass Classification English
                var ClassificationEn = _unitOfWork.CrCasLessorClassification.GetAll();
                var ClassificationDropDownEn = ClassificationAr.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationEnName }).ToList();
                ClassificationDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
                ViewData["ClassificationDropDownEn"] = ClassificationDropDownEn;

            }

            if (lessorVM.BranchPostVM.CrCasBranchPostCity != "")
            {
                // Pass the City Post Arabic key to the view 
                var citiesAr = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityRegionsCode != "10" && l.CrMasSupPostCityRegionsCode != "11");
                var CityDropDownAr = citiesAr.Select(c => new SelectListItem { Value = c.CrMasSupPostCityConcatenateArName?.ToString(), Text = c.CrMasSupPostCityConcatenateArName, Selected = c.CrMasSupPostCityCode == lessorVM.BranchPostVM.CrCasBranchPostCity }).ToList();
                ViewData["CityDropDownAr"] = CityDropDownAr;

                // Pass the City Post English key to the view 
                var citiesEn = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityRegionsCode != "10" && l.CrMasSupPostCityRegionsCode != "11");
                var CityDropDownEn = citiesEn.Select(c => new SelectListItem { Value = c.CrMasSupPostCityConcatenateEnName?.ToString(), Text = c.CrMasSupPostCityConcatenateEnName, Selected = c.CrMasSupPostCityCode == lessorVM.BranchPostVM.CrCasBranchPostCity }).ToList();
                ViewData["CityDropDownEn"] = CityDropDownEn;
            }
            else
            {
                // Pass the City Post Arabic key to the view 
                var citiesAr = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityRegionsCode != "10" && l.CrMasSupPostCityRegionsCode != "11");
                var CityDropDownAr = citiesAr.Select(c => new SelectListItem { Value = c.CrMasSupPostCityConcatenateArName?.ToString(), Text = c.CrMasSupPostCityConcatenateArName }).ToList();
                CityDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
                ViewData["CityDropDownAr"] = CityDropDownAr;

                // Pass the City Post English key to the view 
                var citiesEn = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityRegionsCode != "10" && l.CrMasSupPostCityRegionsCode != "11");
                var CityDropDownEn = citiesEn.Select(c => new SelectListItem { Value = c.CrMasSupPostCityConcatenateEnName?.ToString(), Text = c.CrMasSupPostCityConcatenateEnName }).ToList();
                CityDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
                ViewData["CityDropDownEn"] = CityDropDownEn;
            }

            //To Set Title;
            var titles = await setTitle("101", "1101001", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "اضافة", "Create", titles[3]);

            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList;
            return View("AddLessorKSA", lessorVM);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            await SetPageTitleAsync(Status.Update, pageNumber);
            var user = await _userManager.GetUserAsync(User);

            // جلب بيانات المؤجر
            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(id);
            if (lessor == null /*|| id == "0000"*/)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "LessorsKSA");
            }

            var model = _mapper.Map<CrMasLessorInformationVM>(lessor);
            var branch = "100";
            if (lessor.CrMasLessorInformationCode == "0000") branch = "000";
            var branchPost = await _unitOfWork.CrCasBranchPost.FindAsync(l => l.CrCasBranchPostLessor == lessor.CrMasLessorInformationCode && l.CrCasBranchPostBranch == branch, new[] { "CrCasBranchPostCityNavigation" });
            model.BranchPostVM = _mapper.Map<BranchPostVM>(branchPost);

            var classifications = _unitOfWork.CrCasLessorClassification.GetAll();
            ViewData["ClassificationDropDownAr"] = classifications.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationArName }).ToList();
            ViewData["ClassificationDropDownEn"] = classifications.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationEnName }).ToList();
            var city = _unitOfWork.CrMasSupPostCity.FindAll(l => l.CrMasSupPostCityRegionsCode != "10" && l.CrMasSupPostCityRegionsCode != "11")?.FirstOrDefault(l => l.CrMasSupPostCityCode == branchPost.CrCasBranchPostCity);

            if (city != null)
            {
                ViewBag.CityDropDownAr = city.CrMasSupPostCityConcatenateArName;
                ViewBag.CityDropDownEn = city.CrMasSupPostCityConcatenateEnName;
                ViewBag.CityCode = city.CrMasSupPostCityCode;
            }
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active).Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo.Trim() }).ToList();
            ViewData["CallingKeys"] = callingKeys;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(CrMasLessorInformationVM CrMasLessorInformationVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, pageNumber);
            if (user == null || CrMasLessorInformationVM == null /*|| CrMasLessorInformationVM.CrMasLessorInformationCode == "0000"*/)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return RedirectToAction("Index", "LessorsKSA");
            }
            var lessor = _mapper.Map<CrMasLessorInformation>(CrMasLessorInformationVM);
            var BranchPost = _mapper.Map<CrCasBranchPost>(CrMasLessorInformationVM.BranchPostVM);
            BranchPost.CrCasBranchPostLessor = lessor.CrMasLessorInformationCode;
            BranchPost.CrCasBranchPostBranch = "100";
            if (lessor.CrMasLessorInformationCode == "0000") BranchPost.CrCasBranchPostBranch = "000";
            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.CrMasLessorInformation.Update(lessor);
                    _unitOfWork.CrCasBranchPost.Update(BranchPost);
                    _unitOfWork.Complete();
                    await SaveTracingForLessorChange(user, lessor, Status.Update);
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", });
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                }

            }

            // Pass the KSA callingKeys to the view 
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active && x.CrMasSysCallingKeysNo == "966");
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList;



            //pass Classification Arabic
            var ClassificationAr = _unitOfWork.CrCasLessorClassification.GetAll();
            var ClassificationDropDownAr = ClassificationAr.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationArName }).ToList();
            ClassificationDropDownAr.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ClassificationDropDownAr"] = ClassificationDropDownAr;

            //pass Classification English
            var ClassificationEn = _unitOfWork.CrCasLessorClassification.GetAll();
            var ClassificationDropDownEn = ClassificationAr.Select(c => new SelectListItem { Value = c.CrCasLessorClassificationCode?.ToString(), Text = c.CrCasLessorClassificationEnName }).ToList();
            ClassificationDropDownEn.Add(new SelectListItem { Text = "", Value = "", Selected = true });
            ViewData["ClassificationDropDownEn"] = ClassificationDropDownEn;



            await SetPageTitleAsync(Status.Update, pageNumber);
            return View("Edit", CrMasLessorInformationVM);
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(code);
            if (lessor == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                lessor.CrMasLessorInformationStatus = status;
                _unitOfWork.CrMasLessorInformation.Update(lessor);
                _unitOfWork.Complete();
                await SaveTracingForLessorChange(user, lessor, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        [HttpGet]
        public async Task<IActionResult> Connect(string companyId)
        {
            var status = await WhatsAppServicesExtension.ConnectLessor(companyId);
            return Json(new { status });
        }
        [HttpGet]
        public async Task<IActionResult> CheckClientInitialized(string companyId)
        {
            var status = await WhatsAppServicesExtension.CheckIsClientInitialized(companyId);
            return Json(new { status });
        }
        private async Task AddModelErrorsAsync(CrMasLessorInformation entity)
        {
            if (await _masLessor.ExistsByLongArabicNameAsync(entity.CrMasLessorInformationArLongName, entity.CrMasLessorInformationCode))
            {
                ModelState.AddModelError("CrMasLessorInformationArLongName", _localizer["Existing"]);
            }
            if (await _masLessor.ExistsByLongEnglishNameAsync(entity.CrMasLessorInformationEnLongName, entity.CrMasLessorInformationCode))
            {
                ModelState.AddModelError("CrMasLessorInformationEnLongName", _localizer["Existing"]);
            }
            if (await _masLessor.ExistsByShortArabicNameAsync(entity.CrMasLessorInformationArShortName, entity.CrMasLessorInformationCode))
            {
                ModelState.AddModelError("CrMasLessorInformationArShortName", _localizer["Existing"]);
            }
            if (await _masLessor.ExistsByShortEnglishNameAsync(entity.CrMasLessorInformationEnShortName, entity.CrMasLessorInformationCode))
            {
                ModelState.AddModelError("CrMasLessorInformationEnShortName", _localizer["Existing"]);
            }
        }
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var allLessors = await _unitOfWork.CrMasLessorInformation.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && allLessors != null)
            {
                if (existName == "CrMasLessorInformationArLongName" && allLessors.Any(x => x.CrMasLessorInformationArLongName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasLessorInformationArLongName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrMasLessorInformationEnLongName" && allLessors.Any(x => x.CrMasLessorInformationEnLongName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasLessorInformationEnLongName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrMasLessorInformationArShortName" && allLessors.Any(x => x.CrMasLessorInformationArShortName == dataField))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasLessorInformationArShortName", Message = _localizer["Existing"] });
                }
                else if (existName == "CrMasLessorInformationEnShortName" && allLessors.Any(x => x.CrMasLessorInformationEnShortName?.ToLower() == dataField.ToLower()))
                {
                    errors.Add(new ErrorResponse { Field = "CrMasLessorInformationEnShortName", Message = _localizer["Existing"] });
                }
            }

            return Json(new { errors });
        }
        private async Task SaveTracingForLessorChange(CrMasUserInformation user, CrMasLessorInformation lessor, string status)
        {


            var recordAr = lessor.CrMasLessorInformationArShortName;
            var recordEn = lessor.CrMasLessorInformationEnShortName;
            var (operationAr, operationEn) = GetStatusTranslation(status);

            var (mainTask, subTask, system, currentUser) = await SetTrace(pageNumber);

            await _userLoginsService.SaveTracing(
                currentUser.CrMasUserInformationCode,
                recordAr,
                recordEn,
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
        [HttpPost]
        public IActionResult DisplayToastError_NoUpdate(string messageText)
        {
            //نص الرسالة _localizer["AuthEmplpoyee_NoUpdate"] === messageText ; 
            if (messageText == null || messageText == "") messageText = "..";
            _toastNotification.AddErrorToastMessage(messageText, new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return Json(new { success = true });
        }


        public IActionResult DisplayToastSuccess_withIndex()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            return RedirectToAction("Index", "LessorsKSA");
        }
    }
}