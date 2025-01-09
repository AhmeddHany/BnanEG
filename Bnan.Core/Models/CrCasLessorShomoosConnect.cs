namespace Bnan.Core.Models
{
    public partial class CrCasLessorShomoosConnect
    {
        public string CrMasLessorShomoosConnectLessor { get; set; } = null!;
        public string? CrMasLessorShomoosConnectContentType { get; set; }
        public string? CrMasLessorShomoosConnectAppId { get; set; }
        public string? CrMasLessorShomoosConnectAppKey { get; set; }
        public string? CrMasLessorShomoosConnectAuthorization { get; set; }
        public string? CrMasLessorShomoosConnectStatus { get; set; }

        public virtual CrMasLessorInformation CrMasLessorShomoosConnectLessorNavigation { get; set; } = null!;
    }
}
