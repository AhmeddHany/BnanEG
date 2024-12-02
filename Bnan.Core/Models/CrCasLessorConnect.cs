using System;
using System.Collections.Generic;

namespace Bnan.Core.Models
{
    public partial class CrCasLessorConnect
    {
        public string CrCasLessorConnectId { get; set; } = null!;
        public string CrCasLessorConnectSerial { get; set; } = null!;
        public string? CrCasLessorConnectName { get; set; }
        public string? CrCasLessorConnectMobile { get; set; }
        public string? CrCasLessorConnectDeviceType { get; set; }
        public bool? CrCasLessorConnectIsBusiness { get; set; }
        public string? CrCasLessorConnectUserLogin { get; set; }
        public DateTime? CrCasLessorConnectLoginDatetime { get; set; }
        public string? CrCasLessorConnectUserLogout { get; set; }
        public DateTime? CrCasLessorConnectLogoutDatetime { get; set; }
        public string? CrCasLessorConnectStatus { get; set; }

        public virtual CrMasLessorInformation CrCasLessorConnectNavigation { get; set; } = null!;
        public virtual CrMasUserInformation? CrCasLessorConnectUserLoginNavigation { get; set; }
        public virtual CrMasUserInformation? CrCasLessorConnectUserLogoutNavigation { get; set; }
    }
}
