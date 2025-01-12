using Bnan.Core.Models;

namespace Bnan.Core.Extensions
{
    public static class WarningHelper
    {
        public static bool HasWarnings(CrMasUserInformation userInfo)
        {
            if (userInfo == null)
                return false;

            // التحقق من الخصائص المرتبطة بـ CrMasUserInformationLessorNavigation
            var lessorNavigation = userInfo.CrMasUserInformationLessorNavigation;
            if (lessorNavigation == null)
                return false;

            // التحقق من حالة الشركة أو المستخدم
            if (lessorNavigation.CrMasLessorInformationStatus == Status.Hold ||
                userInfo.CrMasUserInformationStatus == Status.Hold)
            {
                return true;
            }

            // التحقق من العقود
            var contractCompanies = lessorNavigation.CrMasContractCompanies;
            if (contractCompanies == null || !contractCompanies.Any())
                return false;

            // تحقق من حالة العقد
            bool hasContractStatus = contractCompanies.Any(x =>
                x.CrMasContractCompanyLessor == userInfo.CrMasUserInformationLessor &&
                x.CrMasContractCompanyProcedures == "112" &&
                (x.CrMasContractCompanyStatus == Status.Renewed ||
                 x.CrMasContractCompanyStatus == Status.Expire ||
                 x.CrMasContractCompanyStatus == Status.AboutToExpire));

            if (hasContractStatus)
            {
                return true;
            }

            // التحقق من جميع الـ connects
            bool hasShomoosConnect = lessorNavigation.CrCasLessorShomoosConnect != null
                                      && lessorNavigation.CrCasLessorShomoosConnect.CrMasLessorShomoosConnectStatus == Status.Active;

            bool hasSmsConnect = lessorNavigation.CrCasLessorSmsConnect != null
                                  && lessorNavigation.CrCasLessorSmsConnect.CrMasLessorSmsConnectStatus == Status.Active;

            bool hasTgaConnect = lessorNavigation.CrCasLessorTgaConnect != null
                                  && lessorNavigation.CrCasLessorTgaConnect.CrMasLessorTgaConnectStatus == Status.Active;

            bool hasWhatsupConnect = lessorNavigation.CrCasLessorWhatsupConnects.LastOrDefault() is var lastWhatsupConnect
                                     && lastWhatsupConnect?.CrCasLessorWhatsupConnectStatus == Status.Active;

            // إذا كان أي من الـ connects في حالة Active، يظهر التحذير
            if (!hasShomoosConnect || !hasSmsConnect || !hasTgaConnect || !hasWhatsupConnect)
            {
                return true;
            }

            return false;
        }
    }
}
