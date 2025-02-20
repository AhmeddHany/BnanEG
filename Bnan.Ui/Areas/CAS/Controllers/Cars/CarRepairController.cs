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

namespace Bnan.Ui.Areas.CAS.Controllers.Cars
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CarRepairController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CarRepairController> _localizer;
        private readonly string pageNumber = SubTasks.Cas_CarsRepair;
        private readonly IWebHostEnvironment _env;



        public CarRepairController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IWebHostEnvironment env,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CarRepairController> localizer) : base(userManager, unitOfWork, mapper)
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
            List<(string, string, string)> StausData = new List<(string, string, string)>
            {
                 /*( Status.Active,"قيد التشغيل","running" ),*/ (Status.Maintaince,"صيانة","repair" ),
            };
            var branches = _unitOfWork.CrCasBranchInformation.FindAll(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationStatus == Status.Active);

            var cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode &&
                                                                    x.CrCasCarInformationBranchStatus == Status.Active && x.CrCasCarInformationOwnerStatus == Status.Active
                                                                    && (x.CrCasCarInformationStatus == Status.Active || x.CrCasCarInformationStatus == Status.Maintaince)
                                                                   , new[] { "CrCasCarInformation1", "CrCasCarInformation2", "CrCasCarInformationDistributionNavigation", "CrCasCarInformationFuelNavigation", "CrCasCarInformationCvtNavigation", "CrCasCarInformationRegistrationNavigation" });

            var VM = new Cas_CarsCarRepairVM();
            VM.minDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            VM.all_CarsData = cars?.ToList() ?? new List<CrCasCarInformation>();
            VM.all_StatusData = StausData ?? new List<(string, string, string)>();
            VM.all_BranchesData = branches?.ToList() ?? new List<CrCasBranchInformation>();
            return View(VM);
        }


        [HttpGet]
        public async Task<IActionResult> GetDetails(string SerialNo)
        {
            var user = await _userManager.GetUserAsync(User);

            var cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationSerailNo == SerialNo.Trim() &&
                                                                   x.CrCasCarInformationBranchStatus == Status.Active && x.CrCasCarInformationOwnerStatus == Status.Active
                                                                   && (x.CrCasCarInformationStatus == Status.Active || x.CrCasCarInformationStatus == Status.Maintaince)
                                                                   , new[] { "CrCasCarInformation2", "CrCasCarInformation1", "CrCasCarInformationDistributionNavigation" });

            var thisCar = cars?.FirstOrDefault();

            List<(string, string, string)> StausData = new List<(string, string, string)>
            {
                /* ( Status.Active,"قيد التشغيل","running" ),*/ (Status.Maintaince,"صيانة","repair" ),
            };

            if (cars == null) return Json(false);
            CarDetails_CarRepair CarDetails = new CarDetails_CarRepair();

            CarDetails.serialNo = SerialNo;
            CarDetails.customNo = thisCar?.CrCasCarInformationNo;
            CarDetails.carImage = thisCar?.CrCasCarInformationDistributionNavigation?.CrMasSupCarDistributionImage?.Replace("~", "") ?? "";
            CarDetails.carNameAr = thisCar?.CrCasCarInformationConcatenateArName;
            CarDetails.carNameEn = thisCar?.CrCasCarInformationConcatenateEnName;

            CarDetails.statusId = thisCar?.CrCasCarInformationStatus;

            CarDetails.statusNameAr = StausData?.Find(x => x.Item1 == thisCar?.CrCasCarInformationStatus).Item2 ?? " ";
            CarDetails.statusNameEn = StausData?.Find(x => x.Item1 == thisCar?.CrCasCarInformationStatus).Item3 ?? " ";
            //CarDetails.reasons = thisCar?.CrCasCarInformationReasons;

            var dateNow = DateTime.Now;
            if (thisCar?.CrCasCarInformationStatus == Status.Maintaince)
            {
                CarDetails.minDate = dateNow.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                CarDetails.maxDate = dateNow.AddYears(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else
            {
                CarDetails.minDate = dateNow.AddDays(-5).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                CarDetails.maxDate = dateNow.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return Json(CarDetails);
        }

        [HttpPost]
        public async Task<IActionResult> PostData(string SerialNo, string statusId_old, string reasons, string DateRepair)
        {
            var user = await _userManager.GetUserAsync(User);

            var car = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor && x.CrCasCarInformationSerailNo == SerialNo.Trim() &&
                                                                   x.CrCasCarInformationBranchStatus == Status.Active && x.CrCasCarInformationOwnerStatus == Status.Active
                                                                   && (x.CrCasCarInformationStatus == Status.Active || x.CrCasCarInformationStatus == Status.Maintaince)
                                                                   );

            var thisCar = car;

            if (user == null || car == null || DateRepair == "undefiend" || DateRepair == null || DateRepair == "") return Json(false);
            var DateTimeRepair = Convert.ToDateTime(DateRepair);
            var DateNow = DateTime.Now;
            string ar = " ";
            string en = " ";
            string statusIdNew = " ";

            if (statusId_old == Status.Active)
            {
                statusIdNew = Status.Maintaince;
                if (DateTimeRepair > DateNow.AddDays(1) || DateTimeRepair < DateNow.AddDays(-5)) return Json(false);
                ar = "نقل للصيانة";
                en = "Go to Repair";
            }
            else if (statusId_old == Status.Maintaince)
            {
                statusIdNew = Status.Active;
                if (DateTimeRepair < DateNow || DateTimeRepair > DateNow.AddYears(1)) return Json(false);
                ar = "عودة من الصيانة";
                en = "Retrive from Repair";
            }

            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return Json(false);
                    return RedirectToAction("Index", "CarRepair");
                }

                var oldStatus = car.CrCasCarInformationStatus ?? " ";
                car.CrCasCarInformationStatus = statusIdNew;

                _unitOfWork.CrCasCarInformation.Update(car);
                await _unitOfWork.CompleteAsync();
                //if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, car, Status.Update);
                var Target = SerialNo ?? " ";
                var Car_From = oldStatus ?? " ";
                var Car_To = statusIdNew ?? " ";

                await SaveAdminstrative_with_Set_Date(user, statusIdNew, Target, reasons, Car_From, Car_To, DateRepair, ar, en);
                return Json(true);
                return RedirectToAction("Index", "CarRepair");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                await SetPageTitleAsync(Status.Update, pageNumber);
                return Json(false);
                return RedirectToAction("Index", "CarRepair");
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

        private async Task SaveAdminstrative_with_Set_Date(CrMasUserInformation user, string status, string? Target, string? reasons, string Car_From, string Car_To, string repairDate, string ar, string en)
        {
            DateTime SetedDate = Convert.ToDateTime(repairDate);
            // change ProcedureCode and Classification (30 == Financial) , (20 == Admin)
            string procedure_Code = "214";
            string procedure_Classification = "20";
            //var procedureData = await _unitOfWork.CrMasSysProcedure.FindAsync(x=>x.CrMasSysProceduresCode== procedure_Code && x.CrMasSysProceduresClassification== procedure_Classification);
            //var recordAr = procedureData?.CrMasSysProceduresArName??" ";
            //var recordEn = procedureData?.CrMasSysProceduresEnName ?? " ";
            //if (recordAr.Length > 29) recordAr = recordAr?.Substring(0, 30);
            //if (recordEn.Length > 29) recordEn = recordEn?.Substring(0, 30);
            //var recordAr = "تحديث بيانات اصلاح سيارة";
            //var recordEn = "Car Repire Data UpDate";
            var recordAr = ar;
            var recordEn = en;

            string sector = "1";
            var lessorCode = user?.CrMasUserInformationLessor ?? " ";
            var branchCode = "100";
            var target = Target;
            var UserInsert = user?.CrMasUserInformationCode ?? " ";

            string? CarFrom = Car_From ?? " ";
            string? CarTo = Car_To ?? " ";
            string? Reasons = reasons;
            if (Reasons?.Length > 99) Reasons = Reasons?.Substring(0, 100);
            if (CarFrom?.Length > 19) CarFrom = CarFrom?.Substring(0, 20);
            if (CarTo?.Length > 19) CarTo = CarTo?.Substring(0, 20);
            decimal? Debit = null;
            decimal? Credit = null;
            string? Doc_No = null;
            DateTime? Doc_Date = null;
            DateTime? Doc_Start = null;
            DateTime? Doc_End = null;


            await _userLoginsService.SaveAdminstrative_with_Set_Date(
            SetedDate,
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
            Debit, Credit, Doc_No, Doc_Date, Doc_Start, Doc_End
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
            return RedirectToAction("Index", "CarRepair");
        }

    }
}


