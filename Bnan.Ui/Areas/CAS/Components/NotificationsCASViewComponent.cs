using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Ui.ViewModels.CAS.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Security.Claims;

namespace Bnan.Ui.Areas.CAS.Components
{
    public class NotificationsCASViewComponent : ViewComponent
    {
        private readonly UserManager<CrMasUserInformation> _userManager;
        private readonly IStringLocalizer<NotificationsCASViewComponent> _localizer;
        private readonly IUnitOfWork _unitOfWork;
        public NotificationsCASViewComponent(IStringLocalizer<NotificationsCASViewComponent> localizer, UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork)
        {
            _localizer = localizer;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {

            var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);
            var docsForBranches = await GetDocsStatusForBranches(user.CrMasUserInformationLessor);
            var docsForCar = await GetDocsStatusForCar(user.CrMasUserInformationLessor);
            var contractsStatus = await GetContractsStatus(user.CrMasUserInformationLessor);
            var employeesBalance = await GetEmployeesBalance(user.CrMasUserInformationLessor);
            var creditorsAndDebtorsForRenters = await GetCreditorsAndDebtors(user.CrMasUserInformationLessor);
            NotificationsForCasVM notificationsForCas = new NotificationsForCasVM();
            notificationsForCas.docsForCompany = docsForBranches;
            notificationsForCas.docsMainPriceForCar = docsForCar;
            notificationsForCas.contractStatus = contractsStatus;
            notificationsForCas.employeesBalances = employeesBalance;
            notificationsForCas.creditorsAndDebtors = creditorsAndDebtorsForRenters;
            return View("NotificationsCAS", notificationsForCas);
        }


