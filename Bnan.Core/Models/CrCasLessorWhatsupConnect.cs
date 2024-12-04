using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasLessorWhatsupConnect
    {
        public string CrCasLessorWhatsupConnectId { get; set; } = null!;
        public string? CrCasLessorWhatsupConnectLessor { get; set; }
        public int? CrCasLessorWhatsupConnectSerial { get; set; }
        public string? CrCasLessorWhatsupConnectName { get; set; }
        public string? CrCasLessorWhatsupConnectMobile { get; set; }
        public string? CrCasLessorWhatsupConnectDeviceType { get; set; }
        public bool? CrCasLessorWhatsupConnectIsBusiness { get; set; }
        public string? CrCasLessorWhatsupConnectUserLogin { get; set; }
        public DateTime? CrCasLessorWhatsupConnectLoginDatetime { get; set; }
        public string? CrCasLessorWhatsupConnectUserLogout { get; set; }
        public DateTime? CrCasLessorWhatsupConnectLogoutDatetime { get; set; }
        public string? CrCasLessorWhatsupConnectStatus { get; set; }

        public virtual CrMasLessorInformation? CrCasLessorWhatsupConnectLessorNavigation { get; set; }
        public virtual CrMasUserInformation? CrCasLessorWhatsupConnectUserLoginNavigation { get; set; }
        public virtual CrMasUserInformation? CrCasLessorWhatsupConnectUserLogoutNavigation { get; set; }
    }
}
