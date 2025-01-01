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
            var docsForBranches = await GetDocsStatusForBranches(user.CrMasUserInformationLessor); // Adjusted to fetch sections based on the system
            //var sidebarMenu = await BuildSidebarMenu(userInfo, systemType, sections);
            //return View(sidebarMenu);
            NotificationsForCasVM notificationsForCas = new NotificationsForCasVM();
            notificationsForCas.docsForCompany = docsForBranches;
            return View("NotificationsCAS", notificationsForCas);
        }


        private async Task<DocsForCompanyVM> GetDocsStatusForBranches(string lessorCode)
        {
            string currentCulture = CultureInfo.CurrentCulture.Name;

            DocsForCompanyVM docsForCompanyVM = new DocsForCompanyVM();
            var DocsForBranches = await _unitOfWork.CrCasBranchDocument.FindAllWithSelectAsNoTrackingAsync(x => x.CrCasBranchDocumentsLessor == lessorCode &&
                                                                                                                x.CrCasBranchDocumentsStatus != Status.Active &&
                                                                                                                x.CrCasBranchDocumentsStatus != Status.Deleted,
            query => query.Select(x => new
            {
                x.CrCasBranchDocumentsStatus,
                x.CrCasBranchDocumentsProcedures
            })); // Projection to select only the Status field
                 // استخراج قائمة الإجراءات
            var procedureCodes = DocsForBranches.Select(doc => doc.CrCasBranchDocumentsProcedures)
                                                .Distinct()
                                                .ToList();

            // الآن نبحث عن الإجراءات التي تتوافق مع هذه القيم
            var procedures = await _unitOfWork.CrMasSysProcedure
                                              .FindAllWithSelectAsNoTrackingAsync(
                                                  x => procedureCodes.Contains(x.CrMasSysProceduresCode), // افتراض أن "ProcedureCode" هو العمود الذي يمثل الإجراءات
                                                  query => query.Select(x => new
                                                  {
                                                      x.CrMasSysProceduresCode,
                                                      x.CrMasSysProceduresArName,
                                                      x.CrMasSysProceduresEnName
                                                  })
                                              );


            // تنظيم البيانات في الـ ViewModel مع ترتيب الإجراءات حسب الـ Procedure Code
            docsForCompanyVM.DocsForBranches = DocsForBranches
     .GroupBy(doc => doc.CrCasBranchDocumentsProcedures)
     .Select(group =>
     {
         // الحصول على اسم الإجراء باستخدام الـ Code
         var procedure = procedures.FirstOrDefault(p => p.CrMasSysProceduresCode == group.Key);

         // حساب عدد الحالات (Statuses)
         var statusCounts = group
             .GroupBy(doc => doc.CrCasBranchDocumentsStatus)
             .ToDictionary(g => g.Key.ToString(), g => g.Count());

         // إضافة RenewCount إلى ExpireCount
         int combinedExpireCount =
             (statusCounts.ContainsKey(Status.Expire.ToString()) ? statusCounts[Status.Expire.ToString()] : 0) +
             (statusCounts.ContainsKey(Status.Renewed.ToString()) ? statusCounts[Status.Renewed.ToString()] : 0);

         return new StatusForModelNotificationVM
         {
             Code = procedure.CrMasSysProceduresCode,
             Name = currentCulture == "en-US" ? procedure?.CrMasSysProceduresEnName : procedure?.CrMasSysProceduresArName,
             ExpireCount = combinedExpireCount,  // إضافة RenewCount إلى ExpireCount
             AboutExpireCount = statusCounts.ContainsKey(Status.AboutToExpire.ToString()) ? statusCounts[Status.AboutToExpire.ToString()] : 0,
             RenewCount = statusCounts.ContainsKey(Status.Renewed.ToString()) ? statusCounts[Status.Renewed.ToString()] : 0
         };
     })
     .OrderBy(x => procedures.FirstOrDefault(p => p.CrMasSysProceduresCode == x.Code)?.CrMasSysProceduresCode)
     .ToList();
            docsForCompanyVM.HaveExpireOrNot = DocsForBranches.Any(doc =>
        doc.CrCasBranchDocumentsStatus == Status.Expire ||
        doc.CrCasBranchDocumentsStatus == Status.Renewed);
            return docsForCompanyVM;
        }
    }
}