        private async Task<DocsForCompanyVM> GetDocsStatusForBranches(string lessorCode)
        {
            string currentCulture = CultureInfo.CurrentCulture.Name;

            // تهيئة الـ ViewModel
            var docsForCompanyVM = new DocsForCompanyVM
            {
                DocsForBranches = new List<StatusForModelNotificationVM>() // تهيئة قائمة فارغة
            };

            // جلب بيانات الوثائق للفروع مع الإسقاط (projection)
            var docsForBranches = await _unitOfWork.CrCasBranchDocument.FindAllWithSelectAsNoTrackingAsync(
                x => x.CrCasBranchDocumentsLessor == lessorCode &&
                     x.CrCasBranchDocumentsStatus != Status.Active &&
                     x.CrCasBranchDocumentsStatus != Status.Deleted,
                query => query.Select(doc => new
                {
                    doc.CrCasBranchDocumentsStatus,
                    doc.CrCasBranchDocumentsProcedures
                })
            );

            // استخراج الأكواد الفريدة للإجراءات
            var procedureCodes = docsForBranches.Select(doc => doc.CrCasBranchDocumentsProcedures).Distinct().ToList();

            // جلب بيانات الإجراءات حتى إذا لم يكن هناك بيانات فرعية
            var procedures = await _unitOfWork.CrMasSysProcedure.FindAllWithSelectAsNoTrackingAsync(
                x => x.CrMasSysProceduresClassification == "10",
                query => query.Select(proc => new
                {
                    proc.CrMasSysProceduresCode,
                    proc.CrMasSysProceduresArName,
                    proc.CrMasSysProceduresEnName
                })
            );

            // بناء البيانات للـ ViewModel
            docsForCompanyVM.DocsForBranches = procedures
                .Select(procedure =>
                {
                    var group = docsForBranches.Where(doc => doc.CrCasBranchDocumentsProcedures == procedure.CrMasSysProceduresCode);

                    // حساب عدد الحالات (Statuses)
                    var statusCounts = group.GroupBy(doc => doc.CrCasBranchDocumentsStatus).ToDictionary(g => g.Key, g => g.Count());

                    // جمع الحالات Expire و Renewed
                    int combinedExpireCount =
                        (statusCounts.TryGetValue(Status.Expire, out int expireCount) ? expireCount : 0) +
                        (statusCounts.TryGetValue(Status.Renewed, out int renewedCount) ? renewedCount : 0);

                    // إنشاء كائن StatusForModelNotificationVM
                    return new StatusForModelNotificationVM
                    {
                        Code = procedure.CrMasSysProceduresCode,
                        Name = currentCulture == "en-US" ? procedure.CrMasSysProceduresEnName : procedure.CrMasSysProceduresArName,
                        ExpireCount = combinedExpireCount,
                        AboutExpireCount = statusCounts.TryGetValue(Status.AboutToExpire, out int aboutExpireCount) ? aboutExpireCount : 0,
                        //RenewCount = statusCounts.TryGetValue(Status.Renewed, out int renewedCount) ? renewedCount : 0 // التأكد من عدم وجود قيمة فارغة
                    };
                })
                .OrderBy(x => x.Code) // ترتيب حسب كود الإجراء
                .ToList();

            // التحقق إذا كانت هناك حالات Expire أو Renewed
            docsForCompanyVM.HaveExpireOrNot = docsForCompanyVM.DocsForBranches.Any(x => x.ExpireCount > 0 || x.RenewCount > 0);

            return docsForCompanyVM;
        }
        private async Task<DocsMainPriceForCarVM> GetDocsStatusForCar(string lessorCode)
        {
            var docsMainPriceForCar = new DocsMainPriceForCarVM
            {
                Documents = new StatusForModelNotificationVM(),
                Maintainces = new StatusForModelNotificationVM(),
                Prices = new StatusForModelNotificationVM()
            };

            // Count Documents
            docsMainPriceForCar.Documents.AboutExpireCount = await _unitOfWork.CrCasCarDocumentsMaintenance.CountAsync(x =>
                x.CrCasCarDocumentsMaintenanceLessor == lessorCode &&
                x.CrCasCarDocumentsMaintenanceProceduresClassification == "12" &&
                x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire);

            docsMainPriceForCar.Documents.ExpireCount = await _unitOfWork.CrCasCarDocumentsMaintenance.CountAsync(x =>
                x.CrCasCarDocumentsMaintenanceLessor == lessorCode &&
                x.CrCasCarDocumentsMaintenanceProceduresClassification == "12" &&
                x.CrCasCarDocumentsMaintenanceStatus == Status.Expire);

            // Count Maintenance
            docsMainPriceForCar.Maintainces.AboutExpireCount = await _unitOfWork.CrCasCarDocumentsMaintenance.CountAsync(x =>
                x.CrCasCarDocumentsMaintenanceLessor == lessorCode &&
                x.CrCasCarDocumentsMaintenanceProceduresClassification == "13" &&
                x.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire);

            docsMainPriceForCar.Maintainces.ExpireCount = await _unitOfWork.CrCasCarDocumentsMaintenance.CountAsync(x =>
                x.CrCasCarDocumentsMaintenanceLessor == lessorCode &&
                x.CrCasCarDocumentsMaintenanceProceduresClassification == "13" &&
                x.CrCasCarDocumentsMaintenanceStatus == Status.Expire);

            // Count Prices
            docsMainPriceForCar.Prices.AboutExpireCount = await _unitOfWork.CrCasPriceCarBasic.CountAsync(x =>
                x.CrCasPriceCarBasicLessorCode == lessorCode &&
                x.CrCasPriceCarBasicStatus == Status.AboutToExpire);

            docsMainPriceForCar.Prices.ExpireCount = await _unitOfWork.CrCasPriceCarBasic.CountAsync(x =>
                x.CrCasPriceCarBasicLessorCode == lessorCode &&
                x.CrCasPriceCarBasicStatus == Status.Expire);

            // Check if any item has Expire status
            docsMainPriceForCar.HaveExpireOrNot = docsMainPriceForCar.Prices.ExpireCount > 0 || docsMainPriceForCar.Maintainces.ExpireCount > 0 || docsMainPriceForCar.Documents.ExpireCount > 0;



            return docsMainPriceForCar;
        }
        private async Task<ContractStatusVM> GetContractsStatus(string lessorCode)
        {
            var contractStatus = new ContractStatusVM();

            contractStatus.SuspendCount = await _unitOfWork.CrCasRenterContractBasic.CountAsync(x => x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicStatus == Status.Suspend);
            contractStatus.SavedCount = await _unitOfWork.CrCasRenterContractBasic.CountAsync(x => x.CrCasRenterContractBasicLessor == lessorCode && x.CrCasRenterContractBasicStatus == Status.Saved);
            contractStatus.ExpiredCount = await _unitOfWork.CrCasRenterContractAlert.CountAsync(x => x.CrCasRenterContractAlertLessor == lessorCode && x.CrCasRenterContractAlertContractStatus == Status.Expire);
            contractStatus.ExpireLaterCount = await _unitOfWork.CrCasRenterContractAlert.CountAsync(x => x.CrCasRenterContractAlertLessor == lessorCode &&
                                                                                                      x.CrCasRenterContractAlertContractStatus == Status.Active &&
                                                                                                      x.CrCasRenterContractAlertContractActiviteStatus == "0");
            contractStatus.ExpireTommorrowCount = await _unitOfWork.CrCasRenterContractAlert.CountAsync(x => x.CrCasRenterContractAlertLessor == lessorCode &&
                                                                                                          x.CrCasRenterContractAlertContractStatus == Status.Active &&
                                                                                                          x.CrCasRenterContractAlertContractActiviteStatus == "1");
            contractStatus.ExpireTodayCount = await _unitOfWork.CrCasRenterContractAlert.CountAsync(x => x.CrCasRenterContractAlertLessor == lessorCode &&
                                                                                                          x.CrCasRenterContractAlertContractStatus == Status.Active &&
                                                                                                          x.CrCasRenterContractAlertContractActiviteStatus == "2");
            contractStatus.HaveExpireOrNot = contractStatus.SuspendCount > 0 || contractStatus.SavedCount > 0 || contractStatus.ExpiredCount > 0;
            return contractStatus;

        }
        private async Task<EmployeesBalanceVM> GetEmployeesBalance(string lessorCode)
        {
            var employeesBalance = new EmployeesBalanceVM
            {
                employeesInfo = new List<EmployeesInfoVM>() // تهيئة القائمة
            };

            var usersInformation = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                x => x.CrMasUserInformationLessor == lessorCode &&
                     x.CrMasUserInformationStatus != Status.Deleted &&
                     (x.CrMasUserInformationReservedBalance > 0 || x.CrMasUserInformationAvailableBalance > x.CrMasUserInformationCreditLimit), // استخدم OR بين الشروط
                query => query.Select(user => new
                {
                    user.CrMasUserInformationArName,
                    user.CrMasUserInformationEnName,
                    user.CrMasUserInformationCode,
                    user.CrMasUserInformationAvailableBalance,
                    user.CrMasUserInformationReservedBalance,
                    user.CrMasUserInformationCreditLimit,
                    user.CrMasUserInformationPicture,
                    user.CrMasUserInformationEntryLastTime,
                    user.CrMasUserInformationEntryLastDate,
                    user.CrMasUserInformationLastActionDate
                })
            );

