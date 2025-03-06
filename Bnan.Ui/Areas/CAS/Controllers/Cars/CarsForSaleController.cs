using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS.Cars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.CAS.Controllers.Cars
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CarsForSaleController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly ICarInformation _carInformation;
        private readonly IStringLocalizer<CarsInformationController> _localizer;
        private readonly IToastNotification _toastNotification;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly IDocumentsMaintainanceCar _documentsMaintainanceCar;
        private readonly IBaseRepo _baseRepo;
        private readonly string pageNumber = SubTasks.CarForSale_CAS;

        public CarsForSaleController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, ICarInformation carInformation, IUserLoginsService userLoginsService, IToastNotification toastNotification, IAdminstritiveProcedures adminstritiveProcedures, IStringLocalizer<CarsInformationController> localizer, IDocumentsMaintainanceCar documentsMaintainanceCar, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _carInformation = carInformation;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _adminstritiveProcedures = adminstritiveProcedures;
            _localizer = localizer;
            _documentsMaintainanceCar = documentsMaintainanceCar;
            _baseRepo = baseRepo;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            var cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor &&
                                                        x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Hold &&
                                                        x.CrCasCarInformationStatus != Status.Sold &&
                                                        x.CrCasCarInformationForSaleStatus == Status.Active &&
                                                        x.CrCasCarInformationBranchStatus == Status.Active &&
                                                        x.CrCasCarInformationOwnerStatus == Status.Active,
                                                        new[] { "CrCasCarInformation1", "CrCasCarInformationDistributionNavigation" });
            if (!cars.Any())
            {
                cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == user.CrMasUserInformationLessor &&
                                                        x.CrCasCarInformationStatus != Status.Deleted && x.CrCasCarInformationStatus != Status.Hold &&
                                                        x.CrCasCarInformationStatus != Status.Sold &&
                                                        x.CrCasCarInformationBranchStatus == Status.Active &&
                                                        x.CrCasCarInformationOwnerStatus == Status.Active,
                                                        new[] { "CrCasCarInformation1", "CrCasCarInformationDistributionNavigation" });
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

            var carsVM = _mapper.Map<List<Cars_CarsSaleVM>>(cars);

            return View(carsVM);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetCarsByStatus(string status, string search)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var cars = await _unitOfWork.CrCasCarInformation.FindAllAsNoTrackingAsync(x => x.CrCasCarInformationLessor == userLogin.CrMasUserInformationLessor &&
                                                                    x.CrCasCarInformationStatus != Status.Deleted &&
                                                                    x.CrCasCarInformationStatus != Status.Hold &&
                                                                    x.CrCasCarInformationBranchStatus == Status.Active &&
                                                                    x.CrCasCarInformationOwnerStatus == Status.Active,
                                                                    new[] { "CrCasCarInformation1", "CrCasCarInformationDistributionNavigation" });
            var carsVM = _mapper.Map<List<Cars_CarsSaleVM>>(cars);

            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All) return PartialView("_CarsForSaleData", carsVM.Where(x => x.CrCasCarInformationStatus != Status.Sold &&
                                                                         (x.CrCasCarInformationDistributionNavigation.CrMasSupCarDistributionConcatenateArName.Contains(search) ||
                                                                          x.CrCasCarInformationDistributionNavigation.CrMasSupCarDistributionConcatenateEnName.Contains(search.ToLower())
                                                                          )));
                else if (status == Status.Sold) return PartialView("_CarsForSaleData", carsVM.Where(x => x.CrCasCarInformationStatus == Status.Sold &&
                                                                         (x.CrCasCarInformationDistributionNavigation.CrMasSupCarDistributionConcatenateArName.Contains(search) ||
                                                                          x.CrCasCarInformationDistributionNavigation.CrMasSupCarDistributionConcatenateEnName.Contains(search.ToLower())
                                                                          )));
                else return PartialView("_CarsForSaleData", carsVM.Where(x => x.CrCasCarInformationForSaleStatus == status));
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> OfferCarForSale(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarsForSale");
            }
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == id && x.CrCasCarInformationLessor == user.CrMasUserInformationLessor,
                new[] {"CrCasCarInformation1", "CrCasCarInformationDistributionNavigation",
                       "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation"});
            var carVM = _mapper.Map<Cars_CarsSaleVM>(car);
            return View(carVM);
        }

        [HttpPost]
        public async Task<IActionResult> OfferCarForSale(Cars_CarsSaleVM carsInforamtionVM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarsForSale");
            }
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x =>
                x.CrCasCarInformationSerailNo == carsInforamtionVM.CrCasCarInformationSerailNo &&
                x.CrCasCarInformationStatus != Status.Deleted &&
                x.CrCasCarInformationStatus != Status.Hold &&
                x.CrCasCarInformationStatus != Status.Sold &&
                x.CrCasCarInformationLessor == user.CrMasUserInformationLessor &&
                x.CrCasCarInformationBranchStatus == Status.Active &&
                x.CrCasCarInformationOwnerStatus == Status.Active);

            if (car == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }

            // ✅ تحقق من صحة البيانات المدخلة
            ModelState.Remove(nameof(carsInforamtionVM.CrCasCarInformationSoldDate));
            if (!ValidateCarSaleData(carsInforamtionVM))
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return View(carsInforamtionVM);
            }

            // ✅ تجهيز البيانات وتحديث حالة السيارة
            carsInforamtionVM.CrCasCarInformationLessor = user.CrMasUserInformationLessor;
            var carModel = _mapper.Map<CrCasCarInformation>(carsInforamtionVM);
            string status = await _carInformation.UpdateCarToSale(carModel);

            if (!string.IsNullOrEmpty(status) && await _unitOfWork.CompleteAsync() > 0)
            {
                await SaveTracingForCarForSaleChange(user, car.CrCasCarInformationConcatenateArName, car.CrCasCarInformationConcatenateEnName, status);
                await SaveAdminstritiveForCarForSale(user, "217", "20", car.CrCasCarInformationSerailNo, carsInforamtionVM.CrCasCarInformationReasons, status);

                _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index");
            }

            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return View(carsInforamtionVM);
        }

        private bool ValidateCarSaleData(Cars_CarsSaleVM model)
        {
            bool isValid = true;

            // ✅ التحقق من تاريخ العرض
            if (model.CrCasCarInformationOfferedSaleDate < DateTime.Today)
            {
                ModelState.AddModelError("CrCasCarInformationOfferedSaleDate", _localizer["PleaseEnterCorrectFormatDate"]);
                isValid = false;
            }

            // ✅ التحقق من قيمة البيع
            if (!decimal.TryParse(model.OfferValueSaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                ModelState.AddModelError("OfferValueSaleString", _localizer["requiredNoLengthFiled1_2"]);
                isValid = false;
            }
            else if (result >= 99999999999.99m)
            {
                ModelState.AddModelError("OfferValueSaleString", _localizer["requiredNoLengthFiled1_2"]);
                isValid = false;
            }
            else
            {
                model.CrCasCarInformationOfferValueSale = result;
            }

            return isValid;
        }
        [HttpGet]
        public async Task<IActionResult> SoldCar(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "CarsForSale");
            }
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x =>
                x.CrCasCarInformationSerailNo == id &&
                x.CrCasCarInformationStatus != Status.Deleted &&
                x.CrCasCarInformationStatus != Status.Hold &&
                x.CrCasCarInformationStatus != Status.Sold &&
                x.CrCasCarInformationForSaleStatus != Status.Active &&
                x.CrCasCarInformationLessor == user.CrMasUserInformationLessor &&
                x.CrCasCarInformationBranchStatus == Status.Active &&
                x.CrCasCarInformationOwnerStatus == Status.Active,
                new[] {"CrCasCarInformation1", "CrCasCarInformationDistributionNavigation",
                       "CrCasCarDocumentsMaintenances.CrCasCarDocumentsMaintenanceProceduresNavigation"});
            if (car == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            var carVM = _mapper.Map<Cars_CarsSaleVM>(car);
            return View(carVM);
        }
        [HttpPost]
        public async Task<IActionResult> SoldCar(string code, string status, string reasons)
        {
            string sAr = "";
            string sEn = "";
            var userLogin = await _userManager.GetUserAsync(User);
            var car = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationSerailNo == code && x.CrCasCarInformationLessor == userLogin.CrMasUserInformationLessor);
            if (car != null)
            {
                if (status == Status.Save)
                {
                    sAr = "الغاء البيع";
                    sEn = "Cancel offer";
                    car.CrCasCarInformationForSaleStatus = Status.Active;
                    car.CrCasCarInformationOfferValueSale = 0;
                    car.CrCasCarInformationOfferedSaleDate = null;
                    car.CrCasCarInformationSoldDate = null;
                    car.CrCasCarInformationReasons = reasons;
                }
                if (status == Status.Hold)
                {
                    sAr = "تنفيذ البيع";
                    sEn = "Confirm Offer";
                    car.CrCasCarInformationStatus = Status.Sold;
                    car.CrCasCarInformationSoldDate = DateTime.Now.Date;
                    car.CrCasCarInformationReasons = reasons;
                }
                await _unitOfWork.CompleteAsync();
                // SaveTracing
                var (mainTask, subTask, system, currentUserr) = await SetTrace("202", "2202008", "2");
                await _userLoginsService.SaveTracing(currentUserr.CrMasUserInformationCode, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                // Save Adminstrive Procedures
                var proCode = "";
                if (status == Status.Save) proCode = "217";
                else proCode = "218";
                await _adminstritiveProcedures.SaveAdminstritive(currentUserr.CrMasUserInformationCode, "1", proCode, "20", currentUserr.CrMasUserInformationLessor, "100",
                    car.CrCasCarInformationSerailNo, null, null, null, null, null, null, null, null, sAr, sEn, "U", null);
                return RedirectToAction("Index");
            }
            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index");
        }


        private async Task SaveTracingForCarForSaleChange(CrMasUserInformation user, string arDistribution, string enDistribution, string status)
        {
            var (operationAr, operationEn) = GetStatusTranslation(status);

            var (mainTask, subTask, system, currentUser) = await SetTrace(pageNumber);

            await _userLoginsService.SaveTracing(
                currentUser.CrMasUserInformationCode,
                arDistribution,
                enDistribution,
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
        private async Task SaveAdminstritiveForCarForSale(CrMasUserInformation user, string procudureCode, string classification, string serialNo, string reasons, string status)
        {
            var (operationAr, operationEn) = GetStatusTranslation(status);
            await _adminstritiveProcedures.SaveAdminstritive(user.CrMasUserInformationCode, "1", procudureCode, classification, user.CrMasUserInformationLessor, "100",
            serialNo, null, null, null, null, null, null, null, null, operationAr, operationEn, status, reasons);
        }
        public IActionResult SuccesssMessageForCarsInformation()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index");
        }
    }
}
