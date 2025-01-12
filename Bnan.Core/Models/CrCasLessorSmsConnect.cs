namespace Bnan.Core.Models
{
    public partial class CrCasLessorSmsConnect
    {
        public string CrMasLessorSmsConnectLessor { get; set; } = null!;
        public string? CrMasLessorSmsConnectName { get; set; }
        public string? CrMasLessorSmsConnectAuthorization { get; set; }
        public string? CrMasLessorSmsConnectStatus { get; set; }

        public virtual CrMasLessorInformation CrMasLessorSmsConnectLessorNavigation { get; set; } = null!;
    }
}