            foreach (var user in usersInformation)
            {
                var fullName = CultureInfo.CurrentCulture.Name == "en-US" ? user.CrMasUserInformationEnName : user.CrMasUserInformationArName;
                // تقسيم الاسم باستخدام المسافات الفارغة (whitespace)
                var nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                // إذا كان الاسم يحتوي على أكثر من جزء، نأخذ أول جزئين فقط. وإذا كان جزءًا واحدًا، نستخدمه كما هو.
                var name = nameParts.Length > 1 ? string.Join(" ", nameParts.Take(2)) : fullName;
                bool onlineOrOffline = false;

                string lastActionTime = "00:00";
                string lastActionDate = user.CrMasUserInformationEntryLastDate?.ToString("yyyy/MM/dd");

                if (user.CrMasUserInformationEntryLastTime != null)
                {
                    var time = DateTime.Today.Add(user.CrMasUserInformationEntryLastTime.Value);
                    lastActionTime = time.ToString("HH:mm");
                }

                if (user.CrMasUserInformationLastActionDate == null) onlineOrOffline = false;
                else
                {
                    var timeDifference = DateTime.Now - user.CrMasUserInformationLastActionDate;
                    if (timeDifference?.TotalMinutes > 10) onlineOrOffline = false;
                    else onlineOrOffline = true;
                }


                var employeeBalance = new EmployeesInfoVM
                {
                    Name = name,
                    FullName = fullName,
                    OnlineOrOffline = onlineOrOffline,
                    LastActionDate = lastActionDate,
                    LastActionTime = lastActionTime,
                    AvaliableBalance = user.CrMasUserInformationAvailableBalance?.ToString("N2", CultureInfo.InvariantCulture),
                    ResevedBalance = user.CrMasUserInformationReservedBalance?.ToString("N2", CultureInfo.InvariantCulture),
                    HaveCustodyNotAccepted = user.CrMasUserInformationReservedBalance > 0,
                    CreditLimitExceeded = user.CrMasUserInformationAvailableBalance > user.CrMasUserInformationCreditLimit,
                };

                employeesBalance.employeesInfo.Add(employeeBalance);
            }

            // التحقق إذا كان هناك موظف لديه عهدة لم تسلم أو تجاوز الحد
            employeesBalance.IfHaveNotify = employeesBalance.employeesInfo.Any(x => x.HaveCustodyNotAccepted == true || x.CreditLimitExceeded == true);

            return employeesBalance;
        }
        private async Task<CreditorsAndDebtorsVM> GetCreditorsAndDebtors(string lessorCode)
        {
            var creditorsAndDebtorsVM = new CreditorsAndDebtorsVM();

            var renters = await _unitOfWork.CrCasRenterLessor
                                .FindAllWithSelectAsNoTrackingAsync(
                                    x => x.CrCasRenterLessorStatus == Status.Active && x.CrCasRenterLessorCode == lessorCode,
                                    query => query.Select(x => new
                                    {
                                        x.CrCasRenterLessorAvailableBalance
                                    })
            );

            decimal debtors = renters
                                .Where(x => x.CrCasRenterLessorAvailableBalance < 0)
                                .Sum(x => x.CrCasRenterLessorAvailableBalance ?? 0);

            decimal creditors = renters
                                .Where(x => x.CrCasRenterLessorAvailableBalance > 0)
                                .Sum(x => x.CrCasRenterLessorAvailableBalance ?? 0);

            creditorsAndDebtorsVM.Debtors = Math.Abs(debtors).ToString("N2", CultureInfo.InvariantCulture);
            creditorsAndDebtorsVM.Creditors = Math.Abs(creditors).ToString("N2", CultureInfo.InvariantCulture);
            creditorsAndDebtorsVM.IfHaveNotify = debtors != 0 || creditors != 0;
            return creditorsAndDebtorsVM;
        }
    }
}
