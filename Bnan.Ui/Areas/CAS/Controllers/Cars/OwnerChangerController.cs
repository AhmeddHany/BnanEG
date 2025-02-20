using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Linq;
using Bnan.Ui.ViewModels.CAS.Cars;

namespace Bnan.Ui.Areas.CAS.Cars
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class OwnerChangerController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<OwnerChangerController> _localizer;
        private readonly string pageNumber = SubTasks.Cas_CarsChangeOwner;
        private readonly IWebHostEnvironment _env;



        public OwnerChangerController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IWebHostEnvironment env,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<OwnerChangerController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _env = env;
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
            string lessorCode = user?.CrMasUserInformationLessor ?? " ";
            var Owners = _unitOfWork.CrCasOwners.FindAll(x => x.CrCasOwnersLessorCode == lessorCode && x.CrCasOwnersStatus == Status.Active);
            var branches = _unitOfWork.CrCasBranchInformation.FindAll(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus == Status.Active);

            if (Owners?.Count()<2)
            {
                _toastNotification.AddErrorToastMessage(_localizer["No_Owners_Toview"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationStatus == Status.Active &&
                                                                    x.CrCasCarInformationBranchStatus == Status.Active && x.CrCasCarInformationOwnerStatus == Status.Active
                                                                   , new[] { "CrCasCarInformation1", "CrCasCarInformation2", "CrCasCarInformationDistributionNavigation", "CrCasCarInformationFuelNavigation", "CrCasCarInformationCvtNavigation", "CrCasCarInformationRegistrationNavigation" });

            var VM = new Cas_CarsChangeOwnerVM();
            VM.all_CarsData = cars?.ToList() ?? new List<CrCasCarInformation>();
            VM.all_OwnersData = Owners?.ToList() ?? new List<CrCasOwner>();
            VM.all_BranchesData = branches?.ToList() ?? new List<CrCasBranchInformation>();
            return View(VM);
        }


        [HttpGet]
        public async Task<IActionResult> GetDetails(string SerialNo)
        {
            var user = await _userManager.GetUserAsync(User);

            var cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationSerailNo==SerialNo.Trim() && x.CrCasCarInformationStatus == Status.Active &&
                                                                   x.CrCasCarInformationBranchStatus == Status.Active && x.CrCasCarInformationOwnerStatus == Status.Active
                                                                   , new[] { "CrCasCarInformation2" ,"CrCasCarInformation1", "CrCasCarInformationDistributionNavigation" });

            var thisCar = cars?.FirstOrDefault();

            if (cars == null) return Json(false);
            CarDetails_Owner CarDetails = new CarDetails_Owner();

            CarDetails.serialNo = SerialNo;
            CarDetails.customNo = thisCar?.CrCasCarInformationNo;
            CarDetails.carImage = thisCar?.CrCasCarInformationDistributionNavigation?.CrMasSupCarDistributionImage?.Replace("~","") ?? "";
            CarDetails.carNameAr = thisCar?.CrCasCarInformationConcatenateArName;
            CarDetails.carNameEn = thisCar?.CrCasCarInformationConcatenateEnName;

            CarDetails.ownerId = thisCar?.CrCasCarInformationOwner;
            CarDetails.ownerNameAr = thisCar?.CrCasCarInformation2?.CrCasOwnersArName;
            CarDetails.ownerNameEn = thisCar?.CrCasCarInformation2?.CrCasOwnersEnName;
            //CarDetails.reasons = thisCar?.CrCasCarInformationReasons;



            return Json(CarDetails);
        }

        [HttpPost]
        public async Task<IActionResult> PostData(string SerialNo, string ownerIdNew,string reasons)
        {
            var user = await _userManager.GetUserAsync(User);

            var car = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationSerailNo == SerialNo.Trim() && x.CrCasCarInformationStatus == Status.Active &&
                                                                   x.CrCasCarInformationBranchStatus == Status.Active && x.CrCasCarInformationOwnerStatus == Status.Active
                                                                   );

            var thisCar = car;

            if (user == null || car == null) return Json(false);

            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return Json(false);
                    return RedirectToAction("Index", "OwnerChanger");
                }
                var oldOwner = car.CrCasCarInformationOwner??" ";
                car.CrCasCarInformationOwner = ownerIdNew;
                //car.CrCasCarInformationReasons = reasons;
                if (oldOwner== ownerIdNew) return Json(false);

                _unitOfWork.CrCasCarInformation.Update(car);
                await _unitOfWork.CompleteAsync();
                //if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, car, Status.Update);
                var Target = SerialNo ?? " ";
                var Car_From = oldOwner ?? " ";
                var Car_To = ownerIdNew ?? " ";

                await SaveAdminstrative(user,Status.Insert, Target, reasons, Car_From, Car_To);
                return Json(true);
                return RedirectToAction("Index", "OwnerChanger");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return Json(false);
                return RedirectToAction("Index", "OwnerChanger");
            }

            return Json(false);
        }

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrCasCarInformation thisCAr, string status)
        {

            var recordAr = thisCAr.CrCasCarInformationConcatenateArName;
            var recordEn = thisCAr.CrCasCarInformationConcatenateEnName;
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

        private async Task SaveAdminstrative(CrMasUserInformation user, string status, string? Target,string? reasons,string? Car_From , string? Car_To)
        {
            // change ProcedureCode and Classification (30 == Financial) , (20 == Admin)
            string procedure_Code = "216";
            string procedure_Classification= "20";
            //var procedureData = await _unitOfWork.CrMasSysProcedure.FindAsync(x=>x.CrMasSysProceduresCode== procedure_Code && x.CrMasSysProceduresClassification== procedure_Classification);
            //var recordAr = procedureData?.CrMasSysProceduresArName??" ";
            //var recordEn = procedureData?.CrMasSysProceduresEnName ?? " ";
            //if (recordAr.Length > 29) recordAr = recordAr?.Substring(0, 30);
            //if (recordEn.Length > 29) recordEn = recordEn?.Substring(0, 30);
            var recordAr = "تغيير ملكية سيارة";
            var recordEn = "Change Car Owner";

            string sector = "1";
            var lessorCode = user?.CrMasUserInformationLessor??" ";
            var branchCode = "100";
            var target = Target;
            var UserInsert = user?.CrMasUserInformationCode??" ";

            //string? CarFrom = null;
            //string? CarTo = null;
            string? Reasons = reasons;
            string? CarFrom = Car_From;
            string? CarTo = Car_To;
            if (Reasons?.Length > 99) Reasons = Reasons?.Substring(0, 100);
            if (CarFrom?.Length > 19) CarFrom = CarFrom?.Substring(0, 20);
            if (CarTo?.Length > 19) CarTo = CarTo?.Substring(0, 20);
            decimal? Debit = null;
            decimal? Credit = null;
            string? Doc_No = null;
            DateTime? Doc_Date = null;
            DateTime? Doc_Start = null;
            DateTime? Doc_End = null;


            await _userLoginsService.SaveAdminstrative(
            sector,
            procedure_Code,
            lessorCode,
            branchCode,
            procedure_Classification,
            target,
            recordAr,
            recordEn,
            UserInsert,
            status,
            Reasons,
            CarFrom,
            CarTo,
            Debit,Credit, Doc_No,Doc_Date, Doc_Start,Doc_End
            );
        
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
            return RedirectToAction("Index", "OwnerChanger");
        }

    }
}


