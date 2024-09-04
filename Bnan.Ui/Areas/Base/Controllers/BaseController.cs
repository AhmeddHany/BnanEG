using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.MAS;
using Bnan.Ui.ViewModels.Owners;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;

namespace Bnan.Ui.Areas.Base.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager<CrMasUserInformation> _userManager;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;

        public BaseController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<string[]> setTitle(string mainTaskCode, string subTaskCode, string systemCode)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            string MainTaskName;
            string SubTaskName;
            string SystemName;
            string userName;

            string currentCulture = CultureInfo.CurrentCulture.Name;
            if (currentCulture == "en-US")
            {
                MainTaskName = _unitOfWork.CrMasSysMainTasks.GetById(mainTaskCode).CrMasSysMainTasksEnName;
                SubTaskName = _unitOfWork.CrMasSysSubTasks.GetById(subTaskCode).CrMasSysSubTasksEnName;
                SystemName = _unitOfWork.CrMasSysSystems.GetById(systemCode).CrMasSysSystemEnName;
                userName = currentUser.CrMasUserInformationEnName;
            }
            else
            {
                MainTaskName = _unitOfWork.CrMasSysMainTasks.GetById(mainTaskCode).CrMasSysMainTasksArName;
                SubTaskName = _unitOfWork.CrMasSysSubTasks.GetById(subTaskCode).CrMasSysSubTasksArName;
                SystemName = _unitOfWork.CrMasSysSystems.GetById(systemCode).CrMasSysSystemArName;
                userName = currentUser.CrMasUserInformationArName;

            }

            string[] titles = { SystemName, MainTaskName, SubTaskName, userName };
            return titles;
        }

        public async Task<(CrMasSysMainTask, CrMasSysSubTask, CrMasSysSystem, CrMasUserInformation)> SetTrace(string mainTaskCode, string subTaskCode, string systemCode)
        {
            var mainTask = _unitOfWork.CrMasSysMainTasks.GetById(mainTaskCode);
            var subTask = _unitOfWork.CrMasSysSubTasks.GetById(subTaskCode);
            var system = _unitOfWork.CrMasSysSystems.GetById(systemCode);
            var currentUser = await _userManager.GetUserAsync(User);
            return (mainTask, subTask, system, currentUser);
        }

        //Check The Sub Validation of User With Default ActionResult(Index, Home, MAS)
        public async Task<bool> CheckUserSubValidation(string subTaskCode)
        {
            var usersubvalidation = await _userManager.Users.Include(l => l.CrMasUserSubValidations).Include(l => l.CrMasUserMainValidations).FirstOrDefaultAsync(l => l.UserName == User.Identity.Name);
            if (usersubvalidation?.CrMasUserSubValidations == null
                || usersubvalidation.CrMasUserSubValidations.Count == 0
                || usersubvalidation.CrMasUserSubValidations.FirstOrDefault(l => l.CrMasUserSubValidationSubTasks == subTaskCode)?.CrMasUserSubValidationAuthorization == false)
            {
                return false;

            }

            return true;

        }

        //Check The Sub Validation of User With Action Result
        [HttpGet]
        public async Task<bool> CheckUserSubValidationProcdures(string subTaskCodeProcdure, string status)
        {
            var usersubvalidation = await _userManager.Users.Include(l => l.CrMasUserSubValidations)
                                                            .Include(l => l.CrMasUserMainValidations)
                                                            .Include(l => l.CrMasUserProceduresValidations)
                                                            .FirstOrDefaultAsync(l => l.UserName == User.Identity.Name);

            if (usersubvalidation?.CrMasUserProceduresValidations != null || usersubvalidation?.CrMasUserProceduresValidations.Count != 0)
            {
                if (status == Status.Insert && usersubvalidation?.CrMasUserProceduresValidations
                                    .FirstOrDefault(l => l.CrMasUserProceduresValidationSubTasks == subTaskCodeProcdure)?
                                    .CrMasUserProceduresValidationInsertAuthorization == false)
                {
                    return false;
                }
                else if (status == Status.Deleted && usersubvalidation?.CrMasUserProceduresValidations
                                  .FirstOrDefault(l => l.CrMasUserProceduresValidationSubTasks == subTaskCodeProcdure)?
                                  .CrMasUserProceduresValidationDeleteAuthorization == false)
                {
                    return false;
                }
                else if (status == Status.Hold && usersubvalidation?.CrMasUserProceduresValidations
                               .FirstOrDefault(l => l.CrMasUserProceduresValidationSubTasks == subTaskCodeProcdure)?
                               .CrMasUserProceduresValidationHoldAuthorization == false)
                {
                    return false;
                }
                else if (status == Status.Save && usersubvalidation?.CrMasUserProceduresValidations
                             .FirstOrDefault(l => l.CrMasUserProceduresValidationSubTasks == subTaskCodeProcdure)?
                             .CrMasUserProceduresValidationUpDateAuthorization == false)
                {
                    return false;
                }
                else if (status == Status.UnDeleted && usersubvalidation?.CrMasUserProceduresValidations
                          .FirstOrDefault(l => l.CrMasUserProceduresValidationSubTasks == subTaskCodeProcdure)?
                          .CrMasUserProceduresValidationUnDeleteAuthorization == false)
                {
                    return false;
                }
                else if (status == Status.UnHold && usersubvalidation?.CrMasUserProceduresValidations
                        .FirstOrDefault(l => l.CrMasUserProceduresValidationSubTasks == subTaskCodeProcdure)?
                        .CrMasUserProceduresValidationUnHoldAuthorization == false)
                {
                    return false;
                }


            }

            return true;
        }

        [HttpGet]
        public async Task<bool> CheckAuth(string branchCode, string salespoint,string Balance,string CarsCount,string status)
        {
            return false;
        }


        public async Task<BSLayoutVM> GetBranchesAndLayout()
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var userInformation = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode == userLogin.CrMasUserInformationCode, new[] { "CrMasUserBranchValidities.CrMasUserBranchValidity1" });
            var branchesValidite = userInformation.CrMasUserBranchValidities.Where(x => x.CrMasUserBranchValidityBranchStatus == Status.Active);

            List<CrCasBranchInformation> branches = branchesValidite != null
                ? branchesValidite.Select(item => item.CrMasUserBranchValidity1).ToList()
                : new List<CrCasBranchInformation>();

            var selectBranch = userLogin.CrMasUserInformationDefaultBranch;
            if (selectBranch == null || selectBranch == "000") selectBranch = "100";
            var checkBranch = branches.Find(x => x.CrCasBranchInformationCode == selectBranch);
            if (checkBranch == null) selectBranch = branches.FirstOrDefault()?.CrCasBranchInformationCode;

            var branch = _unitOfWork.CrCasBranchInformation.Find(x => x.CrCasBranchInformationCode == selectBranch && x.CrCasBranchInformationLessor == lessorCode, new[] { "CrCasBranchPost", "CrCasBranchPost.CrCasBranchPostCityNavigation", "CrCasBranchInformationLessorNavigation" });



            var Documents = _unitOfWork.CrCasBranchDocument.FindAll(x => x.CrCasBranchDocumentsLessor == lessorCode && x.CrCasBranchDocumentsBranch == branch.CrCasBranchInformationCode).ToList();
            var DocumentsCar = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode && x.CrCasCarDocumentsMaintenanceBranch == branch.CrCasBranchInformationCode && x.CrCasCarDocumentsMaintenanceProceduresClassification == "12").ToList();
            var MaintainceCar = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode && x.CrCasCarDocumentsMaintenanceBranch == branch.CrCasBranchInformationCode && x.CrCasCarDocumentsMaintenanceProceduresClassification == "13").ToList();
            var PriceCar = _unitOfWork.CrCasPriceCarBasic.FindAll(x => x.CrCasPriceCarBasicLessorCode == lessorCode).ToList();
            var c = Documents.Where(x => x.CrCasBranchDocumentsStatus == Status.AboutToExpire).Count();
            var BsLayoutVM = new BSLayoutVM();
            BsLayoutVM.CrCasBranchInformations = branches;
            BsLayoutVM.SelectedBranch = selectBranch;
            BsLayoutVM.CrCasBranchInformation = branch;
            BsLayoutVM.UserInformation = userInformation;
            BsLayoutVM.Alerts = BsLayoutVM.Alerts ?? new AlertsVM(); // Ensure AlertsVM is initialized
            BsLayoutVM.Alerts.BranchDocumentsAboutExpire = Documents.Where(x => x.CrCasBranchDocumentsStatus == Status.AboutToExpire).Count();
            BsLayoutVM.Alerts.BranchDocumentsExpireAndRenewed = Documents.Where(x => x.CrCasBranchDocumentsStatus == Status.Expire || x.CrCasBranchDocumentsStatus == Status.Renewed).Count();
            BsLayoutVM.Alerts.DocumentsCarsAboutExpire = DocumentsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire).Count();
            BsLayoutVM.Alerts.DocumentsCarExpiredAndRenewed = DocumentsCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.Expire || x.CrCasCarDocumentsMaintenanceStatus == Status.Renewed).Count();
            BsLayoutVM.Alerts.MaintainceCarAboutExpire = MaintainceCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire).Count();
            BsLayoutVM.Alerts.MaintainceCarExpireAndRenewed = MaintainceCar.Where(x => x.CrCasCarDocumentsMaintenanceStatus == Status.Expire || x.CrCasCarDocumentsMaintenanceStatus == Status.Renewed).Count();
            BsLayoutVM.Alerts.PriceCarAboutExpire = PriceCar.Where(x => x.CrCasPriceCarBasicStatus == Status.AboutToExpire).Count();
            BsLayoutVM.Alerts.PriceCarExpireAndRenewed = PriceCar.Where(x => x.CrCasPriceCarBasicStatus == Status.Expire || x.CrCasPriceCarBasicStatus == Status.Renewed).Count();
            var TotalCount = BsLayoutVM.Alerts.BranchDocumentsAboutExpire + BsLayoutVM.Alerts.BranchDocumentsExpireAndRenewed + BsLayoutVM.Alerts.DocumentsCarsAboutExpire + BsLayoutVM.Alerts.DocumentsCarExpiredAndRenewed
                             + BsLayoutVM.Alerts.MaintainceCarAboutExpire + BsLayoutVM.Alerts.MaintainceCarExpireAndRenewed + BsLayoutVM.Alerts.PriceCarAboutExpire + BsLayoutVM.Alerts.PriceCarExpireAndRenewed;
            BsLayoutVM.Alerts.AlertOrNot = TotalCount;
            return BsLayoutVM;
        }


        public string GetNextAccountReceiptNo(string LessorCode, string BranchCode, string procedure)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == LessorCode &&
                                                                       x.CrCasAccountReceiptYear == y && x.CrCasAccountReceiptBranchCode == BranchCode && x.CrCasAccountReceiptType == procedure)
                                                             .Max(x => x.CrCasAccountReceiptNo.Substring(x.CrCasAccountReceiptNo.Length - 6, 6));

            string Serial;
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                Serial = val.ToString("000000");
            }
            else
            {
                Serial = "000001";
            }
            var AccountReceiptNo = y + "-" + "1" + procedure + "-" + LessorCode + BranchCode + "-" + Serial;
            return AccountReceiptNo;
        }
        public string GetNextAdministrativeNo(string LessorCode, string BranchCode, string procedure)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == LessorCode &&
                x.CrCasSysAdministrativeProceduresCode == procedure
                && x.CrCasSysAdministrativeProceduresSector == "1"
                && x.CrCasSysAdministrativeProceduresYear == y).Max(x => x.CrCasSysAdministrativeProceduresNo.Substring(x.CrCasSysAdministrativeProceduresNo.Length - 6, 6));
            string Serial;
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                Serial = val.ToString("000000");
            }
            else
            {
                Serial = "000001";
            }
            var AdministritiveNo = y + "-" + "1" + procedure + "-" + LessorCode + "100" + "-" + Serial;
            return AdministritiveNo;
        }





        ///OwnersSystem
        public OwnersLayoutVM OwnersDashboadInfo(string lessorCode)
        {
            OwnersLayoutVM ownersLayoutVM = new OwnersLayoutVM();
            var contracts = _unitOfWork.CrCasRenterContractBasic.FindAll(x => x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicStatus != Status.Extension);
            ownersLayoutVM.OwnContracts = _mapper.Map<List<OwnContractsVM>>(contracts);
            ownersLayoutVM.RateContractsMonthBefore = UpRateBeforeMonthForContracts(ownersLayoutVM.OwnContracts);

            var cars = _unitOfWork.CrCasCarInformation.FindAll(x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationStatus != Status.Sold);
            ownersLayoutVM.OwnCars = _mapper.Map<List<OwnCarsInfoVM>>(cars);
            ownersLayoutVM.RateCarsMonthBefore = UpRateBeforeMonthForCars(ownersLayoutVM.OwnCars);

            var renters = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == lessorCode, new[] { "CrCasRenterContractBasicCrCasRenterContractBasic5s" });
            ownersLayoutVM.OwnRenters = _mapper.Map<List<OwnRentersVM>>(renters);
            ownersLayoutVM.RateRentersMonthBefore = UpRateBeforeMonthForRenters(ownersLayoutVM.OwnRenters);

            var balance = _unitOfWork.CrMasUserBranchValidity.FindAll(x => x.CrMasUserBranchValidityLessor == lessorCode);
            var balanceTotal = balance.Sum(x => x.CrMasUserBranchValidityBranchCashBalance) + balance.Sum(x => x.CrMasUserBranchValidityBranchSalesPointBalance) + balance.Sum(x => x.CrMasUserBranchValidityBranchTransferBalance);
            var avaliableBalance = balance.Sum(x => x.CrMasUserBranchValidityBranchCashAvailable) + balance.Sum(x => x.CrMasUserBranchValidityBranchSalesPointAvailable) + balance.Sum(x => x.CrMasUserBranchValidityBranchTransferAvailable);
            var resevedBalance = balance.Sum(x => x.CrMasUserBranchValidityBranchCashReserved) + balance.Sum(x => x.CrMasUserBranchValidityBranchSalesPointReserved) + balance.Sum(x => x.CrMasUserBranchValidityBranchTransferReserved);
            ownersLayoutVM.TotalBalance = balanceTotal;
            ownersLayoutVM.AvaliableBalance = avaliableBalance;
            ownersLayoutVM.ReservedBalance = resevedBalance;
            ownersLayoutVM.BranchDocuments = _unitOfWork.CrCasBranchDocument.FindAll(x => x.CrCasBranchDocumentsLessor == lessorCode).ToList();
            ownersLayoutVM.CarDocumentsAndMaintainces = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode).ToList();
            return ownersLayoutVM;
        }
        private double UpRateBeforeMonthForContracts(List<OwnContractsVM> contracts)
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var beforeMonth = currentMonth - 1;
            double contractBeforeMonth = contracts.FindAll(x => x.CrCasRenterContractBasicIssuedDate.Value.Month == beforeMonth).Count();
            double contractCurrentMonth = contracts.FindAll(x => x.CrCasRenterContractBasicIssuedDate.Value.Month == currentMonth).Count();
            if (contractBeforeMonth == 0 || contractCurrentMonth == 0) return 0;
            double rate = contractCurrentMonth / contractBeforeMonth;
            return rate;
        }
        private double UpRateBeforeMonthForCars(List<OwnCarsInfoVM> cars)
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var beforeMonth = currentMonth - 1;
            double carsBeforeMonth = cars.FindAll(x => x.CrCasCarInformationJoinedFleetDate.Value.Month == beforeMonth).Count();
            double carsCurrentMonth = cars.FindAll(x => x.CrCasCarInformationJoinedFleetDate.Value.Month == currentMonth).Count();
            if (carsBeforeMonth == 0 || carsCurrentMonth == 0) return 0;
            double rate = carsCurrentMonth / carsBeforeMonth;
            return rate;
        }
        private double UpRateBeforeMonthForRenters(List<OwnRentersVM> renters)
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var beforeMonth = currentMonth - 1;
            double rentersBeforeMonth = renters.FindAll(x => x.CrCasRenterLessorDateFirstInteraction.Value.Month == beforeMonth).Count();
            double rentersCurrentMonth = renters.FindAll(x => x.CrCasRenterLessorDateFirstInteraction.Value.Month == currentMonth).Count();
            if (rentersBeforeMonth == 0 || rentersCurrentMonth == 0) return 0;
            double rate = rentersCurrentMonth / rentersBeforeMonth;
            return rate;
        }
    }
}
