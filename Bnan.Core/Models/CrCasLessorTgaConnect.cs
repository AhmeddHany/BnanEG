namespace Bnan.Core.Models
{
    public partial class CrCasLessorTgaConnect
    {
        public string CrMasLessorTgaConnectLessor { get; set; } = null!;
        public string? CrMasLessorTgaConnectContentType { get; set; }
        public string? CrMasLessorTgaConnectAppId { get; set; }
        public string? CrMasLessorTgaConnectAppKey { get; set; }
        public string? CrMasLessorTgaConnectAuthorization { get; set; }
        public string? CrMasLessorTgaConnectStatus { get; set; }

        public virtual CrMasLessorInformation CrMasLessorTgaConnectLessorNavigation { get; set; } = null!;
    }
}
