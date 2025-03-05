using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.CAS.Controllers.Cars
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class CarPriceController : BaseController
    {
        private readonly IStringLocalizer<CarsInformationController> _localizer;
        private readonly IToastNotification _toastNotification;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly ICarPrice _carPrice;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.CarPrice_CAS;

        public CarPriceController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, ICarPrice carPrice, IAdminstritiveProcedures adminstritiveProcedures, IToastNotification toastNotification, IStringLocalizer<CarsInformationController> localizer, IUserLoginsService userLoginsService, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _carPrice = carPrice;
            _adminstritiveProcedures = adminstritiveProcedures;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _userLoginsService = userLoginsService;
            _baseRepo = baseRepo;
        }

        public async Task<IActionResult> CarPrice()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            var carPrices = (await _unitOfWork.CrCasPriceCarBasic.FindAllAsNoTrackingAsync(x => x.CrCasPriceCarBasicLessorCode == user.CrMasUserInformationLessor && x.CrCasPriceCarBasicStatus == Status.Active,
                                                                        new[] { "CrCasPriceCarBasicLessorCodeNavigation", "CrCasPriceCarBasicDistributionCodeNavigation.CrCasCarInformations" })).DistinctBy(x => x.CrCasPriceCarBasicDistributionCodeNavigation);
            if (!carPrices.Any())
            {
                carPrices = (await _unitOfWork.CrCasPriceCarBasic.FindAllAsNoTrackingAsync(x => x.CrCasPriceCarBasicLessorCode == user.CrMasUserInformationLessor && x.CrCasPriceCarBasicStatus == Status.Expire,
                                                                        new[] { "CrCasPriceCarBasicLessorCodeNavigation", "CrCasPriceCarBasicDistributionCodeNavigation.CrCasCarInformations" })).DistinctBy(x => x.CrCasPriceCarBasicDistributionCodeNavigation);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";

            return View(carPrices);
        }
        [HttpGet]
        public async Task<PartialViewResult> CarPriceByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(status))
            {
                var carPrices = (await _unitOfWork.CrCasPriceCarBasic.FindAllAsNoTrackingAsync(l => l.CrCasPriceCarBasicLessorCode == user.CrMasUserInformationLessor,
                                                                                                              new[] { "CrCasPriceCarBasicLessorCodeNavigation", "CrCasPriceCarBasicDistributionCodeNavigation.CrCasCarInformations" })).DistinctBy(x => x.CrCasPriceCarBasicDistributionCode);
                if (status == Status.All)
                {
                    var carPricesStatusAll = carPrices.Where(x => x.CrCasPriceCarBasicStatus != Status.Deleted &&
                                                                         (x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateArName.Contains(search) ||
                                                                          x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateEnName.Contains(search.ToLower())
                                                                          ));
                    return PartialView("_DataTableCarsPrice", carPricesStatusAll);
                }
                var carPricesbyStatus = carPrices.Where(x => x.CrCasPriceCarBasicStatus == status &&
                                                                         (x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateArName.Contains(search) ||
                                                                          x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateEnName.Contains(search.ToLower())));
                return PartialView("_DataTableCarsPrice", carPricesbyStatus);
            }
            return PartialView();
        }

        public async Task<IActionResult> AddPriceCar()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("CarPrice", "CarPrice");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("CarPrice", "CarPrice");
            }
            bool isEnglish = CultureInfo.CurrentCulture.Name == "en-US";
            var carsInformation = await _unitOfWork.CrMasSupCarDistribution
                .GetAllAsyncAsNoTrackingAsync();
            var excludedCodes = (await _unitOfWork.CrCasPriceCarBasic.GetAllAsyncAsNoTrackingAsync()).Where(x => x.CrCasPriceCarBasicStatus == Status.Active &&
                                                                                                            x.CrCasPriceCarBasicLessorCode == user.CrMasUserInformationLessor)
                                .Select(x => x.CrCasPriceCarBasicDistributionCode).ToHashSet();
            var formattedCars = carsInformation
                .Where(c => !excludedCodes.Contains(c.CrMasSupCarDistributionCode))
                .Select(c => new
                {
                    text = isEnglish ? c.CrMasSupCarDistributionConcatenateEnName : c.CrMasSupCarDistributionConcatenateArName,
                    value = c.CrMasSupCarDistributionCode,
                    count = c.CrMasSupCarDistributionCount,
                    image = c.CrMasSupCarDistributionImage?.TrimStart('~')
                }).ToList();

            ViewBag.Options = await _unitOfWork.CrMasSupContractOption.FindAllAsNoTrackingAsync(x => x.CrMasSupContractOptionsStatus == Status.Active);
            ViewBag.Additional = await _unitOfWork.CrMasSupContractAdditional.FindAllAsNoTrackingAsync(x => x.CrMasSupContractAdditionalStatus == Status.Active);
            ViewBag.CarsList = formattedCars; // تحويل JsonResult إلى List
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddPriceCar(CarPriceVM carPriceVM, bool CrCasPriceCarBasicIsAdditionalDriver, List<string> Additionals, List<string> Choises)
        {
            try
            {
                var userLogin = await _userManager.GetUserAsync(User);
                if (userLogin == null)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Login", "Account");
                }
                // Check Validition
                if (!await _baseRepo.CheckValidation(userLogin.CrMasUserInformationCode, pageNumber, Status.Insert))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    return RedirectToAction("CarPrice", "CarPrice");
                }

                var lessorCode = userLogin.CrMasUserInformationLessor;
                var distribution = await _unitOfWork.CrMasSupCarDistribution.FindAsync(x => x.CrMasSupCarDistributionCode == carPriceVM.CrCasPriceCarBasicDistributionCode);
                if (distribution == null)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    ModelState.AddModelError("CrCasPriceCarBasicDistributionCode", _localizer["RessureFromNameCar"]);
                    return View(carPriceVM);
                }
                var carPrice = (await _unitOfWork.CrCasPriceCarBasic.FindAllAsNoTrackingAsync(x => x.CrCasPriceCarBasicDistributionCode == distribution.CrMasSupCarDistributionCode && x.CrCasPriceCarBasicStatus == Status.Active && x.CrCasPriceCarBasicLessorCode == lessorCode)).Count();
                if (carPrice > 0)
                {
                    ModelState.AddModelError("CrCasPriceCarBasicDistributionCode", _localizer["IsExists"]);
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View(carPriceVM);
                }

                if (!TryParseCarPriceValues(carPriceVM))
                {
                    _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return View(carPriceVM);
                }

                if (ModelState.IsValid)
                {
                    carPriceVM.CrCasPriceCarBasicIsAdditionalDriver = CrCasPriceCarBasicIsAdditionalDriver;
                    carPriceVM.CrCasPriceCarBasicDistributionCode = distribution.CrMasSupCarDistributionCode;
                    carPriceVM.CrCasPriceCarBasicLessorCode = lessorCode;

                    var CarPriceModel = _mapper.Map<CrCasPriceCarBasic>(carPriceVM);
                    var PriceCarNo = await _carPrice.AddPriceCar(CarPriceModel);
                    if (!string.IsNullOrEmpty(PriceCarNo))
                    {
                        await AddAdditionalAndChoices(PriceCarNo, Additionals, Choises);
                        await UpdateCarInformation(CarPriceModel.CrCasPriceCarBasicDistributionCode, lessorCode, PriceCarNo, true);
                    }

                    if (await _unitOfWork.CompleteAsync() > 0)
                    {
                        await SaveTracingForCarPriceChange(userLogin, distribution.CrMasSupCarDistributionConcatenateArName, distribution.CrMasSupCarDistributionConcatenateEnName, Status.Insert);
                        await SaveAdminstritiveForCarPrice(userLogin, "219", "20", PriceCarNo, carPriceVM.CrCasPriceCarBasicReasons, Status.Insert);
                        _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                        return RedirectToAction("CarPrice");
                    }
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            }

            ViewBag.Options = _unitOfWork.CrMasSupContractOption.FindAll(x => x.CrMasSupContractOptionsStatus == Status.Active);
            ViewBag.Additional = _unitOfWork.CrMasSupContractAdditional.FindAll(x => x.CrMasSupContractAdditionalStatus == Status.Active);
            return View(carPriceVM);
        }

        private bool TryParseCarPriceValues(CarPriceVM carPriceVM)
        {
            bool TryAssignDecimal(string value, out decimal result) => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

            bool isValid = true;

            if (TryAssignDecimal(carPriceVM.DailyRent, out decimal dailyRent)) carPriceVM.CrCasPriceCarBasicDailyRent = dailyRent; else isValid = false;
            if (TryAssignDecimal(carPriceVM.WeeklyRent, out decimal weeklyRent)) carPriceVM.CrCasPriceCarBasicWeeklyRent = weeklyRent; else isValid = false;
            if (TryAssignDecimal(carPriceVM.MonthlyRent, out decimal monthlyRent)) carPriceVM.CrCasPriceCarBasicMonthlyRent = monthlyRent; else isValid = false;
            if (TryAssignDecimal(carPriceVM.YearlyRent, out decimal yearlyRent)) carPriceVM.CrCasPriceCarBasicYearlyRent = yearlyRent; else isValid = false;
            if (TryAssignDecimal(carPriceVM.AdditionalKmValue, out decimal additionalKmValue)) carPriceVM.CrCasPriceCarBasicAdditionalKmValue = additionalKmValue; else isValid = false;
            if (TryAssignDecimal(carPriceVM.AdditionalDriverValue, out decimal additionalDriverValue)) carPriceVM.CrCasPriceCarBasicAdditionalDriverValue = additionalDriverValue; else isValid = false;
            if (TryAssignDecimal(carPriceVM.PrivateDriverValue, out decimal privateDriverValue)) carPriceVM.CrCasPriceCarBasicPrivateDriverValue = privateDriverValue; else isValid = false;
            if (TryAssignDecimal(carPriceVM.InFeesTamm, out decimal inFeesTamm)) carPriceVM.CrCasCarPriceBasicInFeesTamm = inFeesTamm; else isValid = false;
            if (TryAssignDecimal(carPriceVM.OutFeesTamm, out decimal outFeesTamm)) carPriceVM.CrCasCarPriceBasicOutFeesTamm = outFeesTamm; else isValid = false;
            if (TryAssignDecimal(carPriceVM.ExtraHourValue, out decimal extraHourValue)) carPriceVM.CrCasPriceCarBasicExtraHourValue = extraHourValue; else isValid = false;
            if (TryAssignDecimal(carPriceVM.RentalTaxRate, out decimal rentalTaxRate)) carPriceVM.CrCasPriceCarBasicRentalTaxRate = rentalTaxRate; else isValid = false;
            if (TryAssignDecimal(carPriceVM.CompensationAccident, out decimal compensationAccident)) carPriceVM.CrCasPriceCarBasicCompensationAccident = compensationAccident; else isValid = false;
            if (TryAssignDecimal(carPriceVM.CompensationFire, out decimal compensationFire)) carPriceVM.CrCasPriceCarBasicCompensationFire = compensationFire; else isValid = false;
            if (TryAssignDecimal(carPriceVM.CompensationTheft, out decimal compensationTheft)) carPriceVM.CrCasPriceCarBasicCompensationTheft = compensationTheft; else isValid = false;
            if (TryAssignDecimal(carPriceVM.CompensationDrowning, out decimal compensationDrowning)) carPriceVM.CrCasPriceCarBasicCompensationDrowning = compensationDrowning; else isValid = false;

            return isValid;
        }
        private bool TryAssignDecimal(string value, out decimal property)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out property);
        }

        private async Task AddAdditionalAndChoices(string priceCarNo, List<string> additionals, List<string> choices)
        {
            bool TryParseDecimal(string value, out decimal result) => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

            if (additionals?.Any() == true)
            {
                var additionalStringData = additionals.SelectMany(item => JsonConvert.DeserializeObject<List<CarPriceAdditionalStringData>>(item) ?? new List<CarPriceAdditionalStringData>())
                    .Where(x => x.Checked && TryParseDecimal(x.Value, out _));

                foreach (var item in additionalStringData)
                    await _carPrice.AddAdditionals(priceCarNo, item.Id, item.Value);
            }

            if (choices?.Any() == true)
            {
                var choicesStringData = choices.SelectMany(item => JsonConvert.DeserializeObject<List<CarPriceChoisesStringData>>(item) ?? new List<CarPriceChoisesStringData>())
                    .Where(x => x.Checked && TryParseDecimal(x.Value, out _));

                foreach (var item in choicesStringData)
                    await _carPrice.AddChoises(priceCarNo, item.Id, item.Value);
            }
        }

        private async Task UpdateCarInformation(string distributionCode, string lessorCode, string priceCarNo, bool isChecked)
        {
            var cars = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationDistribution == distributionCode && x.CrCasCarInformationLessor == lessorCode);
            if (cars.Count() > 0)
            {
                foreach (var car in cars)
                {
                    car.CrCasCarInformationPriceStatus = isChecked;
                    if (isChecked) car.CrCasCarInformationPriceNo = priceCarNo;
                }
                _unitOfWork.CrCasCarInformation.UpdateRange(cars);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string priceNo)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(userLogin.CrMasUserInformationCode, pageNumber, Status.Update))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("CarPrice", "CarPrice");
            }
            var carPrice = _unitOfWork.CrCasPriceCarBasic.Find(x => x.CrCasPriceCarBasicNo == priceNo, new[] { "CrCasPriceCarBasicDistributionCodeNavigation" });
            if (carPrice == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("CarPrice", "CarPrice");
            }
            var carPriceVM = _mapper.Map<CarPriceVM>(carPrice);
            if (carPrice.CrCasPriceCarBasicIsAdditionalDriver == true) ViewBag.CrCasPriceCarBasicIsAdditionalDriver = _localizer["Yes"];
            else ViewBag.CrCasPriceCarBasicIsAdditionalDriver = _localizer["Noo"];
            ViewBag.Additional = await _unitOfWork.CrCasPriceCarAdditional.FindAllAsNoTrackingAsync(x => x.CrCasPriceCarAdditionalNo == priceNo, new[] { "CrCasPriceCarAdditionalCodeNavigation" });
            ViewBag.Options = await _unitOfWork.CrCasPriceCarOption.FindAllAsNoTrackingAsync(x => x.CrCasPriceCarOptionsNo == priceNo, new[] { "CrCasPriceCarOptionsCodeNavigation" });
            ViewBag.NoOfCars = (await _unitOfWork.CrCasCarInformation.FindAllAsNoTrackingAsync(x => x.CrCasCarInformationDistribution == carPrice.CrCasPriceCarBasicDistributionCode && x.CrCasCarInformationLessor == userLogin.CrMasUserInformationLessor)).Count();
            return View(carPriceVM);
        }
        [HttpPost]
        public async Task<string> Delete(string priceNo)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";
            var carPrice = await _unitOfWork.CrCasPriceCarBasic.FindAsync(x => x.CrCasPriceCarBasicNo == priceNo && x.CrCasPriceCarBasicStatus != Status.Deleted, new[] { "CrCasPriceCarBasicDistributionCodeNavigation" });
            if (carPrice == null) return "false";
            try
            {
                var status = Status.Deleted;
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                carPrice.CrCasPriceCarBasicStatus = status;
                _unitOfWork.CrCasPriceCarBasic.Update(carPrice);
                await UpdateCarInformation(carPrice.CrCasPriceCarBasicDistributionCode, user.CrMasUserInformationLessor, carPrice.CrCasPriceCarBasicNo, false);
                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    await SaveTracingForCarPriceChange(user, carPrice.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateArName, carPrice.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateEnName, Status.Deleted);
                    await SaveAdminstritiveForCarPrice(user, "219", "20", carPrice.CrCasPriceCarBasicNo, carPrice.CrCasPriceCarBasicReasons, Status.Deleted);

                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return "true";
                }
                return "false";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetNumberOfCar(string DistribtionCode)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var carsInformation = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationDistribution == DistribtionCode && x.CrCasCarInformationLessor == lessorCode, new[] { "CrCasCarAdvantages" });
            return Json(carsInformation.Count());
        }
        [HttpGet]
        public async Task<PartialViewResult> GetAdvantagesOfCar(string DistribtionCode)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var carInformation = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationDistribution == DistribtionCode && x.CrCasCarInformationLessor == lessorCode).FirstOrDefault();
            var AdvantagesCars = _unitOfWork.CrCasCarAdvantage.FindAll(x => x.CrCasCarAdvantagesSerialNo == carInformation.CrCasCarInformationSerailNo &&
                                                                            x.CrCasCarAdvantagesCategory == carInformation.CrCasCarInformationCategory &&
                                                                            x.CrCasCarAdvantagesModel == carInformation.CrCasCarInformationModel &&
                                                                            x.CrCasCarAdvantagesCarYear == carInformation.CrCasCarInformationYear &&
                                                                            x.CrCasCarAdvantagesBrand == carInformation.CrCasCarInformationBrand, new[] { "CrCasCarAdvantagesCodeNavigation" });

            return PartialView("_AdvantagesPartialView", AdvantagesCars.ToList());
        }
        private async Task SaveTracingForCarPriceChange(CrMasUserInformation user, string arDistribution, string enDistribution, string status)
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
        private async Task SaveAdminstritiveForCarPrice(CrMasUserInformation user, string procudureCode, string classification, string PriceCarNo, string reasons, string status)
        {
            var (operationAr, operationEn) = GetStatusTranslation(status);
            await _adminstritiveProcedures.SaveAdminstritive(user.CrMasUserInformationCode, "1", procudureCode, classification, user.CrMasUserInformationLessor, "100",
            PriceCarNo, null, null, null, null, null, null, null, null, operationAr, operationEn, status, reasons);
        }

        public IActionResult SuccesssMessageForCarPrice()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("CarPrice");
        }

    }
}
