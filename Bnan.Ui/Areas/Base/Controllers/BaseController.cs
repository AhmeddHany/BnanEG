using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.Owners;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        // Set Title
        public async Task<string[]> setTitle(string mainTaskCode, string subTaskCode, string systemCode)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            string currentCulture = CultureInfo.CurrentCulture.Name;

            // جلب البيانات دفعة واحدة
            var mainTask = _unitOfWork.CrMasSysMainTasks.GetById(mainTaskCode);
            var subTask = _unitOfWork.CrMasSysSubTasks.GetById(subTaskCode);
            var system = _unitOfWork.CrMasSysSystems.GetById(systemCode);

            string MainTaskName = currentCulture == "en-US" ? mainTask.CrMasSysMainTasksEnName : mainTask.CrMasSysMainTasksArName;
            string SubTaskName = currentCulture == "en-US" ? subTask.CrMasSysSubTasksEnName : subTask.CrMasSysSubTasksArName;
            string SystemName = currentCulture == "en-US" ? system.CrMasSysSystemEnName : system.CrMasSysSystemArName;
            string userName = currentCulture == "en-US" ? currentUser.CrMasUserInformationEnName : currentUser.CrMasUserInformationArName;

            return new string[] { SystemName, MainTaskName, SubTaskName, userName };
        }

        // Set Title
        public async Task<string[]> setTitle(string subTaskCode)
        {
            if (subTaskCode.Length == 7)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                string currentCulture = CultureInfo.CurrentCulture.Name;

                // جلب البيانات دفعة واحدة
                var subTask = await _unitOfWork.CrMasSysSubTasks.GetByIdAsync(subTaskCode);

                string SubTaskName = currentCulture == "en-US" ? subTask.CrMasSysSubTasksConcatenateEnName : subTask.CrMasSysSubTasksConcatenateArName;
                string userName = currentCulture == "en-US" ? currentUser.CrMasUserInformationEnName : currentUser.CrMasUserInformationArName;

                return new string[] { SubTaskName, userName };
            }
            return new string[] { "0", "0" };
        }

        public async Task SetPageTitleAsync(string status, string subTaskCode)
        {
            var (operationAr, operationEn) = GetStatusTranslation(status);
            var titles = await setTitle(subTaskCode);
            await ViewData.SetPageTitleAsync(titles[0], operationAr, operationEn, titles[1]);
        }
        protected (string Arabic, string English) GetStatusTranslation(string status)
        {
            if (status == Status.ViewInformation) return ("عرض بيانات", "View Information");
            else if (status == Status.Insert) return ("اضافة", "Insert");
            else if (status == Status.Update) return ("تحديث", "Update");
            else if (status == Status.Deleted) return ("حذف", "Remove");
            else if (status == Status.UnHold) return ("استرجاع الايقاف", "Retrieve Hold");
            else if (status == Status.UnDeleted) return ("استرجاع الحذف", "Retrieve Delete");
            else if (status == Status.ResetPassword) return ("استرجاع كلمة السر", "Retrieve Password");
            else if (status == Status.ChangePassword) return ("تغيير كلمة السر", "Change Password");
            else return ("", "");
        }


        public async Task<(CrMasSysMainTask, CrMasSysSubTask, CrMasSysSystem, CrMasUserInformation)> SetTrace(string mainTaskCode, string subTaskCode, string systemCode)
        {
            var mainTask = _unitOfWork.CrMasSysMainTasks.GetById(mainTaskCode);
            var subTask = _unitOfWork.CrMasSysSubTasks.GetById(subTaskCode);
            var system = _unitOfWork.CrMasSysSystems.GetById(systemCode);
            var currentUser = await _userManager.GetUserAsync(User);
            return (mainTask, subTask, system, currentUser);
        }

        public async Task<(CrMasSysMainTask, CrMasSysSubTask, CrMasSysSystem, CrMasUserInformation)> SetTrace(string subTaskCode)
        {
            if (subTaskCode.Length == 7)
            {
                string systemCode = subTaskCode.Substring(0, 1).Trim();
                string mainTaskCode = subTaskCode.Substring(1, 3).Trim();
                var mainTask = _unitOfWork.CrMasSysMainTasks.GetById(mainTaskCode);
                var subTask = _unitOfWork.CrMasSysSubTasks.GetById(subTaskCode);
                var system = _unitOfWork.CrMasSysSystems.GetById(systemCode);
                var currentUser = await _userManager.GetUserAsync(User);
                return (mainTask, subTask, system, currentUser);
            }
            return (null, null, null, null);
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

        public async Task<BSLayoutVM> GetBranchesAndLayout()
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var userInformation = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode == userLogin.CrMasUserInformationCode, new[] { "CrMasUserBranchValidities.CrMasUserBranchValidity1" });
            var branchesValidite = userInformation.CrMasUserBranchValidities.Where(x => x.CrMasUserBranchValidityBranchStatus == Status.Active);

            List<CrCasBranchInformation> branches = branchesValidite != null
                ? branchesValidite.Select(item => item.CrMasUserBranchValidity1).Where(x => x.CrCasBranchInformationStatus != Status.Deleted).ToList()
                : new List<CrCasBranchInformation>();

            var selectBranch = userLogin.CrMasUserInformationDefaultBranch;
            if (selectBranch == null || selectBranch == "000") selectBranch = "100";
            var checkBranch = branches.Find(x => x.CrCasBranchInformationCode == selectBranch);
            if (checkBranch == null) selectBranch = branches.FirstOrDefault()?.CrCasBranchInformationCode;
            var branch = _unitOfWork.CrCasBranchInformation.Find(x => x.CrCasBranchInformationCode == selectBranch && x.CrCasBranchInformationLessor == lessorCode, new[] { "CrCasBranchPost", "CrCasBranchPost.CrCasBranchPostCityNavigation", "CrCasBranchInformationLessorNavigation" });
            var Documents = await _unitOfWork.CrCasBranchDocument.FindAllAsNoTrackingAsync(x => x.CrCasBranchDocumentsLessor == lessorCode && x.CrCasBranchDocumentsBranch == branch.CrCasBranchInformationCode);
            var DocumentsCar = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllAsNoTrackingAsync(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode && x.CrCasCarDocumentsMaintenanceBranch == branch.CrCasBranchInformationCode && x.CrCasCarDocumentsMaintenanceProceduresClassification == "12");
            var MaintainceCar = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllAsNoTrackingAsync(x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode && x.CrCasCarDocumentsMaintenanceBranch == branch.CrCasBranchInformationCode && x.CrCasCarDocumentsMaintenanceProceduresClassification == "13");
            var PriceCar = await _unitOfWork.CrCasPriceCarBasic.FindAllAsNoTrackingAsync(x => x.CrCasPriceCarBasicLessorCode == lessorCode);
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
        public async Task<string> GetNextAccountReceiptNo(string LessorCode, string BranchCode, string procedure)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Records = await _unitOfWork.CrCasAccountReceipt.FindAllAsNoTrackingAsync(x => x.CrCasAccountReceiptLessorCode == LessorCode &&
                                                                       x.CrCasAccountReceiptYear == y && x.CrCasAccountReceiptBranchCode == BranchCode && x.CrCasAccountReceiptType == procedure);


            var Lrecord = Records.Max(x => x.CrCasAccountReceiptNo.Substring(x.CrCasAccountReceiptNo.Length - 6, 6));
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
        public async Task<string> GetNextAdministrativeNo(string LessorCode, string BranchCode, string procedure)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Records = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllAsNoTrackingAsync(x => x.CrCasSysAdministrativeProceduresLessor == LessorCode &&
                x.CrCasSysAdministrativeProceduresCode == procedure
                && x.CrCasSysAdministrativeProceduresSector == "1"
                && x.CrCasSysAdministrativeProceduresYear == y);
            var Lrecord = Records.Max(x => x.CrCasSysAdministrativeProceduresNo.Substring(x.CrCasSysAdministrativeProceduresNo.Length - 6, 6));
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
        public async Task<OwnersLayoutVM> OwnersDashboadInfo(string lessorCode)
        {
            OwnersLayoutVM ownersLayoutVM = new OwnersLayoutVM();

            // استخدام FindAllWithSelectAsNoTrackingAsync مع تحديد الأعمدة فقط
            var contracts = await _unitOfWork.CrCasRenterContractBasic
                .FindAllWithSelectAsNoTrackingAsync<OwnContractsVM>(
                    x => x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicStatus != Status.Extension,
                    query => query.Select(x => new OwnContractsVM
                    {
                        CrCasRenterContractBasicNo = x.CrCasRenterContractBasicNo,
                        CrCasRenterContractBasicCopy = x.CrCasRenterContractBasicCopy,
                        CrCasRenterContractBasicYear = x.CrCasRenterContractBasicYear,
                        CrCasRenterContractBasicSector = x.CrCasRenterContractBasicSector,
                        CrCasRenterContractBasicProcedures = x.CrCasRenterContractBasicProcedures,
                        CrCasRenterContractBasicLessor = x.CrCasRenterContractBasicLessor,
                        CrCasRenterContractBasicBranch = x.CrCasRenterContractBasicBranch,
                        CrCasRenterContractBasicOwner = x.CrCasRenterContractBasicOwner,
                        CrCasRenterContractBasicRenterId = x.CrCasRenterContractBasicRenterId,
                        CrCasRenterContractBasicIssuedDate = x.CrCasRenterContractBasicIssuedDate,
                        CrCasRenterContractBasicExpectedStartDate = x.CrCasRenterContractBasicExpectedStartDate,
                        CrCasRenterContractBasicExpectedEndDate = x.CrCasRenterContractBasicExpectedEndDate,
                        CrCasRenterContractBasicExpectedRentalDays = x.CrCasRenterContractBasicExpectedRentalDays,
                        CrCasRenterContractBasicActualDays = x.CrCasRenterContractBasicActualDays,
                        CrCasRenterContractUserReference = x.CrCasRenterContractUserReference,
                        CrCasRenterContractBasicUserInsert = x.CrCasRenterContractBasicUserInsert,
                        CrCasRenterContractBasicStatus = x.CrCasRenterContractBasicStatus,
                        CrCasRenterContractBasicActualTotal = x.CrCasRenterContractBasicActualTotal,
                        CrCasRenterContractBasicCompensationValue = x.CrCasRenterContractBasicCompensationValue
                    })
                );
            ownersLayoutVM.OwnContracts = contracts;
            ownersLayoutVM.RateContractsMonthBefore = UpRateBeforeMonthForContracts(ownersLayoutVM.OwnContracts);
            // استرجاع السيارات مع تحديد الأعمدة المطلوبة فقط
            var cars = await _unitOfWork.CrCasCarInformation
                .FindAllWithSelectAsNoTrackingAsync<OwnCarsInfoVM>(
                    x => x.CrCasCarInformationLessor == lessorCode && x.CrCasCarInformationStatus != Status.Sold,
                    query => query.Select(x => new OwnCarsInfoVM
                    {
                        CrCasCarInformationJoinedFleetDate = x.CrCasCarInformationJoinedFleetDate,
                        CrCasCarInformationPriceStatus = x.CrCasCarInformationPriceStatus,
                        CrCasCarInformationBranchStatus = x.CrCasCarInformationBranchStatus,
                        CrCasCarInformationOwnerStatus = x.CrCasCarInformationOwnerStatus,
                        CrCasCarInformationForSaleStatus = x.CrCasCarInformationForSaleStatus,
                        CrCasCarInformationStatus = x.CrCasCarInformationStatus
                    })
                );
            ownersLayoutVM.OwnCars = cars;
            ownersLayoutVM.RateCarsMonthBefore = UpRateBeforeMonthForCars(ownersLayoutVM.OwnCars);
            // استرجاع المستأجرين مع تحديد الأعمدة المطلوبة فقط
            var renters = await _unitOfWork.CrCasRenterLessor
                .FindAllWithSelectAsNoTrackingAsync<OwnRentersVM>(
                    x => x.CrCasRenterLessorCode == lessorCode,
                    query => query.Select(x => new OwnRentersVM
                    {
                        CrCasRenterLessorDateFirstInteraction = x.CrCasRenterLessorDateFirstInteraction,
                        CrCasRenterLessorStatus = x.CrCasRenterLessorStatus
                    })
                );
            ownersLayoutVM.OwnRenters = renters;
            ownersLayoutVM.RateRentersMonthBefore = UpRateBeforeMonthForRenters(ownersLayoutVM.OwnRenters);

            var activeContractNumbers = contracts.Where(x => x.CrCasRenterContractBasicStatus == Status.Active).Select(c => c.CrCasRenterContractBasicRenterId).ToList();
            // استرجاع المستأجرين بحالة R فقط
            //var rentersWithActiveContracts = await _unitOfWork.CrCasRenterLessor
            //    .FindAllWithSelectAsNoTrackingAsync<OwnRentersVM>(
            //        x => activeContractNumbers.Contains(x.CrCasRenterLessorId) // المستأجرين المرتبطين بالعقود النشطة
            //             && x.CrCasRenterLessorStatus == Status.Rented,    // المستأجرين بحالة R
            //        query => query.Select(x => new OwnRentersVM
            //        {
            //            CrCasRenterLessorDateFirstInteraction = x.CrCasRenterLessorDateFirstInteraction,
            //            CrCasRenterLessorStatus = x.CrCasRenterLessorStatus
            //        })
            //    );
            //ownersLayoutVM.RentersWithContracts = rentersWithActiveContracts.Count();
            //ownersLayoutVM.RentersWithountContracts = renters.Count() - rentersWithActiveContracts.Count();
            return ownersLayoutVM;
        }
        private double UpRateBeforeMonthForContracts(List<OwnContractsVM> contracts)
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var beforeMonth = currentMonth - 1;
            double contractBeforeMonth = contracts.FindAll(x => x.CrCasRenterContractBasicIssuedDate?.Month == beforeMonth).Count();
            double contractCurrentMonth = contracts.FindAll(x => x.CrCasRenterContractBasicIssuedDate?.Month == currentMonth).Count();
            if (contractBeforeMonth == 0 || contractCurrentMonth == 0) return 0;
            double rate = contractCurrentMonth / contractBeforeMonth;
            return rate;
        }
        private double UpRateBeforeMonthForCars(List<OwnCarsInfoVM> cars)
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var beforeMonth = currentMonth - 1;
            double carsBeforeMonth = cars.FindAll(x => x.CrCasCarInformationJoinedFleetDate?.Month == beforeMonth).Count();
            double carsCurrentMonth = cars.FindAll(x => x.CrCasCarInformationJoinedFleetDate?.Month == currentMonth).Count();
            if (carsBeforeMonth == 0 || carsCurrentMonth == 0) return 0;
            double rate = carsCurrentMonth / carsBeforeMonth;
            return rate;
        }
        private double UpRateBeforeMonthForRenters(List<OwnRentersVM> renters)
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var beforeMonth = currentMonth - 1;
            double rentersBeforeMonth = renters.FindAll(x => x.CrCasRenterLessorDateFirstInteraction?.Month == beforeMonth).Count();
            double rentersCurrentMonth = renters.FindAll(x => x.CrCasRenterLessorDateFirstInteraction?.Month == currentMonth).Count();
            if (rentersBeforeMonth == 0 || rentersCurrentMonth == 0) return 0;
            double rate = rentersCurrentMonth / rentersBeforeMonth;
            return rate;
        }
    }
}
